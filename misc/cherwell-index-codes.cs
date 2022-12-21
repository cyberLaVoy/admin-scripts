using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace importFormat
{
    class Program
    {
        private string mBasePath;
        private string mUpdatePath;
        private string mDuplicateLogFileName;
        private RecordSet mDataBase;
        Program(string basePath, string updatePath)
        {
            mBasePath = basePath;
            mUpdatePath = updatePath;
            mDuplicateLogFileName = "duplicateLog.txt";
            RecordSet recordSet = new RecordSet("default");
            mDataBase = recordSet;
        }
        private void createCSV(ArrayList entries, string type)
        {
            string path = mBasePath + "\\" + type + ".csv";
            using (System.IO.StreamWriter fout = new System.IO.StreamWriter(path)) {
                fout.WriteLine("Index,ID,Last,First,Type");
                foreach (string[] columns in entries)
                {
                    string line = "";
                    foreach (string value in columns)
                    {
                        line += (value + ",");
                    }
                    line = line.Substring(0, line.Length - 1);
                    fout.WriteLine(line);
                }
            }
        }
        private string createUpdateString(string[] row, bool deactivation)
        {
            string line = "";
            string Type = row[4];
            string Signer = row[2] + ", " + row[3];
            Signer = Signer.Replace("'", "''");
            string IndexCode = row[0];
            string ID = row[1];
            line += "UPDATE BudgetAdminSigners ";
            line += ("SET Type='" + Type + "', Signer='" + Signer + "'");
            if (deactivation)
            {
                line += (", Status='Inactive'");
            }
            line += (", LastModDateTime=CURRENT_TIMESTAMP");
            line += (" WHERE IndexCode='" + IndexCode + "' AND ID='" + ID + "';");
            return line;
        }
        private void createSQL(ArrayList updates, int deactivatesLocation)
        {
            string path = mBasePath + "\\Updates" + ".sql";
            using (System.IO.StreamWriter fout = new System.IO.StreamWriter(path))
            {
                for (int i = 0; i < updates.Count; i++)
                {
                    bool deactivation = (i == deactivatesLocation);
                    foreach (string[] row in (ArrayList)updates[i])
                    {
                        string line = createUpdateString(row, deactivation);
                        fout.WriteLine(line);
                    }
                }
            }
        }
        private void updateDataBase(ArrayList updates, int deactivatesLocation)
        {
            for (int i = 0; i < updates.Count; i++)
            {
                bool deactivation = (i == deactivatesLocation);
                foreach (string[] row in (ArrayList)updates[i])
                {
                    string line = createUpdateString(row, deactivation);
                    updateRecord(line);
                }
            }
        }
        void updateRecord(string query)
        {
            mDataBase.Connect();
            mDataBase.Open(query);
            mDataBase.Close();
            mDataBase.Disconnect();
        }
        private void checkEntries( ArrayList currentKeys, Dictionary<string, string[]> currentData, ArrayList updateKeys, Dictionary<string, string[]> updateData )
        {
            ArrayList deactivateEntries = new ArrayList();
            ArrayList updateEntries = new ArrayList();
            ArrayList addEntries = new ArrayList();
            ArrayList updates = new ArrayList();

            currentKeys.Sort();
            updateKeys.Sort();
            foreach (string updateKey in updateKeys)
            {
                int indexLocation = currentKeys.BinarySearch(updateKey);
                bool existsInCurrent = ( indexLocation >= 0);
                // add
                if (!existsInCurrent)
                {
                    addEntries.Add(updateData[updateKey]);
                }
                // update
                else
                {
                    string cFirstName = currentData[ (string)currentKeys[indexLocation] ][2];
                    string uFirstName = updateData[ updateKey ][2];
                    string cLastName = currentData[ (string)currentKeys[indexLocation] ][3];
                    string uLastName = updateData[ updateKey ][3];
                    string cType = currentData[(string)currentKeys[indexLocation]][4];
                    string uType = updateData[updateKey][4];
                    if ( cFirstName != uFirstName || cLastName != uLastName || cType != uType)
                    {
                        updateEntries.Add(updateData[updateKey]);
                    }
                }
            }
            foreach (string currentKey in currentKeys)
            {
                bool existsInUpdate = ( updateKeys.BinarySearch(currentKey) >= 0 );
                // removal
                if (!existsInUpdate)
                {
                    deactivateEntries.Add(currentData[currentKey]); 
                }
            }

            createCSV(addEntries, "Adds");
            updates.Add(updateEntries);
            updates.Add(deactivateEntries);
            createSQL(updates, 1);
            //updateDataBase(updates, 1);
        }
        private string createKey(string index, string id)
        {
            string key = index + id;
            return key;
        }
        public void loadFiles()
        {
            object[] currentObject = loadCurrent();
            object[] updateObject = loadUpdate();
            ArrayList currentKeys = (ArrayList) currentObject[0];
            ArrayList updateKeys = (ArrayList) updateObject[0];
            Dictionary<string, string[]> currentData = (Dictionary<string, string[]>) currentObject[1];
            Dictionary<string, string[]> updateData = (Dictionary<string, string[]>) updateObject[1];
            checkEntries(currentKeys, currentData, updateKeys, updateData);
        }
        public object[] loadUpdate()
        {
            System.IO.StreamReader updateFin = new System.IO.StreamReader(mUpdatePath);
            Dictionary<string, string[]> updateData = new Dictionary<string, string[]>();
            ArrayList updateKeys = new ArrayList();
            string line;
            updateFin.ReadLine(); //skip header row of file
            while ((line = updateFin.ReadLine()) != null)
            {
                string[] line_contents = line.Split(',');
                string key = createKey(line_contents[0], line_contents[1]);
                if (key != "")
                { 
                    if (updateData.ContainsKey(key))
                    {
                        string message = "Duplicate entries in UPDATE file with IndexCode:" + line_contents[0] + " and ID:" + line_contents[1];
                        logMessage(mDuplicateLogFileName, message);
                    }
                    updateKeys.Add(key);
                    string[] entry = { line_contents[0], line_contents[1], line_contents[2], line_contents[3], line_contents[4] };
                    updateData[key] = entry;
                }
            }
            object[] updateObject = { updateKeys, updateData };
            return updateObject;
        }
        
        public object[] loadCurrent()
        {
            Dictionary<string, string[]> currentData = new Dictionary<string, string[]>();
            ArrayList currentKeys = new ArrayList();
            mDataBase.Connect();
            string query = "SELECT IndexCode, ID, Signer, Type FROM BudgetAdminSigners WHERE status != 'Inactive';";
            mDataBase.Open(query);
            while (!mDataBase.IsEof())
            {
                string indexCode = mDataBase.GetString("IndexCode");
                string ID = mDataBase.GetString("ID");
                string[] signer = mDataBase.GetString("Signer").Split(',');
                string firstName = signer[1].Trim();
                string lastName = signer[0].Trim();
                string type = mDataBase.GetString("Type");

                string key = createKey(indexCode, ID);
                if (key != "")
                {
                    if (currentData.ContainsKey(key))
                    {
                        string message = "Duplicate entries in CURRENT database with IndexCode:" + indexCode + " and ID:" + ID;
                        logMessage("duplicateLog.txt", message);
                    }
                    string[] entry = { indexCode, ID, lastName, firstName, type };
                    currentData[key] = entry;
                    currentKeys.Add(key);
                }

                mDataBase.MoveNext();
            }
            mDataBase.Close();
            mDataBase.Disconnect();
            object[] currentObject = { currentKeys, currentData };
            return currentObject;
        }
        public void logMessage(string fileName, string message)
        {
            string path = mBasePath + "\\" + fileName;
            File.AppendAllText(path, message + Environment.NewLine);
        }
        public void clearFile(string fileName)
        {
            string path = mBasePath + "\\" + fileName;
            File.WriteAllText(path, string.Empty);
        }
        public void run()
        {
            clearFile(mDuplicateLogFileName);
            loadFiles();
        }
        
        static void Main(string[] args)
        {
            Program indexImport;
            if (args.Length == 2)
            {
                indexImport = new Program(args[0], args[1]);
            }
            else
            {
                string basePath = @"C:\CherwellIndexCodes\Updates";
                string updatePath = @"C:\CherwellIndexCodes\EmailDrop";
                var fileList = new DirectoryInfo(updatePath).GetFiles("*.csv");
                string latestFile = "";
                DateTime timeCreated = DateTime.MinValue;
                foreach (FileInfo fileInfo in fileList)
                {
                    if (fileInfo.CreationTime > timeCreated)
                    {
                        timeCreated = fileInfo.CreationTime;
                        latestFile = fileInfo.Name;
                    }
                }
                updatePath += ('\\' + latestFile);
                indexImport = new Program(basePath, updatePath);
            }

            indexImport.run();
        }

    }
}