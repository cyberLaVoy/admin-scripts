import csv, os
from datetime import datetime, date
import xml.etree.ElementTree as ET

from transcripts import Transcripts
from sql import runSql

def getPopulation():
    # gathers info for population selected in associated sql query
    result, error = runSql("population.sql")
    result = result.replace('"', '').split("\n")
    population = [row for row in csv.reader(result)]
    return population

def validateXmlFind(element, match):
    value = element.find(match)
    if value is not None:
        return value.text
    else:
        return ""
def xmlTranscript2Dict(xml, slateID, bannerID, ssid, ceeb):
    # create dictionary of values to include in transcript csv
    info = {
    "TranscriptReceivedDate" : str(date.today())
    ,"APIURL" : "https://domain/resource/"
    ,"SlateID" : slateID
    ,"BannerID" : bannerID
    ,"SSID" : ssid
    ,"HSCEEB" : ceeb
    ,"AgencyAssignedID" : None
    ,"BirthDate" : None
    ,"BirthCountry" : None
    ,"FirstName" : None
    ,"LastName" : None
    ,"ParentGuardianFirstName" : None
    ,"ParentGuardianLastName" : None
    ,"NativeLanguage" : None
    ,"ParentLanguage" : None
    ,"AddressLine" : None
    ,"City" : None
    ,"StateProvinceCode" : None
    ,"PostalCode" : None
    ,"PhoneNumber" : None
    ,"GenderCode" : None
    ,"EthnicityCode" : None
    ,"RaceCode" : None
    ,"AcademicAwardDate" : None
    ,"CreditHoursEarned" : None
    ,"GradePointAverage" : None
    ,"ClassRank" : None
    ,"ClassSize" : None
    }
    actTestsTypes = ["Composite", "Mathematics", "ScienceReasoning", "ELA", "STEM", "EnglishWriting", "English", "Writing", "Reading", "TestDate"]
    # add an entry for each type of ACT test score possible
    for testType in actTestsTypes:
        for i in range(1, 7):
            info["ACT"+testType+str(i)] = None

    # set up xml parse tree
    tree = ET.fromstring(xml)
    student = tree.find("Student")

    # parse over xml tree
    extraFoundVals = {}
    testCount = 0
    for el in student.iter():
        if el.tag == "Name":
            info["FirstName"] = validateXmlFind(el, "FirstName")
            info["LastName"] = validateXmlFind(el, "LastName")
        elif el.tag == "ParentGuardianName":
            info["ParentGuardianFirstName"] = validateXmlFind(el, "FirstName")
            info["ParentGuardianLastName"] = validateXmlFind(el, "LastName")
        elif el.tag == "Tests":
            testName = validateXmlFind(el, "TestName").lower()
            # skip over test if not an ACT test score
            if "act" not in testName:
                continue
            testCount += 1
            testDate = validateXmlFind(el, "TestDate")
            info["ACT"+"TestDate"+str(testCount)] = testDate
            for subTest in el.findall("Subtest"):
                subTestName = validateXmlFind(subTest, "SubtestName").lower()
                score = validateXmlFind(subTest, "TestScores/ScoreValue")
                if "high" in subTestName or not score.isdigit():
                    continue
                # condition sequence for validating test score names
                if "comp" in subTestName:
                    scoreType = "Composite"
                elif "math" in subTestName:
                    scoreType = "Mathematics"
                elif "sci" in subTestName:
                    scoreType = "ScienceReasoning"
                elif "ela" in subTestName:
                    scoreType = "ELA" 
                elif "stem" in subTestName:
                    scoreType = "STEM"
                elif "eng" in subTestName and "w" in subTestName:
                    scoreType = "EnglishWriting"
                elif "eng" in subTestName and "w" not in subTestName:
                    scoreType = "English"
                elif "eng" not in subTestName and "w" in subTestName:
                    scoreType = "Writing"
                elif "read" in subTestName:
                    scoreType = "Reading"
                else:
                    print( "Invalid test score name: %s, under test name: %s" % (subTestName, testName) )
                info["ACT"+scoreType+str(testCount)] = score
        elif el.tag == "Language":
            language = validateXmlFind(el, "LanguageCode")
            meta = validateXmlFind(el, "NoteMessage").replace(' ', '')
            if meta in info:
                info[meta] = language 
        else:
            # catch all extra fields not in condition sequece
            if el.tag not in extraFoundVals:
                extraFoundVals[el.tag] = el.text

    # phone number as special case in extra fields found
    if "PhoneNumber" in extraFoundVals and "AreaCityCode" in extraFoundVals:
        info["PhoneNumber"] = extraFoundVals["AreaCityCode"] + '-' + extraFoundVals["PhoneNumber"] 
    elif "PhoneNumber" in extraFoundVals and "AreaCityCode" not in extraFoundVals:
        info["PhoneNumber"] = extraFoundVals["PhoneNumber"] 
    if "PhoneNumber" in extraFoundVals:
        del extraFoundVals["PhoneNumber"]
    if "AreaCityCode" in extraFoundVals:
        del extraFoundVals["AreaCityCode"]

    for key in info:
        # fill values listed to include, and found, in info
        if key in extraFoundVals:
            info[key] = extraFoundVals[key]
        # validate fields in info
        if info[key] is None:
            info[key] = ''
        info[key] = info[key].replace(',', '')

    return info

