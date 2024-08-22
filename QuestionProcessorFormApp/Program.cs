using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Utilities;

namespace QuestionProcessorFormApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string conString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string provider = ConfigurationManager.ConnectionStrings["DefaultConnection"].ProviderName;

            DBHelper.SQLHelper.Init(provider, conString);
            string pandoc = ConfigHelper.GetConfig("PandocTool", @"D:\WebSources\NET\upload_question_tool\Pandoc\pandoc.exe");
            string docx2tex = ConfigHelper.GetConfig("Docx2TexTool", @"D:\WebSources\NET\upload_question_tool\docx2tex-1.6-release\docx2tex\d2t1.bat");
            Converter.Init(pandoc, docx2tex);
            Application.Run(new frmMain());
        }
    }
}
