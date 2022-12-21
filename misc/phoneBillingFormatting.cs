using System;
using System.Collections;
using System.IO;

namespace phoneBillingFormating
{
    class Program
    {
        private string mDirectory;
        private string mInputFilePath;
        private string mOutputFilePath;
        private ArrayList mPhoneBillingEntries;
        public Program(string directory)
        {
            mDirectory = directory;
            mInputFilePath = mostRecentFile();
            mOutputFilePath = directory + '\\' + "formatted\\formatted-telecom-data.csv";
            mPhoneBillingEntries = new ArrayList();
        }
        public void loadData()
        {
            System.IO.StreamReader updateFin = new System.IO.StreamReader(mInputFilePath);
            string line;
            updateFin.ReadLine(); //skip header row of file
            while ((line = updateFin.ReadLine()) != null)
            {
                if (line == "")
                {
                    continue;
                }
                ArrayList rowEntry = new ArrayList();
                string[] entry = line.Split(',');
                string[] parts = entry[0].Split('-');
                char[] trims = { '"', ' ' };
                string department = parts[0].Trim(trims).Replace("&amp;", "&").Replace("&quot;", "'").ToUpper();
                string indexCode = "";
                if (parts.Length > 1)
                {
                    indexCode = parts[1].Trim(trims);
                }
                string grandTotal = entry[entry.Length - 1].Trim(trims);
                rowEntry.Add(department);
                rowEntry.Add(indexCode);
                string accountCode = "710600";
                rowEntry.Add(accountCode);
                rowEntry.Add(grandTotal);
                mPhoneBillingEntries.Add(rowEntry); 
            }
        }
        public void writeFormatedData()
        {
            using (System.IO.StreamWriter fout = new System.IO.StreamWriter(mOutputFilePath))
            {
                fout.WriteLine("Department,IndexCode,AccountCode,GrandTotal");
                foreach (ArrayList row in mPhoneBillingEntries)
                {
                    string line = "";
                    foreach (string value in row)
                    {
                        line += (value + ",");
                    }
                    line = line.Substring(0, line.Length - 1);
                    fout.WriteLine(line);
                }
            }
        }

        public string mostRecentFile()
        {
            var fileList = new DirectoryInfo(mDirectory).GetFiles("*.csv");
            string latestFile = "";
            DateTime timeModified = DateTime.MinValue;
            foreach (FileInfo fileInfo in fileList)
            {
                if (fileInfo.LastWriteTime > timeModified)
                {
                    timeModified = fileInfo.LastWriteTime;
                    latestFile = fileInfo.Name;
                }
            }
            string filePath = mDirectory + '\\' + latestFile;
            return filePath;
        }

        static void Main(string[] args)
        {
            string directory = @"\\server.local\vxtracker"; // network drive directory
            Program program = new Program(directory);
            program.loadData();
            program.writeFormatedData();
        }
    }
}