def sendTranscript(transcript, ftpPath, baseFileName):
    # places transcript csv in folder for auto ftp
    finalPath = os.path.join(ftpPath, baseFileName+".csv")
    tempPath = finalPath + "temp"
    with open(tempPath, 'w') as fout:
        writer = csv.DictWriter(fout, transcript.keys())
        writer.writeheader()
        writer.writerow(transcript)
        # the daemon that runs for FTP transfer only detects file modifications, not creation
        os.rename(tempPath, finalPath)

def recordProcessed(banner_id, load_date):
    # updates temp table to indicate successfuly processed transcript
    result, error = runSql("processed_transcript_update.sql", banner_id, load_date)
    return result, error

def savePaperTrail(transcript, baseFileName, banner_id, last_name):
    # writes transcript info to file to be processed in remote location by webXtender
    with open("remote/"+baseFileName+".xml", 'w') as fout:
        fout.write("****"+banner_id+'|'+last_name+'|'+datetime.now().strftime("%Y-%m-%d %H:%M:%S")+'|'+'\n')
        fout.write(transcript)               

def main():
    print("Attaining authorization for requesting transcripts...")
    transcripts = Transcripts()

    # set up path for transcript csv ftp based on current working directory
    ftpPath = os.getcwd().split('/')
    ftpPath = "/".join(ftpPath[:-2])
    ftpPath = os.path.join(ftpPath, "ftp", "outgoing")

    processedUpdate = []
    print("Gathering population to process possible transcripts...") 
    population = getPopulation()
    print( "Attempting proccessing of transcripts for total population of: " + str(len(population)) ) 
    for person in population:
        banner_id, slate_id, ssid, birth_year, birth_month, birth_day, district, ceeb, load_date = person
        transcriptXML = transcripts.getTranscript(district, ssid, birth_year, birth_month, birth_day)
        if transcriptXML is not None:
            try:
                transcriptDict = xmlTranscript2Dict(transcriptXML, slate_id, banner_id, ssid, ceeb)
                baseFileName = "utrex_hs_etranscript_" + datetime.now().strftime("%Y%m%d%H%M%S")
                sendTranscript(transcriptDict, ftpPath, baseFileName)
                savePaperTrail(transcriptXML, baseFileName, banner_id, transcriptDict["LastName"])
                processedUpdate.append( (banner_id, load_date) )
            except Exception as e:
                print( "Problem processing recieved transcript for %s: %s" % (banner_id, str(e)) )

    print("%s/%s successfully processed transcripts." % ( str(len(processedUpdate)), str(len(population)) ) )

    if len(processedUpdate) > 0:
        print("Recording successfully processed transcripts...") 
        for banner_id, load_date in processedUpdate: 
            recordProcessed(banner_id, load_date)

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        print( "[FEED_PROBLEM][[" + str(e) + "]]" )
        exit(1)