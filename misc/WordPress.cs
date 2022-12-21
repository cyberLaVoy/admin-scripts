using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordPress
{
    class Program
    {
        void createPage()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            string post_content = "<!-- noformat on --><script type='text/javascript'> function resizeIframe(iFrame) { iFrame.width = iFrame.contentWindow.document.body.scrollWidth; iFrame.height = iFrame.contentWindow.document.body.scrollHeight; iFrame.style.border = 'none'; } </script><!-- noformat off --> <iframe src='https://domain/policydocuments/101.html' title='Policy 101' width='100%' onload='resizeIframe(this);'> <p>Your browser does not support iframes.</p> </iframe>";
                      
            startInfo.Arguments = "/C wp post create --post_content=\"" + post_content + "\" --post_type=page --post_title=\"Policy 101\" --post_parent=\"3089\"";
            //startInfo.Arguments = "/C wp post create --post_content=\"Hello world!\" --post_type=page --post_title=\"Policy 101\" --post_parent=\"3089\"";

            process.StartInfo = startInfo;
            process.Start();
        }
        static void Main(string[] args)
        {
            Program program = new Program();
            program.createPage();
        }
    }
}
