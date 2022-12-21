import os, sys, subprocess

def getConnectionStr():
	SID = os.environ["ORACLE_SID"]
	USER = os.environ["DB_USER"]
	PASS = os.environ["DB_PASS"]
	return USER+'/'+PASS+'@'+SID

def runSql(sqlPath, *args):
	connectionStr = getConnectionStr()
	path = os.path.join(os.getcwd(), sqlPath)
	proc = ["sql", "-S", connectionStr, '@'+path]
	proc.extend(args)
	session = subprocess.Popen( proc, stdin=subprocess.PIPE, stdout=subprocess.PIPE )
	result, error =  session.communicate()
	result = result.strip()
	return result, error
