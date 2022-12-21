import xml.etree.ElementTree as ET
import xmltodict
import json, sys

# grab file as command argument
filePath = sys.argv[1]

# parse xml
tree = ET.parse(filePath)
xmlData = tree.getroot()

# convert xml to string
xmlstr = ET.tostring(xmlData, encoding='utf8', method='xml')

# convert xml string to dictionary
dataDict = dict(xmltodict.parse(xmlstr))

# save dictionary as json
# and use same filename but with .json extension
fileName = filePath.split('.')[0]
with open(fileName + '.json', 'w+') as fout:
  json.dump(dataDict, fout, indent=4)
