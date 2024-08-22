
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Utilities
{
    public class HtmlHelper
    {
        public static string RemoveStyle(string html)
        {
            try
            {
                var xmlDoc = XDocument.Parse(html);
                // Remove all inline styles
                xmlDoc.Descendants().Attributes("style").Remove();

                // Remove all classes inserted by 3rd party, without removing our own lovely classes
                /*foreach (var node in xmlDoc.Descendants())
                {
                    var classAttribute = node.Attributes("class").SingleOrDefault();
                    if (classAttribute == null)
                    {
                        continue;
                    }
                    var classesThatShouldStay = classAttribute.Value.Split(' ').Where(className => !className.StartsWith("abc"));
                    classAttribute.SetValue(string.Join(" ", classesThatShouldStay));

                }*/

                return xmlDoc.ToString();
            }
            catch {
            }
            return "";
        }
    }
}
