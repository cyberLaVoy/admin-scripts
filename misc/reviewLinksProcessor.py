import sys

def linkGenerator(inputStream):
    stringFormat = inputStream.readline()
    for line in inputStream:
        params = line.strip().split() 
        formatedString = stringFormat.replace("guestLink", params[1])
        print(params[0])
        print(formatedString)

def main():
    linkGenerator(sys.stdin)
main()
