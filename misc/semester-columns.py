import json
import os

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



with open('metadata.json') as data_file:
    metadata = json.load(data_file)

possible_years = ['2008', '2009', '2010', '2011', '2012', '2013', '2014', '2015', '2016']
for j in range(len(metadata)):
	docID = metadata[j].keys()[0]
	try:
		subject_headings = metadata[j].values()[0]['LC Subject Headings']
		semester_published = subject_headings.split(", ")
		for i in range(len(semester_published)):
			for k in range(len(possible_years)):
				year = possible_years[k]
				if ('Fall ' + year) in semester_published[i]:
					cpSend(year, ('fall' + year), docID)
				if ('Spring ' + year) in semester_published[i]:
					cpSend(year, ('spring' + year), docID)
				if ('Summer ' + year) in semester_published[i]:
					cpSend(year, ('summer' + year), docID)
    	except Exception as e:
		cpSend('blank', '', docID)



