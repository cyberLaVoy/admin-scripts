
def gatherAbbreviations(fileName):
    fin = open(fileName, 'r')
    fileLine = fin.readline().strip()
    abbreviations = fileLine.split(',')
    return abbreviations

def abbCheck(fileName, abbreviations):
    fin = open(fileName, 'r')
    fin.readline() # read the column names
    workStationNames = []
    for line in fin:
        dataRow = line.split(',')
        name = dataRow[0]
        workStationNames.append(name)
    fin.close()
    for name in workStationNames:
        if name[0:3] in abbreviations:
            print(name)
        if "vm" in name.lower():
            print(name)
        if "rename" in name.lower():
            print(name)
        if "change" in name.lower():
            print(name)

def knownCompare(currentFile, knownFile):
    knownAlive = []
    currentComputers = []
    fin = open(knownFile, 'r')
    header = fin.readline()
    for line in fin:
        knownAlive.append(line.split('\t')[5])
    fin.close()
    fin = open(currentFile, 'r')
    header = fin.readline()
    for line in fin:
        currentComputers.append(line.split(',')[0])
    fin.close()
   # for computer in currentComputers:
   #     if computer not in knownAlive:
   #         print(computer)
    for computer in knownAlive:
        if computer not in currentComputers:
            print(computer)

    

def main():
    #abbreviations = gatherAbbreviations("abbreviations.csv")
    #abbCheck("misnamedWorkstations.csv", abbreviations)
    knownCompare("misnamedWorkstations.csv", "knownAlive.csv")
main()