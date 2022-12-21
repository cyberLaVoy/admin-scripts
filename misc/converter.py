import comtypes.client
import os

msword = comtypes.client.CreateObject("Word.Application")
for filename in os.listdir(os.getcwd()):
    if ".doc" in filename:
        fin = os.path.abspath(filename)
        fout = os.path.abspath(filename.split('.')[0]+".pdf")
        document = msword.Documents.Open(fin)
        document.SaveAs(fout, FileFormat=17)
        document.close()
msword.quit()
    
