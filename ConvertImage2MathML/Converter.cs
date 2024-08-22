using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static ConvertImage2MathML.ConvertEquations;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Utilities;
using System.Runtime.InteropServices;

namespace ConvertImage2MathML
{
    public class Converter
    {

        public static string RemoveTexComment(string file)
        {
            string tex = File.ReadAllText(file, Encoding.UTF8);
            string ptComment = @"\%\s.*\n";
            tex = Regex.Replace(tex, ptComment, "");
            File.WriteAllText(file, tex, Encoding.UTF8);
            return tex;
        }
        public static string RemoveMathMLComment(string file)
        {
            string mathml = File.ReadAllText(file, Encoding.UTF8);
            //string ptComment = @"\%\s.*\n";
            //mathml = Regex.Replace(mathml, ptComment, "");
            //File.WriteAllText(file, mathml, Encoding.UTF8);
            return mathml;
        }
        public static string ReFormatMathML(string file)
        {
            string tex = File.ReadAllText(file, Encoding.UTF8);
            string ptComment = @"<img ";
            tex = Regex.Replace(tex, ptComment, Environment.NewLine + @"<img ");
            File.WriteAllText(file, tex, Encoding.UTF8);
            return tex;
        }
        public static string ReFormatTex(string file)
        {
            string tex = File.ReadAllText(file, Encoding.UTF8);
            string ptComment = @"\\includegraphics{";
            tex = Regex.Replace(tex, ptComment, Environment.NewLine + @"\includegraphics{");
            File.WriteAllText(file, tex, Encoding.UTF8);
            return tex;
        }
        public static string ExportWMFToTex(string folderPath, string fileName)
        {
            ConvertEquation ce = new ConvertEquation();
            ce = new ConvertEquation();
            string file = folderPath + Path.DirectorySeparatorChar + fileName;
            ReFormatTex(file);
            string tex = File.ReadAllText(file);
            string pt = @"\\includegraphics{.*(\.wmf|\.emf)}";
            MatchCollection matchs = Regex.Matches(tex, pt);
            foreach (Match m in matchs)
            {
                string imageTex = m.Value;
                //string imgPath = folderPath + Path.DirectorySeparatorChar + imageTex.Replace("\\includegraphics{", "").Replace("}", "").Replace(" ", "");
                string imgPath = imageTex.Replace("\\includegraphics{", "").Replace("}", "").Replace(" ", "");
                string imgPathConverted = imgPath.Replace(".wmf", ".txt");
                if (ce.Convert(new EquationInputFileWMF(imgPath), new EquationOutputFileText(imgPathConverted, "Plain TeX.tdl")))
                {
                    string imageTexNew = RemoveTexComment(imgPathConverted);
                    tex = tex.Replace(imageTex, imageTexNew);
                }
                else
                {
                    string imageTexNew = WMF2Png(imgPath);
                    tex = tex.Replace(imageTex, "\\includegraphics{" + imageTexNew + "}");
                }
            }
            string newFile = file.Replace(".tex", "_f.tex");
            File.WriteAllText(newFile, tex);
            return tex;
        }
        public static string ExportWMFToMathML(string file)
        {
            try
            {
                ConvertEquation ce = new ConvertEquation();
                //string file = folderPath + Path.DirectorySeparatorChar + fileName;
                ReFormatMathML(file);
                string mathml = File.ReadAllText(file);
                string pt = @"<img.*\/>";
                MatchCollection matchs = Regex.Matches(mathml, pt);
                foreach (Match m in matchs)
                {
                    ce = new ConvertEquation();
                    string imageTex = m.Value;
                    //string imgPath = folderPath + Path.DirectorySeparatorChar + imageTex.Replace("\\includegraphics{", "").Replace("}", "").Replace(" ", "");
                    string imgPath = imageTex.Replace("<img src=\"", "").Replace("\" alt=\"image\" />", "").Replace(" ", "");
                    if (imgPath.Contains(".wmf"))
                    {
                        int indexOfWmf = imgPath.IndexOf(".");
                        string imgPathWmf = imgPath.Substring(0, indexOfWmf) + ".wmf";
                        string imgPathConverted = imgPathWmf.Replace(".wmf", ".txt");
                        EquationInputFileWMF ei = new EquationInputFileWMF(imgPathWmf);
                        try
                        {
                            bool rsConverted = false;
                            try
                            {
                                rsConverted = ce.Convert(ei, new EquationOutputFileText(imgPathConverted, "MathML3 (namespace attr).tdl"));
                            }
                            catch (Exception eConvertEMF2MathML)
                            {
                                CLogger.WriteLogException("ce.Conver.emf-EquationInputFileWMF-eConvertEMF2MathML-Exception-" + eConvertEMF2MathML.Message);
                                rsConverted = false;
                            }
                            if (rsConverted)
                            {
                                string imageTexNew = RemoveMathMLComment(imgPathConverted);
                                mathml = mathml.Replace(imageTex, imageTexNew);
                            }
                            else
                            {
                                string imageTexNew = WMF2Png(imgPath);
                                mathml = mathml.Replace(imageTex, "<img src=\"" + imageTexNew + "\" alt =\"image\" />");
                            }
                        }
                        catch (Exception eiEx)
                        {
                            CLogger.WriteLogException("ce.Conver.wmf-EquationInputFileWMF-Exception-" + eiEx.Message);
                        }
                        try
                        {
                            ei.Dispose();
                            ei = null;
                        }
                        catch { }
                    }
                    else if (imgPath.Contains(".emf"))
                    {
                        int indexOfEmf = imgPath.IndexOf(".");
                        string imgPathEmf = imgPath.Substring(0, indexOfEmf) + ".emf";
                        string imgPathConverted = imgPathEmf.Replace(".emf", ".txt");
                        //EquationInputFileWMF2 ei = new EquationInputFileWMF2(imgPathEmf);
                        EquationInputFileWMF ei = new EquationInputFileWMF(imgPathEmf);
                        try
                        {
                            bool rsConverted = false;
                            try
                            {
                                rsConverted = ce.Convert(ei, new EquationOutputFileText(imgPathConverted, "MathML3 (namespace attr).tdl"));
                            }
                            catch (Exception eConvertEMF2MathML){
                                CLogger.WriteLogException("ce.Conver.emf-EquationInputFileWMF-eConvertEMF2MathML-Exception-" + eConvertEMF2MathML.Message);
                                rsConverted = false;
                            }
                            if (rsConverted)
                            {
                                string imageTexNew = RemoveMathMLComment(imgPathConverted);
                                mathml = mathml.Replace(imageTex, imageTexNew);
                            }
                            else
                            {
                                string imageTexNew = Image2Png(imgPathEmf);
                                mathml = mathml.Replace(imgPathEmf, imageTexNew);
                            }
                        }
                        catch (Exception eiEx)
                        {
                            CLogger.WriteLogException("ce.Conver.emf-EquationInputFileWMF-eiEx-Exception-" + eiEx.Message);
                        }
                        try
                        {
                            ei.Dispose();
                            ei = null;
                        }
                        catch { }

                    }
                    GC.Collect();
                }
                mathml = CleanMathML(mathml);
                string newFile = file.Replace(".html", "_f.html");
                File.WriteAllText(newFile, mathml);
                return mathml;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("ExportWMFToMathML-Exception-" + ex.Message);
                throw ex;
            }
        }
        public static string CleanMathML(string mathml)
        {
            try
            {
                mathml = mathml.Replace(@"<strong>", "").Replace(@"</strong>", "");
                mathml = mathml.Replace(@"<!-- MathType@Translator@5@5@MathML3 (namespace attr).tdl@MathML 3.0 (namespace attr)@ -->", "").Replace(@"<!-- MathType@End@5@5@ -->", "");
                mathml = mathml.Replace(@"display='block'", "display='inline-block'");
                mathml = mathml.Replace(@"<p>", "").Replace("</p>", "");
                mathml = Regex.Replace(mathml, @"\s+", " ");
                mathml = Regex.Replace(mathml, "<annotation(.|\n)*?</annotation>", string.Empty);
                mathml = Regex.Replace(mathml, "[\r\n]+", Environment.NewLine);

                return mathml;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("CleanMathML-Exception-" + ex.Message);
                throw ex;
            }
        }
        public static string WMF2Png(string fileName)
        {
            try
            {
                string newFileName = fileName.Replace(".wmf", ".png");
                using (Bitmap image = new Bitmap(fileName))
                {
                    image.Save(newFileName, ImageFormat.Png);
                    image.Dispose();
                }
                return newFileName;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("WMF2Png-Exception-" + ex.Message);
                throw ex;
            }
        }
        public static string Image2Png(string fileName)
        {
            try
            {
                int index = fileName.LastIndexOf(".");
                string newFileName = fileName.Substring(0, index) + ".png";
                using (Bitmap image = new Bitmap(fileName))
                {
                    image.Save(newFileName, ImageFormat.Png);
                    image.Dispose();
                }
                return newFileName;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("Image2Png-Exception-" + ex.Message);
                throw ex;
            }
        }
    }
}
