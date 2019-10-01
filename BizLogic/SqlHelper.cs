using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChinaArea
{
    public class SqlHelper
    {
        #region 生成Mysql插入语句
        /// <summary>
        /// 生成Mysql插入语句
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetMySqlInsert(List<Area> list)
        {
            string create = @"CREATE TABLE IF NOT EXISTS `sys_area` (
                                  `area_code` bigint(10) NOT NULL COMMENT '编号（规则生成）',
                                  `parent_area_code` bigint(10) NOT NULL COMMENT '父地区ID',
                                  `area_name` varchar(100) NOT NULL COMMENT '地区名',
                                  `area_level` int(10) NOT NULL COMMENT '级别',
                                  PRIMARY KEY(`id`,`parent_id`)
                                ) ENGINE = InnoDB DEFAULT CHARSET = utf8 COMMENT = '地区表'; ";
            string insert = "INSERT INTO `sys_area` (`area_code`, `parent_area_code`, `area_name`,`area_level`) VALUES";
            string format = "({0}, {1}, '{2}', {3}),";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(create);
            sb.AppendLine(insert);
            foreach (Area province in list)
            {
                sb.AppendLine(string.Format(format, province.AreaCode, 0, province.AreaName, (int)AreaLevelEnum.Province));
                if (province.ChildrenList != null)
                {
                    foreach (Area city in province.ChildrenList)
                    {
                        sb.AppendLine(string.Format(format, city.AreaCode, province.AreaCode, city.AreaName, (int)AreaLevelEnum.City));
                        if (city.ChildrenList != null)
                        {
                            foreach (Area county in city.ChildrenList)
                            {
                                sb.AppendLine(string.Format(format, county.AreaCode, city.AreaCode, county.AreaName, (int)AreaLevelEnum.County));
                            }
                        }
                    }
                }
            }
            string sql = sb.ToString().Trim().TrimEnd(',');
            return sql;
        }
        #endregion

        #region 生成Sqlserver插入语句
        /// <summary>
        /// 生成Mysql插入语句
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetSqlServerInsert(List<Area> list)
        {
            string create = @"CREATE TABLE [dbo].[sys_area](
	                            [area_code] [int] NOT NULL,
	                            [parent_area_code] [int] NOT NULL,
	                            [area_name] [varchar](50) NOT NULL,
	                            [area_level] [int] NOT NULL,
                              CONSTRAINT [PK_sys_area] PRIMARY KEY CLUSTERED 
                             (
	                             [area_code] ASC
                             )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                             ) ON [PRIMARY]";
            string insert = "INSERT INTO sys_area (area_code, parent_area_code, area_name,area_level) VALUES ({0}, {1}, '{2}', {3})";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(create);
            foreach (Area province in list)
            {
                sb.AppendLine(string.Format(insert, province.AreaCode, 0, province.AreaName, (int)AreaLevelEnum.Province));
                if (province.ChildrenList != null)
                {
                    foreach (Area city in province.ChildrenList)
                    {
                        sb.AppendLine(string.Format(insert, city.AreaCode, province.AreaCode, city.AreaName, (int)AreaLevelEnum.City));
                        if (city.ChildrenList != null)
                        {
                            foreach (Area county in city.ChildrenList)
                            {
                                sb.AppendLine(string.Format(insert, county.AreaCode, city.AreaCode, county.AreaName, (int)AreaLevelEnum.County));
                            }
                        }
                    }
                }
            }
            string sql = sb.ToString().Trim().TrimEnd(',');
            return sql;
        }
        #endregion
    }
}

