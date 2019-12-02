dbSizeLeeway = 7700
numMailBoxesLeeway = 1
numDBs = 96

class Node:
    def __init__(self, item, prev, nxt):
        self.mItem = item
        self.mPrev = prev
        self.mNext = nxt
    def getNext(self):
        return self.mNext
    def getPrev(self):
        return self.mPrev
    def setNext(self, item):
        self.mNext = item
    def setPrev(self, item):
        self.mPrev = item
    def getItem(self):
        return self.mItem

class LinkedList:
    def __init__(self):
        self.mFirst = None
        self.mLast = None
        self.mSize = 0
    def getSize(self):
        return self.mSize
    def isEmpty(self):
        return self.mSize == 0
    def front(self):
        return self.mFirst
    def back(self):
        return self.mLast
    def popFront(self):
        if self.isEmpty():
            return None
        if self.mFirst.getItem()[0] == self.mLast.getItem()[0]:
            self.mLast = None
        front = self.mFirst
        self.mFirst = self.mFirst.getNext()
        self.mSize -= 1
        return front
    def popBack(self):
        if self.isEmpty():
            return None
        if self.mFirst.getItem()[0] == self.mLast.getItem()[0]:
            self.mFirst = None
        back = self.mLast
        self.mLast = self.mLast.getPrev()
        if self.mLast is not None:
            self.mLast.setNext(None)
        self.mSize -= 1
        return back
    def push(self, item):
        if self.mSize == 0:
            node = Node(item, None, None)
            self.mFirst = node
            self.mLast = node
        else:
            node = Node(item, self.mLast, None)
            self.mLast.setNext(node)
            self.mLast = node
        self.mSize += 1
    def remove(self, dummyItem):
        if dummyItem.getItem()[0] == self.mFirst.getItem()[0]:
            self.popFront()
            return
        if dummyItem.getItem()[0] == self.mLast.getItem()[0]:
            self.popBack()
            return
        current = self.mFirst.getNext()
        while current is not None:
            if current.getItem()[0] == dummyItem.getItem()[0]:
                prev = current.getPrev()
                nxt = current.getNext()
                prev.setNext(nxt)
                nxt.setPrev(prev)
                self.mSize -= 1
                break
            current = current.getNext()
            if current.getItem()[0] == self.mLast.getItem()[0]:
                break


def delegate(mailBoxes, maxDBSize, maxPeoplePerDB, importantPeople): # mailBoxes should be a linked-list of key, value pairs
    dbs = []
    while not mailBoxes.isEmpty():
        db = []
        total = 0
        numPeople = 0

        if (len(importantPeople) != 0):
            importantMailBox = importantPeople.pop()
            total += importantMailBox.getItem()[1]
            numPeople += 1
            db.append(importantMailBox)

        mailBox = mailBoxes.front()
        while True:
            if mailBoxes.isEmpty():
                break
            if mailBox is None or len(db) >= 5:
                break
            total += mailBox.getItem()[1]
            numPeople += 1
            if (total > maxDBSize or numPeople > maxPeoplePerDB) and (numPeople >= 2):
                total -= mailBox.getItem()[1]
                numPeople -= 1
            else:
                db.append( mailBox )
                mailBoxes.remove( mailBox )
            mailBox = mailBox.getNext()

        while numPeople < maxPeoplePerDB:
            if mailBoxes.isEmpty():
                break
            back = mailBoxes.back()
            if back is None:
                break
            total += back.getItem()[1]
            if total > maxDBSize + dbSizeLeeway:
                total -= back.getItem()[1]
                break
            back = mailBoxes.popBack()
            db.append( back )
            numPeople += 1

        dbs.append(db)
    return dbs

def parseMailboxes(fileName, mailBoxes, delegatedUsers, importantPeopleList): # populates the linked list from file data
    fin = open(fileName, 'r')
    importantPeople = []
    totalData = 0
    fin.readline() # skip over header info
    for line in fin:
        line = line.strip()
        values = line.split(',')
        user = values[0]
        if (user not in delegatedUsers):
            guid = values[1]
            size = int(values[2])
            totalData += size
            mailBox = (guid, size, user)
            if (user in importantPeopleList):
                importantPeople.append(Node(mailBox,None,None))
            else:
                mailBoxes.push(mailBox)
    return totalData, mailBoxes.getSize(), importantPeople

def checkForDuplicateDisplayNames(fileName):
    users = []
    fin = open(fileName, 'r')
    for line in fin:
        line = line.strip()
        values = line.split(',')
        user = values[0]
        if user in users:
            print("Duplicate user found: " + user)
        else:
            users.append(user)

def loadDelegatedUsers(fileName):
    delegatedUsers = []
    fin = open(fileName, 'r')
    for line in fin:
        line = line.strip()
        rowData = line.split(',')
        displayName = rowData[0].replace('"', '')
        delegatedUsers.append(displayName)
    return delegatedUsers
def loadImportantPeople(fileName):
    importantPeople = []
    fin = open(fileName, 'r')
    for line in fin:
        line = line.strip()
        temp = line.split()
        temp.reverse()
        result = " ".join(temp)
        importantPeople.append(result)
    return importantPeople

def formatDbNum(dbNum):
    dbTag = str(dbNum)
    while len(dbTag) < 3:
        dbTag = "0" + dbTag
    dbTag = "db" + dbTag
    return dbTag

def writeDelegationScript(dbs, fileName):
    fin = open(fileName, 'w')
    dbNum = 1
    delagationCommands = []
    for db in dbs:
        delegationString = ""
        displayNameString = ""
        for item in db:
            guid = item.getItem()[0]
            user = item.getItem()[2]
            delegationString += ('"' + guid + '", ')
            displayNameString += ('"' + user + '", ')
        delegationString = delegationString[:-2] + " "
        displayNameString = displayNameString[:-2]
        delegationString += "| new-moverequest -targetdatabase " + formatDbNum(dbNum) + " -BadItemLimit unlimited"
        delegationString += "\n#" + displayNameString + "\n\n"
        delagationCommands.append(delegationString)
        dbNum += 1
    for cmd in delagationCommands:
        fin.write(cmd)
    fin.close()

def checkDbSpread(dbs):
    for db in dbs:
        print(len(db))
        total = 0
        for item in db:
            total += item.getItem()[1]
        print(total)
    print("# of DBs: ", len(dbs))

def checkForDuplicateGUIDs(dbs):
    guids = []
    for db in dbs:
        for item in db:
            guid = item.getItem()[0]
            if guid in guids:
                print(guid)
            else: 
                guids.append(item.getItem()[0])

def main():
    #checkForDuplicateDisplayNames("allMailboxes.csv")
    mailBoxes = LinkedList()
    delegatedUsers = loadDelegatedUsers("delegatedUsers.csv")
    importantPeopleList = loadImportantPeople("importantPeople.csv")
    totalSize, totalMailBoxes, importantPeople = parseMailboxes("allMailboxes.csv", mailBoxes, delegatedUsers, importantPeopleList)
    maxPeoplePerDB = totalMailBoxes / numDBs + numMailBoxesLeeway
    maxDBSize = totalSize / numDBs
    dbs = delegate(mailBoxes, maxDBSize, maxPeoplePerDB, importantPeople)

    #checkDbSpread(dbs)    
    #writeDelegationScript(dbs, "dbDelegationScripts.ps1")
    checkForDuplicateGUIDs(dbs)

main()