import requests
from bs4 import BeautifulSoup
import json

def docIDGrabber():
    docID_list = []
    fin = open('docIDs.txt', 'r')
    for line in fin:
        docID_list.append(line.strip())
    fin.close()
    return docID_list

def fileDictionaryMaker(docID):
    file_metadata = {}
    source_code = requests.get("http://site.ebrary.com/lib/dash/detail.action?docID=" + docID)
    soupObj = BeautifulSoup(source_code.text, "html.parser")
    bib_data = soupObj.find("div", id="bibliographic-data")

    bib_labels = bib_data.find_all("div", class_="bib-label")
    bib_fields = bib_data.find_all("div", class_="bib-field")

    for i in range(len(bib_labels)):
        label = bib_labels[i].get_text().strip()
        field = bib_fields[i].get_text().strip().replace('\n\t\t\t\t\t\t', ', ')
        file_metadata[label] = field
    return file_metadata

def processDocIDs(docID_list):
    i = len(docID_list)
    file_name = 'metadata.json'
    fout = open(file_name, 'a')
    for docID in docID_list:
        file_metadata = {}
        file_metadata[docID] = fileDictionaryMaker(docID) 
        fout.write(json.dumps(file_metadata) + '\n')
        print(str(i))
        i -= 1 
    fout.close()

def main():
    docID_list = docIDGrabber()
    processDocIDs(docID_list)

if __name__ == '__main__':
    main()
