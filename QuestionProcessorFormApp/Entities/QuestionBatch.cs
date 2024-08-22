using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EARS.Entities
{
    [Serializable]
    public class QuestionBatch
    {
        public int id;
        public string title;
        public int chapter_id;
        public int resource_id;
        public int type;
        public int level;
        public int course_id;
        public int subject_id;
        public string description;
        public int status;
        public string question_file;
        public DateTime created;
        public DateTime modified;
        public string created_by;
        public string modified_by;
        public int num_of_question;
        public int num_of_question_succ;
        public int num_of_question_fail;
        public string list_fail;
        public int class_id;
        public int subject_type;
        public int lession_id;
        public long user_id;
        public long school_id;
    }
}
