using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using System.IO;
using static QuestionProcessorFormApp.ConvertEquations;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Utilities;
using System.Runtime.InteropServices;

namespace QuestionProcessorFormApp
{
    public class Converter
    {
        const string cmd = "cmd.exe";
        const string cmdArgs = "/c dir ";
        static string pandocCMD = ConfigHelper.GetConfig("PandocTool", @"D:\WebSources\NET\upload_question_tool\Pandoc\pandoc.exe");

        static string docx2texCMD = ConfigHelper.GetConfig("Docx2TexTool", @"D:\WebSources\NET\upload_question_tool\docx2tex-1.6-release\docx2tex\d2t1.bat");

        static string image2MathMLCMD = ConfigHelper.GetConfig("Image2MathML", @"D:\WebSources\NET\upload_question_tool\ConvertImage2MathML\ConvertImage2MathML.exe");

        public static void Init(string pandoc, string docx2tex)
        {
            pandocCMD = pandoc;
            docx2texCMD = docx2tex;
        }

        public static string CMD(string cmd, string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo(cmd, args);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = new Process();
            p.StartInfo = psi;
            psi.UseShellExecute = false;
            p.Start();
            p.StandardInput.Close();
            p.WaitForExit(3000);
            string outputString;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(
                                                   p.StandardOutput.BaseStream))
            {
                outputString = sr.ReadToEnd();
            }

            if (p.HasExited == false)
                //Process is still running.
                //Test to see if the process is hung up.
                if (p.Responding)
                    //Process was responding; close the main window.
                    p.CloseMainWindow();
                else
                    //Process was not responding; force the process to close.
                    p.Kill();

            return outputString;
        }
        //static async Task Main()
        //{
        //    const string tempPath = "c:\\temp\\*.odt";
        //    var list = await ListDirectory(tempPath);
        //    Console.WriteLine(list.StandardOutput);
        //}

