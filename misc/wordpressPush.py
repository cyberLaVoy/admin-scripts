from getpass import getpass
import requests
import json
import os
import PyPDF2

def pushOfficialPolicies():

    with open("policyMetadata.json", 'r') as stream:
        metadata = json.load(stream)

    wp_login = "https://domain/login"
    wp_admin = "https://domain/admin/"
    username = input("Username: ")
    password = getpass()

    formEndpoint = "https://domain/form-entries"
    policiesDir = "official-policies"

    with requests.Session() as S:
        headers = { 'Cookie':'wordpress_test_cookie=WP Cookie check' }
        data ={ 
            'log':username, 'pwd':password, 'wp-submit':'Log In', 
            'redirect_to':wp_admin, 'testcookie':'1'  
        }
        result = S.post(wp_login, headers=headers, data=data) # authenticate with Wordpress
        
        for filename in os.listdir(policiesDir):
            if ".pdf" in filename:
                policyNumber = filename.split('.')[0]
                fin = open(os.path.join(policiesDir, filename), 'rb')
                pdfReader = PyPDF2.PdfFileReader(fin)
                content = ""
                for i in range(pdfReader.numPages):
                    content += pdfReader.getPage(i).extractText().strip().replace('\t', '').replace('\n', '')
                metadata[policyNumber]["740"] = content
                metadata[policyNumber]["726"] = "http://domain/content" + filename
                result = S.post(formEndpoint, data=metadata[policyNumber])
                print(policyNumber, ": ", result)

def pushPolicyDrafts():
    ownerEmails = { "President" : "",
                    "General Counsel" : "",
                    "Exec. Director Human Resources" : "",
                    "VP Academic Affairs / Provost" : "",
                    "VP Administrative Affairs" : "",
                    "VP Development" : "",
                    "VP Student Affairs" : "",
                    "VP Marketing and Communication" : ""}

    with open("draftsMetadata.json", 'r') as stream:
        metadata = json.load(stream)

    wp_login = "https://domain/login"
    wp_admin = "https://domain/admin/"
    username = input("Username: ")
    password = getpass()

    formEndpoint = "https://domain/content"

    with requests.Session() as S:
        headers = { 'Cookie':'wordpress_test_cookie=WP Cookie check' }
        data ={ 
            'log':username, 'pwd':password, 'wp-submit':'Log In', 
            'redirect_to':wp_admin, 'testcookie':'1'  
        }
        result = S.post(wp_login, headers=headers, data=data) # authenticate with Wordpress
        print("Auth Result: ", result)

        for policyNumber in metadata:
            metadata[policyNumber]["758"] = ownerEmails[metadata[policyNumber]["758"]]
            result = S.post(formEndpoint, data=metadata[policyNumber])
            #print(metadata[policyNumber])
            print(policyNumber, ": ", result)


def main():
    #pushOfficialPolicies()
    pushPolicyDrafts()

if __name__ == "__main__":
    main()