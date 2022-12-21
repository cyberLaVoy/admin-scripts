import json
import os
"""
def runcp(current_location, destination):
	args = ['cp', current_location, destination]
	pid = os.fork()
	if pid > 0:
		os.waitpid(pid, 0)
	else:
		os.execvp('cp', args)


def cpSend(year, semester, docID):
	if year == '2008' or year == '2009' or year == '2010':
		file_location = '2008-2010/'
	else:
		file_location = year + '/'

	#current_location = file_location + docID + '.pdf' 
	current_location = 'library-syllabi/' + docID + '.pdf' 
	destination = file_location + semester
	#print(current_location + ' ' + destination)
	runcp(current_location, destination)
"""

with open('metadata.json') as data_file:
    metadata = json.load(data_file)

for j in range(len(metadata)):
	docID = metadata[j].keys()[0]
	try:
		lc_call_num = metadata[j].values()[0]['LC Call Number']
		year = lc_call_num[0:4]
		semester = lc_call_num[4:6]
		if "N/A" in year:
			print(docID)
    	except Exception as e:
		print(docID)



