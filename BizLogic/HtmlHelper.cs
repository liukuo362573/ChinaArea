using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

namespace ChinaArea
{
    public class HtmlHelper
    {
        public static string GetHtmlContent(string url, string encoding = "")
        {
            string html = string.Empty;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                Stream myResponseStream = response.Content.ReadAsStreamAsync().Result;
                StreamReader myStreamReader = new StreamReader(myResponseStream, string.IsNullOrEmpty(encoding) ? Encoding.UTF8 : Encoding.GetEncoding(encoding));
                html = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
            }
            return html;
        }

        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = Regex.Replace(html, "<[^>]+>", string.Empty);
            strText = Regex.Replace(strText, "&[^;]+;", string.Empty);

            if (length > 0 && strText.Length > length)
            {
                return strText.Substring(0, length);
            }
            return strText;
        }
    }
}
