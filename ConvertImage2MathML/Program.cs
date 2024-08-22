using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertImage2MathML
{
    class Program
    {
        static void Main(string[] args)
        {
            string rs = "fail";
            if(args == null || args.Length <= 0)
            {
                Response(rs);
                return;
            }
            string fileName = args[0];
            if (string.IsNullOrEmpty(fileName))
            {
                Response(rs);
                return;
            }
            if (!File.Exists(fileName))
            {
                Response(rs);
                return;
            }
            try
            {
                string batchMathML = Converter.ExportWMFToMathML(fileName);
                rs = "succ";
                Response(rs);
                return;
            }
            catch
            {
                Response(rs);
                return;
            }
            

            Console.WriteLine(args[0]);
        }
        static void Response(string rs)
        {
            Console.WriteLine(rs);
        }
    }
}
