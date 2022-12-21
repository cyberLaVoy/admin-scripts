import sys, os, urllib2, base64, json, csv
from objects import ApplicationCollection
from datetime import datetime, timedelta


def getCredentials(credentialsPath):
	# reads from the file path provided to return the credentials
	with open(credentialsPath, 'r') as fin:
		credentials = [ s.strip() for s in fin.readlines() ]
	return credentials[0], credentials[1]

def retrieveCurrApps(startDate, endDate, url, credentialsPath):
	# makes a GET request from the API to retrieve applications
	username, password = getCredentials(credentialsPath)
	queryString = "?" + "start_date=" + startDate + "&end_date=" + endDate  
	request = urllib2.Request(url + queryString)
	base64string = base64.encodestring( '%s:%s' % (username, password) )[:-1]
	request.add_header("Authorization", "Basic %s" % base64string)   
	response = urllib2.urlopen(request)
	apps = [row for row in csv.DictReader(response)]
	return ApplicationCollection( apps )
	
def retrievePrevApps(pushedAppsPath):
	# reads from the provided file path to obtain past applications
	apps = json.load( open(pushedAppsPath,'r') )
	return ApplicationCollection( apps )

def transferNewApps(newApps, FTPpath):
	# drops the new applications in the appropriate location for FTP transfer
	baseFileName = "ushe_common_app_" + datetime.now().strftime("%Y%m%d%H%M%S") 
	newApps.formatApps()
	apps = newApps.getAppsList()
	for i in range(len(apps)):
		finalPath = os.path.join(FTPpath, baseFileName + '_' + str(i)+ ".csv")
		tempPath = finalPath + "temp"
		out = csv.writer( open(tempPath, 'w') )
		out.writerow(apps[i].keys())
		out.writerow(apps[i].values())
		# the daemon that runs for FTP transfer only detects file modifications, not creation
		os.rename(tempPath, finalPath) 

def recordNewApps(newApps, prevApps, pushedAppsPath):
	# adds the new applications to the list of applications processed
	prevApps.merge(newApps)
	json.dump( prevApps.getAppsList(), open(pushedAppsPath, 'w'), indent=4 )


def main():
	# url for the provided API
	appsUrl = "https://domain/resource" # for csv format
	# number of days to look back for new appilations
	daysNew = 29
	# number of days to keep record of past applications (must be longer than daysNew)
	daysOld = 120

	# calculate required dates and format them
	appsStartDate = ( datetime.now() - timedelta(days=daysNew) ).strftime("%Y-%m-%dT%X")
	appsEndDate = datetime.now().strftime("%Y-%m-%dT%X")

	# set required file path from current working directory
	pushedAppsPath = os.path.join(os.getcwd(), "pushed_apps.json")
	credentialsPath = os.path.join(os.getcwd(), ".credentials")
	ftpPath = os.getcwd().split('/')
	ftpPath = "/".join(ftpPath[:len(ftpPath)-2])
	ftpPath = os.path.join(ftpPath, "ftp", "outgoing")

	print("Retrieving all applications from " + appsStartDate + " - " + appsEndDate)
	currApps = retrieveCurrApps(appsStartDate, appsEndDate, appsUrl, credentialsPath)
	prevApps = retrievePrevApps(pushedAppsPath)

	# remove any applications already sent from the current application pool
	currApps.dedup(prevApps)

	if not currApps.isEmpty():
		print("Sending new applications for tranfser...")
		transferNewApps(currApps, ftpPath)
	else:
		print("There are no new applications available.")

	print("Removing applications " + str(daysOld) + " old, or older, from " + pushedAppsPath)
	prevApps.removeOld(daysOld)

	print("Recording changes in " + pushedAppsPath)
	recordNewApps(currApps, prevApps, pushedAppsPath)

if __name__ == "__main__":
	try:
		main()
	except Exception as e:
		print( "[FEED_PROBLEM][[" + str(e) + "]]" )
		exit(1)

