using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Doc;
using System.IO;
using EARS.Entities;
using Utilities;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using System.Data;
using System.Text.RegularExpressions;
using Spire.Doc.Collections;
using Body = Spire.Doc.Body;
using Word = Microsoft.Office.Interop.Word;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualBasic.Devices;
using Spire.Doc.Fields.OMath;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Security.Cryptography;
using System.Net;
using System.Web.Script.Serialization;

namespace QuestionProcessorFormApp
{
    public class QuestionProcessorConfig
    {
        public string UploadFolder = ConfigHelper.GetConfig("QuestionFilePath", "");
        public string UploadURL = ConfigHelper.GetConfig("QuestionImageURL", "");
        public string FormQuestion = ConfigHelper.GetConfig("Question");
        public string OptionA = ConfigHelper.GetConfig("OptionA");
        public string OptionB = ConfigHelper.GetConfig("OptionB");
        public string OptionC = ConfigHelper.GetConfig("OptionC");
        public string OptionD = ConfigHelper.GetConfig("OptionD");
        public string OptionE = ConfigHelper.GetConfig("OptionE");
        public string OptionF = ConfigHelper.GetConfig("OptionF");
        public string OptionG = ConfigHelper.GetConfig("OptionG");
        public int ByPassCheckExplain = ConfigHelper.GetConfig("ByPassCheckExplain", 0);


        public QuestionProcessorConfig()
        {
            UploadFolder = ConfigHelper.GetConfig("QuestionFilePath", "");
            UploadURL = ConfigHelper.GetConfig("QuestionImageURL", "");
            FormQuestion = ConfigHelper.GetConfig("Question");

            OptionA = ConfigHelper.GetConfig("OptionA");
            OptionB = ConfigHelper.GetConfig("OptionB");
            OptionC = ConfigHelper.GetConfig("OptionC");
            OptionD = ConfigHelper.GetConfig("OptionD");
            OptionE = ConfigHelper.GetConfig("OptionE");
            OptionF = ConfigHelper.GetConfig("OptionF");
            OptionG = ConfigHelper.GetConfig("OptionD");

            ByPassCheckExplain = ConfigHelper.GetConfig("ByPassCheckExplain", 0);
        }
    }
    public class QuestionProcessor
    {
        public QuestionProcessorConfig Config = new QuestionProcessorConfig();
        public BaseRequest BaseRequest = null;
        public BaseResponse BaseResponse = null;
        public string FormTitle = string.Empty;
        public int TotalRecords = 0;

        public QuestionProcessor()
        {
            BaseResponse = new BaseResponse();
            BaseResponse.IsCancel = false;
        }
        protected int CopyFiles(string sourceFolder, string desFolder, ref string log)
        {
            int i = 1;
            string dirPath = sourceFolder;
            try
            {
                foreach (string file in Directory.GetFiles(dirPath))
                {
                    string destFile = desFolder + Path.GetFileName(file);
                    FileInfo fi = new FileInfo(file);
                    if (!File.Exists(destFile))
                    {
                        string log1 = CLogger.Append("Copy", file, destFile);
                        long start = DateTime.Now.Millisecond;
                        File.Copy(file, destFile);
                        long end = DateTime.Now.Millisecond;
                        log1 = CLogger.Append("Time", (end - start));
                        CLogger.WriteInfo(log1);
                        log = CLogger.Append(log, log1);
                    }
                    else
                    {
                        log = CLogger.Append(log, "file exists=", destFile);
                    }

                }
            }
            catch (Exception ex)
            {
                i = 0;
                log = CLogger.Append(log, "Exception", ex.Message);
            }
            return i;
        }

