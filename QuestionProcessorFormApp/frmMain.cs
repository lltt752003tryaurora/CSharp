#define PRO
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;
using EARS.Entities;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Data.Common;
using static QuestionProcessorFormApp.S3Helper;
using static QuestionProcessorFormApp.ConvertEquations;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Word;
using CliWrap.Buffered;
using System.Drawing.Imaging;
using System.Threading;

namespace QuestionProcessorFormApp
{
    public partial class frmMain : Form
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        //private object _lockFirstObj = new object();
        QuestionProcessor qp = new QuestionProcessor();
        QuestionProcessorConfig config = new QuestionProcessorConfig();
        System.Timers.Timer timerMain = new System.Timers.Timer();

        public frmMain()
        {
            InitializeComponent();

        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = ConfigHelper.GetConfig("ApplicationName", "Form 1");
#if DEBUG
            groupBox1.Visible = true;
#endif
        }
        private void btnStatus_Click(object sender, EventArgs e)
        {
            //Process();
            //return;
            if (btnStatus.Text == "Start")
            {
                btnStatus.Text = "Stop";
                StartTimer();
            }
            else
            {
                btnStatus.Text = "Start";
                StopTimer();
            }

        }
        private void StartTimer()
        {
            timerMain.Interval = ConfigHelper.GetConfig("TimerInterval", 5000);
            timerMain.Elapsed += TimerMain_Elapsed;
            timerMain.Start();
        }
        private void StopTimer()
        {
            timerMain.Stop();
        }
        private async void TimerMain_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Random r = new Random();
                int rnd = r.Next(1, 34);
                CLogger.WriteInfo("TimerMain_Elapsed: START " + rnd + ">>" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                await _semaphoreSlim.WaitAsync();
                CLogger.WriteInfo("Run at: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                var rs = await ProcessV2(rnd);
                CLogger.WriteInfo("Return at: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ">>" + rs);
                _semaphoreSlim.Release();
                CLogger.WriteInfo("TimerMain_Elapsed: END " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            }
            catch(Exception ex) {
                CLogger.WriteInfo("TimerMain_Elapsed exception: " + ex.Message);
            }
        }
        string GetLog(BufferedCommandResult cmd)
        {
            string output = "";
            try
            {
                if (cmd != null)
                {
                    output = cmd.StandardOutput;
                }
            }
            catch { }
            return output;

        }

        async Task<string> ProcessV2(int rnd)
        {
            string log = "ProcessV2>";
#if LOCAL
            QuestionBatch qbTest = new QuestionBatch();
            qbTest.id = rnd;

            qbTest.question_file = @"a/" + rnd;
            //ProcessBatchQuestion(config, qbTest, ref log);
            List<QuestionBatch> qbs = new List<QuestionBatch>();// GetQuestionBatch();
            qbs.Add(qbTest);
#else

            List<QuestionBatch> qbs = GetQuestionBatch(ref log);
            if (qbs == null || qbs.Count <= 0)
            {
                log = "not found";
                CLogger.WriteInfo(log);
                return "";
            }
#endif

            S3Config s3Config = new S3Config();

            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
#if !LOCAL
                foreach (QuestionBatch qb in qbs)
                {
                    //download file
                    S3Helper.GetFile3S(s3Config, "", qb.question_file, ".docx", config.UploadFolder, ref log);
                }
#endif
                string succIds = "";
                string failIds = "";
                foreach (QuestionBatch qb in qbs)
                {
                    string logQB = "";
                    qb.question_file = qb.question_file.Replace("/", "\\");
                    try
                    {
                        SessionInfo sessionInfo = new SessionInfo();
                        logQB = CLogger.Append(logQB, "ProcessBatchQuestion");
                        ProcessQuestionResult rs = await ProcessBatchQuestion(config, qb, dateFolder);
                        if (!rs.IsValid)
                        {
                            string errorJson = JsonHelper.ToJsonString(rs.QuestionErrors);
                            //txtLog.Text = rs.Log + errorJson;

#if !LOCAL
                            UpdateQuestionBatch(qb.id.ToString(), -1, errorJson, ref log);
#endif
                            continue;
                        }
                        List<QuestionExt> questions = rs.Questions;
                        foreach (QuestionExt q in questions)
                        {
                            string questionFolderPath = config.UploadFolder + Path.DirectorySeparatorChar + q.full_question_file;
                            string logQ = "QuestionBatch:" + qb.id;
                            try
                            {
                                //insert to DB
                                q.batch_id = qb.id;
#if !LOCAL
                                q.id = AddQuestionV1(q, ref log);
                                //if (q.id > 0)
                                //{
                                //    //mathml
                                //    q.type = 1;
                                //    AddQuestionExt(q, ref log);
                                //    //latex
                                //    q.type = 2;
                                //    AddQuestionExt(q, ref log);
                                //}
#endif
                                CLogger.WriteLogSuccess(logQ);
                            }
                            catch (Exception e0)
                            {
                                logQ = CLogger.Append(logQ, e0.Message);
                                CLogger.WriteLogException(logQ);
                            }
                            CLogger.WriteLogSuccess(logQ);
                        }
                        succIds += qb.id + ",";
#if !LOCAL
                        SaveFolder3S(s3Config, "questions/" + dateFolder + @"/" + qb.id, config.UploadFolder + Path.DirectorySeparatorChar + dateFolder + Path.DirectorySeparatorChar + qb.id, ref log);
                        UpdateQuestionBatch(qb.id.ToString(), 2, "", ref log);
#endif
                    }
                    catch (Exception e1)
                    {
#if !LOCAL
                        UpdateQuestionBatch(qb.id.ToString(), -1, "Lỗi không xác định, thông báo cho admin kiểm tra", ref log);
#endif
                        failIds += qb.id + ",";
                        log = CLogger.Append(log, "Batch exception", e1.Message);
                        CLogger.WriteLogException(log);
                    }
                }
                succIds = succIds.TrimEnd(',');
                failIds = failIds.TrimEnd(',');
#if !LOCAL
                //if (!string.IsNullOrEmpty(succIds))
                //{
                //    UpdateQuestionBatch(succIds, 2, "", ref log);
                //}
#endif
                CLogger.WriteLogSuccess(log);
            }
            catch (Exception e2)
            {
                log = CLogger.Append(log, "S3 exception", e2.Message);
                CLogger.WriteLogException(log);
            }
            return log;
        }
        async void Process()
        {
            //QuestionBatch qbTest = new QuestionBatch();
            //qbTest.id = 1000;
            //qbTest.question_file = @"c/c";
            //ProcessBatchQuestion(config, qbTest, ref log);
            //List<QuestionBatch> qbs = new List<QuestionBatch>();// GetQuestionBatch();
            //qbs.Add(qbTest
            string log = "Process>";
            List<QuestionBatch> qbs = GetQuestionBatch(ref log);
            S3Config s3Config = new S3Config();

            try
            {
                foreach (QuestionBatch qb in qbs)
                {
                    //download file
                    S3Helper.GetFile3S(s3Config, "", qb.question_file, ".docx", config.UploadFolder, ref log);
                }
                string succIds = "";
                string failIds = "";
                foreach (QuestionBatch qb in qbs)
                {
                    qb.question_file = qb.question_file.Replace("/", "\\");
                    try
                    {
                        int numOfMathType = 0;
                        SessionInfo sessionInfo = new SessionInfo();
                        List<QuestionExt> questions = ProcessQuestion_HTML(qb, ref log, ref numOfMathType, ref sessionInfo);

                        foreach (QuestionExt q in questions)
                        {
                            string questionFolderPath = config.UploadFolder + Path.DirectorySeparatorChar + q.full_question_file;
                            string logQ = "QuestionBatch:" + qb.id;
                            try
                            {
                                string uploadFolderURL = config.UploadFolder.Replace(@"\", "/");
                                var cmd = await Converter.Docx2TexPandoc(questionFolderPath, q.contentDocx);
                                logQ = CLogger.Append(log, GetLog(cmd));
                                q.contentTex = Converter.ExportWMFToTex(questionFolderPath, q.contentDocx.Replace(".docx", ".tex"));
                                q.contentTex = q.contentTex.Replace(uploadFolderURL, config.UploadURL);
                                //tex to html
                                cmd = await Converter.Tex2MathMLPandoc(questionFolderPath, q.contentDocx.Replace(".docx", "_final.tex"));
                                q.contentMathMLHtml = File.ReadAllText(questionFolderPath + Path.DirectorySeparatorChar + q.contentDocx.Replace(".docx", "_final.html"));
                                q.contentMathMLHtml = q.contentMathMLHtml.Replace(uploadFolderURL, config.UploadURL);
                                //convert to tex
                                cmd = await Converter.Docx2TexPandoc(questionFolderPath, q.a1Docx);
                                logQ = CLogger.Append(log, GetLog(cmd));
                                q.a1Tex = Converter.ExportWMFToTex(questionFolderPath, q.a1Docx.Replace(".docx", ".tex"));
                                q.a1Tex = q.a1Tex.Replace(uploadFolderURL, config.UploadURL);
                                //tex to html
                                cmd = await Converter.Tex2MathMLPandoc(questionFolderPath, q.a1Docx.Replace(".docx", "_final.tex"));
                                q.a1MathMLHtml = File.ReadAllText(questionFolderPath + Path.DirectorySeparatorChar + q.a1Docx.Replace(".docx", "_final.html"));
                                q.a1MathMLHtml = q.a1MathMLHtml.Replace(uploadFolderURL, config.UploadURL);

                                cmd = await Converter.Docx2TexPandoc(questionFolderPath, q.a2Docx);
                                logQ = CLogger.Append(log, GetLog(cmd));
                                q.a2Tex = Converter.ExportWMFToTex(questionFolderPath, q.a2Docx.Replace(".docx", ".tex"));
                                q.a2Tex = q.a2Tex.Replace(uploadFolderURL, config.UploadURL);
                                //tex to html
                                cmd = await Converter.Tex2MathMLPandoc(questionFolderPath, q.a2Docx.Replace(".docx", "_final.tex"));
                                q.a2MathMLHtml = File.ReadAllText(questionFolderPath + Path.DirectorySeparatorChar + q.a2Docx.Replace(".docx", "_final.html"));
                                q.a2MathMLHtml = q.a2MathMLHtml.Replace(uploadFolderURL, config.UploadURL);

                                cmd = await Converter.Docx2TexPandoc(questionFolderPath, q.a3Docx);
                                logQ = CLogger.Append(log, GetLog(cmd));
                                q.a3Tex = Converter.ExportWMFToTex(questionFolderPath, q.a3Docx.Replace(".docx", ".tex"));
                                q.a3Tex = q.a3Tex.Replace(uploadFolderURL, config.UploadURL);
                                //tex to html
                                cmd = await Converter.Tex2MathMLPandoc(questionFolderPath, q.a3Docx.Replace(".docx", "_final.tex"));
                                q.a3MathMLHtml = File.ReadAllText(questionFolderPath + Path.DirectorySeparatorChar + q.a3Docx.Replace(".docx", "_final.html"));
                                q.a3MathMLHtml = q.a3MathMLHtml.Replace(uploadFolderURL, config.UploadURL);

                                cmd = await Converter.Docx2TexPandoc(questionFolderPath, q.a4Docx);
                                logQ = CLogger.Append(log, GetLog(cmd));
                                q.a4Tex = Converter.ExportWMFToTex(questionFolderPath, q.a4Docx.Replace(".docx", ".tex"));
                                q.a4Tex = q.a4Tex.Replace(uploadFolderURL, config.UploadURL);
                                //tex to html
                                cmd = await Converter.Tex2MathMLPandoc(questionFolderPath, q.a4Docx.Replace(".docx", "_final.tex"));
                                q.a4MathMLHtml = File.ReadAllText(questionFolderPath + Path.DirectorySeparatorChar + q.a4Docx.Replace(".docx", "_final.html"));
                                q.a4MathMLHtml = q.a4MathMLHtml.Replace(uploadFolderURL, config.UploadURL);

                                cmd = await Converter.Docx2TexPandoc(questionFolderPath, q.aFullDocx);
                                logQ = CLogger.Append(log, GetLog(cmd));
                                q.aFullTex = Converter.ExportWMFToTex(questionFolderPath, q.aFullDocx.Replace(".docx", ".tex"));
                                q.aFullTex = q.aFullTex.Replace(uploadFolderURL, config.UploadURL);
                                //tex to html
                                cmd = await Converter.Tex2MathMLPandoc(questionFolderPath, q.aFullDocx.Replace(".docx", "_final.tex"));
                                q.aFullMathMLHtml = File.ReadAllText(questionFolderPath + Path.DirectorySeparatorChar + q.aFullDocx.Replace(".docx", "_final.html"));
                                q.aFullMathMLHtml = q.aFullMathMLHtml.Replace(uploadFolderURL, config.UploadURL);
                                string jsonString = JsonHelper.ToJsonString(q);

                                //insert to DB
                                q.batch_id = qb.id;
                                q.id = AddQuestion(q, ref log);
                                //if (q.id > 0)
                                //{
                                //    //mathml
                                //    q.type = 1;
                                //    AddQuestionExt(q, ref log);
                                //    //latex
                                //    q.type = 2;
                                //    AddQuestionExt(q, ref log);
                                //}
                                SaveFolder3S(s3Config, "questions/" + q.full_question_file + @"/" + q.question_file, config.UploadFolder + Path.DirectorySeparatorChar + q.full_question_file + Path.DirectorySeparatorChar + q.question_file, ref log);
                                CLogger.WriteLogSuccess(logQ);
                            }
                            catch (Exception e0)
                            {
                                logQ = CLogger.Append(logQ, e0.Message);
                                CLogger.WriteLogException(logQ);
                            }
                            CLogger.WriteLogSuccess(logQ);
                        }
                        succIds += qb.id + ",";
                    }
                    catch (Exception e1)
                    {
                        failIds += qb.id + ",";
                        log = CLogger.Append(log, "Batch exception", e1.Message);
                        CLogger.WriteLogException(log);
                    }
                }
                succIds = succIds.TrimEnd(',');
                failIds = failIds.TrimEnd(',');

                if (!string.IsNullOrEmpty(succIds))
                {
                    UpdateQuestionBatch(succIds, 2, "", ref log);
                }
                if (!string.IsNullOrEmpty(failIds))
                {
                    UpdateQuestionBatch(failIds, -1, "", ref log);
                }
                CLogger.WriteLogSuccess(log);
            }
            catch (Exception e2)
            {
                log = CLogger.Append(log, "S3 exception", e2.Message);
                CLogger.WriteLogException(log);
            }

        }
        void Test()
        {
            QuestionBatch qb = new QuestionBatch();
            qb.id = 1000;
            qb.question_file = @"f/f";
            if (string.IsNullOrEmpty(qb.question_file))
            {
                MessageBox.Show("Vui lòng chọn file");
                return;
            }

            QuestionProcessor qp = new QuestionProcessor();
            //qp.SaveToOpenXML(qb.question_file);
            int numMathType = 0;
            string log = "";
            SessionInfo SessionInfo = new SessionInfo();
            List<QuestionExt> questions = ProcessQuestion_HTMLV1(qb, ref log, ref numMathType, ref SessionInfo);
            //List<QuestionV2> questions = qp.ProcessBatchQuestionSVG(qb.question_file, "C:\\temp\\eduquestionbatch", "file:///C:/temp/eduquestionbatch/", qb.id.ToString(), ref log);
            txtLog.Text += log;
            foreach (QuestionExt q in questions)
            {
                string jsonString = JsonHelper.ToJsonString(q);
                //Console.WriteLine("------------------------------------------------------------------------------------");
                //Console.WriteLine(jsonString);
                txtLog.Text += jsonString;
            }
        }
#region Function

        private List<QuestionBatch> GetQuestionBatch(ref string log)
        {
            log = CLogger.Append(log, "GetQuestionBatch");
            //string query1 = "SP_GetQuestionBatch";// 
            string query = "SELECT * from edu_question_batch where status = 0 and created > '2021-08-28 02:26:55' order by id asc limit 0,5";
            List<QuestionBatch> quesionBatchs = new List<QuestionBatch>();
            DbCommand dbCommand = null;
            log = CLogger.Append(log, query);
            try
            {
                //dbCommand = DBHelper.SQLHelper.GetStoredProcCommand(query1);
                //dbCommand.CommandType = CommandType.StoredProcedure;

                dbCommand = DBHelper.SQLHelper.CreateCommand(query);
                dbCommand.Connection.Open();
                log = CLogger.Append(log, "BeginTransaction");
                dbCommand.Transaction = dbCommand.Connection.BeginTransaction();
                var reader = dbCommand.ExecuteReader();
                string ids = "";
                while (reader.Read())
                {
                    QuestionBatch quesionBatch = new QuestionBatch();
                    if (!reader.IsDBNull(reader.GetOrdinal("id"))) quesionBatch.id = NumberHelper.ConvertToInt(reader["id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("title"))) quesionBatch.title = (string)reader["title"];
                    if (!reader.IsDBNull(reader.GetOrdinal("chapter_id"))) quesionBatch.chapter_id = NumberHelper.ConvertToInt(reader["chapter_id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("resource_id"))) quesionBatch.resource_id = NumberHelper.ConvertToInt(reader["resource_id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("type"))) quesionBatch.type = NumberHelper.ConvertToInt(reader["type"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("level"))) quesionBatch.level = NumberHelper.ConvertToInt(reader["level"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("course_id"))) quesionBatch.course_id = NumberHelper.ConvertToInt(reader["course_id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("subject_id"))) quesionBatch.subject_id = NumberHelper.ConvertToInt(reader["subject_id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("status"))) quesionBatch.status = NumberHelper.ConvertToInt(reader["status"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("question_file"))) quesionBatch.question_file = (string)reader["question_file"];
                    if (!reader.IsDBNull(reader.GetOrdinal("class_id"))) quesionBatch.class_id = NumberHelper.ConvertToInt(reader["class_id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("created_by"))) quesionBatch.created_by = (string)reader["created_by"];
                    if (!reader.IsDBNull(reader.GetOrdinal("lesson_id"))) quesionBatch.lession_id = NumberHelper.ConvertToInt(reader["lesson_id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("user_id"))) quesionBatch.user_id = NumberHelper.ConvertToLong(reader["user_id"]);
                    if (!reader.IsDBNull(reader.GetOrdinal("school_id"))) quesionBatch.school_id = NumberHelper.ConvertToLong(reader["school_id"]);

                    ids += quesionBatch.id + ",";

                    quesionBatchs.Add(quesionBatch);
                }
                log = CLogger.Append(log, "ids=", ids);
                reader.Close();

                if (!string.IsNullOrEmpty(ids))
                {
                    ids = ids.TrimEnd(',');
                    string uSQL = "update edu_question_batch set status = 1, modified = now() where id in (" + ids + ")";
                    dbCommand.CommandText = uSQL;
                    log = CLogger.Append(log, uSQL);
                    int count = dbCommand.ExecuteNonQuery();
                    log = CLogger.Append(log, count);
                    if (count > 0)
                    {
                        log = CLogger.Append(log, "Commit");
                        dbCommand.Transaction.Commit();
                    }
                    else
                    {
                        log = CLogger.Append(log, "Rollback");
                        dbCommand.Transaction.Rollback();
                        quesionBatchs = new List<QuestionBatch>();
                    }
                }
                else
                {
                    log = CLogger.Append(log, "not found");
                    dbCommand.Transaction.Commit();
                }
            }
            catch (Exception qryBatchEx)
            {
                CLogger.WriteLogException("BatchProcess: " + query + ": " + qryBatchEx.Message);
            }
            finally
            {
                if (dbCommand != null) dbCommand.Connection.Dispose();
            }
            return quesionBatchs;
        }
        private int UpdateQuestionBatch(string ids, int status, string error, ref string log)
        {
            int rs = 0;
            string query = string.Format("update edu_question_batch set status = {0} ,modified = now(), list_fail = '{1}' where id in ({2})", status, error, ids);
            log = CLogger.Append(log, "UpdateQuestionBatch", query);
            DbCommand dbCommand = null;
            try
            {
                dbCommand = DBHelper.SQLHelper.CreateCommand(query);
                dbCommand.Connection.Open();
                rs = dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                log = CLogger.Append(log, "UpdateQuestionBatch>Exception>", e.Message);
            }
            finally
            {
                if (dbCommand != null) dbCommand.Connection.Dispose();
            }
            log = CLogger.Append(log, rs);
            return rs;
        }
        private int AddQuestion(QuestionExt q, ref string log)
        {
            string query1 = "SP_InsertIntoEduquestion";

            DbCommand cmd1 = null;
            int qId = 0;
            try
            {
                cmd1 = DBHelper.SQLHelper.GetStoredProcCommand(query1);
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.Parameters.Add(new MySqlParameter("@pChapter_id", q.chapter_id));
                cmd1.Parameters.Add(new MySqlParameter("@pResource_id", q.resource_id));
                cmd1.Parameters.Add(new MySqlParameter("@pContent", q.content));
                cmd1.Parameters.Add(new MySqlParameter("@pShort_explain", q.short_explain));
                cmd1.Parameters.Add(new MySqlParameter("@pFull_explain", q.full_explain));
                cmd1.Parameters.Add(new MySqlParameter("@pType", q.type));
                cmd1.Parameters.Add(new MySqlParameter("@pLevel", q.level));
                cmd1.Parameters.Add(new MySqlParameter("@pQuestion_file", q.question_file));//folder name
                cmd1.Parameters.Add(new MySqlParameter("@pFull_question_file", q.full_question_file));//doc file, user can download and update
                cmd1.Parameters.Add(new MySqlParameter("@pCourse_id", q.course_id));
                cmd1.Parameters.Add(new MySqlParameter("@pSubject_id", q.subject_id));
                cmd1.Parameters.Add(new MySqlParameter("@pA1", q.a1));
                cmd1.Parameters.Add(new MySqlParameter("@pA2", q.a2));
                cmd1.Parameters.Add(new MySqlParameter("@pA3", q.a3));
                cmd1.Parameters.Add(new MySqlParameter("@pA4", q.a4));
                cmd1.Parameters.Add(new MySqlParameter("@pAr", q.ar));
                cmd1.Parameters.Add(new MySqlParameter("@pClass_id", q.class_id));
                cmd1.Parameters.Add(new MySqlParameter("@pCreated", q.created_by));
                cmd1.Parameters.Add(new MySqlParameter("@pBatch_id", q.batch_id));
                cmd1.Parameters.Add(new MySqlParameter("@pLesson_id", q.lesson_id));
                if (cmd1.Connection.State == ConnectionState.Closed)
                {
                    cmd1.Connection.Open();
                }
                var reader1 = cmd1.ExecuteReader();


                while (reader1.Read())
                {
                    //qId = reader1.GetValue("id");
                    if (!reader1.IsDBNull(reader1.GetOrdinal("id"))) qId = NumberHelper.ConvertToInt(reader1["id"]);

                }
            }
            catch (Exception e)
            {
                log = CLogger.Append(log, e.Message);
            }
            finally
            {
                if (cmd1 != null) cmd1.Connection.Dispose();
            }
            return qId;
        }
        private int AddQuestionV1(QuestionExt q, ref string log)
        {
            string query = @"INSERT INTO `edu_question` (`chapter_id`, `resource_id`, `content`, `short_explain`, `full_explain`, `type`, `level`, `question_file`, `full_question_file`, `created`, `modified`, `created_by`, `modified_by`, `published`, `course_id`, `subject_id`, `a1`, `a2`, `a3`, `a4`, `ar`, `class_id`,`batch_id`,`lession_id`,`user_id`,`school_id`)
                            VALUES(@pChapter_id, @pResource_id, @pContent, @pShort_explain, @pFull_explain, @pType, @pLevel, @pQuestion_file, @pFull_question_file, now(), now(), @pCreated, @pCreated, 1, @pCourse_id, @pSubject_id, @pA1, @pA2, @pA3, @pA4, @pAr, @pClass_id, @pBatch_id, @pLesson_id, @pUser_id, @pSchool_id); 
                            SELECT max(`id`) as id FROM `edu_question`;";

            DbCommand cmd1 = null;
            int qId = 0;
            try
            {
                cmd1 = DBHelper.SQLHelper.CreateCommand(query);
                cmd1.CommandType = CommandType.Text;
                cmd1.Parameters.Add(new MySqlParameter("@pChapter_id", q.chapter_id));
                cmd1.Parameters.Add(new MySqlParameter("@pResource_id", q.resource_id));
                cmd1.Parameters.Add(new MySqlParameter("@pContent", q.content));
                cmd1.Parameters.Add(new MySqlParameter("@pShort_explain", q.short_explain));
                cmd1.Parameters.Add(new MySqlParameter("@pFull_explain", q.full_explain));
                cmd1.Parameters.Add(new MySqlParameter("@pType", q.type));
                cmd1.Parameters.Add(new MySqlParameter("@pLevel", q.level));
                cmd1.Parameters.Add(new MySqlParameter("@pQuestion_file", q.question_file));//folder name
                cmd1.Parameters.Add(new MySqlParameter("@pFull_question_file", q.full_question_file));//doc file, user can download and update
                cmd1.Parameters.Add(new MySqlParameter("@pCourse_id", q.course_id));
                cmd1.Parameters.Add(new MySqlParameter("@pSubject_id", q.subject_id));
                cmd1.Parameters.Add(new MySqlParameter("@pA1", q.a1));
                cmd1.Parameters.Add(new MySqlParameter("@pA2", q.a2));
                cmd1.Parameters.Add(new MySqlParameter("@pA3", q.a3));
                cmd1.Parameters.Add(new MySqlParameter("@pA4", q.a4));
                cmd1.Parameters.Add(new MySqlParameter("@pAr", q.ar));
                cmd1.Parameters.Add(new MySqlParameter("@pClass_id", q.class_id));
                cmd1.Parameters.Add(new MySqlParameter("@pCreated", q.created_by));
                cmd1.Parameters.Add(new MySqlParameter("@pBatch_id", q.batch_id));
                cmd1.Parameters.Add(new MySqlParameter("@pLesson_id", q.lesson_id));
                cmd1.Parameters.Add(new MySqlParameter("@pUser_id", q.user_id));
                cmd1.Parameters.Add(new MySqlParameter("@pSchool_id", q.school_id));
                if (cmd1.Connection.State == ConnectionState.Closed)
                {
                    cmd1.Connection.Open();
                }
                var reader1 = cmd1.ExecuteReader();


                while (reader1.Read())
                {
                    //qId = reader1.GetValue("id");
                    if (!reader1.IsDBNull(reader1.GetOrdinal("id"))) qId = NumberHelper.ConvertToInt(reader1["id"]);

                }
            }
            catch (Exception e)
            {
                log = CLogger.Append(log, e.Message);
            }
            finally
            {
                if (cmd1 != null) cmd1.Connection.Dispose();
            }
            return qId;
        }
        private void AddQuestionExt(QuestionExt q, ref string log)
        {
            string query1 = "SP_InsertIntoEduquestionExt";

            DbCommand cmd1 = null;
            int qId = 0;
            try
            {
                cmd1 = DBHelper.SQLHelper.GetStoredProcCommand(query1);
                cmd1.CommandType = CommandType.StoredProcedure;
                cmd1.Parameters.Add(new MySqlParameter("@pID", q.id));
                cmd1.Parameters.Add(new MySqlParameter("@pContent", q.type == 1 ? q.contentMathMLHtml : q.contentTex));
                cmd1.Parameters.Add(new MySqlParameter("@pShort_explain", q.type == 1 ? q.aShortMathMLHtml : q.aShortTex));
                cmd1.Parameters.Add(new MySqlParameter("@pFull_explain", q.type == 1 ? q.aFullMathMLHtml : q.aFullTex));
                cmd1.Parameters.Add(new MySqlParameter("@pType", q.type));
                cmd1.Parameters.Add(new MySqlParameter("@pA1", q.type == 1 ? q.a1MathMLHtml : q.a1Tex));
                cmd1.Parameters.Add(new MySqlParameter("@pA2", q.type == 1 ? q.a2MathMLHtml : q.a2Tex));
                cmd1.Parameters.Add(new MySqlParameter("@pA3", q.type == 1 ? q.a3MathMLHtml : q.a3Tex));
                cmd1.Parameters.Add(new MySqlParameter("@pA4", q.type == 1 ? q.a4MathMLHtml : q.a4Tex));
                cmd1.Parameters.Add(new MySqlParameter("@pAr", q.type == 1 ? q.arMathMLHtml : q.arTex));
                if (cmd1.Connection.State == ConnectionState.Closed)
                {
                    cmd1.Connection.Open();
                }
                var reader1 = cmd1.ExecuteReader();


                while (reader1.Read())
                {
                    //qId = reader1.GetValue("id");
                    if (!reader1.IsDBNull(reader1.GetOrdinal("id"))) qId = NumberHelper.ConvertToInt(reader1["id"]);

                }
            }
            catch (Exception e)
            {
                log = CLogger.Append(log, e.Message);
            }
            finally
            {
                if (cmd1 != null) cmd1.Connection.Dispose();
            }
        }
        public List<QuestionExt> ProcessQuestion_HTML(QuestionBatch qb, ref string log, ref int numMathType, ref SessionInfo SessionInfo)
        {
            List<QuestionExt> questions = new List<QuestionExt>();
            Dictionary<int, string> ar = new Dictionary<int, string>();
            List<int> listQuesError = new List<int>();
            List<int> listQuestionMathTypeError = new List<int>();
            int totalQues = 0;
            string logSB = "";

            int classId = qb.class_id;
            int subjectId = qb.subject_id;
            int subjectTypeId = qb.subject_type;
            int levelId = qb.level;
            int chapterId = qb.chapter_id;
            int lessonId = qb.lession_id;
            string fileName = config.UploadFolder + Path.DirectorySeparatorChar + qb.question_file + ".docx";
            if (string.IsNullOrEmpty(fileName))
            {
                return questions;
            }

            string filePath = fileName;
            string destFileOrg = fileName.Replace(".docx", "_org.docx");
            string destFileConvert = fileName.Replace(".docx", "_convert.docx");

            qp.CloneDocument_MC(filePath, destFileOrg);
            logSB += "CloneDocument_MC-OK> ";

            qp.RemoveParagragh_NULL1(destFileOrg);

            totalQues = qp.ReCountDocument(destFileOrg);
            logSB += "ReCountDocument-OK> ";

            qp.RemoveParagragh_NULL1(destFileOrg);
            logSB += "RemoveParagragh_NULL1-OK> ";

            qp.CheckFormatException_MC(destFileOrg, ref listQuesError, SessionInfo.IsSkipError);
            logSB += "CheckFormatException_MC-OK> ";

            qp.CheckFormatDocument_MC(destFileOrg, ref ar, SessionInfo.IsSkipError);
            logSB += "CheckFormatDocument_MC-OK> ";

            qp.RemoveParagragh_NULL2(destFileOrg);
            logSB += "RemoveParagragh_NULL2-OK> ";

            qp.CloneDocument_MC(destFileOrg, destFileConvert);
            logSB += "CloneDocument_MC-OK> ";
            // 80%
            qp.RemoveParagragh_NULL3(destFileConvert);
            logSB += "RemoveParagragh_NULL3-OK> ";

            qp.ReplaceFromQuestion(destFileConvert);
            logSB += "ReplaceFromQuestion-OK> ";

            qp.ReformatEquations(destFileConvert, ref logSB);

            /*qb = new QuestionBatch();
            qb.title = "";
            qb.description = "";
            qb.created_by = "8xx.vn";
            qb.modified_by = " ";
            qb.list_fail = " ";
            qb.created = DateTime.Now;
            qb.modified = DateTime.Now;
            qb.chapter_id = chapterId;
            qb.class_id = classId;
            qb.course_id = 1;
            qb.lession_id = lessonId;
            qb.type = subjectTypeId;
            qb.level = levelId;
            qb.subject_id = subjectId;
            qb.resource_id = 1;
            qb.question_file = Path.AltDirectorySeparatorChar + Path.GetFileName(filePath);
            qb.user_id = 0;
            qb.subject_type = 0;*/

            questions = qp.BatchProcess_Multiple_Choice_HTML(qb, destFileConvert, "1", ar, 0, listQuesError, destFileOrg, ref listQuestionMathTypeError, ref log, ref SessionInfo);
            // 90%
            return questions;
        }
        public List<QuestionExt> ProcessQuestion_HTMLV1(QuestionBatch qb, ref string log, ref int numMathType, ref SessionInfo SessionInfo)
        {
            List<QuestionExt> questions = new List<QuestionExt>();
            Dictionary<int, string> ar = new Dictionary<int, string>();
            List<int> listQuesError = new List<int>();
            List<int> listQuestionMathTypeError = new List<int>();
            int totalQues = 0;
            string logSB = "";

            int classId = qb.class_id;
            int subjectId = qb.subject_id;
            int subjectTypeId = qb.subject_type;
            int levelId = qb.level;
            int chapterId = qb.chapter_id;
            int lessonId = qb.lession_id;
            string fileName = config.UploadFolder + Path.DirectorySeparatorChar + qb.question_file + ".docx";
            if (string.IsNullOrEmpty(fileName))
            {
                return questions;
            }

            string filePath = fileName;
            string destFileOrg = fileName.Replace(".docx", "_org.docx");
            string destFileConvert = fileName.Replace(".docx", "_convert.docx");

            qp.CloneDocument_MC(filePath, destFileOrg);
            logSB += "CloneDocument_MC-OK> ";

            qp.RemoveParagragh_NULL1(destFileOrg);

            totalQues = qp.ReCountDocument(destFileOrg);
            logSB += "ReCountDocument-OK> ";

            qp.RemoveParagragh_NULL1(destFileOrg);
            logSB += "RemoveParagragh_NULL1-OK> ";

            qp.CheckFormatException_MC(destFileOrg, ref listQuesError, SessionInfo.IsSkipError);
            logSB += "CheckFormatException_MC-OK> ";

            qp.CheckFormatDocument_MC(destFileOrg, ref ar, SessionInfo.IsSkipError);
            logSB += "CheckFormatDocument_MC-OK> ";

            qp.RemoveParagragh_NULL2(destFileOrg);
            logSB += "RemoveParagragh_NULL2-OK> ";

            qp.CloneDocument_MC(destFileOrg, destFileConvert);
            logSB += "CloneDocument_MC-OK> ";
            // 80%
            qp.RemoveParagragh_NULL3(destFileConvert);
            logSB += "RemoveParagragh_NULL3-OK> ";

            qp.ReplaceFromQuestion(destFileConvert);
            logSB += "ReplaceFromQuestion-OK> ";

            bool isMathTypeObject = qp.CheckMathTypeObject(destFileOrg, destFileConvert, ref numMathType);
            logSB += "checkMathTypeObject-OK> ";

            qp.RemoveParagragh_NULL3(destFileConvert);
            logSB += "RemoveParagragh_NULL3-OK> ";

            if (isMathTypeObject)
            {
                foreach (var process in System.Diagnostics.Process.GetProcessesByName("MathType"))
                    process.PriorityClass = ProcessPriorityClass.RealTime;

                qp.ReformatEquations(destFileConvert, ref log);
                qp.ExtractMathML_double(1, ref destFileConvert, ref SessionInfo, ref log);
                //if (SessionInfo.isMathTypeError) qp.ExtractMathML_double(2, ref destFileConvert, ref SessionInfo);
                //if (SessionInfo.isMathTypeError) qp.ExtractMathML_double(3, ref destFileConvert, ref SessionInfo);
                //if (SessionInfo.isMathTypeError) qp.ExtractMathML_final(4, ref destFileConvert, ref SessionInfo);
                //if (SessionInfo.isMathTypeError)
                //{
                //    qp.SaveMathML(SessionInfo.LocalFolder, 5, ref destFileConvert, ref SessionInfo);
                //    qp.ConvertMathML(SessionInfo.LocalFolder, destFileConvert, ref SessionInfo);
                //}

                logSB += "ConvertMathML-OK> ";
            }

            qp.RepairChemicalFormula_MC(destFileConvert);
            logSB += "RepairChemicalFormula_MC-OK> ";

            /*qb = new QuestionBatch();
            qb.title = "";
            qb.description = "";
            qb.created_by = "8xx.vn";
            qb.modified_by = " ";
            qb.list_fail = " ";
            qb.created = DateTime.Now;
            qb.modified = DateTime.Now;
            qb.chapter_id = chapterId;
            qb.class_id = classId;
            qb.course_id = 1;
            qb.lession_id = lessonId;
            qb.type = subjectTypeId;
            qb.level = levelId;
            qb.subject_id = subjectId;
            qb.resource_id = 1;
            qb.question_file = Path.AltDirectorySeparatorChar + Path.GetFileName(filePath);
            qb.user_id = 0;
            qb.subject_type = 0;*/

            questions = qp.BatchProcess_Multiple_Choice_HTML(qb, destFileConvert, "1", ar, 0, listQuesError, destFileOrg, ref listQuestionMathTypeError, ref log, ref SessionInfo);
            // 90%
            return questions;
        }

#endregion Function

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            DialogResult file = openFileDialog1.ShowDialog();
            if (file == DialogResult.OK)
            {
                txtFileName.Text = openFileDialog1.FileName;

                if (openFileDialog1.FileName.Contains(".wmf") || openFileDialog1.FileName.Contains(".emf"))
                {
                    // Create a Metafile object
                    Metafile curMetafile = new Metafile(openFileDialog1.FileName);
                    // Get metafile header
                    MetafileHeader header = curMetafile.GetMetafileHeader();

                    string mfAttributes = " ";
                    mfAttributes += "Type :" + header.Type.ToString();
                    mfAttributes += "Bounds :" + header.Bounds.ToString();
                    mfAttributes += "Size :" + header.MetafileSize.ToString();
                    mfAttributes += "Version :" + header.Version.ToString();
                    mfAttributes += "WmfHeader :" + header.WmfHeader.ToString();

                    txtLog.Text = mfAttributes;
                }
                return;
            }
        }

        async private void btnConvert_Click(object sender, EventArgs e)
        {
            QuestionProcessor qp = new QuestionProcessor();
            string docx = @"C:\temp1\e\e_table.docx";
            SessionInfo SessionInfo = new SessionInfo();
            //qp.ExtractMathML_repair(docx);
            //qp.ExtractMathML_double(1, ref docx, ref SessionInfo);

            //return;
            //qp.SaveAs(docx, WdSaveFormat.wdFormatHTML, "html");
            //return;
            ConvertEquation ce = new ConvertEquation();
            ce = new ConvertEquation();

            string fileName = @"C:\temp1\e\e_convert\media\image58.wmf";
            //string a = "MathJax-MathML.tdl";
            string format = "MathML3 (namespace attr).tdl";

            //format = a;
            //ce.Convert(new EquationInputFileGIF(fileName), new EquationOutputFileText(fileName.Replace(".gif", ".txt"), format));



            ///string fileName = @"C:\temp1\b\kibac843-5.gif";
            //string a = "MathJax-MathML.tdl";
            //string format = "Plain TeX.tdl";

            ////format = a;
            ce.Convert(new EquationInputFileWMF(fileName), new EquationOutputFileText(fileName.Replace(".wmf", ".txt"), format));



            /*string folderPath = @"C:\temp1\";
            string filePath = @"a_content.docx";
            var text = await Converter.Docx2TexPandoc(folderPath, filePath);
            txtLog.Text = text.StandardOutput;
            Console.WriteLine(text.StandardOutput);

            filePath = filePath.Replace(".docx",".tex");
            var text1 = await Converter.Tex2MathMLPandoc(folderPath, filePath);
            txtLog.Text = text1.StandardOutput;
            Console.WriteLine(text1.StandardOutput);*/
            /*var text2 = await Converter.Docx2Tex1(@"C:\temp\eduquestionbatch\202106243732650315927\obj202106243732650315927489425_content.docx");
            txtLog.Text = text2.StandardOutput;
            Console.WriteLine(text2.StandardOutput);*/






        }

        async private void btnWord2MathML_Click(object sender, EventArgs e)
        {
            string log = "btnWord2MathML_Click>";
            DateTime s = DateTime.Now;
            QuestionBatch qbTest = new QuestionBatch();
            qbTest.id = 1000;
            qbTest.question_file = @"e/e";
            ProcessQuestionResult rs = await ProcessBatchQuestion(config, qbTest, DateTime.Now.ToString("yyyy-MM-dd"));
            List<QuestionExt> qs = rs.Questions;
            TimeSpan t = DateTime.Now.Subtract(s);

            log = CLogger.Append(log, "total time = ", t.Seconds);
            CLogger.WriteLogSuccess(log);

        }

#region pandoc
        class ProcessQuestionResult
        {
            public bool IsValid = true;
            public List<QuestionExt> Questions = new List<QuestionExt>();
            public List<QuestionError> QuestionErrors = new List<QuestionError>();
            public string Log = "";
        }
        async private Task<ProcessQuestionResult> ProcessBatchQuestion(QuestionProcessorConfig config, QuestionBatch qb, string dateFolder)
        {
            ProcessQuestionResult rs = new ProcessQuestionResult();
            string log = "ProcessBatchQuestion";
            DateTime st = DateTime.Now;
            List<QuestionExt> questions = new List<QuestionExt>();
            //pre process file
            string fileName = config.UploadFolder + Path.DirectorySeparatorChar + qb.question_file + ".docx";
            if (string.IsNullOrEmpty(fileName))
            {
                log = CLogger.Append(log, "file is empty");
                rs.Log = log;
                rs.Questions = questions;
                
                return rs;
            }

            string filePath = fileName;

            string questionFile = qb.id + ".docx";
            string questionFolder = config.UploadFolder +
                Path.DirectorySeparatorChar +
                dateFolder +
                Path.DirectorySeparatorChar +
                qb.id;
            if (!Directory.Exists(questionFolder))
            {
                Directory.CreateDirectory(questionFolder);
            }

            string destFileOrg = questionFolder +
                Path.DirectorySeparatorChar +
                questionFile;
            string destFileConvert = destFileOrg.Replace(".docx", "_c.docx");

            log = CLogger.Append(log, "CloneDocument_MC");
            qp.CloneDocument_MC(filePath, destFileOrg);
            log += "OK> ";

            CheckQuestion checkQuestion = await IsValidDocx(questionFolder, questionFile);
            if (!checkQuestion.IsValid)
            {
                log = CLogger.Append(log, "file is not valid");
                rs.IsValid = false;
                rs.Log = checkQuestion.Log;
                rs.Questions = questions;
                rs.QuestionErrors = checkQuestion.BatchError.questions;
                return rs;
            }

            try
            {
                qp.RemoveParagragh_NULL1(destFileOrg);

                int totalQues = qp.ReCountDocument(destFileOrg);
                log += "ReCountDocument-OK> ";

                qp.RemoveParagragh_NULL1(destFileOrg);
                log += "RemoveParagragh_NULL1-OK> ";

                qp.RemoveParagragh_NULL2(destFileOrg);
                log += "RemoveParagragh_NULL2-OK> ";

                qp.CloneDocument_MC(destFileOrg, destFileConvert);
                log += "CloneDocument_MC-OK> ";

                qp.ConvertObjectToPicture(destFileConvert);
                log += "ConvertObjectToPicture-OK> ";


                qp.RemoveParagragh_NULL3(destFileConvert);
                log += "RemoveParagragh_NULL3-OK> ";

                qp.ReplaceFromQuestion(destFileConvert);
                log += "ReplaceFromQuestion-OK> ";

                //Thread splitQuestionDoc = new Thread(() => qp.SplitQuestionDocument(config.UploadFolder, dateFolder, qb.id.ToString(), destFileOrg));
                //splitQuestionDoc.Start();
                qp.SplitQuestionDocument(config.UploadFolder, dateFolder, qb.id.ToString(), destFileOrg);
                log += "SplitQuestionDocument-OK> ";

                int enableReformatMathType = ConfigHelper.GetConfig("EnableReformatMathType", 0);
                if (enableReformatMathType == 1)
                {
                    qp.ReformatEquations(destFileConvert, ref log);
                    log += "ExtractMathML_repair-OK> ";
                }
            }catch(Exception eFormat)
            {
                log = CLogger.Append(log, "File lỗi hệ thống không xử lý được - ", eFormat.Message);
                rs.IsValid = false;
                rs.Log = "File lỗi hệ thống không xử lý được - " + eFormat.Message;
                rs.Questions = questions;
                rs.QuestionErrors = new List<QuestionError>();
                return rs;
            }
            //qp.RepairChemicalFormula_MC(destFileConvert);
            //log += "RepairChemicalFormula_MC-OK> ";
            //==========================================================
            try
            {
                string convertFile = questionFile.Replace(".docx", "_c.docx");
                log = CLogger.Append(log, "Docx2TexPandoc");
                var cmd = await Converter.Docx2TexPandoc(questionFolder, convertFile);
                log = CLogger.Append(log, GetLog(cmd));
                log = CLogger.Append(log, "Tex2MathMLPandoc");
                cmd = await Converter.Tex2MathMLPandoc(questionFolder, convertFile.Replace(".docx", ".tex"));
                log = CLogger.Append(log, GetLog(cmd));
                log = CLogger.Append(log, "Image2MathML");
                //string batchMathML = Converter.ExportWMFToMathML(questionFolder, convertFile.Replace(".docx", ".html"));
                cmd = await Converter.Image2MathML(questionFolder, convertFile.Replace(".docx", ".html"));
                log = CLogger.Append(log, GetLog(cmd));

                string finalFile = questionFolder + Path.DirectorySeparatorChar + convertFile.Replace(".docx", "_f.html");
                log = CLogger.Append(log, "ReadAllText");
                string htmlFromFile = File.ReadAllText(finalFile);
                //htmlFromFile = htmlFromFile.Replace("\r\n", Environment.NewLine);
                string uploadFolderURL = config.UploadFolder.Replace(@"\", "/");
                htmlFromFile = htmlFromFile.Replace(uploadFolderURL, config.UploadURL);
                string pt = ConfigHelper.GetConfig("Question_Replace");
                List<string> htmls = htmlFromFile.Split(new string[] { pt }, StringSplitOptions.None).ToList();
                int i = 0;
                log = CLogger.Append(log, "BuildQuestion");
                foreach (string html in htmls)
                {
                    if (string.IsNullOrEmpty(html)) continue;
                    if (i == 0 && html.IndexOf("A.") == -1) continue;

                    log = CLogger.Append(log, i);
                    QuestionExt q = BuildQuestion(qb, html, dateFolder, i.ToString());
                    questions.Add(q);
                    i++;
                    //txtLog.Text += Environment.NewLine + html + Environment.NewLine + "---------------------------";
                }
                //txtLog.Text = JsonHelper.ToJsonString(questions);
                TimeSpan tp = DateTime.Now.Subtract(st);
                log = CLogger.Append(log, "process seconds: ", tp.TotalSeconds);
                //CLogger.WriteLogSuccess(log);
                rs.IsValid = true;
            }catch(Exception exxx)
            {
                rs.IsValid = false;
                log = CLogger.Append(log, "exxx: ", exxx.Message);
                CLogger.WriteLogException("Xu ly html--->" + log);
                rs.Log = log;
                rs.Questions = questions;
                rs.QuestionErrors = new List<QuestionError>();
            }
            rs.Log = log;
            rs.Questions = questions;
            return rs;
        }
#endregion pandoc


        private void btnTable2Image_Click(object sender, EventArgs e)
        {
            //           string s = @" 

            //<math xmlns='http://www.w3.org/1998/Math/MathML'>
            // <semantics>
            //  <mrow>
            //   <mfenced>
            //    <mrow>
            //     <mo>&#x2212;</mo><mo>&#x221E;</mo><mo>;</mo><mn>0</mn></mrow>
            //   </mfenced></mrow>
            //  <annotation encoding='MathType-MTEF'>MathType@MTEF@5@5@+=
            //  feaahqart1ev3aqatCvAUfeBSjuyZL2yd9gzLbvyNv2CaerbuLwBLn
            //  hiov2DGi1BTfMBaeXatLxBI9gBaerbd9wDYLwzYbItLDharqqtubsr
            //  4rNCHbGeaGqiVu0Je9sqqrpepC0xbbL8F4rqqrFfpeea0xe9Lq=Jc9
            //  vqaqpepm0xbba9pwe9Q8fs0=yqaqpepae9pg0FirpepeKkFr0xfr=x
            //  fr=xb9adbaqaaeGaciGaaiaabeqaamaabaabaaGcbaWaaeWaaeaacq
            //  GHsislcqGHEisPcaGG7aGaaGimaaGaayjkaiaawMcaaaaa@3B54@
            //  </annotation>
            // </semantics>
            //</math>


            //.
            //";
            //           string pt = @"<annotation .*<\annotation>";
            //           s = Regex.Replace(s, "<annotation(.|\n)*?</annotation>", string.Empty);
            //           txtLog.Text = s;
            //               //Test();
            //           return;
            QuestionProcessor qp = new QuestionProcessor();
            string file = @"C:\temp1\e\e.docx";
            qp.ConvertObjectToPicture(file);
        }

        string GetRightAnswer(ref string q)
        {
            string rs = "";
            if (q.Contains("<u>A.</u>")) rs = "A";
            if (q.Contains("<u>B.</u>")) rs = "B";
            if (q.Contains("<u>C.</u>")) rs = "C";
            if (q.Contains("<u>D.</u>")) rs = "D";
            q = q.Replace("<u>", "").Replace("</u>", "");
            return rs;
        }
        QuestionExt BuildQuestion(QuestionBatch quesionBatch, string html, string dateFolder, string questionFile)
        {
            QuestionExt q = new QuestionExt();
            string ar = GetRightAnswer(ref html);
            int indexA = html.IndexOf("A.");
            int indexB = html.IndexOf("B.");
            int indexC = html.IndexOf("C.");
            int indexD = html.IndexOf("D.");
            int indexAnswer = html.IndexOf(ConfigHelper.GetConfig("Question_Answer", "LOIGIAI"));
            int lengthAnswer = 7;
            if (indexAnswer == -1)
            {
                indexAnswer = html.IndexOf("Lời giải");
                lengthAnswer = 9;
            }

            string content = html.Substring(0, indexA);
            string a = html.Substring(indexA + 2, indexB - indexA - 2);

            string b = html.Substring(indexB + 2, indexC - indexB - 2);
            string c = html.Substring(indexC + 2, indexD - indexC - 2);

            string d = "";
            if(indexAnswer != -1)
            {
                d = html.Substring(indexD + 2, indexAnswer - indexD - 2);
            }
            else
            {
                d = html.Substring(indexD + 2, html.Length - indexD - 2);
            }
            
            string answer = "";
            if(indexAnswer != -1)
            {
                answer = html.Substring(indexAnswer + lengthAnswer, html.Length - indexAnswer - lengthAnswer);
            }

            q.chapter_id = quesionBatch.chapter_id;
            q.resource_id = quesionBatch.resource_id;

            q.type = quesionBatch.type;
            q.level = quesionBatch.level;
            q.question_file = questionFile;
            q.full_question_file = dateFolder;

            q.course_id = quesionBatch.course_id;
            q.subject_id = quesionBatch.subject_id;

            q.content = content;
            q.short_explain = answer;
            q.full_explain = answer;
            q.a1 = a;
            q.a2 = b;
            q.a3 = c;
            q.a4 = d;
            q.ar = ar;

            q.contentTex = content;
            q.aShortTex = answer;
            q.aFullTex = answer;
            q.a1Tex = a;
            q.a2Tex = b;
            q.a3Tex = c;
            q.a4Tex = d;
            q.arTex = ar;

            q.contentMathMLHtml = content;
            q.aShortMathMLHtml = answer;
            q.aFullMathMLHtml = answer;
            q.a1MathMLHtml = a;
            q.a2MathMLHtml = b;
            q.a3MathMLHtml = c;
            q.a4MathMLHtml = d;
            q.arMathMLHtml = ar;


            q.class_id = quesionBatch.class_id;
            q.created_by = quesionBatch.created_by;
            q.modified_by = questionFile;
            q.published = 0;
            q.review_status = 0;
            q.part_id = 0;
            q.tool_id = 0;
            q.school_id = NumberHelper.ConvertToInt(quesionBatch.school_id);
            q.lesson_id = quesionBatch.lession_id;
            q.batch_id = quesionBatch.id;
            q.user_id = quesionBatch.user_id;

            return q;

        }
        private void parseHtmlFile_Click(object sender, EventArgs e)
        {
            string log = "";
            List<QuestionBatch> qbs = GetQuestionBatch(ref log);
            return;
            string file = @"C:\temp1\e\e_convert_final.html";

            string htmlFromFile = File.ReadAllText(file);
            List<Question> questions = new List<Question>();
            string pt = ConfigHelper.GetConfig("Question_Replace");
            List<string> htmls = htmlFromFile.Split(new string[] { pt }, StringSplitOptions.None).ToList();
            foreach (string html in htmls)
            {
                txtLog.Text += Environment.NewLine + html + Environment.NewLine + "---------------------------";
            }
        }

        private void btnParseV2_Click(object sender, EventArgs e)
        {
            ProcessV2(3);
        }

        private void btnW2ML_Click(object sender, EventArgs e)
        {
            QuestionProcessor qb = new QuestionProcessor();
            SessionInfo s = new SessionInfo();
            DialogResult file = openFileDialog1.ShowDialog();
            string log = "";
            if (file == DialogResult.OK)
            {
                txtFileName.Text = openFileDialog1.FileName;
                string f = openFileDialog1.FileName;
                if (!string.IsNullOrEmpty(openFileDialog1.FileName) && openFileDialog1.FileName.Contains(".docx"))
                {
                    int n = 11;
                    qb.ExtractMathML_double(n, ref f, ref s, ref log);
                    txtLog.Text = log;
                    //string fHTML = f.Replace(".docx", ".html");

                    //qb.SaveAs(f, WdSaveFormat.wdFormatRTF, ".rtf");

                    //qb.SpireDoc2Html(f);
                }
            }
        }

        bool IsValidQuestion(string html, ref QuestionError q, ref string log)
        {
            bool rs = true;
            string ar = GetRightAnswer(ref html);
            int indexA = html.IndexOf("A.");
            int indexB = html.IndexOf("B.");
            int indexC = html.IndexOf("C.");
            int indexD = html.IndexOf("D.");
            //int indexAnswer = html.IndexOf(ConfigHelper.GetConfig("Question_Answer", "LOIGIAI"));
            //if (indexAnswer == -1)
            //{
            //    indexAnswer = html.IndexOf("Lời giải");
            //}

            int indexAnswer = html.IndexOf(ConfigHelper.GetConfig("Question_Answer", "LOIGIAI"));
            int lengthAnswer = 7;
            if (indexAnswer == -1)
            {
                indexAnswer = html.IndexOf("Lời giải");
                lengthAnswer = 9;
            }
            if (indexA == -1)
            {
                rs = false;
                q.a = "Thiếu đáp án A. ";
                log += q.a;
            }
            if (indexB == -1)
            {
                rs = false;
                q.b = "Thiếu đáp án B. ";
                log += q.b;
            }
            if (indexC == -1)
            {
                rs = false;
                q.c = "Thiếu đáp án C. ";
                log += q.c;
            }
            if (indexD == -1)
            {
                rs = false;
                q.d = "Thiếu đáp án D. ";
                log += q.d;
            }
            if (indexAnswer == -1 && config.ByPassCheckExplain != 1)
            {
                rs = false;
                q.answer = "Thiếu lời giải ";
                log += q.answer;
            }

            if (indexD < indexC)
            {
                rs = false;
                q.d = "Dư D. trong đề thi ";
                log += q.d;
            }
            if (indexC < indexB)
            {
                rs = false;
                q.d = "Dư C. trong đề thi ";
                log += q.d;
            }
            if (indexB < indexA)
            {
                rs = false;
                q.d = "Dư B. trong đề thi ";
                log += q.d;
            }
            try
            {
                string content = html.Substring(0, indexA);
                string a = html.Substring(indexA + 2, indexB - indexA - 2);

                string b = html.Substring(indexB + 2, indexC - indexB - 2);
                string c = html.Substring(indexC + 2, indexD - indexC - 2);

                string d = "";
                if (indexAnswer != -1)
                {
                    d = html.Substring(indexD + 2, indexAnswer - indexD - 2);
                }
                else
                {
                    d = html.Substring(indexD + 2, html.Length - indexD - 2);
                }

                string answer = "";
                if (indexAnswer != -1)
                {
                    answer = html.Substring(indexAnswer + lengthAnswer, html.Length - indexAnswer - lengthAnswer);
                }
            }catch(Exception e)
            {
                rs = false;
                q.d = "Đề sai cấu trúc, vui lòng kiểm tra lại ";
                log += q.d;
            }
            return rs;
        }
        [Serializable]
        public class BatchError{
            public string error = "";
            public List<QuestionError> questions = new List<QuestionError>();
        }
        [Serializable]
        public class QuestionError {
            public long id = 0;
            public string error = "";
            public string content = "";
            public string a = "";
            public string b = "";
            public string c = "";
            public string d = "";
            public string ar = "";
            public string answer = "";
        }
        class CheckQuestion
        {
            public bool IsValid = true;
            public string Log = "";
            public BatchError BatchError = new BatchError();
            
            public CheckQuestion(bool isValid, string log)
            {
                this.IsValid = isValid;
                this.Log = log;
            }
        }
        async Task<CheckQuestion> IsValidDocx(string questionFolder, string file)
        {
            CheckQuestion rs = new CheckQuestion(true, "");
            string checkText = questionFolder + Path.DirectorySeparatorChar + file.Replace(".docx", "_check.txt");
            var cmd = await Converter.Docx2TextPandoc(questionFolder, file, checkText);
            string htmlFromFile = File.ReadAllText(checkText);
            htmlFromFile = htmlFromFile.Trim();
            string replacement = ConfigHelper.GetConfig("Question_Replace", "CAUXXX.") + " ";
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\d+\.", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\d+\s+", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s{2,1000}\d+\.", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s{2,1000}\d+\s+\.", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s\d+\s+", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s\d+\.{2,1000}", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s\d+\.", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s\d+\s+\.", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s+\d+\.\s+", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s+\d+\:\s+", replacement);

            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s+\d+\.", replacement);
            htmlFromFile = Regex.Replace(htmlFromFile, @"Câu\s+\d+\:", replacement);

            htmlFromFile = Regex.Replace(htmlFromFile, @"CAUXXX\.\s+", replacement);

            htmlFromFile = Regex.Replace(htmlFromFile, @"(\s*|\t*)Lời giải", ConfigHelper.GetConfig("Question_Answer", "LOIGIAI"));

            int indexReplacement = htmlFromFile.IndexOf(replacement);
            if (indexReplacement > 0)
            {
                htmlFromFile = htmlFromFile.Substring(indexReplacement);
            }
            string pt = ConfigHelper.GetConfig("Question_Replace");
            List<string> htmls = htmlFromFile.Split(new string[] { pt }, StringSplitOptions.None).ToList();
            int i = 1;
            string log = "";
            BatchError batchError = new BatchError();
            List<QuestionError> qs = new List<QuestionError>();
            
            foreach (string html in htmls)
            {
                if (string.IsNullOrEmpty(html))
                {
                    continue;
                }
                string logQuestion = "";
                QuestionError q = new QuestionError();
                if (!IsValidQuestion(html, ref q, ref logQuestion))
                {
                    rs.IsValid = false;
                    log += string.Format("Lỗi tại câu {0}: {1}", i, logQuestion);
                    q.error = string.Format("Lỗi tại câu {0}", i);
                    q.id = i;
                    qs.Add(q);
                }
                i++;
            }
            rs.Log = log;
            if(rs.IsValid == false)
            {
                batchError.questions = qs;
            }
            rs.BatchError = batchError;
            return rs;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            CLogger.WriteInfo("Start at: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            var task = System.Threading.Tasks.Task.Run(async () => await ProcessV2(1));
            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception;
            }
            string rs = task.Result;
            CLogger.WriteInfo("Return at: " + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + ">>" + rs);
        }
    }
}
