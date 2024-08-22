using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EARS.Entities
{
    [Serializable]
    public class UploadQuestion
    {
        public QuestionBatch questionBatchTeacher { get; set; }
        public List<Question> questions { get; set; }
    }
    [Serializable]
    public class Question
    {
        public long id;
        public int chapter_id;
        public int resource_id;
        public string content;
        public string short_explain;
        public string full_explain;
        public int type;
        public int level;
        public string question_file;
        public string full_question_file;
        public DateTime created;
        public DateTime modified;
        public string created_by;
        public string modified_by;
        public int published;
        public int review_status;
        public int course_id;
        public int subject_id;
        public string a1;
        public string a2;
        public string a3;
        public string a4;
        public string ar;
        public int class_id;
        public int part_id;
        public int tool_id;
        public int school_id;
        public int batch_id;
        public int lesson_id;
        public long user_id;
    }
    [Serializable]
    public class QuestionPart
    {
        public const string content = "content";
        public const string a1 = "a1";
        public const string a2 = "a2";
        public const string a3 = "a3";
        public const string a4 = "a4";
        public const string a5 = "a5";
        public const string a6 = "a6";
        public const string a7 = "a7";
        public const string a8 = "a8";
        public const string ar = "ar";

        public const string aShort = "ashort";
        public const string aFull = "afull";
    }
    [Serializable]
    public class QuestionExt : Question
    {
        public string question_file_optimization;
        public string question_file_pdf;
        public string question_file_table;
        public string a5;
        public string a6;
        public string a7;
        public string a8;


        public string contentDocx;
        public string aShortDocx;
        public string aFullDocx;
        public string a1Docx;
        public string a2Docx;
        public string a3Docx;
        public string a4Docx;
        public string a5Docx;
        public string a6Docx;
        public string a7Docx;
        public string a8Docx;
        public string arDocx;

        public string contentBase64ImageHtml;
        public string aShortBase64ImageHtml;
        public string aFullBase64ImageHtml;
        public string a1Base64ImageHtml;
        public string a2Base64ImageHtml;
        public string a3Base64ImageHtml;
        public string a4Base64ImageHtml;
        public string a5Base64ImageHtml;
        public string a6Base64ImageHtml;
        public string a7Base64ImageHtml;
        public string a8Base64ImageHtml;
        public string arBase64ImageHtml;

        public string contentPngImageHtml;
        public string aShortPngImageHtml;
        public string aFullPngImageHtml;
        public string a1PngImageHtml;
        public string a2PngImageHtml;
        public string a3PngImageHtml;
        public string a4PngImageHtml;
        public string a5PngImageHtml;
        public string a6PngImageHtml;
        public string a7PngImageHtml;
        public string a8PngImageHtml;
        public string arPngImageHtml;

        public string contentTex;
        public string aShortTex;
        public string aFullTex;
        public string a1Tex;
        public string a2Tex;
        public string a3Tex;
        public string a4Tex;
        public string a5Tex;
        public string a6Tex;
        public string a7Tex;
        public string a8Tex;
        public string arTex;

        public string contentMathMLHtml;
        public string aShortMathMLHtml;
        public string aFullMathMLHtml;
        public string a1MathMLHtml;
        public string a2MathMLHtml;
        public string a3MathMLHtml;
        public string a4MathMLHtml;
        public string a5MathMLHtml;
        public string a6MathMLHtml;
        public string a7MathMLHtml;
        public string a8MathMLHtml;
        public string arMathMLHtml;
    }
}
