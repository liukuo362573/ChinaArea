using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace ChinaArea
{
    class Program
    {

        static void Main(string[] args)
        {
            string url = GlobalConstants.UrlArea;
            string html = HtmlHelper.GetHtmlContent(url);

            Console.WriteLine("开始抓取数据...");
            List<Area> areaList = AreaHelper.GetAreaList(html);
            List<Area> result = AreaHelper.ProcessArea(areaList);
            Console.WriteLine("抓取数据完毕!");

            string directory = AppDomain.CurrentDomain.BaseDirectory + @"data\";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string json = JsonConvert.SerializeObject(result, Formatting.Indented, jsonSetting);
            using (StreamWriter sw = new StreamWriter(directory + "json.txt"))
            {
                sw.WriteLine(json);
            }

            string mySql = SqlHelper.GetMySqlInsert(result);
            using (StreamWriter sw = new StreamWriter(directory + "mysql.txt"))
            {
                sw.WriteLine(mySql);
            }

            string sqlServer = SqlHelper.GetSqlServerInsert(result);
            using (StreamWriter sw = new StreamWriter(directory + "sqlserver.txt"))
            {
                sw.WriteLine(sqlServer);
            }


            Console.Read();
        }

    }
}

