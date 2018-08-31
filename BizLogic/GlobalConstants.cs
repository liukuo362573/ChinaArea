using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ChinaArea
{
    public class GlobalConstants
    {
        public static string UrlArea = "http://www.mca.gov.cn/article/sj/xzqh/2018/201804-12/20180708230813.html";
        public static Regex RegexAreaTr = new Regex("<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        public static Regex RegexAreaTd = new Regex("<td[^>]*>(.*?)</td>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        public static string UrlCounty = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/2017/{0}/{1}.html";
        public static Regex RegexCountyTr = new Regex("<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        public static Regex RegexCountyTd = new Regex("<td[^>]*>(.*?)</td>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

    }
}