        #region Question Using Bo Giao Duc Template
        public void SplitQuestionDocument(string folder, string dateFolder, string batchId, string fileName)
        {
            string log = "SplitQuestionDocument(" + batchId + ")";
            try
            {
                Document doc = new Document();
                doc.LoadFromFile(fileName);
                int tempCountIndex = 0;
                TextSelection[] selectionsQuestion = doc.FindAllPattern(new Regex(@"^Câu\s\d+\."));
                Dictionary<int, int> questionsIndex = new Dictionary<int, int>();

                if (selectionsQuestion != null)
                {
                    for (int i = 0; i < selectionsQuestion.Length; i++)
                    {
                        TextRange newrange = new TextRange(doc);
                        newrange = selectionsQuestion[i].GetAsOneRange();
                        Paragraph paragraph = newrange.OwnerParagraph;
                        Body body = paragraph.OwnerTextBody;
                        int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                        if (paragraph.Text.StartsWith(Config.FormQuestion))
                        {
                            log = CLogger.Append(log, "Câu ", i, paragraph.Text);
                            questionsIndex.Add(tempCountIndex, body.ChildObjects.IndexOf(paragraph));
                            tempCountIndex++;
                        }
                    }
                }
                try
                {
                    for (int i = 0; i < questionsIndex.Count; i++)
                    {
                        //log = CLogger.Append(log, "Câu ", i);
                        try
                        {
                            Document docTemp = new Document();
                            Section TempsectionQuestion = docTemp.AddSection();

                            int paraIndex = 0;

                            int benginQues = questionsIndex[i];
                            int endQues = benginQues + 1;
                            if (questionsIndex.Count == i + 1) endQues = doc.Sections[0].Paragraphs.Count;
                            else endQues = questionsIndex[i + 1];

                            for (int j = benginQues; j < endQues; j++)
                            {
                                TempsectionQuestion.Paragraphs.Insert(paraIndex, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                                paraIndex++;
                            }

                            string docxFileName = folder + Path.DirectorySeparatorChar + dateFolder + Path.DirectorySeparatorChar + batchId + Path.DirectorySeparatorChar + batchId + "_" + i + ".docx";

                            docTemp.SaveToFile(docxFileName, FileFormat.Docx);
                            docTemp.Close();
                            docTemp.Dispose();
                            docTemp = null;
                            GC.Collect();
                        }
                        catch (Exception splitFileErr)
                        {
                            log = CLogger.Append(log, "Exception splitFileErr ", splitFileErr.Message);
                        }
                    }

                }
                catch (Exception ex)
                {
                    log = CLogger.Append(log, "Exception ", ex.Message);
                    throw new Exception(ex.Message);
                }

                doc.Close();
                doc.Dispose();
                doc = null;
                GC.Collect();

                CLogger.WriteLogSuccess(log);
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException(log);
                throw new Exception(ex.Message);
            }
        }
        public List<QuestionExt> BatchProcess_Multiple_Choice_HTML(QuestionBatch quesionBatch, string docxFile, string isAnswer, Dictionary<int, string> ar, int typeSchool, List<int> listQuesError, string fileNameOrg, ref List<int> listQuestionMathTypeError, ref string log, ref SessionInfo SessionInfo)
        {
            long endProcess = 0;
            long startProcess = DateHelper.CurrentMilisecond();
            /**
             * Lấy toàn bộ những lô chưa được xử lý, Cập nhật lại trạng thái là đang xử lý
             * Tạo mới câu hỏi với nội dung từ lô đã chọn, trả về ID để làm folder
             * Lấy thông tin từ câu hỏi trong word để update về DB
             * Next đến câu hỏi kế tiếp
             */
            int countQuestion = 0;
            string randomQuestion = new Random().Next(000001, 999999).ToString();
            List<QuestionExt> qs = new List<QuestionExt>();
            Dictionary<int, int> questionsIndex = new Dictionary<int, int>();
            Dictionary<int, string> listQuestions = new Dictionary<int, string>();
            try
            {
                string file = docxFile;
                Document doc = new Document();
                doc.LoadFromFile(file);

                string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                string formQuestionReplace = ConfigHelper.GetConfig("Question_Replace");
                TextSelection[] selectionsQuestion = doc.FindAllString(formQuestionReplace, true, true);

                int questionIndex = 0;
                for (int i = 0; i < selectionsQuestion.Length; i++)
                {
                    TextRange newrange = new TextRange(doc);
                    newrange = selectionsQuestion[i].GetAsOneRange();
                    Paragraph paragraph = newrange.OwnerParagraph;
                    Body body = paragraph.OwnerTextBody;
                    int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                    if (paragraph.Text.StartsWith(formQuestionReplace) && paragraph.Text.Length > 8)
                    {
                        questionsIndex.Add(questionIndex, body.ChildObjects.IndexOf(paragraph));
                        questionIndex++;
                    }
                }

                questionIndex = 0;

                for (int i = 0; i < questionsIndex.Count; i++)
                {
                    Document docTemp = new Document();
                    Section sectionQuestion = docTemp.AddSection();
                    Section sectionA = docTemp.AddSection();
                    Section sectionB = docTemp.AddSection();
                    Section sectionC = docTemp.AddSection();
                    Section sectionD = docTemp.AddSection();
                    Section sectionAnSwer = docTemp.AddSection();

                    Dictionary<string, string> jO = new Dictionary<string, string>();
                    string questionId = DateTime.Now.ToString("yyyymmdd") + randomQuestion + questionIndex.ToString() + new Random().Next(111111, 999999);
                    string folder = Config.UploadFolder + Path.DirectorySeparatorChar + dateFolder + Path.DirectorySeparatorChar + questionId;
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string imagePath = "";
                    int indexQues = 0;
                    int indexAnSwer = 0;
                    int optionA_Index = 0;

                    listQuestions.Add(i, questionId);

                    int benginQues = questionsIndex[i];
                    int endQues = benginQues + 1;
                    if (questionsIndex.Count == i + 1) endQues = doc.Sections[0].Paragraphs.Count;
                    else endQues = questionsIndex[i + 1];

                    if (String.IsNullOrWhiteSpace(ar[i]))
                    {
                        if (SessionInfo.IsSkipError == true) continue;
                    }
                    if (listQuesError.Contains(benginQues))
                    {
                        if (SessionInfo.IsSkipError == true) continue;
                    }

                    for (int j = benginQues; j < endQues; j++)
                    {
                        bool isPicture = false;
                        if (isAnswer == "1")
                        {
                            if (optionA_Index == 4)
                            {
                                sectionAnSwer.Paragraphs.Insert(indexAnSwer, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                                //sectionAnSwer.Paragraphs[indexAnSwer].AppendText(br);
                                indexAnSwer++;
                            }
                        }
                        if (Regex.IsMatch(doc.Sections[0].Paragraphs[j].Text, @"^A\."))
                        {
                            optionA_Index = 1;
                            sectionA.Paragraphs.Insert(0, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                            //sectionA.Paragraphs[0].AppendText(br);
                        }
                        if ((Regex.IsMatch(doc.Sections[0].Paragraphs[j].Text, @"^B\.")) && (optionA_Index == 1))
                        {
                            optionA_Index = 2;
                            sectionB.Paragraphs.Insert(0, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                            //sectionB.Paragraphs[0].AppendText(br);
                        }
                        if ((Regex.IsMatch(doc.Sections[0].Paragraphs[j].Text, @"^C\.")) && (optionA_Index == 2))
                        {
                            optionA_Index = 3;
                            sectionC.Paragraphs.Insert(0, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                            //sectionC.Paragraphs[0].AppendText(br);
                        }
                        if ((Regex.IsMatch(doc.Sections[0].Paragraphs[j].Text, @"^D\.")) && (optionA_Index == 3))
                        {
                            optionA_Index = 4;
                            sectionD.Paragraphs.Insert(0, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                            //sectionD.Paragraphs[0].AppendText(br);
                        }
                        if (optionA_Index == 0)
                        {
                            foreach (DocumentObject o1 in doc.Sections[0].Paragraphs[j].ChildObjects)
                            {
                                if (o1.DocumentObjectType == DocumentObjectType.Picture)
                                {
                                    isPicture = true;
                                }
                            }

                            if (!isPicture)
                            {
                                string paraText = doc.Sections[0].Paragraphs[j].Text.Trim();
                                doc.Sections[0].Paragraphs[j].Text = paraText;
                            }
                            sectionQuestion.Paragraphs.Insert(indexQues, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                            //sectionQuestion.Paragraphs[indexQues].AppendText(br);
                            indexQues++;
                        }
                    }

                    docTemp.Replace(formQuestionReplace, "", true, true);
                    docTemp.Replace(Config.OptionA, "", true, true);
                    docTemp.Replace(Config.OptionB, "", true, true);
                    docTemp.Replace(Config.OptionC, "", true, true);
                    docTemp.Replace(Config.OptionD, "", true, true);


                    QuestionExt q = new QuestionExt();
                    string newFileName = "";
                    string imageUrl = Config.UploadURL + "/" + dateFolder;
                    jO.Add("ar", ar[i]);
                    imagePath = SaveToHtml(sectionQuestion, folder, questionId, imageUrl, QuestionPart.content, out newFileName);
                    jO.Add("q", imagePath);

                    q.contentBase64ImageHtml = questionId + Path.DirectorySeparatorChar + newFileName + ".html";
                    q.contentDocx = questionId + Path.DirectorySeparatorChar + newFileName + ".docx";

                    imagePath = SaveToHtml(sectionA, folder, questionId, imageUrl, QuestionPart.a1, out newFileName);
                    jO.Add("a1", imagePath);
                    q.a1Base64ImageHtml = questionId + Path.DirectorySeparatorChar + newFileName + ".html";
                    q.a1Docx = questionId + Path.DirectorySeparatorChar + newFileName + ".docx";

                    imagePath = SaveToHtml(sectionB, folder, questionId, imageUrl, QuestionPart.a2, out newFileName);
                    jO.Add("a2", imagePath);
                    q.a2Base64ImageHtml = questionId + Path.DirectorySeparatorChar + newFileName + ".html";
                    q.a2Docx = questionId + Path.DirectorySeparatorChar + newFileName + ".docx";

                    imagePath = SaveToHtml(sectionC, folder, questionId, imageUrl, QuestionPart.a3, out newFileName);
                    jO.Add("a3", imagePath);
                    q.a3Base64ImageHtml = questionId + Path.DirectorySeparatorChar + newFileName + ".html";
                    q.a3Docx = questionId + Path.DirectorySeparatorChar + newFileName + ".docx";

                    imagePath = SaveToHtml(sectionD, folder, questionId, imageUrl, QuestionPart.a4, out newFileName);
                    jO.Add("a4", imagePath);
                    q.a4Base64ImageHtml = questionId + Path.DirectorySeparatorChar + newFileName + ".html";
                    q.a4Docx = questionId + Path.DirectorySeparatorChar + newFileName + ".docx";

                    imagePath = SaveToHtml(sectionAnSwer, folder, questionId, imageUrl, QuestionPart.aFull, out newFileName);
                    jO.Add("a", imagePath);
                    q.aFullBase64ImageHtml = questionId + Path.DirectorySeparatorChar + newFileName + ".html";
                    q.aFullDocx = questionId + Path.DirectorySeparatorChar + newFileName + ".docx";

                    docTemp.Close();
                    docTemp = null;
                    GC.Collect();


                    q.chapter_id = quesionBatch.chapter_id;
                    q.resource_id = quesionBatch.resource_id;
                    q.content = jO["q"];
                    q.short_explain = jO["a"];
                    q.full_explain = jO["a"];
                    q.type = quesionBatch.type;
                    q.level = quesionBatch.level;
                    q.question_file = questionId;
                    q.full_question_file = dateFolder;

                    q.course_id = quesionBatch.course_id;
                    q.subject_id = quesionBatch.subject_id;
                    q.a1 = jO["a1"];
                    q.a2 = jO["a2"];
                    q.a3 = jO["a3"];
                    q.a4 = jO["a4"];
                    q.ar = jO["ar"];
                    q.class_id = quesionBatch.class_id;
                    q.created_by = quesionBatch.created_by;
                    q.modified_by = questionId;
                    q.published = 0;
                    q.review_status = 0;
                    q.part_id = 0;
                    q.tool_id = 0;
                    q.school_id = 0;
                    q.lesson_id = quesionBatch.lession_id;

                    qs.Add(q);

                    countQuestion++;
                    questionIndex++;


                }

                doc.Close();
                doc = null;
                GC.Collect();

                string docFile = "";
                try
                {
                    docFile = SaveDocument_MC_convert(listQuestions, Config.UploadFolder + Path.DirectorySeparatorChar, dateFolder, fileNameOrg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + ">>>>" + string.Format("SaveDocument_convert(folder = {0});", Config.UploadFolder + Path.DirectorySeparatorChar));
                    CLogger.WriteInfo(string.Format("SaveDocument_convert(folder = {0});", Config.UploadFolder + Path.DirectorySeparatorChar));
                }
                try
                {
                    docFile = SaveDocument_MC_table(listQuestions, ar, Config.UploadFolder + Path.DirectorySeparatorChar, dateFolder, fileNameOrg, isAnswer);
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.Message + ">>>>" + string.Format("SaveDocument_table(folder = {0});", Config.UploadFolder + Path.DirectorySeparatorChar));
                    CLogger.WriteInfo(string.Format("SaveDocument_table(folder = {0});", Config.UploadFolder + Path.DirectorySeparatorChar));
                }


                endProcess = DateHelper.CurrentMilisecond();
                log = "Thành công > time: " + (endProcess - startProcess).ToString() + "ms";
                return qs;
            }
            catch (Exception exx_BatchProcess_New)
            {
                CLogger.WriteLogException(exx_BatchProcess_New.Message);
                endProcess = DateHelper.CurrentMilisecond();
                log = "Thất bại time: " + (endProcess - startProcess).ToString() + "ms";
                throw new Exception(exx_BatchProcess_New.Message);
            }
        }
        /// <summary>
        /// Lưu image trong nội dung từng câu hỏi
        /// </summary>
        /// <param name="sInput"></param>
        /// <param name="folder"></param>
        /// <param name="questionId"></param>
        /// <param name="url"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string SaveToImage_New(Spire.Doc.Section sInput, string folder, string questionId, string url, int type)
        {
            if (sInput == null) return string.Empty;
            try
            {
                string objectId = questionId + new Random().Next(111111, 999999);
                //image2017272434293937148497.jpeg hoặc image2017272434293937148497.png
                string fileName = "image" + objectId;
                string txt_file = folder + Path.DirectorySeparatorChar + fileName + ".txt";

                Document docTemp = new Document();
                docTemp.Sections.Insert(0, sInput.Clone());

                string txt_fileSVG = folder + Path.DirectorySeparatorChar + fileName + ".svg";
                docTemp.SaveToFile(txt_fileSVG, FileFormat.SVG);

                int count = 1;
                foreach (Section section in docTemp.Sections)
                {
                    foreach (Paragraph paragraph in section.Paragraphs)
                    {
                        List<DocumentObject> pictures = new List<DocumentObject>();
                        foreach (DocumentObject docObject in paragraph.ChildObjects)
                        {
                            if (docObject.DocumentObjectType == DocumentObjectType.Picture)
                            {
                                pictures.Add(docObject);
                            }
                        }
                        foreach (DocumentObject pic in pictures)
                        {
                            int index = paragraph.ChildObjects.IndexOf(pic);
                            TextRange range = new TextRange(docTemp);
                            range.Text = GenerateImageHTML(pic, folder, questionId, url);
                            paragraph.ChildObjects.Insert(index, range);
                            paragraph.ChildObjects.Remove(pic);
                            count++;
                        }
                    }
                }

                switch (type)
                {
                    case 0:
                        txt_file = folder + Path.DirectorySeparatorChar + "Cau_Hoi.txt";//câu hỏi
                        break;
                    case 1:
                        txt_file = folder + Path.DirectorySeparatorChar + "Dap_An_A.txt";//đáp án A
                        break;
                    case 2:
                        txt_file = folder + Path.DirectorySeparatorChar + "Dap_An_B.txt";//đáp án B
                        break;
                    case 3:
                        txt_file = folder + Path.DirectorySeparatorChar + "Dap_An_C.txt";//đáp án C
                        break;
                    case 4:
                        txt_file = folder + Path.DirectorySeparatorChar + "Dap_An_D.txt";//đáp án D
                        break;
                    case 5:
                        txt_file = folder + Path.DirectorySeparatorChar + "Loi_Giai.txt";//lời giải
                        break;
                }

                docTemp.SaveToFile(txt_file, FileFormat.Txt);

                docTemp.Close();
                docTemp = null;
                GC.Collect();
                string html = System.IO.File.ReadAllText(txt_file.Trim());

                try
                {
                    System.IO.File.Delete(txt_file);
                }
                catch (Exception e1)
                {
                    CLogger.WriteLogException("delete: " + txt_file + "-->" + e1.Message);
                }

                return html;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("SaveToImage: -->" + ex.Message);
                return string.Empty;
            }
        }
        /// <summary>
        /// Tạo thẻ image từ đối tượng DocumentObject
        /// </summary>
        /// <param name="sInput"></param>
        /// <param name="folder"></param>
        /// <param name="questionId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GenerateImageHTML(DocumentObject sInput, string folder, string questionId, string url)
        {
            if (sInput == null) return string.Empty;
            try
            {
                string objectId = questionId + new Random().Next(111111, 999999);
                //image2017272434293937148497.jpeg hoặc image2017272434293937148497.png
                string fileName = "image" + objectId;
                string htmlFile = folder + Path.DirectorySeparatorChar + fileName + ".html";
                string styleFile = folder + Path.DirectorySeparatorChar + fileName + "_styles.css";

                Document docTemp = new Document();
                Section sectionTemp = docTemp.AddSection();
                Paragraph p = sectionTemp.AddParagraph();
                p.ChildObjects.Add(sInput.Clone());
                docTemp.SaveToFile(htmlFile, FileFormat.Html);
                docTemp.Close();
                docTemp = null;

                string html = File.ReadAllText(htmlFile);
                //get only html
                html = html.Substring(html.IndexOf("<body"));
                html = html.Substring(html.IndexOf(">") + 1);

                html = html.Substring(0, html.IndexOf("</body>"));
                html = html.Replace("> </span>", ">&nbsp;</span>");
                html = html.Replace("<img src=\"", "<img class=\"equation-image\" src=\"" + url + questionId + "/");
                html = SanitizeHtml(html);

                try
                {
                    System.IO.File.Delete(htmlFile);

                }
                catch (Exception e1)
                {
                    CLogger.WriteLogException("delete: " + htmlFile + "-->" + e1.Message);
                }
                try
                {
                    System.IO.File.Delete(styleFile);

                }
                catch (Exception e2)
                {
                    CLogger.WriteLogException("delete: " + styleFile + "-->" + e2.Message);
                }

                return html.Replace("&#xa0;\r\n", "").Trim();
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("SaveToImage: -->" + ex.Message);
                return string.Empty;
            }
        }
        /// <summary>
        /// Lưu từng câu hỏi trong bộ đề định dạng ĐỀ TRẮC NGHIỆM
        /// </summary>
        /// <param name="listQuestions"></param>
        /// <param name="ar"></param>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string SaveDocument_MC_convert(Dictionary<int, string> listQuestions, string folder, string dateFolder, string fileName)
        {
            if (listQuestions == null) return string.Empty;
            try
            {
                Document doc = new Document();
                doc.LoadFromFile(fileName);

                doc.Replace("<b> </b>", "", false, true);
                doc.Replace("<b>	</b>", "", false, true);
                doc.Replace("<b>\t</b>", "", false, true);
                doc.Replace("<b>", "", false, true);
                doc.Replace("</b>", "", false, true);
                doc.Replace("&lt;", "<", false, true);
                doc.Replace("&gt;", ">", false, true);
                int tempCountIndex = 0;

                TextSelection[] selectionsQuestion = doc.FindAllPattern(new Regex(@"^Câu\s\d+\."));
                Dictionary<int, int> questionsIndex = new Dictionary<int, int>();

                for (int i = 0; i < selectionsQuestion.Length; i++)
                {
                    TextRange newrange = new TextRange(doc);
                    newrange = selectionsQuestion[i].GetAsOneRange();
                    Paragraph paragraph = newrange.OwnerParagraph;
                    Body body = paragraph.OwnerTextBody;
                    int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                    if (paragraph.Text.StartsWith(Config.FormQuestion) && paragraph.Text.Length > 8)
                    {
                        questionsIndex.Add(tempCountIndex, body.ChildObjects.IndexOf(paragraph));
                        tempCountIndex++;
                    }
                }

                try
                {
                    for (int i = 0; i < questionsIndex.Count; i++)
                    {
                        Document docTemp = new Document();
                        Section TempsectionQuestion = docTemp.AddSection();

                        int paraIndex = 0;

                        int benginQues = questionsIndex[i];
                        int endQues = benginQues + 1;
                        if (questionsIndex.Count == i + 1) endQues = doc.Sections[0].Paragraphs.Count;
                        else endQues = questionsIndex[i + 1];

                        for (int j = benginQues; j < endQues; j++)
                        {
                            TempsectionQuestion.Paragraphs.Insert(paraIndex, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                            paraIndex++;
                        }

                        string docxFileName = folder + Path.DirectorySeparatorChar + dateFolder + Path.DirectorySeparatorChar + listQuestions[i] + Path.DirectorySeparatorChar + listQuestions[i] + "_convert.docx";

                        docTemp.SaveToFile(docxFileName, FileFormat.Docx);
                        docTemp.Close();
                        docTemp = null;
                        GC.Collect();

                    }

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                doc.Close();
                doc = null;
                GC.Collect();

                return "OK";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Lưu từng câu hỏi trong bộ đề định dạng BẢNG ĐỀ TRẮC NGHIỆM
        /// </summary>
        /// <param name="listQuestions"></param>
        /// <param name="ar"></param>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <param name="isAnswer"></param>
        /// <returns></returns>
        public string SaveDocument_MC_table(Dictionary<int, string> listQuestions, Dictionary<int, string> ar, string folder, string dateFolder, string fileName, string isAnswer)
        {
            if (listQuestions == null) return string.Empty;
            try
            {
                Document doc = new Document();
                doc.LoadFromFile(fileName);

                doc.Replace("<b> </b>", "", false, true);
                doc.Replace("<b>	</b>", "", false, true);
                doc.Replace("<b>\t</b>", "", false, true);
                doc.Replace("<b>", "", false, true);
                doc.Replace("</b>", "", false, true);
                doc.Replace("&lt;", "<", false, true);
                doc.Replace("&gt;", ">", false, true);
                int tempCountIndex = 0;

                TextSelection[] selectionsQuestion1 = doc.FindAllPattern(new Regex(@"^Câu\s\d+\."));
                Dictionary<int, int> questionsIndex1 = new Dictionary<int, int>();

                for (int i = 0; i < selectionsQuestion1.Length; i++)
                {
                    TextRange newrange = new TextRange(doc);
                    newrange = selectionsQuestion1[i].GetAsOneRange();
                    Paragraph paragraph = newrange.OwnerParagraph;
                    Body body = paragraph.OwnerTextBody;
                    int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                    if (paragraph.Text.StartsWith(Config.FormQuestion) && paragraph.Text.Length > 8)
                    {
                        questionsIndex1.Add(tempCountIndex, body.ChildObjects.IndexOf(paragraph));
                        tempCountIndex++;
                    }
                }

                try
                {
                    for (int i = 0; i < questionsIndex1.Count; i++)
                    {
                        Document docTemp = new Document();
                        Section TempsectionQuestion = docTemp.AddSection();
                        Table table1 = TempsectionQuestion.AddTable(true);

                        int paraIndex = 0;
                        int rowIndex = 0;
                        bool isOptionD = false;

                        int benginQues = questionsIndex1[i];
                        int endQues = benginQues + 1;
                        if (questionsIndex1.Count == i + 1) endQues = doc.Sections[0].Paragraphs.Count;
                        else endQues = questionsIndex1[i + 1];

                        for (int j = benginQues; j < endQues; j++)
                        {
                            if (j == benginQues)
                            {
                                table1.AddRow(true, 2);
                                table1.Rows[rowIndex].Cells[0].InsertXHTML("<b>Câu hỏi</b>");
                            }
                            if (rowIndex == 5 && isAnswer == "1")
                            {
                                rowIndex = 6;
                                paraIndex = 0;
                                table1.AddRow(true, 2);
                                table1.Rows[rowIndex].Cells[0].InsertXHTML("<b>Lời giải</b>");
                            }
                            if (rowIndex == 4 && isOptionD)
                            {
                                rowIndex = 5;
                                paraIndex = 0;
                                table1.AddRow(true, 2);
                                table1.Rows[rowIndex].Cells[0].InsertXHTML("<b>Đáp án đúng</b>");
                            }
                            if (doc.Sections[0].Paragraphs[j].Text.StartsWith(Config.OptionA) && rowIndex == 0)
                            {
                                rowIndex = 1;
                                paraIndex = 0;
                                table1.AddRow(true, 2);
                                table1.Rows[rowIndex].Cells[0].InsertXHTML("<b>Đáp án A</b>");
                            }
                            if (doc.Sections[0].Paragraphs[j].Text.StartsWith(Config.OptionB) && rowIndex == 1)
                            {
                                rowIndex = 2;
                                paraIndex = 0;
                                table1.AddRow(true, 2);
                                table1.Rows[rowIndex].Cells[0].InsertXHTML("<b>Đáp án B</b>");
                            }
                            if (doc.Sections[0].Paragraphs[j].Text.StartsWith(Config.OptionC) && rowIndex == 2)
                            {
                                rowIndex = 3;
                                paraIndex = 0;
                                table1.AddRow(true, 2);
                                table1.Rows[rowIndex].Cells[0].InsertXHTML("<b>Đáp án C</b>");
                            }
                            if (doc.Sections[0].Paragraphs[j].Text.StartsWith(Config.OptionD) && rowIndex == 3)
                            {
                                rowIndex = 4;
                                paraIndex = 0;
                                isOptionD = true;
                                table1.AddRow(true, 2);
                                table1.Rows[rowIndex].Cells[0].InsertXHTML("<b>Đáp án D</b>");
                            }

                            TableCell cell1 = table1.Rows[rowIndex].Cells[1];
                            if (rowIndex == 5 && isOptionD)
                            {
                                isOptionD = false;
                                cell1.AddParagraph().AppendText(ar[i]);
                            }
                            else
                            {
                                cell1.Paragraphs.Insert(paraIndex, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                            }

                            paraIndex++;
                        }

                        docTemp.Replace("\t", "", false, true);
                        docTemp.Replace(Config.FormQuestion + " " + (i + 1).ToString() + ".\t", "", false, true);
                        docTemp.Replace(Config.FormQuestion + " " + (i + 1).ToString() + ".	", "", false, true);
                        docTemp.Replace(Config.FormQuestion + " " + (i + 1).ToString() + ". ", "", false, true);
                        docTemp.Replace(Config.FormQuestion + " " + (i + 1).ToString() + ".", "", false, true);
                        docTemp.Replace(Config.OptionA + " ", "", true, true);
                        docTemp.Replace(Config.OptionA, "", true, true);
                        docTemp.Replace(Config.OptionB + " ", "", true, true);
                        docTemp.Replace(Config.OptionB, "", true, true);
                        docTemp.Replace(Config.OptionC + " ", "", true, true);
                        docTemp.Replace(Config.OptionC, "", true, true);
                        docTemp.Replace(Config.OptionD + " ", "", true, true);
                        docTemp.Replace(Config.OptionD, "", true, true);
                        docTemp.Replace("\t", "", true, true);

                        //string docxFileName = folder + listQuestions[i] + Path.DirectorySeparatorChar + listQuestions[i] + "_table.docx";
                        string docxFileName = folder + Path.DirectorySeparatorChar + dateFolder + Path.DirectorySeparatorChar + listQuestions[i] + Path.DirectorySeparatorChar + listQuestions[i] + "_table.docx";
                        docTemp.SaveToFile(docxFileName, FileFormat.Docx);
                        docTemp.Close();
                        docTemp = null;
                        GC.Collect();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                doc.Close();
                doc = null;
                GC.Collect();

                return "OK";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Lưu tất cả các câu hỏi bị lỗi MathType và mất đáp án
        /// </summary>
        /// <param name="listQuestionsError"></param>
        /// <param name="ar"></param>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string SaveDocument_error(List<int> listQuestionsError, Dictionary<int, string> ar, string folder, string fileName)
        {
            if (listQuestionsError.Count == 0) return string.Empty;
            try
            {
                List<string> keys = Path.GetFileName(fileName.Trim()).Split('.').ToList();
                string destFileTable = folder + keys[0] + "_org.docx";

                Document doc = new Document();
                doc.LoadFromFile(destFileTable);

                doc.Replace("<b> </b>", "", false, true);
                doc.Replace("<b>	</b>", "", false, true);
                doc.Replace("<b>\t</b>", "", false, true);
                doc.Replace("<b>", "", false, true);
                doc.Replace("</b>", "", false, true);
                int tempCountIndex = 0;

                TextSelection[] selectionsQuestion = doc.FindAllPattern(new Regex(@"^Câu\s\d+\."));
                Dictionary<int, int> questionsIndex = new Dictionary<int, int>();

                for (int i = 0; i < selectionsQuestion.Length; i++)
                {
                    TextRange newrange = new TextRange(doc);
                    newrange = selectionsQuestion[i].GetAsOneRange();
                    Paragraph paragraph = newrange.OwnerParagraph;
                    Body body = paragraph.OwnerTextBody;
                    int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                    if (paragraph.Text.StartsWith(Config.FormQuestion) && paragraph.Text.Length > 8)
                    {
                        questionsIndex.Add(tempCountIndex, body.ChildObjects.IndexOf(paragraph));
                        tempCountIndex++;
                    }
                }

                try
                {
                    Document docTemp = new Document();
                    Section TempsectionQuestion = docTemp.AddSection();

                    for (int i = 0; i < questionsIndex.Count; i++)
                    {
                        if (listQuestionsError.Contains(i))
                        {
                            int paraIndex = 0;

                            int benginQues = questionsIndex[i];
                            int endQues = benginQues + 1;
                            if (questionsIndex.Count == i + 1) endQues = doc.Sections[0].Paragraphs.Count;
                            else endQues = questionsIndex[i + 1];

                            for (int j = benginQues; j < endQues; j++)
                            {
                                TempsectionQuestion.Paragraphs.Insert(paraIndex, (Paragraph)doc.Sections[0].Paragraphs[j].Clone());
                                paraIndex++;
                            }
                        }
                    }

                    clearFolder("C:/789VN");
                    string docxFileName = "C:/789VN/" + keys[0].Replace("_convert", "_ErrorMathType") + "." + keys[1];

                    docTemp.SaveToFile(docxFileName, FileFormat.Docx);
                    docTemp.Close();
                    docTemp = null;
                    GC.Collect();

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                doc.Close();
                doc = null;
                GC.Collect();


                return "OK";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public Image GetShapeGroupImage(ShapeGroup shape)
        {
            Document docTemp = new Document();
            Spire.Doc.Section section = docTemp.AddSection();
            Paragraph p1 = section.AddParagraph();
            section.PageSetup.Margins.Top = 0;
            section.PageSetup.Margins.Left = 0;
            section.PageSetup.PageSize = new SizeF((int)shape.Width, (int)shape.Height);

            ShapeGroup shape1 = (ShapeGroup)shape.Clone();
            shape1.DistanceLeft = 0;
            shape1.DistanceTop = 0;
            shape1.DistanceRight = 0;
            shape1.DistanceBottom = 0;
            shape1.VerticalAlignment = ShapeVerticalAlignment.Top;
            shape1.VerticalOrigin = 0;
            shape1.VerticalPosition = 0;
            shape1.HorizontalAlignment = ShapeHorizontalAlignment.Left;
            shape1.HorizontalOrigin = 0;
            shape1.HorizontalPosition = 0;
            p1.Format.ClearFormatting();
            p1.ChildObjects.Add(shape1.Clone());
            p1.Format.TextAlignment = TextAlignment.Top;
            p1.Format.SetLeftIndent(0);
            p1.Format.SetRightIndent(0);
            p1.Format.LineSpacing = 0;

            Image image = docTemp.SaveToImages(0, ImageType.Bitmap);
            docTemp.Close();
            docTemp.Dispose();
            docTemp = null;
            return image;
        }
        public void ConvertObjectToPicture(string file)
        {
            Document doc = new Document();
            doc.LoadFromFile(file);

            SectionCollection sections = doc.Sections;
            int i = 0;
            foreach (Spire.Doc.Section sec in sections)
            {
                foreach (Paragraph p in sec.Paragraphs)
                {
                    int pi = 0;
                    Dictionary<int, Image> dic = new Dictionary<int, Image>();
                    foreach (DocumentObject docObj in p.ChildObjects)
                    {
                        if (docObj.DocumentObjectType == DocumentObjectType.ShapeGroup)//OleObject
                        {
                            ShapeGroup shape = (ShapeGroup)docObj;
                            Image img = GetShapeGroupImage(shape);
                            dic.Add(pi, img);
                        }
                        pi++;
                    }
                    foreach (KeyValuePair<int, Image> kv in dic)
                    {
                        ShapeGroup shape = (ShapeGroup)p.ChildObjects[kv.Key].Clone();
                        p.ChildObjects.RemoveAt(kv.Key);
                        DocPicture dp = p.AppendPicture(kv.Value);
                        dp.VerticalAlignment = shape.VerticalAlignment;
                        dp.VerticalOrigin = shape.VerticalOrigin;
                        dp.VerticalPosition = shape.VerticalPosition;

                        dp.HorizontalAlignment = shape.HorizontalAlignment;
                        dp.HorizontalOrigin = shape.HorizontalOrigin;
                        dp.HorizontalPosition = shape.HorizontalPosition;

                        dp.Height = (float)shape.Height;
                        dp.Width = (float)shape.Width;
                        dp.DistanceLeft = shape.DistanceLeft;
                        dp.DistanceTop = shape.DistanceTop;
                        dp.DistanceBottom = shape.DistanceBottom;
                        dp.DistanceRight = shape.DistanceRight;

                    }
                    foreach (KeyValuePair<int, Image> kv in dic)
                    {
                        try
                        {
                            kv.Value.Dispose();
                        }
                        catch { }

                    }
                }
            }
            doc.SaveToFile(file);
            sections = null;
            doc.Close();
            doc.Dispose();
            doc = null;
            GC.Collect();
        }

        #endregion  

        #region Hàm Public
        public void RemoveParagragh_NULL1(string destFileOrg)
        {
            Document document = new Document();
            document.LoadFromFile(destFileOrg);

            try
            {
                foreach (Section section in document.Sections)
                {
                    for (int j = 0; j < section.Body.ChildObjects.Count; j++)
                    {
                        (section.Body.ChildObjects[j] as Paragraph).Format.LeftIndent = 0;
                        (section.Body.ChildObjects[j] as Paragraph).Format.FirstLineIndent = 0;
                        (section.Body.ChildObjects[j] as Paragraph).Format.BeforeAutoSpacing = false;
                        (section.Body.ChildObjects[j] as Paragraph).Format.AfterAutoSpacing = false;
                        (section.Body.ChildObjects[j] as Paragraph).Format.AfterSpacing = 0;
                        (section.Body.ChildObjects[j] as Paragraph).Format.BeforeSpacing = 0;
                        (section.Body.ChildObjects[j] as Paragraph).Format.Tabs.Clear();

                        try
                        {
                            if (String.IsNullOrEmpty((section.Body.ChildObjects[j] as Paragraph).Text.Trim()))
                            {
                                DocumentObjectCollection o = (section.Body.ChildObjects[j] as Paragraph).ChildObjects;
                                if (o.Count == 0)
                                {
                                    section.Body.ChildObjects.Remove(section.Body.ChildObjects[j]);
                                    j--;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex_ReCountDocument)
            {
                document.Close();
                document.Dispose();
                document = null;
                GC.Collect();
                //throw new Exception(ConfigHelper.GetConfig("Error1"));
            }

            document.SaveToFile(destFileOrg, FileFormat.Docx);
            document.Close();
            document.Dispose();
            document = null;
            GC.Collect();
        }

        public void RemoveParagragh_NULL2(string destFileOrg)
        {
            Document document = new Document();
            document.LoadFromFile(destFileOrg);

            document.Replace("\t", "", false, true);

            foreach (Section section in document.Sections)
            {
                foreach (Paragraph para in section.Paragraphs)
                {
                    para.Format.LeftIndent = 0;
                    para.Format.FirstLineIndent = 0;
                    para.Format.BeforeAutoSpacing = false;
                    para.Format.AfterAutoSpacing = false;
                    para.Format.AfterSpacing = 0;
                    para.Format.BeforeSpacing = 0;
                    para.Format.Tabs.Clear();

                    if ((String.IsNullOrEmpty(para.Text.Trim())) || (para.Text == "\t"))
                    {
                        DocumentObjectCollection o = para.ChildObjects;
                        if (o.Count == 0)
                        {
                            section.Body.ChildObjects.Remove(para);
                        }
                    }
                }
            }

            document.SaveToFile(destFileOrg, FileFormat.Docx);
            document.Close();
            document.Dispose();
            document = null;
            GC.Collect();
        }

        public void RemoveParagragh_NULL3(string filePath)
        {
            Document document = new Document();
            document.LoadFromFile(filePath);

            foreach (Section section in document.Sections)
            {
                foreach (Paragraph para in section.Paragraphs)
                {
                    para.Format.LeftIndent = 0;
                    para.Format.FirstLineIndent = 0;
                    para.Format.BeforeAutoSpacing = false;
                    para.Format.AfterAutoSpacing = false;
                    para.Format.AfterSpacing = 0;
                    para.Format.BeforeSpacing = 0;
                    para.Format.Tabs.Clear();

                    if ((String.IsNullOrWhiteSpace(para.Text)) || (para.Text.Trim() == "\t"))
                    {
                        DocumentObjectCollection o = para.ChildObjects;
                        if (o.Count == 0)
                        {
                            section.Body.ChildObjects.Remove(para);
                        }
                    }
                }
            }

            document.SaveToFile(filePath, FileFormat.Docx);
            document.Close();
            document.Dispose();
            document = null;
            GC.Collect();
        }
        public void SaveDocxAs(string filePath, FileFormat fmt, string ext)
        {
            Document document = new Document();
            document.LoadFromFile(filePath);
            document.HtmlExportOptions.ImageEmbedded = true;
            string newFilePath = filePath.Replace(".docx", ext);
            document.SaveToFile(newFilePath, fmt);
            document.Close();
            document = null;
            GC.Collect();
        }

        public void clearFolder(string FolderName)
        {
            if (!System.IO.Directory.Exists(FolderName)) System.IO.Directory.CreateDirectory(FolderName);

            DirectoryInfo dir = new DirectoryInfo(FolderName);
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName);
                di.Delete();
            }
        }

        public string format_Text(string textFormat)
        {
            string tempText = textFormat.Replace("<b> </b>", "");
            tempText = tempText.Replace("<b> </b>", "");
            tempText = tempText.Replace(".\t", " ");
            tempText = tempText.Replace("\t", " ");
            tempText = tempText.Replace(".", ". ");
            tempText = tempText.Replace("www. w3. org/1998/Math/MathML", "www.w3.org/1998/Math/MathML");
            tempText = tempText.Replace(" xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
            tempText = tempText.Replace("xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
            tempText = tempText.Replace("<!-- MathType@Translator@5@5@MathML3 (namespace attr). tdl@MathML 3. 0 (namespace attr)@ -->", "");

            while (tempText.Contains("  ") == true)
            {
                tempText = tempText.Replace("  ", " ");
            }

            if (tempText.Contains("<math xmlns=") ||
                tempText.Contains("<semantics>") ||
                tempText.Contains("<mi>") ||
                tempText.Contains("<mo>") ||
                tempText.Contains("<mrow>") ||
                tempText.Contains("<mn>"))
            {
                tempText = tempText.Replace("<b>", "");
                tempText = tempText.Replace("</b>", "");
                tempText = tempText.Replace("<i>", "");
                tempText = tempText.Replace("</i>", "");
                tempText = tempText.Replace("<u>", "");
                tempText = tempText.Replace("</u>", "");
                tempText = tempText.Replace("<mstyledisplaystyle='true'>", "<mstyle displaystyle='true'>");
                tempText = tempText.Replace("<mstyledisplaystyle=\"true\">", "<mstyle displaystyle=\"true\">");
                tempText = tempText.Replace("<moveraccent='true'>", "<mover accent='true'>");
                tempText = tempText.Replace("<moveraccent=\"true\">", "<mover accent=\"true\">");

                if (tempText.Contains("<msup>") && tempText.Contains("<mo stretchy='false'>(") && tempText.Contains("<mo stretchy='false'>)"))
                {
                    tempText = tempText.Replace("<mo stretchy='false'>(</mo>", "<mrow><mo stretchy='false'>(</mo>");
                    tempText = tempText.Replace("<mo stretchy='false'>)</mo>", "<mo stretchy='false'>)</mo></mrow>");
                }

            }

            tempText = tempText.Replace("xmlns:mml=\"http://www.w3.org/1998/Math/MathML\" xmlns:m=\"http://schemas.openxmlformats.org/officeDocument/2006/math\"", "xmlns='http://www.w3.org/1998/Math/MathML'");
            tempText = tempText.Replace("<mml:", "<");
            tempText = tempText.Replace("</mml:", "</");

            return tempText;
        }

        public string format_HTML(string textFormat)
        {
            string tempText = textFormat.Replace("&", "&amp;");
            tempText = tempText.Replace("<", "&lt;");
            tempText = tempText.Replace(">", "&gt;");

            return tempText;
        }
        public bool checkOptions(string optionABCD)
        {
            bool checkOption = false;
            string tempCheck = optionABCD.Replace("<br>", "");
            tempCheck = tempCheck.Replace("<b>", "");
            tempCheck = tempCheck.Replace("</b>", "");
            tempCheck = tempCheck.Replace("<u>", "");
            tempCheck = tempCheck.Replace("</u>", "");
            tempCheck = tempCheck.Replace("<i>", "");
            tempCheck = tempCheck.Replace("</i>", "");
            tempCheck = tempCheck.Replace("<br/>", "");
            tempCheck = tempCheck.Replace("<br />", "");
            tempCheck = tempCheck.Replace("</br>", "");
            tempCheck = tempCheck.Replace("</ br>", "");
            tempCheck = tempCheck.Replace("A.", "");
            tempCheck = tempCheck.Replace("B.", "");
            tempCheck = tempCheck.Replace("C.", "");
            tempCheck = tempCheck.Replace("D.", "");
            if (String.IsNullOrWhiteSpace(tempCheck.Replace(".", "")))
            {
                checkOption = true;
            }

            return checkOption;
        }

        public void SaveMathML(string directoryPath, int number, ref string destFileConvert, ref SessionInfo SessionInfo)
        {
            //Thread.Sleep(500);
            Word.Application word = new Word.Application();
            object miss = System.Reflection.Missing.Value;
            object path = destFileConvert;
            object readOnly = false;
            //object Revert = false;
            //object openAndRepair = false;
            object Visible = false;

            int count = 0;
            SessionInfo.isMathTypeError = false;
            bool isConvertOK = true;

            Word.Document doc = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss,
                ref miss, ref miss, ref Visible, ref miss, ref miss, ref miss, ref miss);

            try
            {
                doc.Activate();

                Word.InlineShapes shapesList = doc.InlineShapes;
                for (int i = 1; i <= shapesList.Count; i++)
                {
                    Word.InlineShape s = shapesList[i];
                    if (s != null)
                    {
                        if (s.Type == Word.WdInlineShapeType.wdInlineShapeEmbeddedOLEObject)
                        {
                            string filePath = directoryPath + "\\MT_" + count + ".eps";
                            ConvertEquations.ConvertEquation ce = new ConvertEquations.ConvertEquation();
                            ConvertEquations.EquationInputClipboardEmbeddedObject iEqua = new ConvertEquations.EquationInputClipboardEmbeddedObject();
                            ConvertEquations.EquationOutputFileEPS oEqua = new ConvertEquations.EquationOutputFileEPS(filePath);
                            oEqua.strOutTrans = "MathML3 (namespace attr).tdl";

                            Word.Range newRange = s.Range;
                            newRange.Font.Bold = 0;
                            newRange.Font.Italic = 0;
                            newRange.Font.Underline = Word.WdUnderline.wdUnderlineNone;

                            s.Range.Cut();

                            if (ce.Convert(iEqua, oEqua))
                            {
                                newRange.InsertBefore("{{MathTypeError}}");

                                count++;
                                //System.Threading.Thread.Sleep(5);
                            }
                            else isConvertOK = false;

                            GC.Collect();
                            Clipboard.Clear();
                            //Thread.Sleep(5);
                            Application.DoEvents();
                            i--;
                        }
                        else isConvertOK = false;
                    }
                }

            }
            catch (Exception ex_ExtractMathML)
            {
                doc.Save();
                doc.Close();
                doc = null;

                word.Quit();
                word = null;
                GC.Collect();

                string errorMathType = ConfigHelper.GetConfig("Error5");
                errorMathType += "\r\n" + "Bước 1: Khởi động lại máy tính.";
                errorMathType += "\r\n" + "Bước 2: Xóa MathType trên máy tính và cài bản đi kèm phần mềm.";
                errorMathType += "\r\n" + "Bước 3: Cho MathType tự động định dạng lại các ký tự (trong mục Format Equations).";
                throw new Exception(errorMathType);
            }

            if (!isConvertOK) SessionInfo.isMathTypeError = true;
            else SessionInfo.isMathTypeError = false;

            string temp = destFileConvert.Replace(".docx", "") + number + ".docx";
            destFileConvert = temp;
            doc.SaveAs2(destFileConvert);
            doc.Close();
            doc = null;

            word.Quit();
            word = null;
            GC.Collect();
        }

        public void ConvertMathML(string directoryPath, string destFileConvert, ref SessionInfo SessionInfo)
        {
            Document document = new Document();
            document.LoadFromFile(destFileConvert);

            string MathDele1 = "<!-- MathType@Translator@5@5@MathML3 (namespace attr).tdl@MathML 3.0 (namespace attr)@ -->";
            string MathDele2 = "<!-- MathType@End@5@5@ -->";
            string MathReplaceFrom = "<math display='block' xmlns=";
            string MathReplaceTo = "<math xmlns=";

            bool isConvertOK = true;
            SessionInfo.isMathTypeError = false;
            TextSelection[] MathTypeError = document.FindAllPattern(new Regex(@"{{MathTypeError}}"));
            if (MathTypeError != null)
            {

                for (int i = 0; i < MathTypeError.Length; i++)
                {
                    TextRange newrange = new TextRange(document);
                    newrange = MathTypeError[i].GetAsOneRange();

                    string MathML_Converted = "";
                    string filePath = directoryPath + "\\MT_" + i + ".eps";
                    ConvertEquations.ConvertEquation ce = new ConvertEquations.ConvertEquation();
                    ConvertEquations.EquationOutputClipboardText oEqua = new ConvertEquations.EquationOutputClipboardText();
                    oEqua.strOutTrans = "MathML3 (namespace attr).tdl";
                    ConvertEquations.EquationInputFileEPS iEqua = new ConvertEquations.EquationInputFileEPS(filePath);

                    if (ce.Convert(iEqua, oEqua))
                    {
                        if (!String.IsNullOrEmpty(Clipboard.GetText(TextDataFormat.UnicodeText)))
                        {
                            MathML_Converted = Clipboard.GetText(TextDataFormat.UnicodeText).Trim().Replace("\r\n     ", "\r\n");
                            MathML_Converted = format_Text(MathML_Converted.Trim());
                            MathML_Converted = MathML_Converted.Trim().Replace("\r\n    ", "\r\n");
                            MathML_Converted = MathML_Converted.Trim().Replace("\r\n   ", "\r\n");
                            MathML_Converted = MathML_Converted.Trim().Replace("\r\n  ", "\r\n");
                            MathML_Converted = MathML_Converted.Trim().Replace("\r\n ", "\r\n");
                            MathML_Converted = MathML_Converted.Trim().Replace(MathDele1, "");
                            MathML_Converted = MathML_Converted.Trim().Replace(MathDele2, "");
                            MathML_Converted = MathML_Converted.Trim().Replace(MathReplaceFrom, MathReplaceTo);
                            MathML_Converted = MathML_Converted.Trim().Replace("\r\n", "");

                            StringBuilder sb = new StringBuilder();
                            sb.Append(MathML_Converted);
                            int annotation1 = sb.ToString().IndexOf("<annotation encoding");
                            int annotation2 = sb.ToString().IndexOf("</annotation>");
                            int annotation_length = (annotation2 - annotation1) + 13;
                            if (sb.Length > (annotation_length + annotation1))
                            {
                                MathML_Converted = sb.Remove(annotation1, annotation_length).ToString().Trim();
                            }
                            MathML_Converted = MathML_Converted.Replace("www. w3. org/1998/Math/MathML", "www.w3.org/1998/Math/MathML");
                            MathML_Converted = MathML_Converted.Replace(" xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                            MathML_Converted = MathML_Converted.Replace("xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                            MathML_Converted = MathML_Converted.Replace("<!-- MathType@Translator@5@5@MathML3 (namespace attr). tdl@MathML 3. 0 (namespace attr)@ -->", "");
                        }

                        newrange.Text = " " + MathML_Converted + " ";
                    }
                    else
                    {
                        newrange.Text = "<font style=\"color: red;\">{{MathType_convert_error}}</font>";
                        isConvertOK = false;
                    }
                    Clipboard.Clear();
                    Application.DoEvents();

                    newrange.CharacterFormat.Bold = false;
                    newrange.CharacterFormat.Italic = false;
                    newrange.CharacterFormat.TextColor = Color.Black;
                    newrange.CharacterFormat.HighlightColor = Color.White;
                }

            }

            if (!isConvertOK) SessionInfo.isMathTypeError = true;
            else SessionInfo.isMathTypeError = false;

            document.SaveToFile(destFileConvert, FileFormat.Docx);

            document.Close();
            document = null;
            GC.Collect();
        }

        public void ReformatEquations(string destFileConvert, ref string log)
        {
            log = CLogger.Append(log, "ReformatEquations");
            Word.Application word = new Word.Application();
            object miss = System.Reflection.Missing.Value;
            object path = destFileConvert;
            object readOnly = false;
            //object Revert = false;
            //object openAndRepair = false;
            object Visible = false;

            Word.Document doc = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss,
                ref miss, ref miss, ref Visible, ref miss, ref miss, ref miss, ref miss);

            try
            {
                doc.Activate();

                Word.InlineShapes shapesList = doc.InlineShapes;
                foreach (Word.InlineShape s in doc.InlineShapes)
                {
                    if (s != null)
                    {
                        if (s.Type == Word.WdInlineShapeType.wdInlineShapeEmbeddedOLEObject)
                        {
                            s.Range.Font.Bold = 0;
                            s.Range.Font.Italic = 0;
                            s.Range.Font.Name = ConfigHelper.GetConfig("FontName", "");
                            s.Range.Font.Underline = Word.WdUnderline.wdUnderlineNone;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log = CLogger.Append(log, e.Message);
            }

            doc.Save();
            doc.Close();
            doc = null;

            word.Quit();
            word = null;
            GC.Collect();
        }

        public void ExtractMathML_double(int number, ref string destFileConvert, ref SessionInfo SessionInfo, ref string log)
        {
            DateTime st = DateTime.Now;
            Word.Application word = new Word.Application();
            object miss = System.Reflection.Missing.Value;
            object path = destFileConvert;
            object readOnly = false;
            //object Revert = false;
            //object openAndRepair = false;
            object Visible = false;

            string MathDele1 = "<!-- MathType@Translator@5@5@MathML3 (namespace attr).tdl@MathML 3.0 (namespace attr)@ -->";
            string MathDele2 = "<!-- MathType@End@5@5@ -->";
            string MathReplaceFrom = "<math display='block' xmlns=";
            string MathReplaceTo = "<math xmlns=";

            SessionInfo.isMathTypeError = false;
            bool isConvertOK = true;
            Word.Document doc = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss,
                ref miss, ref miss, ref Visible, ref miss, ref miss, ref miss, ref miss);

            try
            {
                doc.Activate();

                ConvertEquations.ConvertEquation ce = new ConvertEquations.ConvertEquation();
                ConvertEquations.EquationOutputClipboardText oEqua = new ConvertEquations.EquationOutputClipboardText();
                oEqua.strOutTrans = "MathML3 (namespace attr).tdl";
                int i = 0;
                Word.Shapes shapes = doc.Shapes;
                List<string> groupItemIds = new List<string>();
                int nameIndex = 1;
                foreach (Word.Shape s in shapes)
                {

                    if (s != null)
                    {
                        s.ConvertToInlineShape();
                        CLogger.WriteLogError(s.Type.ToString() + "----------------" + nameIndex++);
                        try
                        {
                            //s.TextFrame.TextRange.ShapeRange.Select();
                            //s.Select();
                            //Computer com1 = new Computer();

                            ////s.Range.Copy();
                            //s.Select();
                            //doc.Application.Selection.CopyAsPicture();

                            //if (Clipboard.GetDataObject() != null)
                            //{
                            //    var data = Clipboard.GetDataObject();

                            //    // Check if the data conforms to a bitmap format
                            //    if (data != null && data.GetDataPresent(DataFormats.Bitmap))
                            //    {
                            //        // Fetch the image and convert it to a Bitmap
                            //        var image = (Image)data.GetData(DataFormats.Bitmap, true);
                            //        var currentBitmap = new Bitmap(image);

                            //        // Save the bitmap to a file
                            //        currentBitmap.Save(@"C:\temp1\f" + String.Format("img_{0}.png", nameIndex));
                            //    }
                            //}
                            //nameIndex++;

                            //if(s.GroupItems.Count > 0)
                            //{
                            //    int j = 0;
                            //    foreach(Microsoft.Office.Interop.Word.Shape groupShape in s.GroupItems)
                            //    {
                            //        string name = "kibac" + nameIndex++ + "-" + j++;
                            //        //Microsoft.Office.Interop.Word.Shape groupShape = dyShapes[j];
                            //        //groupShape.Name = name;
                            //        //groupItemIds.Add(name);
                            //        //groupShape.Title = name;
                            //        groupShape.AlternativeText = name;
                            //        //groupItemIds.Add(name);
                            //        CLogger.WriteLogError(s.Type.ToString() + ">>>aaa>" + groupShape.ParentGroup.Type.ToString() + ">>" + groupShape.Type.ToString());
                            //        // do something
                            //    }
                            //    s.Select();
                            //    doc.ActiveWindow.Selection.CopyAsPicture();

                            //    Image image = Clipboard.GetImage();
                            //    Bitmap bitmap = new Bitmap(image);
                            //    bitmap.Save(@"c:\temp1\e\kibac" +nameIndex+ ".jpg");
                            //    nameIndex++;

                            //}
                        }
                        catch (Exception ee)
                        {
                            string see = ee.Message;
                            CLogger.WriteLogError(s.Type.ToString() + ">>>>" + see);
                        }
                    }
                }
                Word.InlineShapes shapesList = doc.InlineShapes;
                foreach (Word.InlineShape s in doc.InlineShapes)
                {

                    if (s != null)
                    {

                        CLogger.WriteInfo(s.Type.ToString() + ">>>>-------------------------------");
                        if (s.Type == Word.WdInlineShapeType.wdInlineShapeEmbeddedOLEObject)
                        {

                            try
                            {
                                Computer com1 = new Computer();
                                string MathML_Converted = "";

                                Word.Range newRange = s.Range;
                                newRange.Font.Bold = 0;
                                newRange.Font.Italic = 0;
                                newRange.Font.Underline = Word.WdUnderline.wdUnderlineNone;

                                //s.Range.Select();
                                s.Range.Copy();
                                //doc.Application.Selection.Copy();

                                IDataObject ido = com1.Clipboard.GetDataObject();
                                Bitmap bm = (Bitmap)ido.GetData(DataFormats.Bitmap);

                                MemoryStream ms = Converter.MakeMetafileStream(bm);
                                string f = @"c:\temp1\f\kibac" + DateTime.Now.Millisecond + "-" + (i++) + ".wmf";
                                File.WriteAllBytes(f, ms.ToArray());
                                //wmf.Save(@"c:\temp1\b\kibac" + DateTime.Now.Millisecond + "-" + (i++) + ".gif");

                                //ConvertEquations.EquationInputClipboardText iEqua = new ConvertEquations.EquationInputClipboardText("GAT");
                                //ce = new ConvertEquations.ConvertEquation();
                                oEqua = new ConvertEquations.EquationOutputClipboardText();
                                ConvertEquations.EquationInputClipboardEmbeddedObject iEqua = new ConvertEquations.EquationInputClipboardEmbeddedObject();
                                if (ce.Convert(iEqua, oEqua))
                                {
                                    string newText = com1.Clipboard.GetText(TextDataFormat.Text);
                                    if (!String.IsNullOrEmpty(newText))
                                    {
                                        MathML_Converted = newText;
                                        //.Replace("\r\n     ", "\r\n");
                                        //MathML_Converted = format_Text(MathML_Converted.Trim());
                                        //MathML_Converted = MathML_Converted.Trim().Replace("\r\n    ", "\r\n");
                                        //MathML_Converted = MathML_Converted.Trim().Replace("\r\n   ", "\r\n");
                                        //MathML_Converted = MathML_Converted.Trim().Replace("\r\n  ", "\r\n");
                                        //MathML_Converted = MathML_Converted.Trim().Replace("\r\n ", "\r\n");
                                        //MathML_Converted = MathML_Converted.Trim().Replace(MathDele1, "");
                                        //MathML_Converted = MathML_Converted.Trim().Replace(MathDele2, "");
                                        //MathML_Converted = MathML_Converted.Trim().Replace(MathReplaceFrom, MathReplaceTo);
                                        //MathML_Converted = MathML_Converted.Trim().Replace("\r\n", "");

                                        //StringBuilder sb = new StringBuilder();
                                        //sb.Append(MathML_Converted);
                                        //int annotation1 = sb.ToString().IndexOf("<annotation encoding");
                                        //int annotation2 = sb.ToString().IndexOf("</annotation>");
                                        //int annotation_length = (annotation2 - annotation1) + 13;
                                        //if (sb.Length > (annotation_length + annotation1))
                                        //{
                                        //    MathML_Converted = sb.Remove(annotation1, annotation_length).ToString().Trim();
                                        //}
                                        //MathML_Converted = MathML_Converted.Replace("www. w3. org/1998/Math/MathML", "www.w3.org/1998/Math/MathML");
                                        //MathML_Converted = MathML_Converted.Replace(" xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                                        //MathML_Converted = MathML_Converted.Replace("xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                                        //MathML_Converted = MathML_Converted.Replace("<!-- MathType@Translator@5@5@MathML3 (namespace attr). tdl@MathML 3. 0 (namespace attr)@ -->", "");

                                        //MathML_Converted = MathML_Converted.Replace("<math xmlns='http://www.w3.org/1998/Math/MathML'>", "");
                                        //MathML_Converted = MathML_Converted.Replace("</math>", "");
                                        newRange.InsertBefore(" " + MathML_Converted + " ");
                                        s.Delete();
                                        CLogger.WriteLogError("item: " + MathML_Converted);
                                    }
                                    else
                                    {
                                        CLogger.WriteLogError("item - error null: " + MathML_Converted);
                                        isConvertOK = false;
                                    }
                                }
                                else
                                {
                                    s.Range.Copy();
                                    ConvertEquations.EquationInputClipboardText iEqua1 = new ConvertEquations.EquationInputClipboardText("GAT");
                                    if (ce.Convert(iEqua1, oEqua))
                                    {
                                        string newText = com1.Clipboard.GetText(TextDataFormat.Text);
                                        if (!String.IsNullOrEmpty(newText))
                                        {
                                            newRange.InsertBefore(" " + MathML_Converted + " ");
                                            s.Delete();
                                            CLogger.WriteLogError("item: " + MathML_Converted);
                                        }
                                    }
                                    else
                                    {
                                        CLogger.WriteLogError("item - error convert: " + MathML_Converted);
                                    }

                                    isConvertOK = false;
                                }

                                Clipboard.Clear();
                                com1.Clipboard.Clear();
                                GC.Collect();
                                Thread.Sleep(5);
                                Application.DoEvents();
                            }
                            catch (Exception e)
                            {
                                CLogger.WriteLogException("kibac>>" + e.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex_ExtractMathML_double)
            {
                doc.Save();
                doc.Close();
                doc = null;

                word.Quit();
                word = null;
                GC.Collect();

                string errorMathType = ConfigHelper.GetConfig("Error5");
                errorMathType += "\r\n" + "Bước 1: Khởi động lại máy tính.";
                errorMathType += "\r\n" + "Bước 2: Xóa MathType trên máy tính và cài bản đi kèm phần mềm.";
                errorMathType += "\r\n" + "Bước 3: Cho MathType tự động định dạng lại các ký tự (trong mục Format Equations).";
                //sssthrow new Exception(errorMathType);
            }

            if (!isConvertOK) SessionInfo.isMathTypeError = true;
            else SessionInfo.isMathTypeError = false;

            string temp = destFileConvert.Replace(".docx", "") + number + ".docx";
            destFileConvert = temp;
            doc.SaveAs2(destFileConvert);
            doc.Close();
            doc = null;

            word.Quit();
            word = null;
            GC.Collect();
            TimeSpan se = DateTime.Now.Subtract(st);
            log = CLogger.Append(log, se.TotalSeconds);
        }
        protected void SaveInlineShapeToFile(int inlineShapeId, Word.Application wordApplication)
        {
            // Get the shape, select, and copy it to the clipboard
            var inlineShape = wordApplication.ActiveDocument.InlineShapes[inlineShapeId];
            inlineShape.Select();
            wordApplication.Selection.Copy();

            // Check data is in the clipboard
            if (Clipboard.GetDataObject() != null)
            {
                var data = Clipboard.GetDataObject();

                // Check if the data conforms to a bitmap format
                if (data != null && data.GetDataPresent(DataFormats.Bitmap))
                {
                    // Fetch the image and convert it to a Bitmap
                    var image = (Image)data.GetData(DataFormats.Bitmap, true);
                    var currentBitmap = new Bitmap(image);

                    // Save the bitmap to a file
                    currentBitmap.Save(@"C:\temp1\f" + String.Format("img_{0}.png", inlineShapeId));
                }
            }
        }
        public void ExtractMathML_final(int number, ref string destFileConvert, ref SessionInfo SessionInfo)
        {
            Word.Application word = new Word.Application();
            object miss = System.Reflection.Missing.Value;
            object path = destFileConvert;
            object readOnly = false;
            //object Revert = false;
            //object openAndRepair = false;
            object Visible = false;

            string MathDele1 = "<!-- MathType@Translator@5@5@MathML3 (namespace attr).tdl@MathML 3.0 (namespace attr)@ -->";
            string MathDele2 = "<!-- MathType@End@5@5@ -->";
            string MathReplaceFrom = "<math display='block' xmlns=";
            string MathReplaceTo = "<math xmlns=";

            SessionInfo.isMathTypeError = false;
            bool isConvertOK = true;
            Word.Document doc = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss,
                ref miss, ref miss, ref Visible, ref miss, ref miss, ref miss, ref miss);

            try
            {
                doc.Activate();

                Word.InlineShapes shapesList = doc.InlineShapes;
                foreach (Word.InlineShape s in doc.InlineShapes)
                {
                    if (s != null)
                    {
                        if (s.Type == Word.WdInlineShapeType.wdInlineShapeEmbeddedOLEObject)
                        {
                            Computer com1 = new Computer();
                            ConvertEquations.ConvertEquation ce = new ConvertEquations.ConvertEquation();
                            ConvertEquations.EquationInputClipboardEmbeddedObject iEqua = new ConvertEquations.EquationInputClipboardEmbeddedObject();

                            ConvertEquations.EquationOutputClipboardText oEqua = new ConvertEquations.EquationOutputClipboardText();
                            oEqua.strOutTrans = "MathML3 (namespace attr).tdl";
                            string MathML_Converted = "";

                            Word.Range newRange = s.Range;
                            newRange.Font.Bold = 0;
                            newRange.Font.Italic = 0;
                            newRange.Font.Underline = Word.WdUnderline.wdUnderlineNone;

                            s.Range.Copy();

                            if (ce.Convert(iEqua, oEqua))
                            {
                                if (!String.IsNullOrEmpty(com1.Clipboard.GetText(TextDataFormat.UnicodeText)))
                                {
                                    MathML_Converted = com1.Clipboard.GetText(TextDataFormat.UnicodeText).Trim().Replace("\r\n     ", "\r\n");
                                    MathML_Converted = format_Text(MathML_Converted.Trim());
                                    MathML_Converted = MathML_Converted.Trim().Replace("\r\n    ", "\r\n");
                                    MathML_Converted = MathML_Converted.Trim().Replace("\r\n   ", "\r\n");
                                    MathML_Converted = MathML_Converted.Trim().Replace("\r\n  ", "\r\n");
                                    MathML_Converted = MathML_Converted.Trim().Replace("\r\n ", "\r\n");
                                    MathML_Converted = MathML_Converted.Trim().Replace(MathDele1, "");
                                    MathML_Converted = MathML_Converted.Trim().Replace(MathDele2, "");
                                    MathML_Converted = MathML_Converted.Trim().Replace(MathReplaceFrom, MathReplaceTo);
                                    MathML_Converted = MathML_Converted.Trim().Replace("\r\n", "");

                                    StringBuilder sb = new StringBuilder();
                                    sb.Append(MathML_Converted);
                                    int annotation1 = sb.ToString().IndexOf("<annotation encoding");
                                    int annotation2 = sb.ToString().IndexOf("</annotation>");
                                    int annotation_length = (annotation2 - annotation1) + 13;
                                    if (sb.Length > (annotation_length + annotation1))
                                    {
                                        MathML_Converted = sb.Remove(annotation1, annotation_length).ToString().Trim();
                                    }
                                    MathML_Converted = MathML_Converted.Replace("www. w3. org/1998/Math/MathML", "www.w3.org/1998/Math/MathML");
                                    MathML_Converted = MathML_Converted.Replace(" xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                                    MathML_Converted = MathML_Converted.Replace("xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                                    MathML_Converted = MathML_Converted.Replace("<!-- MathType@Translator@5@5@MathML3 (namespace attr). tdl@MathML 3. 0 (namespace attr)@ -->", "");

                                    newRange.InsertBefore(" " + MathML_Converted + " ");
                                    s.Delete();
                                }
                                else isConvertOK = false;
                            }
                            else isConvertOK = false;

                            com1.Clipboard.Clear();
                            Thread.Sleep(5);
                            Application.DoEvents();
                        }
                    }
                }
            }
            catch (Exception ex_ExtractMathML_final)
            {
                doc.Save();
                doc.Close();
                doc = null;

                word.Quit();
                word = null;
                GC.Collect();

                string errorMathType = ConfigHelper.GetConfig("Error5");
                errorMathType += "\r\n" + "Bước 1: Khởi động lại máy tính.";
                errorMathType += "\r\n" + "Bước 2: Xóa MathType trên máy tính và cài bản đi kèm phần mềm.";
                errorMathType += "\r\n" + "Bước 3: Cho MathType tự động định dạng lại các ký tự (trong mục Format Equations).";
                throw new Exception(errorMathType);
            }

            if (!isConvertOK) SessionInfo.isMathTypeError = true;
            else SessionInfo.isMathTypeError = false;

            string temp = destFileConvert.Replace(".docx", "") + number + ".docx";
            destFileConvert = temp;
            doc.SaveAs2(destFileConvert);
            doc.Close();
            doc = null;

            word.Quit();
            word = null;
            GC.Collect();
        }

        public bool CheckMathTypeObject(string destFileOrg, string destFileConvert, ref int numMathType)
        {
            bool checkMathType = false;
            Document document = new Document();
            document.LoadFromFile(destFileOrg);

            int count = 0;
            foreach (Section section in document.Sections)
            {
                foreach (Paragraph paragraph in section.Paragraphs)
                {
                    foreach (DocumentObject docObject in paragraph.ChildObjects)
                    {
                        if (docObject.DocumentObjectType == DocumentObjectType.OleObject)
                        {
                            DocOleObject ole = docObject as DocOleObject;
                            if (ole.ObjectType == "Equation.DSMT4")
                            {
                                checkMathType = true;
                                count++;
                            }
                            if (ole.ObjectType == "Equation.3")
                            {
                                //TODO: dont need to exit
                                //throw new Exception(ConfigHelper.GetConfig("Error3"));
                            }
                        }
                    }
                }
            }

            //TODO: dont need to save
            //document.SaveToFile(destFileConvert, FileFormat.Docx);

            document.Close();
            document = null;
            GC.Collect();

            numMathType = count;

            return checkMathType;
        }

        public int ReCountDocument(string filePath)
        {
            int quesCount = 1;
            TextSelection[] selectionsQuestion = null;

            Document document = new Document();
            document.LoadFromFile(filePath);

            if (document.IsContainMacro) document.ClearMacros();

            if (document.FindAllPattern(new Regex(@"^Câu\d+\.")) != null ||
                document.FindAllPattern(new Regex(@"^Câu\d+\s+")) != null ||
                document.FindAllPattern(new Regex(@"^Câu\s{2,1000}\d+\.")) != null ||
                document.FindAllPattern(new Regex(@"^Câu\s{2,1000}\d+\s+\.")) != null ||
                document.FindAllPattern(new Regex(@"^Câu\s\d+\s+")) != null ||
                document.FindAllPattern(new Regex(@"^Câu\s\d+\.{2,1000}")) != null ||
                document.FindAllPattern(new Regex(@"^Câu\s\d+\.\w")) != null ||
                document.FindAllPattern(new Regex(@"^Câu\s\d+\s+\.")) != null)
            {
                string ErrorCaution = "Câu hỏi không đúng định dạng quy định về từ khóa 'Câu + khoảng trắng + số + chấm + khoảng trắng'\r\n";
                ErrorCaution += "\r\n Chọn YES để phần mềm tự động sửa ngay tại file đề gốc.";
                ErrorCaution += "\r\n Chọn NO nếu Thầy Cô muốn tự tìm và sửa.";
                //var dialogResult = MessageBox.Show(ErrorCaution, @"Câu hỏi sai quy định", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (1 == 1) // dialogResult.Equals(DialogResult.Yes))
                {
                    document.Replace(new Regex(@"^Câu\d+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                    document.Replace(new Regex(@"^Câu\d+\s+"), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                    document.Replace(new Regex(@"^Câu\s{2,1000}\d+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                    document.Replace(new Regex(@"^Câu\s{2,1000}\d+\s+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                    document.Replace(new Regex(@"^Câu\s\d+\s+"), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                    document.Replace(new Regex(@"^Câu\s\d+\.{2,1000}"), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                    document.Replace(new Regex(@"^Câu\s\d+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                    document.Replace(new Regex(@"^Câu\s\d+\s+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
                }
                else
                {
                    string listError = "Lỗi ở các câu sau:\r\n";

                    if (document.FindAllPattern(new Regex(@"^Câu\d+\.")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\d+\.")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }
                    if (document.FindAllPattern(new Regex(@"^Câu\d+\s+")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\d+\s+")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }
                    if (document.FindAllPattern(new Regex(@"^Câu\s{2,1000}\d+\.")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\s{2,1000}\d+\.")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }
                    if (document.FindAllPattern(new Regex(@"^Câu\s{2,1000}\d+\s+\.")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\s{2,1000}\d+\s+\.")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }
                    if (document.FindAllPattern(new Regex(@"^Câu\s\d+\s+")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\s\d+\s+")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }
                    if (document.FindAllPattern(new Regex(@"^Câu\s\d+\.{2,1000}")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\s\d+\.{2,1000}")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }
                    if (document.FindAllPattern(new Regex(@"^Câu\s\d+\.\w")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\s\d+\.\w")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }
                    if (document.FindAllPattern(new Regex(@"^Câu\s\d+\s+\.")) != null)
                    {
                        foreach (TextSelection ques in document.FindAllPattern(new Regex(@"^Câu\s\d+\s+\.")))
                        {
                            TextRange range = new TextRange(document);
                            range = ques.GetAsOneRange();
                            listError += "- " + range.Text + "\r\n";
                        }
                    }


                    throw new Exception(listError);
                }
            }

            document.Replace(new Regex(@"^Câu\s+\d+\.\s+"), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
            document.Replace(new Regex(@"^Câu\s+\d+\:\s+"), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");

            document.Replace(new Regex(@"^Câu\s+\d+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
            document.Replace(new Regex(@"^Câu\s+\d+\:"), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");

            document.Replace(new Regex(@"^CAUXXX\.\s+"), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");

            try
            {
                selectionsQuestion = document.FindAllString(ConfigHelper.GetConfig("Question_Replace", "CAUXXX."), true, true);
                foreach (TextSelection ques in selectionsQuestion)
                {
                    string Cau_Number_Replace = Config.FormQuestion + " " + quesCount.ToString() + ".";
                    TextRange range = new TextRange(document);
                    range = ques.GetAsOneRange();
                    range.CharacterFormat.TextColor = Color.Black;
                    range.CharacterFormat.HighlightColor = Color.White;
                    range.Text = Cau_Number_Replace;
                    range.CharacterFormat.Bold = false;
                    range.CharacterFormat.Italic = false;
                    range.CharacterFormat.UnderlineStyle = UnderlineStyle.None;
                    quesCount++;
                }
            }
            catch (Exception ex_ReCountDocument)
            {
                document.Close();
                document.Dispose();
                document = null;
                GC.Collect();
                throw new Exception(ex_ReCountDocument.Message);
            }

            document.SaveToFile(filePath, FileFormat.Docx);

            document.Close();
            document.Dispose();
            document = null;
            GC.Collect();

            return quesCount;
        }
        #region Các hàm dùng dạng TRẮC NGHIỆM

        public void CloneDocument_MC(string filePath, string destFileOrg)
        {
            Document document = new Document();
            document.LoadFromFile(filePath);

            document.Replace(Config.OptionA, Environment.NewLine + Config.OptionA, true, true);
            document.Replace(Config.OptionB, Environment.NewLine + Config.OptionB, true, true);
            document.Replace(Config.OptionC, Environment.NewLine + Config.OptionC, true, true);
            document.Replace(Config.OptionD, Environment.NewLine + Config.OptionD, true, true);
            document.Replace("\t", "", true, true);
            document.Replace("\t", "", false, true);

            Document docTemp = new Document();
            Section sectionQuestion = docTemp.AddSection();
            int paragraphCount = 0;
            foreach (Section section in document.Sections)
            {
                foreach (Paragraph paragraph in section.Paragraphs)
                {
                    sectionQuestion.Paragraphs.Insert(paragraphCount, (Paragraph)paragraph.Clone());
                    paragraphCount++;
                }
            }

            docTemp.SaveToFile(destFileOrg, FileFormat.Docx);

            docTemp.Close();
            docTemp.Dispose();
            docTemp = null;

            document.Close();
            document.Dispose();
            document = null;
            GC.Collect();
        }

        public void CheckFormatException_MC(string destFileOrg, ref List<int> listQuesError, bool IsSkipError)
        {
            Document document = new Document();
            document.LoadFromFile(destFileOrg);

            try
            {
                TextSelection[] selectionsQuestion = document.FindAllPattern(new Regex(@"^Câu\s\d+\."));
                int tempCountIndex = 0;

                Dictionary<int, int> question_Index = new Dictionary<int, int>();
                for (int i = 0; i < selectionsQuestion.Length; i++)
                {
                    TextRange newrange = new TextRange(document);
                    newrange = selectionsQuestion[i].GetAsOneRange();
                    Paragraph paragraph = newrange.OwnerParagraph;
                    Body body = paragraph.OwnerTextBody;
                    int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                    if (paragraph.Text.StartsWith(Config.FormQuestion) && paragraph.Text.Length > 8)
                    {
                        question_Index.Add(tempCountIndex, body.ChildObjects.IndexOf(paragraph));
                        tempCountIndex++;
                    }
                }

                bool isError = false;
                string listError = "";
                for (int i = 0; i < question_Index.Count; i++)
                {
                    int benginQues_index = question_Index[i];
                    int endQues_index = benginQues_index + 1;
                    if (question_Index.Count == i + 1) endQues_index = document.Sections[0].Paragraphs.Count;
                    else endQues_index = question_Index[i + 1];

                    int countA = 0;
                    int countB = 0;
                    int countC = 0;
                    int countD = 0;
                    int indexA = 0;
                    int indexB = 0;
                    int indexC = 0;
                    int indexD = 0;
                    for (int j = benginQues_index; j < endQues_index; j++)
                    {
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^A\."))
                        {
                            countA++;
                            indexA = j;
                        }
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^B\."))
                        {
                            countB++;
                            indexB = j;
                        }
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^C\."))
                        {
                            countC++;
                            indexC = j;
                        }
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^D\."))
                        {
                            countD++;
                            indexD = j;
                        }
                    }

                    if (countA != 1)
                    {
                        isError = true;
                        if (countA < 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " thiếu/sai cú pháp đáp án A.";
                        if (countA > 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " có nhiều A.";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }
                    if (countB != 1)
                    {
                        isError = true;
                        if (countB < 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " thiếu/sai cú pháp đáp án B.";
                        if (countB > 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " có nhiều B.";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }
                    if (countC != 1)
                    {
                        isError = true;
                        if (countC < 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " thiếu/sai cú pháp đáp án C.";
                        if (countC > 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " có nhiều C.";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }
                    if (countD != 1)
                    {
                        isError = true;
                        if (countD < 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " thiếu/sai cú pháp đáp án D.";
                        if (countD > 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " có nhiều D.";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }

                    if ((indexB <= indexA) || (indexC <= indexB) || (indexD <= indexC))
                    {
                        isError = true;
                        listError = listError + "\r\n" + "- Câu " + (i + 1) + " sai thứ tự đáp án A/B/C/D.";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }
                }

                if (isError)
                {
                    //if (SessionInfo.IsAsk == false) AutoIgnore();

                    if (IsSkipError == false)
                    {
                        throw new Exception(ConfigHelper.GetConfig("Error6") + listError);
                    }
                }
            }
            catch (Exception ex_CheckFormatException)
            {
                document.Close();
                document = null;
                GC.Collect();
                throw new Exception(ex_CheckFormatException.Message);
            }

            //document.SaveToFile(destFileOrg, FileFormat.Docx);

            document.Close();
            document = null;
            GC.Collect();
        }

        public void CheckFormatDocument_MC(string destFileOrg, ref Dictionary<int, string> ar, bool IsSkipError)
        {
            Document document = new Document();
            document.LoadFromFile(destFileOrg);

            document.Replace("\t", "", false, true);
            document.Replace("\t", "", true, true);

            TextRange range = new TextRange(document);
            range.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
            range.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
            range.CharacterFormat.TextColor = Color.Black;
            range.CharacterFormat.HighlightColor = Color.White;

            bool isError = false;
            string listError = "";

            try
            {
                ar.Clear();
                TextSelection[] selectionsQuestion = document.FindAllPattern(new Regex(@"^Câu\s\d+\."));
                Dictionary<int, int> question_Index = new Dictionary<int, int>();

                int tempCountIndex = 0;
                for (int i = 0; i < selectionsQuestion.Length; i++)
                {
                    TextRange newrange = new TextRange(document);
                    newrange = selectionsQuestion[i].GetAsOneRange();
                    Paragraph paragraph = newrange.OwnerParagraph;
                    Body body = paragraph.OwnerTextBody;
                    int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                    if (paragraph.Text.StartsWith(Config.FormQuestion) && paragraph.Text.Length > 8)
                    {
                        question_Index.Add(tempCountIndex, body.ChildObjects.IndexOf(paragraph));
                        tempCountIndex++;
                    }
                }

                for (int i = 0; i < question_Index.Count; i++)
                {
                    int benginQues_index = question_Index[i];
                    int endQues_index = benginQues_index + 1;
                    if (question_Index.Count == i + 1) endQues_index = document.Sections[0].Paragraphs.Count;
                    else endQues_index = question_Index[i + 1];

                    ar[i] = " ";
                    for (int j = benginQues_index; j < endQues_index; j++)
                    {
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^A."))
                        {
                            DocumentObject o1 = document.Sections[0].Paragraphs[j].ChildObjects[0];
                            if (o1.DocumentObjectType == DocumentObjectType.TextRange)
                            {
                                TextRange textRang1 = o1 as TextRange;
                                string text1 = textRang1.Text.Replace(" ", "").Trim();
                                if (String.Compare(text1, Config.OptionA, false) == 0)
                                {
                                    if (textRang1.CharacterFormat.Font.Underline == true)
                                    {
                                        ar[i] = "A";
                                    }
                                    textRang1.CharacterFormat.Bold = false;
                                    textRang1.CharacterFormat.Italic = false;
                                    textRang1.CharacterFormat.SubSuperScript = SubSuperScript.None;
                                    textRang1.CharacterFormat.TextColor = Color.Black;
                                    textRang1.CharacterFormat.HighlightColor = Color.White;
                                    textRang1.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
                                    textRang1.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
                                }
                            }
                        }
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^B."))
                        {
                            DocumentObject o1 = document.Sections[0].Paragraphs[j].ChildObjects[0];
                            if (o1.DocumentObjectType == DocumentObjectType.TextRange)
                            {
                                TextRange textRang1 = o1 as TextRange;
                                string text1 = textRang1.Text.Replace(" ", "").Trim();
                                if (String.Compare(text1, Config.OptionB, false) == 0)
                                {
                                    if (textRang1.CharacterFormat.Font.Underline == true)
                                    {
                                        ar[i] = "B";
                                    }
                                    textRang1.CharacterFormat.Bold = false;
                                    textRang1.CharacterFormat.Italic = false;
                                    textRang1.CharacterFormat.SubSuperScript = SubSuperScript.None;
                                    textRang1.CharacterFormat.TextColor = Color.Black;
                                    textRang1.CharacterFormat.HighlightColor = Color.White;
                                    textRang1.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
                                    textRang1.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
                                }
                            }
                        }
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^C."))
                        {
                            DocumentObject o1 = document.Sections[0].Paragraphs[j].ChildObjects[0];
                            if (o1.DocumentObjectType == DocumentObjectType.TextRange)
                            {
                                TextRange textRang1 = o1 as TextRange;
                                string text1 = textRang1.Text.Replace(" ", "").Trim();
                                if (String.Compare(text1, Config.OptionC, false) == 0)
                                {
                                    if (textRang1.CharacterFormat.Font.Underline == true)
                                    {
                                        ar[i] = "C";
                                    }
                                    textRang1.CharacterFormat.Bold = false;
                                    textRang1.CharacterFormat.Italic = false;
                                    textRang1.CharacterFormat.SubSuperScript = SubSuperScript.None;
                                    textRang1.CharacterFormat.TextColor = Color.Black;
                                    textRang1.CharacterFormat.HighlightColor = Color.White;
                                    textRang1.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
                                    textRang1.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
                                }
                            }
                        }
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^D."))
                        {
                            DocumentObject o1 = document.Sections[0].Paragraphs[j].ChildObjects[0];
                            if (o1.DocumentObjectType == DocumentObjectType.TextRange)
                            {
                                TextRange textRang1 = o1 as TextRange;
                                string text1 = textRang1.Text.Replace(" ", "").Trim();
                                if (String.Compare(text1, Config.OptionD, false) == 0)
                                {
                                    if (textRang1.CharacterFormat.Font.Underline == true)
                                    {
                                        ar[i] = "D";
                                    }
                                    textRang1.CharacterFormat.Bold = false;
                                    textRang1.CharacterFormat.Italic = false;
                                    textRang1.CharacterFormat.SubSuperScript = SubSuperScript.None;
                                    textRang1.CharacterFormat.TextColor = Color.Black;
                                    textRang1.CharacterFormat.HighlightColor = Color.White;
                                    textRang1.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
                                    textRang1.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
                                }
                            }
                        }

                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^Lời giải"))
                        {
                            document.Sections[0].Paragraphs[j].Format.LeftIndent = 0;
                            document.Sections[0].Paragraphs[j].Format.FirstLineIndent = 0;
                            document.Sections[0].Paragraphs[j].Format.BeforeAutoSpacing = false;
                            document.Sections[0].Paragraphs[j].Format.AfterAutoSpacing = false;
                            document.Sections[0].Paragraphs[j].Format.AfterSpacing = 0;
                            document.Sections[0].Paragraphs[j].Format.BeforeSpacing = 0;
                            document.Sections[0].Paragraphs[j].Format.Tabs.Clear();

                            DocumentObject o1 = document.Sections[0].Paragraphs[j].ChildObjects[0];
                            if (o1.DocumentObjectType == DocumentObjectType.TextRange)
                            {
                                TextRange textRang1 = o1 as TextRange;
                                textRang1.CharacterFormat.UnderlineStyle = UnderlineStyle.None;
                                textRang1.CharacterFormat.Bold = false;
                                textRang1.CharacterFormat.Italic = false;
                                textRang1.CharacterFormat.SubSuperScript = SubSuperScript.None;
                                textRang1.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
                                textRang1.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
                            }
                        }

                        foreach (DocumentObject oChild in document.Sections[0].Paragraphs[j].ChildObjects)
                        {
                            if (oChild.DocumentObjectType == DocumentObjectType.TextRange)
                            {
                                TextRange textRang1 = oChild as TextRange;
                                string text1 = format_HTML(textRang1.Text);
                                textRang1.Text = text1;
                            }
                        }
                    }

                    if (ar[i] == " ")
                    {
                        int numberErr = i + 1;
                        listError = listError + "\r\n" + "- Câu " + numberErr.ToString() + ".";
                        isError = true;
                    }
                }

                if (isError)
                {

                    if (IsSkipError == false)
                    {
                        throw new Exception(ConfigHelper.GetConfig("Error2") + listError);
                    }
                }

            }
            catch (Exception ex_FormatDocument)
            {
                document.Close();
                document = null;
                GC.Collect();

                throw new Exception(ex_FormatDocument.Message);
            }

            document.SaveToFile(destFileOrg, FileFormat.Docx);

            document.Close();
            document = null;
            GC.Collect();
        }

        public void RepairChemicalFormula_MC(string filePath)
        {
            Document document = new Document();
            document.LoadFromFile(filePath);

            string openMathML = "<math";
            string closeMathML = "</math>";

            foreach (Section section in document.Sections)
            {
                foreach (Paragraph para in section.Paragraphs)
                {
                    StringBuilder sb = new StringBuilder();
                    bool isPicture = false;
                    bool isInMathMLCode = false;
                    for (int i = 0; i < para.ChildObjects.Count; i++)
                    {
                        DocumentObject o1 = para.ChildObjects[i];
                        string textReplace = "";

                        if (o1.DocumentObjectType == DocumentObjectType.TextRange)
                        {
                            TextRange textRang1 = o1 as TextRange;
                            textReplace = textRang1.Text;

                            if (textRang1.Text.Contains(openMathML)) isInMathMLCode = true;
                            if (textRang1.Text.Contains(closeMathML)) isInMathMLCode = false;

                            if (isInMathMLCode == false)
                            {
                                if (textRang1.CharacterFormat.SubSuperScript == SubSuperScript.SuperScript)
                                {
                                    string tempReplace = "<sup>" + textRang1.Text + "</sup>";
                                    textReplace = textReplace.Replace(textRang1.Text, tempReplace);
                                }
                                if (textRang1.CharacterFormat.SubSuperScript == SubSuperScript.SubScript)
                                {
                                    string tempReplace = "<sub>" + textRang1.Text + "</sub>";
                                    textReplace = textReplace.Replace(textRang1.Text, tempReplace);
                                }

                                if (textRang1.CharacterFormat.Font.Bold == true && !String.IsNullOrWhiteSpace(textRang1.Text))
                                {
                                    if (textRang1.Text.Contains(Config.OptionA) || textRang1.Text.Contains(Config.OptionB) ||
                                        textRang1.Text.Contains(Config.OptionC) || textRang1.Text.Contains(Config.OptionD)) textReplace = textRang1.Text;
                                    else textReplace = textReplace.Replace(textRang1.Text, "<b>" + textRang1.Text + "</b>");

                                }

                                if (textRang1.CharacterFormat.Font.Italic == true && !String.IsNullOrWhiteSpace(textRang1.Text))
                                {
                                    if (textRang1.Text.Contains(Config.OptionA) || textRang1.Text.Contains(Config.OptionB) ||
                                        textRang1.Text.Contains(Config.OptionC) || textRang1.Text.Contains(Config.OptionD)) textReplace = textRang1.Text;
                                    else textReplace = textReplace.Replace(textRang1.Text, "<i>" + textRang1.Text + "</i>");
                                }

                                if (textRang1.CharacterFormat.Font.Underline == true && !String.IsNullOrWhiteSpace(textRang1.Text))
                                {
                                    if (textRang1.Text.Contains(Config.OptionA) || textRang1.Text.Contains(Config.OptionB) ||
                                        textRang1.Text.Contains(Config.OptionC) || textRang1.Text.Contains(Config.OptionD)) textReplace = textRang1.Text;
                                    else textReplace = textReplace.Replace(textRang1.Text, "<u>" + textRang1.Text + "</u>");
                                }
                            }

                        }
                        if (o1.DocumentObjectType == DocumentObjectType.OfficeMath)
                        {
                            OfficeMath oMath1 = o1 as OfficeMath;
                            textReplace = oMath1.ToMathMLCode();
                            textReplace = textReplace.Replace(" xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                            textReplace = textReplace.Replace("xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                        }
                        if (o1.DocumentObjectType == DocumentObjectType.Picture) isPicture = true;

                        sb.Append(textReplace);
                    }

                    if (!isPicture)
                    {
                        para.Text = format_Text(sb.ToString());
                        for (int i = 0; i < para.ChildObjects.Count; i++)
                        {
                            TextRange textRang2 = (TextRange)para.ChildObjects[i];
                            textRang2.CharacterFormat.HighlightColor = Color.White;
                            textRang2.CharacterFormat.TextColor = Color.Black;
                            textRang2.CharacterFormat.Bold = false;
                            textRang2.CharacterFormat.Italic = false;
                            textRang2.CharacterFormat.UnderlineStyle = UnderlineStyle.None;
                        }
                    }
                }
            }

            document.Replace(new Regex(@"^Câu\s\d+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");

            document.Replace(new Regex(@"<b>{{MathTypeError}}</b>"), "{{MathTypeError}}");

            document.SaveToFile(filePath, FileFormat.Docx);
            document.Close();
            document = null;
            GC.Collect();
        }
        public void ReplaceFromQuestion(string filePath)
        {
            Document document = new Document();
            document.LoadFromFile(filePath);

            document.Replace(new Regex(@"^(\s*|\t*)Câu\s\d+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");
            document.Replace(new Regex(@"^(\s*|\t*)Lời giải"), ConfigHelper.GetConfig("Question_Answer", "LOIGIAI"));
            //document.Replace(new Regex(@"^Lời giải"), ConfigHelper.GetConfig("Question_Answer", "LOIGIAI"));
            //@"^Lời giải"
            document.SaveToFile(filePath, FileFormat.Docx);
            document.Close();
            document.Dispose();
            document = null;
            GC.Collect();
        }

        #endregion

        #region Các hàm dùng dạng TỰ LUẬN

        public void CloneDocument_Essay(string filePath, string destFileOrg)
        {
            Document document = new Document();
            document.LoadFromFile(filePath);

            Document docTemp = new Document();
            Section sectionQuestion = docTemp.AddSection();
            int paragraphCount = 0;
            foreach (Section section in document.Sections)
            {
                foreach (Paragraph paragraph in section.Paragraphs)
                {
                    sectionQuestion.Paragraphs.Insert(paragraphCount, (Paragraph)paragraph.Clone());
                    paragraphCount++;
                }
            }

            docTemp.SaveToFile(destFileOrg, FileFormat.Docx);

            docTemp.Close();
            docTemp = null;

            document = null;
            GC.Collect();
        }

        public void CheckFormatException_Essay(string destFileOrg, ref List<int> listQuesError, bool IsSkipError)
        {
            Document document = new Document();
            document.LoadFromFile(destFileOrg);

            try
            {
                TextSelection[] selectionsQuestion = document.FindAllPattern(new Regex(@"^Câu\s\d+\."));
                int tempCountIndex = 0;

                Dictionary<int, int> question_Index = new Dictionary<int, int>();
                for (int i = 0; i < selectionsQuestion.Length; i++)
                {
                    TextRange newrange = new TextRange(document);
                    newrange = selectionsQuestion[i].GetAsOneRange();
                    Paragraph paragraph = newrange.OwnerParagraph;
                    Body body = paragraph.OwnerTextBody;
                    int newrange_index = newrange.OwnerParagraph.ChildObjects.IndexOf(newrange);
                    if (paragraph.Text.StartsWith(Config.FormQuestion) && paragraph.Text.Length > 8)
                    {
                        question_Index.Add(tempCountIndex, body.ChildObjects.IndexOf(paragraph));
                        tempCountIndex++;
                    }
                }

                bool isError = false;
                string listError = "";
                for (int i = 0; i < question_Index.Count; i++)
                {
                    int benginQues_index = question_Index[i];
                    int endQues_index = benginQues_index + 1;
                    if (question_Index.Count == i + 1) endQues_index = document.Sections[0].Paragraphs.Count;
                    else endQues_index = question_Index[i + 1];

                    int countHDG = 0;
                    int countDS = 0;

                    int indexHDG = 0;
                    int indexDS = 0;

                    for (int j = benginQues_index; j < endQues_index; j++)
                    {
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^Hướng dẫn giải\:"))
                        {
                            countHDG++;
                            indexHDG = j;
                        }
                        if (Regex.IsMatch(document.Sections[0].Paragraphs[j].Text, @"^Đáp số\:"))
                        {
                            countDS++;
                            indexDS = j;
                        }
                    }

                    if (countHDG != 1)
                    {
                        isError = true;
                        if (countHDG < 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " thiếu/sai cú pháp \"Hướng dẫn giải\"";
                        if (countHDG > 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " có nhiều \"Hướng dẫn giải\"";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }
                    if (countDS != 1)
                    {
                        isError = true;
                        if (countDS < 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " thiếu/sai cú pháp \"Đáp số\"";
                        if (countDS > 1) listError = listError + "\r\n" + "- Câu " + (i + 1) + " có nhiều \"Đáp số\"";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }

                    if (indexDS <= indexHDG)
                    {
                        isError = true;
                        listError = listError + "\r\n" + "- Câu " + (i + 1) + " sai thứ tự \"Hướng dẫn giải\" -> \"Đáp số\"";
                        if (!listQuesError.Contains(benginQues_index)) listQuesError.Add(benginQues_index);
                    }
                }

                if (isError)
                {

                    if (IsSkipError == false)
                    {
                        throw new Exception(ConfigHelper.GetConfig("Error6") + listError);
                    }
                }
            }
            catch (Exception ex_CheckFormatException)
            {
                document.Close();
                document = null;
                GC.Collect();
                throw new Exception(ex_CheckFormatException.Message);
            }

            //document.SaveToFile(destFileOrg, FileFormat.Docx);

            document.Close();
            document = null;
            GC.Collect();
        }

        public void CheckFormatDocument_Essay(string destFileOrg)
        {
            Document document = new Document();
            document.LoadFromFile(destFileOrg);

            document.Replace("\t", "", false, true);
            document.Replace("\t", "", true, true);

            TextRange range = new TextRange(document);
            range.CharacterFormat.TextColor = Color.Black;
            range.CharacterFormat.HighlightColor = Color.White;

            TextSelection[] answerHDG = document.FindAllPattern(new Regex(@"^Hướng dẫn giải\:"));
            for (int i = 0; i < answerHDG.Length; i++)
            {
                TextRange newrange = new TextRange(document);
                newrange = answerHDG[i].GetAsOneRange();

                newrange.CharacterFormat.Bold = false;
                newrange.CharacterFormat.Italic = false;
                newrange.CharacterFormat.SubSuperScript = SubSuperScript.None;
                newrange.CharacterFormat.TextColor = Color.Black;
                newrange.CharacterFormat.HighlightColor = Color.White;
                newrange.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
                newrange.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
            }
            TextSelection[] answerDS = document.FindAllPattern(new Regex(@"^Đáp số\:"));
            for (int i = 0; i < answerDS.Length; i++)
            {
                TextRange newrange = new TextRange(document);
                newrange = answerDS[i].GetAsOneRange();

                newrange.CharacterFormat.Bold = false;
                newrange.CharacterFormat.Italic = false;
                newrange.CharacterFormat.SubSuperScript = SubSuperScript.None;
                newrange.CharacterFormat.TextColor = Color.Black;
                newrange.CharacterFormat.HighlightColor = Color.White;
                newrange.CharacterFormat.FontName = ConfigHelper.GetConfig("FontName", "");
                newrange.CharacterFormat.FontSize = float.Parse(ConfigHelper.GetConfig("FontSize", ""));
            }

            document.SaveToFile(destFileOrg, FileFormat.Docx);

            document.Close();
            document = null;
            GC.Collect();
        }

        public void RepairChemicalFormula_Essay(string filePath)
        {
            Document document = new Document();
            document.LoadFromFile(filePath);

            string openMathML = "<math";
            string closeMathML = "</math>";

            foreach (Section section in document.Sections)
            {
                foreach (Paragraph para in section.Paragraphs)
                {
                    StringBuilder sb = new StringBuilder();
                    bool isPicture = false;
                    bool isInMathMLCode = false;
                    for (int i = 0; i < para.ChildObjects.Count; i++)
                    {
                        DocumentObject o1 = para.ChildObjects[i];
                        string textReplace = "";

                        if (o1.DocumentObjectType == DocumentObjectType.TextRange)
                        {
                            TextRange textRang1 = o1 as TextRange;
                            textReplace = textRang1.Text;

                            if (textRang1.Text.Contains(openMathML)) isInMathMLCode = true;
                            if (textRang1.Text.Contains(closeMathML)) isInMathMLCode = false;

                            if (isInMathMLCode == false)
                            {
                                if (textRang1.CharacterFormat.SubSuperScript == SubSuperScript.SuperScript)
                                {
                                    string tempReplace = "<sup>" + textRang1.Text + "</sup>";
                                    textReplace = textReplace.Replace(textRang1.Text, tempReplace);
                                }
                                if (textRang1.CharacterFormat.SubSuperScript == SubSuperScript.SubScript)
                                {
                                    string tempReplace = "<sub>" + textRang1.Text + "</sub>";
                                    textReplace = textReplace.Replace(textRang1.Text, tempReplace);
                                }

                                if (textRang1.CharacterFormat.Font.Bold == true && !String.IsNullOrWhiteSpace(textRang1.Text))
                                {
                                    if (textRang1.Text.Contains("Hướng dẫn giải:") || textRang1.Text.Contains("Đáp số:"))
                                        textReplace = textRang1.Text;
                                    else textReplace = textReplace.Replace(textRang1.Text, "<b>" + textRang1.Text + "</b>");
                                }

                                if (textRang1.CharacterFormat.Font.Italic == true && !String.IsNullOrWhiteSpace(textRang1.Text))
                                {
                                    if (textRang1.Text.Contains("Hướng dẫn giải:") || textRang1.Text.Contains("Đáp số:"))
                                        textReplace = textRang1.Text;
                                    else textReplace = textReplace.Replace(textRang1.Text, "<i>" + textRang1.Text + "</i>");
                                }

                                if (textRang1.CharacterFormat.Font.Underline == true && !String.IsNullOrWhiteSpace(textRang1.Text))
                                {
                                    if (textRang1.Text.Contains("Hướng dẫn giải:") || textRang1.Text.Contains("Đáp số:"))
                                        textReplace = textRang1.Text;
                                    else textReplace = textReplace.Replace(textRang1.Text, "<u>" + textRang1.Text + "</u>");
                                }
                            }

                        }
                        if (o1.DocumentObjectType == DocumentObjectType.OfficeMath)
                        {
                            OfficeMath oMath1 = o1 as OfficeMath;
                            textReplace = oMath1.ToMathMLCode();
                            textReplace = textReplace.Replace(" xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                            textReplace = textReplace.Replace("xmlns:m=\"http://schemas. openxmlformats. org/officeDocument/2006/math\"", "");
                        }
                        if (o1.DocumentObjectType == DocumentObjectType.Picture) isPicture = true;

                        sb.Append(textReplace);
                    }

                    if (!isPicture)
                    {
                        para.Text = format_Text(sb.ToString());
                        for (int i = 0; i < para.ChildObjects.Count; i++)
                        {
                            TextRange textRang2 = (TextRange)para.ChildObjects[i];
                            textRang2.CharacterFormat.HighlightColor = Color.White;
                            textRang2.CharacterFormat.TextColor = Color.Black;
                            textRang2.CharacterFormat.Bold = false;
                            textRang2.CharacterFormat.Italic = false;
                            textRang2.CharacterFormat.UnderlineStyle = UnderlineStyle.None;
                        }
                    }
                }
            }

            document.Replace(new Regex(@"^Câu\s\d+\."), ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ");

            document.Replace(new Regex(@"<b>{{MathTypeError}}</b>"), "{{MathTypeError}}");

            document.SaveToFile(filePath, FileFormat.Docx);
            document.Close();
            document = null;
            GC.Collect();
        }

        #endregion



        #endregion

        #region Question Using Table Template

        protected List<Question> BatchProcess(QuestionBatch quesionBatch, string fileName, ref string log)
        {
            long endProcess = 0;
            long startProcess = DateHelper.CurrentMilisecond();
            /**
             * Lấy toàn bộ những lô chưa được xử lý, Cập nhật lại trạng thái là đang xử lý
             * Tạo mới câu hỏi với nội dung từ lô đã chọn, trả về ID để làm folder
             * Lấy thông tin từ câu hỏi trong word để update về DB
             * Next đến câu hỏi kế tiếp
             */
            int countQuestion = 0;

            string uploadURL = ConfigHelper.GetConfig("QuestionImageURL", "");
            string uploadFolder = ConfigHelper.GetConfig("QuestionFilePath", "");
            string inputType = ConfigHelper.GetConfig("InputType", "i");
            string randomQuestion = new Random().Next(000001, 999999).ToString();
            List<Question> qs = new List<Question>();
            try
            {
                //create a question folder
                string file = fileName;
                Document doc = new Document();
                doc.LoadFromFile(file);
                SectionCollection sections = doc.Sections;

                foreach (Spire.Doc.Section sec in sections)
                {
                    TableCollection tables = sec.Tables;
                    int questionIndex = 0;
                    foreach (Spire.Doc.Table tbl in tables)
                    {
                        Dictionary<string, string> jO = new Dictionary<string, string>();
                        string questionId = DateTime.Now.ToString("yyyymmdd") + randomQuestion + questionIndex.ToString() + new Random().Next(111111, 999999);
                        string folder = uploadFolder + Path.DirectorySeparatorChar + questionId;
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {
                            //0=câu hỏi, 1=DA1, 2=DA2, 3=DA3, 4=DA4, 5=DA đúng, 6=Chi tiết trả lời
                            string type = "";
                            switch (i)
                            {
                                case 0:
                                    type = "q";//câu hỏi
                                    break;
                                case 1:
                                    type = "a1";//đáp án 1
                                    break;
                                case 2:
                                    type = "a2";//đáp án 2
                                    break;
                                case 3:
                                    type = "a3";//đáp án 3
                                    break;
                                case 4:
                                    type = "a4";//đáp án 4
                                    break;
                                case 5:
                                    type = "ar";//câu trả lời đúng
                                    break;
                                case 6:
                                    type = "a";//trả lời chi tiết
                                    break;
                            }
                            if (string.IsNullOrEmpty(type))
                            {
                                continue;
                            }
                            string newFileName = questionId + type;


                            string docxFileName = folder + Path.DirectorySeparatorChar + newFileName + ".docx";
                            string rftFileName = folder + Path.DirectorySeparatorChar + newFileName + ".rtf";
                            string imageFileName = folder + Path.DirectorySeparatorChar + newFileName + ".JPG";

                            Document docTemp = new Document();
                            Spire.Doc.Section section = docTemp.AddSection();
                            Spire.Doc.Documents.Paragraph p = section.AddParagraph();
                            string imagePath = "";
                            if (inputType == "i")
                            {
                                Document docTemp1 = new Document();
                                Spire.Doc.Section section1 = docTemp1.AddSection();
                                int secIndex = 0;
                                foreach (Spire.Doc.Documents.Paragraph p1 in tbl.Rows[i].Cells[1].Paragraphs)
                                {
                                    if (p1 == null) continue;
                                    if (type != "ar")
                                    {
                                        try
                                        {
                                            section1.Paragraphs.Insert(secIndex, (Spire.Doc.Documents.Paragraph)p1.Clone());
                                            secIndex++;
                                        }
                                        catch (Exception exx)
                                        {
                                            #region catch
                                            //CLogger.WriteInfo(exx.Message);


                                            //#region nhieu child qua ko clone duoc, can chuyen thanh hinh va dua vao pararaph chinh

                                            //foreach (DocumentObject docObject in p1.ChildObjects)
                                            //{
                                            //    if (docObject.DocumentObjectType == DocumentObjectType.TextRange)
                                            //    {
                                            //        p.ChildObjects.Add(docObject.Clone());
                                            //    }
                                            //    else
                                            //    {
                                            //        //chi xuat cong thuc hoac hinh anh
                                            //        if (docObject.DocumentObjectType == DocumentObjectType.Picture ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.XmlParaItem ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.OleObject ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Body ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Break ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.CheckBox ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.ControlField ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.CustomXml ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Document ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.DropDownFormField ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.EmbededField ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Field ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.FieldMark ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.MergeField ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.SDTBlockContent ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.SDTCellContent ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.SDTInlineContent ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.SDTRowContent ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Section ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.SeqField ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Shape ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.ShapeGroup ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.ShapeLine ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.ShapePath ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.ShapeRect ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.SmartTag ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTag ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTagCell ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTagInline ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTagRow ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Symbol ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Table ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.TableCell ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.TableRow ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.TextBox ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.TextFormField ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.TOC ||
                                            //            docObject.DocumentObjectType == DocumentObjectType.Undefined)
                                            //        {
                                            //            imagePath = SaveToImage(docObject, folder, questionId);
                                            //            if (!string.IsNullOrEmpty(imagePath))
                                            //            {
                                            //                p.AppendText(string.Format("<img class=\"equation-image\" src=\"{0}/{1}\">", uploadURL, imagePath));
                                            //            }
                                            //        }
                                            //    }

                                            //}

                                            //p.AppendText("<br/>");

                                            //#endregion nhieu child qua ko clone duoc
                                            #endregion catch

                                        }

                                    }
                                    else
                                    {
                                        p.AppendText(p1.Text);
                                    }
                                }
                                if (type != "ar")
                                {
                                    imagePath = SaveToImage(section1, folder, questionId, uploadURL);
                                    //imagePath = SaveToImageV3(section1, folder, questionId, uploadURL);
                                    if (!string.IsNullOrEmpty(imagePath))
                                    {
                                        p.AppendText(" ");
                                        p.AppendText(imagePath);
                                        p.AppendText(" ");
                                    }
                                }
                            }
                            jO.Add(type, p.Text);
                            docTemp.Close();
                            docTemp = null;
                            GC.Collect();

                        }
                        string docFile = "";
                        try
                        {
                            docFile = SaveDocument(tbl.Clone(), folder, questionId);
                        }
                        catch
                        {
                            CLogger.WriteInfo(string.Format("SaveDocument(tbl.Clone(), folder = {0}, questionId = {1});", folder, questionId));
                        }
                        //insert into question table
                        Question q = new Question();

                        q.chapter_id = quesionBatch.chapter_id;
                        q.resource_id = quesionBatch.resource_id;
                        q.content = jO["q"];
                        q.short_explain = jO["a"];
                        q.full_explain = jO["a"];
                        q.type = quesionBatch.type;
                        q.level = quesionBatch.level;
                        q.question_file = questionId;
                        q.full_question_file = docFile;
                        q.course_id = quesionBatch.course_id;
                        q.subject_id = quesionBatch.subject_id;
                        q.a1 = jO["a1"];
                        q.a2 = jO["a2"];
                        q.a3 = jO["a3"];
                        q.a4 = jO["a4"];
                        q.ar = jO["ar"];
                        q.class_id = quesionBatch.class_id;
                        q.created_by = quesionBatch.created_by;
                        // q.ba = quesionBatch.chapter_id;
                        q.chapter_id = quesionBatch.chapter_id;
                        q.modified_by = questionId;

                        qs.Add(q);
                        countQuestion++;
                        questionIndex++;

                    }

                }
                endProcess = DateHelper.CurrentMilisecond();
                log = "Thành công > time: " + (endProcess - startProcess).ToString() + "ms";
                return qs;
            }
            catch (Exception exx)
            {
                CLogger.WriteLogException(exx.Message);
                endProcess = DateHelper.CurrentMilisecond();
                log = "Thất bại time: " + (endProcess - startProcess).ToString() + "ms";
                return null;
            }
        }
        public string SaveToImage(DocumentObject docObject, string folder, string questionId)
        {
            if (docObject == null) return string.Empty;
            try
            {


                string objectId = questionId + new Random().Next(111111, 999999);
                string fileName = "obj" + objectId;
                //string docxFileName = folder + Path.DirectorySeparatorChar + fileName + ".docx";
                //string rftFileName = folder + Path.DirectorySeparatorChar + fileName + ".rtf";
                //string imageFileName = folder + Path.DirectorySeparatorChar + fileName + ".JPG";
                //string imageFileName1 = folder + Path.DirectorySeparatorChar + fileName + "1.JPG";
                string htmlFile = folder + Path.DirectorySeparatorChar + fileName + ".html";
                Document docTemp = new Document();
                Spire.Doc.Section section = docTemp.AddSection();
                Spire.Doc.Documents.Paragraph p = section.AddParagraph();
                p.ChildObjects.Add(docObject.Clone());

                //docTemp.SaveToFile(docxFileName, FileFormat.Docx);//save docx
                //ExportWordToRtf(docxFileName, rftFileName);//save rtf
                //System.Drawing.Image img = docTemp.SaveToImages(0, ImageType.Metafile);
                //img.Save(imageFileName1, ImageFormat.Png);
                docTemp.SaveToFile(htmlFile, FileFormat.Html);

                //rtbQuestion.Width = Convert.ToInt32(tbl.Rows[i].Cells[1].Width);//load & save to image
                //rtbQuestion.LoadFile(rftFileName, RichTextBoxStreamType.RichText);
                //Bitmap bmp = RtbToBitmap(rtbQuestion, rtbQuestion.Width, rtbQuestion.Height);
                //bmp.Save(imageFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                //bmp.Dispose();
                docTemp.Close();
                docTemp = null;
                GC.Collect();
                string html = System.IO.File.ReadAllText(htmlFile);
                //
                return questionId + "/" + fileName + ".JPG";
            }
            catch
            {
                return string.Empty;
            }
        }
        public void SaveAs(string destFileConvert, Word.WdSaveFormat format, string ext)
        {
            Word.Application word = new Word.Application();
            object miss = System.Reflection.Missing.Value;
            object path = destFileConvert;
            object readOnly = false;
            //object Revert = false;
            //object openAndRepair = false;
            object Visible = false;
            Word.Document doc = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss,
                ref miss, ref miss, ref Visible, ref miss, ref miss, ref miss, ref miss);
            doc.Fields.Update(); // ** this is the new line of code.

            doc.WebOptions.Encoding = Microsoft.Office.Core.MsoEncoding.msoEncodingUTF8;
            doc.SaveAs2(destFileConvert.Replace(".docx", "." + ext), format);

            doc.Close();
            doc = null;

            word.Quit();
            word = null;
            GC.Collect();
        }
        public string SaveToHtml(Spire.Doc.Section sInput, string folder, string questionId, string url, string questionPart, out string newFileName)
        {
            newFileName = "";
            if (sInput == null)
            {
                return string.Empty;
            }
            try
            {
                string objectId = questionId + new Random().Next(111111, 999999);
                string fileName = "obj" + objectId + "_" + questionPart;

                newFileName = fileName;

                string docxFile = folder + Path.DirectorySeparatorChar + fileName + ".docx";
                string htmlFile = folder + Path.DirectorySeparatorChar + fileName + ".html";
                string styleFile = folder + Path.DirectorySeparatorChar + fileName + "_styles.css";
                //obj2017272434293937148497_styles.css
                Document docTemp = new Document();
                docTemp.Sections.Insert(0, sInput.Clone());

                //save docx for other process create html png image, latex
                docTemp.SaveToFile(docxFile, FileFormat.Docx);

                //save xml
                //string xmlFile = folder + Path.DirectorySeparatorChar + fileName + ".xml";
                //docTemp.SaveToFile(xmlFile, FileFormat.Xml);

                //save html with base64Image
                docTemp.HtmlExportOptions.ImageEmbedded = false;
                docTemp.SaveToFile(htmlFile, FileFormat.Html);
                GC.Collect();
                string html = System.IO.File.ReadAllText(htmlFile);
                //get only html
                //html = html.Substring(html.IndexOf("<body>") + 6);
                html = html.Substring(html.IndexOf("<body"));
                html = html.Substring(html.IndexOf(">") + 1);

                html = html.Substring(0, html.IndexOf("</body>"));
                html = html.Replace("> </span>", ">&nbsp;</span>");
                //html = html.Replace("<img src=\"", "<img class=\"formula-image\" src=\"");
                html = html.Replace("<img src=\"", "<img class=\"formula-image\" src=\"" + url + "/" + questionId + "/");
                html = SanitizeHtml(html);
                return html.Trim();
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("SaveToImage: -->" + ex.Message);
                return string.Empty;
            }
        }
        public string SaveToImage(Spire.Doc.Section sInput, string folder, string questionId, string url)
        {
            if (sInput == null) return string.Empty;
            try
            {
                string objectId = questionId + new Random().Next(111111, 999999);
                string fileName = "obj" + objectId;
                string htmlFile = folder + Path.DirectorySeparatorChar + fileName + ".html";
                string styleFile = folder + Path.DirectorySeparatorChar + fileName + "_styles.css";
                //obj2017272434293937148497_styles.css
                Document docTemp = new Document();
                docTemp.Sections.Insert(0, sInput.Clone());
                docTemp.HtmlExportOptions.ImageEmbedded = true;
                docTemp.SaveToFile(htmlFile, FileFormat.Html);
                GC.Collect();
                string html = System.IO.File.ReadAllText(htmlFile);
                //get only html
                //html = html.Substring(html.IndexOf("<body>") + 6);
                html = html.Substring(html.IndexOf("<body"));
                html = html.Substring(html.IndexOf(">") + 1);

                html = html.Substring(0, html.IndexOf("</body>"));
                html = html.Replace("> </span>", ">&nbsp;</span>");
                //html = html.Replace("<img src=\"", "<img class=\"equation-image\" src=\"" + url + "/" + questionId + "/");
                html = SanitizeHtml(html);
                try
                {
                    System.IO.File.Delete(htmlFile);

                }
                catch (Exception e1)
                {
                    CLogger.WriteLogException("delete: " + htmlFile + "-->" + e1.Message);
                }
                try
                {
                    System.IO.File.Delete(styleFile);

                }
                catch (Exception e2)
                {
                    CLogger.WriteLogException("delete: " + styleFile + "-->" + e2.Message);
                }
                return html;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("SaveToImage: -->" + ex.Message);
                return string.Empty;
            }
        }
        public bool AppendDocument(ref Document doc, Spire.Doc.Table tblInput)
        {
            if (tblInput == null) return false;
            try
            {
                Spire.Doc.Section section = doc.AddSection();
                section.Tables.Insert(0, (Spire.Doc.Table)tblInput.Clone());
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public string SaveDocument(Spire.Doc.Table tblInput, string folder, string questionId)
        {
            if (tblInput == null) return string.Empty;
            try
            {
                string fileName = questionId + ".docx";
                string docxFileName = folder + Path.DirectorySeparatorChar + fileName;

                Document docTemp = new Document();
                Spire.Doc.Section section = docTemp.AddSection();
                section.Tables.Insert(0, (Spire.Doc.Table)tblInput.Clone());

                docTemp.SaveToFile(docxFileName, FileFormat.Docx);

                docTemp.Close();
                docTemp = null;
                GC.Collect();
                return fileName;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// save all doc to image, not use html
        /// </summary>
        /// <param name="sInput"></param>
        /// <param name="folder"></param>
        /// <param name="questionId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public string SaveToImageV3(Spire.Doc.Section sInput, string folder, string questionId, string url)
        {
            if (sInput == null) return string.Empty;
            try
            {
                string objectId = questionId + new Random().Next(111111, 999999);
                string fileName = "obj" + objectId;
                string file = folder + Path.DirectorySeparatorChar + fileName;

                //obj2017272434293937148497_styles.css
                Document docTemp = new Document();
                docTemp.Sections.Insert(0, sInput.Clone());

                //Save doc file.
                System.Drawing.Image img = docTemp.SaveToImages(0, ImageType.Metafile);
                img.Save(file + ".jpeg", ImageFormat.Jpeg);

                string html = "<img class=\"equation-image\" src=\"" + url + questionId + Path.DirectorySeparatorChar + fileName + ".jpeg\" />";

                return html;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("SaveToImage: -->" + ex.Message);
                return string.Empty;
            }
        }
        public string SpireDoc2Html(string file)
        {

            try
            {
                Document doc = new Document();
                doc.LoadFromFile(file);
                string htmlFile = file.Replace(".docx", ".html");
                doc.HtmlExportOptions.ImageEmbedded = false;
                doc.SaveToFile(htmlFile, FileFormat.Html);
                return htmlFile;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("SaveToImage: -->" + ex.Message);
                return string.Empty;
            }
        }
        #endregion

        #region QuestionBatchV1

        public List<QuestionV1> LoadBatchFile(string uploadFileFullPathBatch, string uploadFolder, string uploadURL, string batchId, out string returnMessage)
        {
            List<QuestionV1> questions = new List<QuestionV1>();
            string functionName = "LoadBatchFile";
            returnMessage = "";
            long startProcess = DateHelper.CurrentMilisecond();
            long endProcess = 0;
            try
            {

                //string uploadFolder = Path.GetDirectoryName(uploadFileFullPath);

                string folder = uploadFolder + Path.DirectorySeparatorChar + batchId;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                //create a question folder
                Document doc = new Document();
                doc.LoadFromFile(uploadFileFullPathBatch);
                for (int i = doc.Bookmarks.Count - 1; i >= 0; i--)
                {
                    doc.Bookmarks.RemoveAt(i);
                }

                QuestionV1 question = new QuestionV1();

                string rawQuestionText = "";
                //Document docQuestion = new Document();
                int indexQuestion = 0;
                //int secIndex = 0;
                //Spire.Doc.Section sectionQuestion = docQuestion.AddSection();

                SectionCollection sections = doc.Sections;
                foreach (Spire.Doc.Section sec in sections)
                {
                    ParagraphCollection ps = sec.Paragraphs;
                    int questionIndex = 0;
                    Dictionary<string, string> jO = new Dictionary<string, string>();
                    foreach (Paragraph p in ps)
                    {
                        Document docTemp = new Document();
                        Spire.Doc.Section section = docTemp.AddSection();
                        Spire.Doc.Documents.Paragraph p1 = section.AddParagraph();

                        Spire.Doc.Documents.Paragraph pTemp = (Spire.Doc.Documents.Paragraph)p.Clone();
                        //sectionQuestion.Paragraphs.Add(pTemp);
                        //secIndex++;
                        //create document 
                        foreach (DocumentObject docObject in p.ChildObjects)
                        {
                            if (docObject.DocumentObjectType == DocumentObjectType.TextRange)
                            {
                                Spire.Doc.Fields.TextRange txtRange = (Spire.Doc.Fields.TextRange)docObject;
                                Color c = txtRange.CharacterFormat.HighlightColor;
                                Color c1 = txtRange.CharacterFormat.TextColor;

                                string aPT = @"(\r\n)*\s*A\.\s*";
                                if (Regex.IsMatch(txtRange.Text, aPT))
                                {
                                    string aName = GetStringByPattern(txtRange.Text, aPT);
                                    //aName = Environment.NewLine + aName;
                                    aName = "@BEGIN_A@";
                                    txtRange.Text = Regex.Replace(txtRange.Text, aPT, aName);
                                }

                                aPT = @"(\r\n)*\s*B\.\s*";
                                if (Regex.IsMatch(txtRange.Text, aPT))
                                {
                                    string aName = GetStringByPattern(txtRange.Text, aPT);
                                    //aName = Environment.NewLine + aName;
                                    aName = "@BEGIN_B@";
                                    txtRange.Text = Regex.Replace(txtRange.Text, aPT, aName);
                                }

                                aPT = @"(\r\n)*\s*C\.\s*";
                                if (Regex.IsMatch(txtRange.Text, aPT))
                                {
                                    string aName = GetStringByPattern(txtRange.Text, aPT);
                                    //aName = Environment.NewLine + aName;
                                    aName = "@BEGIN_C@";
                                    txtRange.Text = Regex.Replace(txtRange.Text, aPT, aName);
                                }

                                aPT = @"(\r\n)*\s*D\.\s*";
                                if (Regex.IsMatch(txtRange.Text, aPT))
                                {
                                    string aName = GetStringByPattern(txtRange.Text, aPT);
                                    //aName = Environment.NewLine + aName;
                                    aName = "@BEGIN_D@";
                                    txtRange.Text = Regex.Replace(txtRange.Text, aPT, aName);
                                }

                                if (c1.Name != "0" || c.Name.ToLower() != "white")
                                {
                                    txtRange.Text = txtRange.Text + "@CAUDUNG@";
                                }
                                //p1.ChildObjects.Add(docObject.Clone());
                                p1.ChildObjects.Add(txtRange.Clone());
                            }
                            else
                            {
                                //chi xuat cong thuc hoac hinh anh
                                if (docObject.DocumentObjectType == DocumentObjectType.Picture ||
                                    docObject.DocumentObjectType == DocumentObjectType.XmlParaItem ||
                                    docObject.DocumentObjectType == DocumentObjectType.OleObject ||
                                    docObject.DocumentObjectType == DocumentObjectType.Body ||
                                    docObject.DocumentObjectType == DocumentObjectType.Break ||
                                    docObject.DocumentObjectType == DocumentObjectType.CheckBox ||
                                    docObject.DocumentObjectType == DocumentObjectType.ControlField ||
                                    docObject.DocumentObjectType == DocumentObjectType.CustomXml ||
                                    docObject.DocumentObjectType == DocumentObjectType.Document ||
                                    docObject.DocumentObjectType == DocumentObjectType.DropDownFormField ||
                                    docObject.DocumentObjectType == DocumentObjectType.EmbededField ||
                                    docObject.DocumentObjectType == DocumentObjectType.Field ||
                                    docObject.DocumentObjectType == DocumentObjectType.FieldMark ||
                                    docObject.DocumentObjectType == DocumentObjectType.MergeField ||
                                    docObject.DocumentObjectType == DocumentObjectType.SDTBlockContent ||
                                    docObject.DocumentObjectType == DocumentObjectType.SDTCellContent ||
                                    docObject.DocumentObjectType == DocumentObjectType.SDTInlineContent ||
                                    docObject.DocumentObjectType == DocumentObjectType.SDTRowContent ||
                                    docObject.DocumentObjectType == DocumentObjectType.Section ||
                                    docObject.DocumentObjectType == DocumentObjectType.SeqField ||
                                    docObject.DocumentObjectType == DocumentObjectType.Shape ||
                                    docObject.DocumentObjectType == DocumentObjectType.ShapeGroup ||
                                    docObject.DocumentObjectType == DocumentObjectType.ShapeLine ||
                                    docObject.DocumentObjectType == DocumentObjectType.ShapePath ||
                                    docObject.DocumentObjectType == DocumentObjectType.ShapeRect ||
                                    docObject.DocumentObjectType == DocumentObjectType.SmartTag ||
                                    docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTag ||
                                    docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTagCell ||
                                    docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTagInline ||
                                    docObject.DocumentObjectType == DocumentObjectType.StructureDocumentTagRow ||
                                    docObject.DocumentObjectType == DocumentObjectType.Symbol ||
                                    docObject.DocumentObjectType == DocumentObjectType.Table ||
                                    docObject.DocumentObjectType == DocumentObjectType.TableCell ||
                                    docObject.DocumentObjectType == DocumentObjectType.TableRow ||
                                    docObject.DocumentObjectType == DocumentObjectType.TextBox ||
                                    docObject.DocumentObjectType == DocumentObjectType.TextFormField ||
                                    docObject.DocumentObjectType == DocumentObjectType.TOC ||
                                    docObject.DocumentObjectType == DocumentObjectType.Undefined)
                                {

                                    string imagePath = SaveToImageV1(docObject, folder, batchId, uploadURL);
                                    if (!string.IsNullOrEmpty(imagePath))
                                    {
                                        //p1.AppendText(string.Format("<img class=\"equation-image\" src=\"{0}/{1}\">", uploadURL, imagePath));
                                        p1.AppendText(imagePath);
                                    }
                                }
                                else
                                {
                                    //txtRS.Text += "$docObject.DocumentObjectType$" + docObject.DocumentObjectType + "@";
                                }
                            }
                        }

                        string pt = @"Câu\s+\w+\.";
                        if (System.Text.RegularExpressions.Regex.IsMatch(p1.Text.TrimStart(), pt, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            string qName = GetStringByPattern(p1.Text, pt);
                            //qName += "@BEGIN@" +qName;
                            qName = "";// "@BEGIN@";
                            p1.Text = Regex.Replace(p1.Text, pt, qName);

                            //thiet lap cho 1 cau hoi
                            question.HTML = rawQuestionText;

                            questions.Add(question);//them vao danh sach cau sau do tao moi cau hoi khac

                            if (indexQuestion > 0)
                            {
                                //init new question
                                question = new QuestionV1();
                                rawQuestionText = "";

                                question.File = batchId + new Random().Next(111111, 999999) + ".docx";
                                //string docFolder = uploadFolder + Path.DirectorySeparatorChar + batchId + Path.DirectorySeparatorChar;
                                //doc.Sections.Add(sectionQuestion.Clone());
                                try
                                {
                                    //Section s = docQuestion.AddSection();
                                    //Paragraph p00 = s.AddParagraph();
                                    //p00.AppendText("dfdjfhdjhfkjdfdf");
                                    //PictureWatermark WM = new PictureWatermark();
                                    //WM.Picture = Image.FromFile(uploadFolder + Path.DirectorySeparatorChar + "bg.png");
                                    //docQuestion.Watermark = WM;
                                    //docQuestion.SaveToFile(docFolder + question.File, FileFormat.Docx2010);
                                }
                                catch (Exception ex)
                                {
                                    returnMessage += ex.Message;
                                }



                                //docQuestion.Close();
                                //docQuestion = null;
                                //docQuestion = new Document();
                                //sectionQuestion = null;
                                //sectionQuestion = docQuestion.AddSection();
                                //secIndex = 0;
                            }
                            indexQuestion++;
                        }
                        string hd = "Hướng dẫn giải:";
                        if (System.Text.RegularExpressions.Regex.IsMatch(p1.Text, hd, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            string qName = GetStringByPattern(p1.Text, hd);
                            qName += "@BEGINHUONGDAN@" + qName;
                            p1.Text = Regex.Replace(p1.Text, hd, qName);
                            //p1.Text += Environment.NewLine + "Start trả lời";
                        }
                        rawQuestionText += p1.Text;
                        GC.Collect();
                    }
                    questionIndex++;
                }
                endProcess = DateHelper.CurrentMilisecond();
                returnMessage = "Thành công " + " time: " + (endProcess - startProcess).ToString() + "ms";

                CLogger.WriteInfo(functionName + ": " + returnMessage);
                return questions;
            }
            catch (Exception exx)
            {
                CLogger.WriteLogException(exx.Message);
                endProcess = DateHelper.CurrentMilisecond();
                returnMessage = "Thất bại " + " time: " + (endProcess - startProcess).ToString() + "ms. " + exx.Message; ;
                return questions;// 
            }
        }
        private string GetStringByPattern(string input, string pattern)
        {
            Match match = Regex.Match(input, pattern);
            return match.Value;
        }
        public string SaveToImageV1(DocumentObject docObject, string folder, string questionId, string url)
        {
            if (docObject == null) return string.Empty;
            try
            {


                string objectId = questionId + new Random().Next(111111, 999999);
                string fileName = "obj" + objectId;
                //string docxFileName = folder + Path.DirectorySeparatorChar + fileName + ".docx";
                //string rftFileName = folder + Path.DirectorySeparatorChar + fileName + ".rtf";
                //string imageFileName = folder + Path.DirectorySeparatorChar + fileName + ".JPG";
                //string imageFileName1 = folder + Path.DirectorySeparatorChar + fileName + "1.JPG";
                string htmlFile = folder + Path.DirectorySeparatorChar + fileName + ".html";
                string styleFile = folder + Path.DirectorySeparatorChar + fileName + "_styles.css";
                Document docTemp = new Document();
                Spire.Doc.Section section = docTemp.AddSection();
                Spire.Doc.Documents.Paragraph p = section.AddParagraph();
                p.ChildObjects.Add(docObject.Clone());

                //docTemp.SaveToFile(docxFileName, FileFormat.Docx);//save docx
                //ExportWordToRtf(docxFileName, rftFileName);//save rtf
                //System.Drawing.Image img = docTemp.SaveToImages(0, ImageType.Metafile);
                //img.Save(imageFileName1, ImageFormat.Png);
                docTemp.SaveToFile(htmlFile, FileFormat.Html);

                //rtbQuestion.Width = Convert.ToInt32(tbl.Rows[i].Cells[1].Width);//load & save to image
                //rtbQuestion.LoadFile(rftFileName, RichTextBoxStreamType.RichText);
                //Bitmap bmp = RtbToBitmap(rtbQuestion, rtbQuestion.Width, rtbQuestion.Height);
                //bmp.Save(imageFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                //bmp.Dispose();
                docTemp.Close();
                docTemp = null;
                GC.Collect();
                string html = System.IO.File.ReadAllText(htmlFile);
                //
                //return questionId + "/" + fileName + ".JPG";

                html = html.Substring(html.IndexOf("<body>") + 6);
                html = html.Substring(0, html.IndexOf("</body>"));
                html = html.Replace("> </span>", ">&nbsp;</span>");
                html = html.Replace("<img src=\"", "<img class=\"equation-image\" src=\"" + url + "/" + questionId + "/");
                html = SanitizeHtml(html);
                try
                {
                    System.IO.File.Delete(htmlFile);

                }
                catch (Exception e1)
                {
                    CLogger.WriteLogException("delete: " + htmlFile + "-->" + e1.Message);
                }
                try
                {
                    System.IO.File.Delete(styleFile);

                }
                catch (Exception e2)
                {
                    CLogger.WriteLogException("delete: " + styleFile + "-->" + e2.Message);
                }
                return html;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion QuestionBatchV1

        #region QuestionBatchV2

        QuestionV2Col CleanContent(ref Paragraph p, ref string rightAnswer)
        {
            string pt = @"Câu\s+\w+\.";
            if (System.Text.RegularExpressions.Regex.IsMatch(p.Text.TrimStart(), pt, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                string qName = GetStringByPattern(p.Text, pt);
                qName = "";
                //p.Text = Regex.Replace(p.Text, pt, qName);
                return QuestionV2Col.QC;
            }
            string hd = "Hướng dẫn giải:";
            if (System.Text.RegularExpressions.Regex.IsMatch(p.Text, hd, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return QuestionV2Col.QA;
            }
            hd = "Lời giải";
            if (System.Text.RegularExpressions.Regex.IsMatch(p.Text, hd, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return QuestionV2Col.QA;
            }
            foreach (DocumentObject docObject in p.ChildObjects)
            {
                if (docObject.DocumentObjectType == DocumentObjectType.TextRange)
                {
                    Spire.Doc.Fields.TextRange txtRange = (Spire.Doc.Fields.TextRange)docObject;
                    Color c = txtRange.CharacterFormat.HighlightColor;
                    Color c1 = txtRange.CharacterFormat.TextColor;

                    string aPT = @"(\r\n)*\s*A\.\s*";
                    if (Regex.IsMatch(txtRange.Text, aPT))
                    {
                        if (c1.Name != "0" || c.Name.ToLower() != "white")
                        {
                            rightAnswer = "A";
                        }
                        return QuestionV2Col.A;
                        /*
                         string aName = GetStringByPattern(txtRange.Text, aPT);
                         aName = ";
                         txtRange.Text = Regex.Replace(txtRange.Text, aPT, aName);    
                        */
                    }

                    aPT = @"(\r\n)*\s*B\.\s*";
                    if (Regex.IsMatch(txtRange.Text, aPT))
                    {
                        if (c1.Name != "0" || c.Name.ToLower() != "white")
                        {
                            rightAnswer = "B";
                        }
                        return QuestionV2Col.B;
                    }

                    aPT = @"(\r\n)*\s*C\.\s*";
                    if (Regex.IsMatch(txtRange.Text, aPT))
                    {
                        if (c1.Name != "0" || c.Name.ToLower() != "white")
                        {
                            rightAnswer = "C";
                        }
                        return QuestionV2Col.C;
                    }

                    aPT = @"(\r\n)*\s*D\.\s*";
                    if (Regex.IsMatch(txtRange.Text, aPT))
                    {
                        if (c1.Name != "0" || c.Name.ToLower() != "white")
                        {
                            rightAnswer = "D";
                        }
                        return QuestionV2Col.D;
                    }
                    aPT = @"(\r\n)*\s*E\.\s*";
                    if (Regex.IsMatch(txtRange.Text, aPT))
                    {
                        if (c1.Name != "0" || c.Name.ToLower() != "white")
                        {
                            rightAnswer = "E";
                        }
                        return QuestionV2Col.E;
                    }
                    aPT = @"(\r\n)*\s*F\.\s*";
                    if (Regex.IsMatch(txtRange.Text, aPT))
                    {
                        if (c1.Name != "0" || c.Name.ToLower() != "white")
                        {
                            rightAnswer = "F";
                        }
                        return QuestionV2Col.F;
                    }
                    aPT = @"(\r\n)*\s*G\.\s*";
                    if (Regex.IsMatch(txtRange.Text, aPT))
                    {
                        if (c1.Name != "0" || c.Name.ToLower() != "white")
                        {
                            rightAnswer = "G";
                        }
                        return QuestionV2Col.G;
                    }
                }
            }
            return QuestionV2Col.Undefined;
        }
        private List<List<Paragraph>> LoadBatchFile(string uploadFileFullPath, ref string log)
        {
            List<List<Paragraph>> questions = new List<List<Paragraph>>();
            log = CLogger.Append(log, "LoadBatchFileToParagraph");
            try
            {
                Document doc = new Document();
                doc.LoadFromFile(uploadFileFullPath);
                SectionCollection sections = doc.Sections;

                List<Paragraph> paras = new List<Paragraph>();

                foreach (Spire.Doc.Section sec in sections)
                {

                    ParagraphCollection ps = sec.Paragraphs;
                    foreach (Paragraph p in ps)
                    {
                        string pt = @"Câu\s+\w+\.";
                        if (System.Text.RegularExpressions.Regex.IsMatch(p.Text.TrimStart(), pt, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            if (paras.Count > 0)//bo cau hoi dau tien
                            {
                                questions.Add(paras);//ket thuc cau hoi truoc
                                paras = new List<Paragraph>();//tao list cau hoi moi
                            }
                        }
                        paras.Add((Paragraph)p.Clone());
                    }
                }
                if (questions.Count == 0 && paras != null && paras.Count > 0)
                {
                    questions.Add(paras);//xu ly cho 1 cau duy nhat
                }
            }
            catch (Exception exx)
            {
                log = CLogger.Append(log, "Exception", exx.Message);
            }
            return questions;
        }
        public List<Document> LoadQuestionDoc(string uploadFileFullPath, ref string log)
        {
            List<Document> questionDocs = new List<Document>();
            List<List<Paragraph>> questions = LoadBatchFile(uploadFileFullPath, ref log);
            foreach (List<Paragraph> paras in questions)
            {
                Document doc = new Document();
                Section section = doc.AddSection();
                List<Paragraph> newParas = new List<Paragraph>();
                foreach (Paragraph p in paras)
                {
                    Paragraph newP = section.AddParagraph();
                    foreach (DocumentObject docObject in p.ChildObjects)
                    {
                        if (docObject.DocumentObjectType == DocumentObjectType.TextRange)
                        {
                            Spire.Doc.Fields.TextRange txtRange = (Spire.Doc.Fields.TextRange)docObject;
                            Color c = txtRange.CharacterFormat.HighlightColor;
                            Color c1 = txtRange.CharacterFormat.TextColor;

                            string aPT = @"(\r\n)*\s*A\.\s*";
                            if (Regex.IsMatch(txtRange.Text, aPT))
                            {
                                newP = section.AddParagraph();
                            }

                            aPT = @"(\r\n)*\s*B\.\s*";
                            if (Regex.IsMatch(txtRange.Text, aPT))
                            {
                                newP = section.AddParagraph();
                            }

                            aPT = @"(\r\n)*\s*C\.\s*";
                            if (Regex.IsMatch(txtRange.Text, aPT))
                            {
                                newP = section.AddParagraph();
                            }

                            aPT = @"(\r\n)*\s*D\.\s*";
                            if (Regex.IsMatch(txtRange.Text, aPT))
                            {
                                newP = section.AddParagraph();
                            }

                            aPT = @"(\r\n)*\s*E\.\s*";
                            if (Regex.IsMatch(txtRange.Text, aPT))
                            {
                                newP = section.AddParagraph();
                            }

                            aPT = @"(\r\n)*\s*F\.\s*";
                            if (Regex.IsMatch(txtRange.Text, aPT))
                            {
                                newP = section.AddParagraph();
                            }

                            aPT = @"(\r\n)*\s*G\.\s*";
                            if (Regex.IsMatch(txtRange.Text, aPT))
                            {
                                newP = section.AddParagraph();
                            }

                            string hd = "Hướng dẫn giải:";
                            if (System.Text.RegularExpressions.Regex.IsMatch(txtRange.Text, hd, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                newP = section.AddParagraph();
                            }


                            if (c1.Name != "0" || c.Name.ToLower() != "white")
                            {
                                //txtRange.Text = txtRange.Text + "@CAUDUNG@";
                            }
                        }
                        newP.ChildObjects.Add(docObject.Clone());
                    }
                }
                questionDocs.Add(doc);
            }
            return questionDocs;
        }
        public List<QuestionV2> ProcessBatchQuestion(string uploadFileFullPath, string uploadQuestionFolder, string uploadQuestionUrl, string batchId, ref string log)
        {
            List<QuestionV2> questions = new List<QuestionV2>();
            List<Document> questionDocs = LoadQuestionDoc(uploadFileFullPath, ref log);

            foreach (Document doc in questionDocs)
            {
                SectionCollection sections = doc.Sections;
                QuestionV2 question = new QuestionV2();
                string docFile = "";
                foreach (Spire.Doc.Section sec in sections)
                {
                    foreach (Paragraph p in sec.Paragraphs)
                    {
                        string rightAnswer = "";
                        Paragraph pTemp = (Paragraph)p.Clone();
                        QuestionV2Col col = CleanContent(ref pTemp, ref rightAnswer);
                        if (!string.IsNullOrEmpty(rightAnswer))
                        {
                            question.AR = rightAnswer;
                        }
                        string html = SaveToImage(pTemp, uploadQuestionFolder, batchId, uploadQuestionUrl, ref docFile);
                        switch (col)
                        {
                            case QuestionV2Col.QC:
                                question.QC = html;
                                break;
                            case QuestionV2Col.A:
                                question.A = html;
                                break;
                            case QuestionV2Col.B:
                                question.B = html;
                                break;
                            case QuestionV2Col.C:
                                question.C = html;
                                break;
                            case QuestionV2Col.D:
                                question.D = html;
                                break;
                            case QuestionV2Col.E:
                                question.E = html;
                                break;
                            case QuestionV2Col.F:
                                question.F = html;
                                break;
                            case QuestionV2Col.G:
                                question.G = html;
                                break;
                            case QuestionV2Col.QA:
                                question.QA = html;
                                break;
                        }
                    }
                    questions.Add(question);
                }
                question.FileDoc = docFile + ".docx";
                doc.SaveToFile(uploadQuestionFolder + Path.DirectorySeparatorChar + question.FileDoc, FileFormat.Docx);
            }
            return questions;
        }
        public string SaveToImage(Paragraph p, string folder, string questionId, string url, ref string docFile)
        {
            if (p == null) return string.Empty;
            try
            {
                string objectId = questionId + new Random().Next(111111, 999999);
                string fileName = "obj" + objectId;
                docFile = fileName;
                string htmlFile = folder + Path.DirectorySeparatorChar + fileName + ".html";
                string styleFile = folder + Path.DirectorySeparatorChar + fileName + "_styles.css";
                //obj2017272434293937148497_styles.css
                Document docTemp = new Document();
                Section sec = docTemp.AddSection();
                sec.Paragraphs.Insert(0, (Paragraph)p.Clone());
                //docTemp.Sections.Insert(0, sInput.Clone());

                docTemp.SaveToFile(htmlFile, FileFormat.Html);
                GC.Collect();
                string html = System.IO.File.ReadAllText(htmlFile);
                //get only html


                //html = html.Substring(html.IndexOf("<body>") + 6);
                html = html.Substring(html.IndexOf("<body"));
                html = html.Substring(html.IndexOf(">") + 1);
                html = html.Substring(0, html.IndexOf("</body>"));
                html = html.Replace("> </span>", ">&nbsp;</span>");
                html = html.Replace("<img src=\"", "<img class=\"equation-image\" src=\"" + url + "/" + questionId + "/");
                html = SanitizeHtml(html);

                try
                {
                    System.IO.File.Delete(htmlFile);

                }
                catch (Exception e1)
                {
                    CLogger.WriteLogException("delete: " + htmlFile + "-->" + e1.Message);
                }
                try
                {
                    System.IO.File.Delete(styleFile);

                }
                catch (Exception e2)
                {
                    CLogger.WriteLogException("delete: " + styleFile + "-->" + e2.Message);
                }
                return html;
            }
            catch (Exception ex)
            {
                CLogger.WriteLogException("SaveToImage: -->" + ex.Message);
                return string.Empty;
            }
        }
        public string SanitizeHtml(string html)
        {
            string acceptable = "img";
            string stringPattern = @"</?(?(?=" + acceptable + @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>";
            return Regex.Replace(html, stringPattern, "");
        }

        #endregion QuestionBatchV2

        #region upload file
        protected bool Upload(string url, string filePath, string localFilename, string uploadFileName, out string message)
        {
            Boolean isFileUploaded = false;
            message = "";

            try
            {
                HttpClient httpClient = new HttpClient();

                var fileStream = File.Open(localFilename, FileMode.Open);
                var fileInfo = new FileInfo(localFilename);
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Headers.Add("filePath", filePath);
                content.Headers.Add("fileName", uploadFileName);
                content.Add(new StreamContent(fileStream), "\"file\"", string.Format("\"{0}\"", uploadFileName));

                var taskUpload = httpClient.PostAsync(url, content);

                taskUpload.Wait();
                if (taskUpload.Result.IsSuccessStatusCode)
                {
                    isFileUploaded = true;
                    string uploadResult1 = taskUpload.Result.Content.ReadAsStringAsync().Result;
                    message = uploadResult1;
                    CLogger.WriteInfo(uploadResult1);
                }

                httpClient.Dispose();

            }
            catch (Exception ex)
            {
                isFileUploaded = false;
            }


            return isFileUploaded;
        }
        #endregion upload file
    }


    public class APIHeader
    {
        public string aid = ApiHelper.APP_ID;
        public string ts = ApiHelper.CurrentMilisecond().ToString();
        public string uagent = "1";
        public string cip = "1";
        public string v = "1";
        public string device = "1";
        public string ck = "1";
        //public string uid = "1";
        public string uid = "1";
        public string sid = "1";
        public string sign = "1";
    }
    public class ApiHelper
    {
        public static string APP_ID = ConfigHelper.GetConfig("APP_ID");
        public static string KEY = "a";
        public static string API_URL = ConfigHelper.GetConfig("API_URL");

        public ApiHelper()
        {
            //
            // TODO: Add constructor logic here
            //

        }
        public static string CreateChecksum(params object[] list)
        {
            if (list.Length < 1) throw new Exception("CreateChecksum: list.Length < 1");

            string dataSignKey = list[0].ToString();
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < list.Length; i++)
            {
                string value = list[i].ToString();
                sb.AppendFormat("{0}|", value);
            }
            sb.Append(dataSignKey);
            return Common.SHA512(sb.ToString());
        }
        public static string Base46Encode(string rawString)
        {
            byte[] data = System.Text.ASCIIEncoding.UTF8.GetBytes(rawString);
            return System.Convert.ToBase64String(data);
        }
        public static string Base46Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }
        public static string Dic2JsonString(Dictionary<string, string> dicData)
        {
            try
            {
                return new JavaScriptSerializer().Serialize(dicData);
            }
            catch
            {
                return "";
            }

        }
        public static Dictionary<string, string> JsonString2Dic(string jsonParams)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonParams)) return new Dictionary<string, string>();
                Dictionary<string, string> dicData = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(jsonParams);
                return dicData;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
        public static string JsonToS(object o)
        {
            try
            {
                return new JavaScriptSerializer().Serialize(o);
            }
            catch
            {
                return "";
            }
        }
        public static string SHA256(string sToEncrypt)
        {
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] arrText = System.Text.Encoding.UTF8.GetBytes(sToEncrypt);
            arrText = sha256.ComputeHash(arrText);
            StringBuilder sBuilder = new StringBuilder();
            foreach (byte bi in arrText)
            {
                sBuilder.Append(bi.ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public static string CreateURLParam(Dictionary<string, string> paras)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in paras)
            {
                sb.Append(pair.Key + "=" + pair.Value + "&");
            }
            string url = sb.ToString();
            url = url.Remove(url.LastIndexOf("&"));
            return url;
        }
        public static string HttpPost(string method, string parameters, string contentType = "application/x-www-form-urlencoded")
        {
            string url = API_URL + method;
            HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);
            //Add these, as we're doing a POST
            //req.ContentType = "application/x-www-form-urlencoded";
            req.ContentType = contentType;
            req.Accept = "application/json";
            req.Method = "POST";


            APIHeader h = new APIHeader();
            string rawCkHeader = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", h.aid, h.ts, h.uagent, h.cip, h.v, h.device, h.uid, h.sid, h.sign);
            h.ck = CreateChecksum(KEY, rawCkHeader, parameters);
            req.Headers.Add("aid", h.aid);
            req.Headers.Add("ts", h.ts);
            req.Headers.Add("uagent", h.uagent);
            req.Headers.Add("cip", h.cip);
            req.Headers.Add("v", h.v);
            req.Headers.Add("ck", h.ck);
            req.Headers.Add("device", h.device);
            req.Headers.Add("uid", h.uid);
            req.Headers.Add("sid", h.sid);
            req.Headers.Add("sign", h.sign);


            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(parameters);
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();
            System.Net.WebResponse resp = req.GetResponse();
            if (resp == null) return null;
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }
        public static string HttpGet(string method, string contentType = "application/x-www-form-urlencoded")
        {
            string url = ConfigHelper.GetConfig("ABENLA_API_URL") + method;

            var request = (HttpWebRequest)WebRequest.Create(url);

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;
        }
        public static long CurrentMilisecond()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ms = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
            return ms;
        }
    }

    public class SessionInfo
    {
        public string LocalFolder = "";
        public bool isMathTypeError = false;
        public bool IsSkipError = true;
    }
}
