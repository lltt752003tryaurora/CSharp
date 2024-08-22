using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EARS.Entities
{
    public class QuestionV1
    {
        public string File { get; set; }
        public string HTML { get; set; }
        public string q { get; set; }
        public string a1 { get; set; }
        public string a2 { get; set; }
        public string a3 { get; set; }
        public string a4 { get; set; }
        public string ar { get; set; }
        public string a { get; set; }

        public QuestionV1()
        {
        }

        public void ParseData()
        {
            int indexOfA = this.HTML.IndexOf("@BEGIN_A@");
            this.q = HTML.Substring(0, indexOfA);

            int indexOfB = this.HTML.IndexOf("@BEGIN_B@");
            this.a1 = HTML.Substring(indexOfA + 9, indexOfB - (indexOfA + 9));

            int indexOfC = this.HTML.IndexOf("@BEGIN_C@");
            this.a2 = HTML.Substring(indexOfB + 9, indexOfC - (indexOfB + 9));

            int indexOfD = this.HTML.IndexOf("@BEGIN_D@");
            this.a3 = HTML.Substring(indexOfC + 9, indexOfD - (indexOfC + 9));

            int indexOfHD = this.HTML.IndexOf("@BEGINHUONGDAN@");
            this.a4 = HTML.Substring(indexOfD + 9, indexOfHD - (indexOfD + 9));

            this.a = HTML.Substring(indexOfHD + 15);

            //set ar
            if (this.a1.Contains("@CAUDUNG@"))
            {
                this.a1 = this.a1.Replace("@CAUDUNG@", "");
                this.ar = "A";
            }
            if (this.a2.Contains("@CAUDUNG@"))
            {
                this.a2 = this.a2.Replace("@CAUDUNG@", "");
                this.ar = "B";
            }
            if (this.a3.Contains("@CAUDUNG@"))
            {
                this.a3 = this.a3.Replace("@CAUDUNG@", "");
                this.ar = "C";
            }
            if (this.a4.Contains("@CAUDUNG@"))
            {
                this.a4 = this.a4.Replace("@CAUDUNG@", "");
                this.ar = "D";
            }
        }
    }
}
