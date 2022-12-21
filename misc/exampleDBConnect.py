#!/bin/python
import os, sys, subprocess

#Critical and Warning time limits are in fractions of an hour. So "1" = 1 hour and ".25" = 15 minutes
CLIMIT = 1
WLIMIT = .5
 
def runSqlQuery(sqlCommand, connectionString):
        session = subprocess.Popen( ["sql", "-L", "-S", connectionString], stdin=subprocess.PIPE, stdout=subprocess.PIPE )
        return session.communicate(sqlCommand)

def main():
        with open(".credentials", 'r') as fin:
                username = fin.readline().strip().split('=')[1]
                password = fin.readline().strip().split('=')[1]
        SID = ""
        HOST = ""
        PORT = ""
        connectionString = username+'/'+password+'@'+HOST+':'+PORT+'/'+SID
        sqlCommand = """set pagesize 0 feedback off verify off heading off echo off;
                        select ((sysdate - applied_time)*24) from v$logstdby_progress; """
        queryResult, errorMessage = runSqlQuery(sqlCommand, connectionString)
        queryResult = float(queryResult)
        if queryResult < WLIMIT:
                queryResult = round(float(queryResult * 3600), 2)
                print( "OK - Logical standby is keeping up - %s Seconds" % queryResult )
                sys.exit(0)
        elif queryResult < CLIMIT:
                queryResult = round(float(queryResult * 60), 2)
                print( "WARNING - Logical standby is falling behind - %s Minutes" % queryResult )
                sys.exit(1)
        elif queryResult >= CLIMIT:
                queryResult = round(float(queryResult), 2)
                print( "CRITICAL - Logical standby is very behind - %s Hours" % queryResult )
                sys.exit(2)
        else:
                print( "UNKNOWN - Logical standby not making sense" )
                sys.exit(3)


if __name__ == "__main__":
        try:
                main()
        except Exception as e:
                print("Problem with script...")
                print(e)
                sys.exit(2)