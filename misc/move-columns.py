import json

def printRow(year):
	print(year + ' ' + 'library-syllabi/' + docID + '.pdf')

with open('metadata.json') as data_file:
    metadata = json.load(data_file)

for i in range(len(metadata)):
    docID = metadata[i].keys()[0] 
    date_published = metadata[i].values()[0]['Date Published'] 
    if date_published == '':
        printRow('blank')
    elif '2008' in date_published:
        printRow('2008')
    elif '2009' in date_published:
        printRow('2009')
    elif '2010' in date_published:
        printRow('2010')
    elif '2011' in date_published:
        printRow('2011')
    elif '2012' in date_published: 
        printRow('2012')
    elif '2013' in date_published:
        printRow('2013')
    elif '2014' in date_published:
        printRow('2014')
    elif '2015' in date_published:
        printRow('2015')
    elif '2016' in date_published:
        printRow('2016')
    else:
	print('error')
