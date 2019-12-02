using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ArtifactsProcessor
{
    class Program
    {
        private string mDirectory;
        private Random mRandom;
        private int mNumRaters;
        private int mIdealSample;
        private int[] mSampleNumbers;
        Program(int numRaters, int idealSample) {
            mDirectory = Directory.GetCurrentDirectory();
            mNumRaters = numRaters;
            mIdealSample = idealSample;
            mRandom = new Random();
            mSampleNumbers = getSampleNumbers();
        }
        Program(int numRaters, int idealSample, string directory) {
            mDirectory = directory;
            mNumRaters = numRaters;
            mIdealSample = idealSample;
            mRandom = new Random();
            mSampleNumbers = getSampleNumbers();
        }
        private int[] getSampleNumbers()
        {
            int[] sampleNumbers = new int[mIdealSample];
            var directoryInfo = new DirectoryInfo(mDirectory + "\\Student Artifacts");
            int[] artifactNums = countFilesInEachFolder(directoryInfo);
            ArrayList sampleArray = dynamicSample(mIdealSample, artifactNums);
            int i = 0;
            foreach (ArrayList array in sampleArray)
            {
                foreach (int value in array) {
                    sampleNumbers[i] = value;
                    i++;
                }
            }
            Array.Sort(sampleNumbers);
            return sampleNumbers;
        }
        private ArrayList sample(int min, int max, double small, double medium, double large)
        {
            int total = max - min + 1;
            int amount = 0;
            if (total < 10) {
                amount = total;
            }
            else if (total >= 10 && total <= 20) {
                amount = (int)(total * small);
            }
            else if (total > 20 && total <= 45 ) {
                amount = (int)(total * medium);
            }
            else {
                amount = (int)(total * large);
            }
            ArrayList sampleNumbers = new ArrayList();
            int i = 0;
            while (i < amount) { 
                int r = mRandom.Next(min, max+1);
                if (!sampleNumbers.Contains(r)) {
                    sampleNumbers.Add(r);
                    i += 1;
                }
            }
            return sampleNumbers;
        }
        private ArrayList dynamicSample(int idealSampled, int[] numArtifacts) {
            int totalArtifactsSampled = 0;
            ArrayList sampledArtifacts = new ArrayList();
            double small = .4;
            double medium = .2;
            double large = .1;
            int iteration = 0;
            while (totalArtifactsSampled != idealSampled) {
                totalArtifactsSampled = 0;
                sampledArtifacts.Clear();
                int counter = 0;
                foreach (int value in numArtifacts) {
                    ArrayList artifactsSampled = sample(counter+1, counter+value, small, medium, large);
                    totalArtifactsSampled += artifactsSampled.Count;
                    sampledArtifacts.Add(artifactsSampled);
                    counter += value;
                }
                if (totalArtifactsSampled != idealSampled) { 
                    bool overlap = totalArtifactsSampled > idealSampled;
                    if (!overlap) {
                        if (iteration % 3 == 0)
                            large += .01;
                        if (iteration % 3 == 1)
                            medium += .01;
                        if (iteration % 3 == 2)
                            small += .01;
                        if (large > 1)
                            large = 1;
                        if (medium > 1)
                            medium = 1;
                        if (small > 1)
                            small = 1;
                    }
                    else {
                        medium -= .01;
                        if (iteration % 4 == 0)
                            small -= .01;
                        if (iteration % 4 == 1)
                            large -= .01;
                        if (small < .1)
                            small = .1;
                        if (medium < .1)
                            medium = .1;
                        if (large < .1)
                            large = .1;
                    }
                }
                iteration += 1;
            }
        return sampledArtifacts; //returns an ArrayList of ArrayLists
        }
        private int[,,] createDelegationMatrix(int numArtifacts)
        {
            int numSections = numArtifacts / mNumRaters;
            int[,,] matrix = new int[numSections, mNumRaters, mNumRaters];
            for (int i = 0; i < numSections; i++) {
                for (int j = 0; j < mNumRaters; j++) {
                    for (int k = 0; k < mNumRaters; k++) {
                        matrix[i, j, k] = 0;
                    }
                }
            }
            for (int i = 0; i < numSections; i++) {
                int startIndex = i % 4 + 1;
                for (int j = 0; j < mNumRaters; j++) {
                    matrix[i,j,j] = 1;                                              // diagnal X's
                    matrix[i,j,(j + startIndex) % mNumRaters] = 1;                  // the rest of the pattern
                    }
                }
            return matrix;
        }
        private string formatArtifactNumber(int sn)
        {
            string studentNumber = sn.ToString();
            while (studentNumber.Length < 3)
            {
                studentNumber = "0" + studentNumber;
            }
            return studentNumber;
        }
        private void renameArtifact(string currentPath, string newPath)
        {
            System.IO.File.Move(currentPath, newPath);
        }
        private void sampleArtifact(FileInfo file)
        {
            string newPath = mDirectory + "\\Random Sampled Artifacts\\" + file.Name;
            System.IO.File.Copy(file.FullName, newPath);
        }
        private void delegateArtifacts()
        {
            string sampledDirectory = mDirectory + "\\Random Sampled Artifacts";
            FileInfo[] artrifacts = new DirectoryInfo(sampledDirectory).GetFiles();
            int[,,] delegationMatrix = createDelegationMatrix(artrifacts.Length);
            documentMatrix(delegationMatrix);                                       // writes the layout of the generation matrix to a file
            for (int rater = 0; rater < mNumRaters; rater++)
            {
                for (int section = 0; section < delegationMatrix.GetLength(0); section++)
                {
                    for (int row = 0; row < delegationMatrix.GetLength(1); row++)
                    {
                        int matrixValue = delegationMatrix[section, row, rater];
                        if ( matrixValue == 1)
                        {
                            string currentName = artrifacts[section * delegationMatrix.GetLength(1) + row].Name;
                            string currentPath = artrifacts[section * delegationMatrix.GetLength(1) + row].FullName;
                            string newPath = mDirectory + "\\Rater " + (rater+1).ToString() + "\\" + currentName;
                            System.IO.File.Copy(currentPath, newPath);
                        }
                    }
                }
            }

        }
        private void printMatrix(int[,,] matrix)
        {
            for (int section = 0; section < matrix.GetLength(0); section++)
            {
                for (int row = 0; row < matrix.GetLength(1); row++)
                {
                    string rowString = "";
                    for (int value = 0; value < matrix.GetLength(2); value++)
                    {
                        rowString += matrix[section,row,value].ToString() + ", ";
                    }
                    Console.WriteLine(rowString);
                }
            }
        }
        private void documentDelegation(Dictionary<int, ArrayList> raterDelegations)
        {
            string delegationArchiveDirectory = mDirectory + "\\Documentation\\raterArtifactDelegation.csv";
            foreach (KeyValuePair<int, ArrayList> entry in raterDelegations)
            {
                string rowString = 'R' + entry.Key.ToString() + ", ";
                foreach (int value in entry.Value)
                {
                    rowString += ( "Artifact " + value.ToString() + ", ");
                }
                File.AppendAllText(delegationArchiveDirectory, rowString + Environment.NewLine);
            }
        }
        private void documentMatrix(int[,,] matrix)
        {
            string matrixArchiveDirectory = mDirectory + "\\Documentation\\raterDistributionMatrix.csv";
            string headerString = "ArtifactID, ";
            Dictionary<int, ArrayList> raterDelegations = new Dictionary<int, ArrayList>();
            for (int i = 1; i <= mNumRaters; i++)
            {
                string raterTag = 'R' + i.ToString();
                raterDelegations.Add(i, new ArrayList());
                headerString += (raterTag + ", ");
            }
            File.AppendAllText(matrixArchiveDirectory, headerString + Environment.NewLine);
            for (int section = 0; section < matrix.GetLength(0); section++)
            {
                for (int row = 0; row < matrix.GetLength(1); row++)
                {
                    int artifactNumber = mSampleNumbers[matrix.GetLength(1) * section + row];
                    string rowString = "Artifact " + artifactNumber.ToString() + ", ";
                    for (int value = 0; value < matrix.GetLength(2); value++)
                    {
                        int matrixValue = matrix[section, row, value];
                        rowString += matrix[section,row,value].ToString() + ", ";
                        if (matrixValue == 1) {
                            raterDelegations[value+1].Add(artifactNumber);
                        }
                    }

                    File.AppendAllText(matrixArchiveDirectory, rowString + Environment.NewLine);
                }
            }
            documentDelegation(raterDelegations); 
        }
        private void renameArtifacts()
        {
            var directoryInfo = new DirectoryInfo(mDirectory + "\\Student Artifacts");
            var dirs = directoryInfo.EnumerateDirectories();
            int fileNumber = 1;
            string namesArchiveDirectory = mDirectory + "\\Documentation\\artifactsNamesArchive.csv";
            File.AppendAllText(namesArchiveDirectory, "Original Name" + ',' + "New Name" + Environment.NewLine);
            foreach (var di in dirs)
            {
                string directoryName = di.Name;
                FileInfo[] files = di.GetFiles();
                foreach (var file in files)
                {
                    string currentPath = file.FullName;
                    string newFileName = directoryName + "_" + formatArtifactNumber(fileNumber) + file.Extension;
                    string newPath = file.DirectoryName + "\\" + newFileName;
                    File.AppendAllText(namesArchiveDirectory, file.Name.Replace(',', ' ') + ',' + newFileName + Environment.NewLine);
                    renameArtifact(currentPath, newPath);
                    fileNumber++;
                }
            }
        }
        private int[] countFilesInEachFolder(DirectoryInfo directory)
        {
            var dirs = directory.EnumerateDirectories();
            int totalFolders = 0;
            foreach (var di in dirs)
            {
                totalFolders++;
            }
            int[] artifactNums = new int[totalFolders];
            int i = 0;
            foreach (var di in dirs)
            {
                artifactNums[i] = di.GetFiles().Length;
                i++;
            }
            return artifactNums;
        }
        private bool isSampled(int fileNumber)
        {
            bool isSampled = false;
            for (int i = 0; i < mSampleNumbers.Length; i++)
            {
                if (mSampleNumbers[i] == fileNumber)
                {
                    isSampled = true;
                }
            }
            return isSampled;
        }
        private void sampleArtifacts()
        {
            var directoryInfo = new DirectoryInfo(mDirectory + "\\Student Artifacts");
            var dirs = directoryInfo.EnumerateDirectories();
            int fileNumber = 1;
            foreach (var di in dirs)
            {
                string directoryName = di.Name;
                FileInfo[] files = di.GetFiles();
                foreach (var file in files)
                {
                    if (isSampled(fileNumber)) {
                        sampleArtifact(file);
                    }
                    fileNumber++;
                }
            }
        }
        public void processArtifacts()
        {
            renameArtifacts();
            sampleArtifacts();
            delegateArtifacts();
        }
        public void createStructure()
        {
            Directory.CreateDirectory(mDirectory + "//Random Sampled Artifacts");
            Directory.CreateDirectory(mDirectory + "//Documentation");
            for (int i = 1; i <= mNumRaters; i++)
            {
                Directory.CreateDirectory(mDirectory + "//Rater " + i.ToString());
            }
        }

        static void Main(string[] args)
        {
            int numRaters = 1;
            int numArtifactsPerRater = 1;
            while (numRaters % 2 == 1 && numArtifactsPerRater % 2 == 1)
            {
                Console.Write("Please enter the number of raters for this assessment: ");
                while (!Int32.TryParse(Console.ReadLine(), out numRaters))
                {
                    Console.WriteLine("Not a valid number.");
                    Console.Write("Please enter the number of raters for this assessment: ");
                }
                Console.Write("Please enter the number of artifacts per rater for this assessment: ");
                while (!Int32.TryParse(Console.ReadLine(), out numArtifactsPerRater))
                {
                    Console.WriteLine("Not a valid number.");
                    Console.Write("Please enter the number of artifacts per rater for this assessment: ");
                }
                if (numRaters % 2 == 1 && numArtifactsPerRater % 2 == 1)
                {
                    Console.WriteLine("The number of raters and number of artifacts chosen can not both be odd numbers, please choose again.");
                }
            }
            int totalNumArtifacts = (numArtifactsPerRater * numRaters) / 2;

            //string directory = @"C:\Users\d00295458\Desktop\GEAssessment\Student Artifacts";
            //Program program = new Program(numRaters, totalNumArtifacts, directory);
            Program program = new Program(numRaters, totalNumArtifacts);
            program.createStructure();
            program.processArtifacts();
        }
    }
}