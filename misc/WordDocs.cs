using Microsoft.Office.Interop.Word;
using System;
using System.Collections;
using System.IO;
using System.Linq;

//service account with propper permissions (read from SharePoint, write to folders in web servers)
//get directory list from SharePoint document library mapping

namespace wordDocuments
{
    class PolicyLibrary
    {
        private string[] mFileNames;
        private ArrayList mPolicies = new ArrayList();
        private string mDirectoryPath;
        public PolicyLibrary(string directoryPath)
        {
            mDirectoryPath = directoryPath;
            setFileNames();
            populateLibrary();
        }
        private void setFileNames()
        {
            mFileNames = Directory.GetFiles(mDirectoryPath, "*.docx").Select(Path.GetFileName).ToArray();
        }
        private void populateLibrary()
        {
            if (mPolicies.Count == 0)
            {
                foreach (string fileName in mFileNames)
                {
                    PolicyDocument policy = new PolicyDocument(fileName, mDirectoryPath);
                    mPolicies.Add(policy);
                }
            }
        }
        public void addPolicy(PolicyDocument policy)
        {
            mPolicies.Add(policy);
        } 
        public void convertToHtml(string destinationDirectory)
        {
            foreach (PolicyDocument policy in mPolicies)
            {
                policy.convertToHTML(destinationDirectory);
            }
        }
    }
    class PolicyDocument
    {
        private string mTitle;
        private string mExtention;
        private string mSourceDirectory;
        public PolicyDocument(string fileName, string sourceDirectory)
        {
            string[] parts = fileName.Split('.');
            mTitle = parts[0];
            mExtention = '.' + parts[1];
            mSourceDirectory = sourceDirectory;
        }
        public void convertToHTML(object destinationDirectory)
        {
            object missingType = Type.Missing;
            object readOnly = true;
            object isVisible = false;
            object documentFormat = 8;
            //object documentFormat = WdSaveFormat.wdFormatPDF;
            object sourcePath = mSourceDirectory + mTitle + mExtention;
            object destinationPath = destinationDirectory + mTitle + ".html";
            //Open the word document in background
            ApplicationClass applicationclass = new ApplicationClass();
            applicationclass.Documents.Open(ref sourcePath,
                                            ref readOnly,
                                            ref missingType, ref missingType, ref missingType,
                                            ref missingType, ref missingType, ref missingType,
                                            ref missingType, ref missingType, ref isVisible,
                                            ref missingType, ref missingType, ref missingType,
                                            ref missingType, ref missingType);
            applicationclass.Visible = false;
            Document document = applicationclass.ActiveDocument;
            //Save the word document as HTML file
            document.SaveAs(ref destinationPath, ref documentFormat, ref missingType,
                            ref missingType, ref missingType, ref missingType,
                            ref missingType, ref missingType, ref missingType,
                            ref missingType, ref missingType, ref missingType,
                            ref missingType, ref missingType, ref missingType,
                            ref missingType);
            //Close the word document
            document.Close(ref missingType, ref missingType, ref missingType);
        }
    }
    class WordPress
    {
        void createPage()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            string post_content = "<iframe src='https://domain/policydocuments/101.html' title='Policy 101' width='100%' onload='resizeIframe(this);'> <p>Your browser does not support iframes.</p> </iframe>";

            startInfo.Arguments = "/C wp post create --post_content=\"" + post_content + "\" --post_type=page --post_title=\"Policy 101\" --post_parent=\"3089\"";
            //startInfo.Arguments = "/C wp post create --post_content=\"Hello world!\" --post_type=page --post_title=\"Policy 101\" --post_parent=\"3089\"";

            process.StartInfo = startInfo;
            process.Start();
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            PolicyLibrary policyLibrary = new PolicyLibrary(@"Z:\");
            policyLibrary.convertToHtml(@"C:\TestLocation\");
        }
    }
}


//wp post list --post_type=page --format=json --fields=ID,post_name --post_parent=3089 > current-pages.json

//wp post create --post_content="<div id='link-list'></div><iframe id='policyIFrame' src='https://domain/policydocuments/101.html' onload='resizeIframe(this);' scrolling='no'><p>Your browser does not support iframes.</p></iframe>" --post_type=page --post_title="Policy #101" --post_name="policy101" --page_template="page-policylibrary.php" --post_parent=3089