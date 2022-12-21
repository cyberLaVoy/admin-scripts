import urllib, urllib2

KEY_PATH = ".api_key"

class Transcripts:
    def __init__(self, keyPath=KEY_PATH):
        self.keyPath = keyPath
        self.auth_token = self.getAuthToken()

    def getAuthToken(self):
        with open(self.keyPath, 'r') as fin:
            key = fin.read().strip()
        url = "https://domain/token"
        data = urllib.urlencode( {"grant_type" : "client_credentials", "client_id" : key} )
        request = urllib2.Request(url, data=data)
        response = urllib2.urlopen(request)
        return eval(response.read())["access_token"]

    def getTranscript(self, district, ssid, birth_year, birth_month, birth_day):
        url = "https://domain/resource/%s/%s/%s/%s/%s" % (district, ssid, birth_year, birth_month, birth_day) 
        request = urllib2.Request(url)
        request.add_header("Authorization", "bearer %s" % self.auth_token)   
        try:
            response = urllib2.urlopen(request)
            return response.read()
        except:
            return None