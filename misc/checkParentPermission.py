import urllib2, base64

def retrieveParentPermissionInfo(ssid):
	username, password = "", ""
	url = "" + ssid
	request = urllib2.Request(url)
	base64string = base64.encodestring('%s:%s' % (username, password))[:-1]
	request.add_header("Authorization", "Basic %s" % base64string)
	success = True
	try:
		response = urllib2.urlopen(request)
		return success, response.read()
	except Exception as e:
		success = False
		return success, e

def main():
	with open("data.csv", 'r') as fin:
		fin.readline()
		print( "pidm, id, permission_date" )
		for line in fin:
			pidm, id, ssid = line.split(',') 
			success, result = retrieveParentPermissionInfo(ssid)
			if success:
				result = eval(result)
				print( pidm + ',' + id + ',' + result["result"]["signature_date"] )
			else:
				print( pidm + ',' + id + ',' )

main()

