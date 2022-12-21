import os, re

def cleanFileName(fileName):
    # remove any character that isn't A-Z, a-z, or a period
    cleaned = re.sub("[^a-zA-Z.]", "", fileName)
    return cleaned

def renameFiles(dirPath, fileNames):
    for fileName in fileNames:
        cleanName = cleanFileName(fileName)
        src = os.path.join(dirPath, fileName)
        dst = os.path.join(dirPath, cleanName)
        # name change occurs here
        os.rename(src, dst)

def main():
    cwd = os.getcwd()
    # walk over all subdirectories
    for dirPath, _, fileNames in os.walk(cwd):
        renameFiles(dirPath, fileNames)
main()