        public static async Task<BufferedCommandResult> Docx2TexPandoc(string folderPath, string fileName)
        {
            fileName = fileName.TrimStart(Path.DirectorySeparatorChar);
            string docx = folderPath + Path.DirectorySeparatorChar + fileName;
            string imagePath = folderPath;// docx.TrimEnd(".docx".ToCharArray());
            string tex = docx.Replace(".docx", ".tex");
            //const string pandocARGS = string.Format(" -s  c:\\temp\\index.html -o  c:\\temp\\output.odt");
            //string pandocARGS = string.Format(@" --extract-media={0} -r docx {1} -s -o  {2}", folderPath, docx, tex);//-s has header and footer
            string pandocARGS = string.Format(@" --extract-media={0} -r docx {1} -o {2}", imagePath, docx, tex);
            return await HtmlToODT(pandocCMD, pandocARGS);
            //    var args = cmdArgs;// + path;
            //    var cmdLine = Cli.Wrap(cmd).WithArguments(args);
            //    var result = await cmdLine.ExecuteBufferedAsync();
            //    return result;
        }
        public static async Task<BufferedCommandResult> Docx2TextPandoc(string folderPath, string fileName, string newTexFile)
        {
            fileName = fileName.TrimStart(Path.DirectorySeparatorChar);
            string docx = folderPath + Path.DirectorySeparatorChar + fileName;
            string text = newTexFile;
            string pandocARGS = string.Format(@" -r docx {0} -t plain -o {1}", docx, text);
            return await HtmlToODT(pandocCMD, pandocARGS);

        }
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
        public static string ExportWMFToMathML(string folderPath, string fileName)
        {
            try
            {
                ConvertEquation ce = new ConvertEquation();
                string file = folderPath + Path.DirectorySeparatorChar + fileName;
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
                            if (ce.Convert(ei, new EquationOutputFileText(imgPathConverted, "MathML3 (namespace attr).tdl")))
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
                            if (ce.Convert(ei, new EquationOutputFileText(imgPathConverted, "MathML3 (namespace attr).tdl")))
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
                            CLogger.WriteLogException("ce.Conver.emf-EquationInputFileWMF-Exception-" + eiEx.Message);
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
        public static async Task<BufferedCommandResult> Tex2MathMLPandoc(string folderPath, string fileName)
        {
            fileName = fileName.TrimStart(Path.DirectorySeparatorChar);
            string tex = folderPath + Path.DirectorySeparatorChar + fileName;
            string imagePath = tex.Replace(".tex", "_media");
            imagePath = imagePath.Replace(@"\", "/");
            string html = tex.Replace(".tex", ".html");

            //string pandocARGS = string.Format(@" --extract-media={0} --mathml {1} -s -o  {2}", tex, html);//has header and footer
            string pandocARGS = string.Format(@" --extract-media={0} --mathml {1} -o {2}", imagePath, tex, html);
            return await HtmlToODT(pandocCMD, pandocARGS);
            //var args = cmdArgs;
            //var cmdLine = Cli.Wrap(cmd).WithArguments(args);
            //var result = await cmdLine.ExecuteBufferedAsync();
            //return result;
        }
        public static async Task<BufferedCommandResult> Image2MathML(string folderPath, string fileName)
        {
            fileName = fileName.TrimStart(Path.DirectorySeparatorChar);
            string file = folderPath + Path.DirectorySeparatorChar + fileName;
            string image3MathMLARGS = string.Format(@" {0}", file);
            return await HtmlToODT(image2MathMLCMD, image3MathMLARGS);
        }

        static async Task<BufferedCommandResult> HtmlToODT(string pandocCMD, string pandocARGS)
        {
            //CMD(pandocCMD, pandocARGS);
            //return null;
            //var cts = new CancellationTokenSource();
            int timeOutConvertSecond = ConfigHelper.GetConfig("TimeOutConvertSecond", 1800);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(timeOutConvertSecond));
            var cmd = Cli.Wrap(pandocCMD).WithArguments(pandocARGS).WithValidation(CommandResultValidation.None);
            var result = await cmd.ExecuteBufferedAsync(cts.Token);
            return result;

        }

        public static async Task<BufferedCommandResult> Docx2Tex(string docx)
        {
            string doc2textARGS = string.Format(@" {0}", docx);
            await HtmlToODT(docx2texCMD, doc2textARGS);
            var args = cmdArgs;
            var cmdLine = Cli.Wrap(cmd).WithArguments(args);
            var result = await cmdLine.ExecuteBufferedAsync();
            return result;
        }

        #region wmf file
        [Flags]
        private enum EmfToWmfBitsFlags
        {
            EmfToWmfBitsFlagsDefault = 0x00000000,
            EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
            EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
            EmfToWmfBitsFlagsNoXORClip = 0x00000004
        }

        private static int MM_ISOTROPIC = 7;
        private static int MM_ANISOTROPIC = 8;

        [DllImport("gdiplus.dll")]
        private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize,
            byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SetMetaFileBitsEx(uint _bufferSize,
            byte[] _buffer);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CopyMetaFile(IntPtr hWmf,
            string filename);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteMetaFile(IntPtr hWmf);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteEnhMetaFile(IntPtr hEmf);

        public static MemoryStream MakeMetafileStream(System.Drawing.Bitmap image)
        {
            Metafile metafile = null;
            using (Graphics g = Graphics.FromImage(image))
            {
                IntPtr hDC = g.GetHdc();
                metafile = new Metafile(hDC, EmfType.EmfOnly);
                g.ReleaseHdc(hDC);
            }

            using (Graphics g = Graphics.FromImage(metafile))
            {
                g.DrawImage(image, 0, 0);
            }
            IntPtr _hEmf = metafile.GetHenhmetafile();
            uint _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC,
                EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
            byte[] _buffer = new byte[_bufferSize];
            GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC,
                    EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
            DeleteEnhMetaFile(_hEmf);

            var stream = new MemoryStream();
            stream.Write(_buffer, 0, (int)_bufferSize);
            stream.Seek(0, 0);

            return stream;
        }
        #endregion wmf file

    }
}
