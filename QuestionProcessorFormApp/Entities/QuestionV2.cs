using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EARS.Entities
{
    public enum QuestionV2Col
    {
        QC,
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        AR,
        QA,
        Undefined
    }
    public class QuestionV2
    {
        public string QC { get; set; }//noi dung ca hoi
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public string G { get; set; }
        public string AR { get; set; }//cau dung
        public string QA { get; set; }//cau tra loi
        public string FileDoc { get; set; }//file doc
    }
}
