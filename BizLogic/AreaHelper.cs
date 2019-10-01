using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ChinaArea
{
    public class AreaHelper
    {
        /// <summary>
        /// 获取全国的的省市县
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<Area> GetAreaList(string html)
        {
            string areaName = string.Empty;
            string areaCode = string.Empty;

            List<Area> areaList = new List<Area>();

            MatchCollection mcTr = GlobalConstants.RegexAreaTr.Matches(html);
            foreach (Match matchTr in mcTr)
            {
                List<string> oneArea = new List<string>();
                string tr = matchTr.Value;
                MatchCollection mcTd = GlobalConstants.RegexAreaTd.Matches(tr);
                foreach (Match matchTd in mcTd)
                {
                    string td = matchTd.Groups[1].Value;
                    if (!string.IsNullOrEmpty(td))
                    {
                        oneArea.Add(td);
                    }
                }
                if (oneArea.Count == 2)
                {
                    areaCode = HtmlHelper.ReplaceHtmlTag(oneArea[0]);
                    areaName = HtmlHelper.ReplaceHtmlTag(oneArea[1]);
                    if (!string.IsNullOrEmpty(areaCode) && !string.IsNullOrEmpty(areaName))
                    {
                        areaList.Add(new Area
                        {
                            AreaCode = areaCode,
                            AreaName = areaName
                        });
                    }
                }
            }
            return areaList;
        }

        /// <summary>
        /// 生成省市县树结构
        /// </summary>
        /// <param name="areaList"></param>
        /// <returns></returns>
        public static List<Area> ProcessArea(List<Area> areaList)
        {
            List<Area> sortedList = areaList.OrderBy(p => p.AreaCode).ToList();
            // 前2位代表省份
            List<Area> provinceList = sortedList.Where(p => p.AreaCode.Substring(2, 4) == "0000").ToList();
            List<Area> includeList = new List<Area>();
            for (int i = 0; i < provinceList.Count; i++)
            {
                Area province = provinceList[i];
                includeList.Clear();
                WriteLine(string.Format("[{0} {1}/{2}]", province.AreaName, i + 1, provinceList.Count));

                // 直辖市
                if (province.AreaName.Contains("市"))
                {
                    Area city = new Area
                    {
                        AreaName = province.AreaName,
                        AreaCode = province.AreaCode.Substring(0, 2) + "0100",
                    };
                    province.AreaName = province.AreaName.Replace("市", "");
                    province.ChildrenList = new List<Area>();
                    province.ChildrenList.Add(city);

                    List<Area> countyList = sortedList.Where(p => p.AreaCode.Substring(0, 4) == city.AreaCode.Substring(0, 4) &&
                                                              p.AreaCode != city.AreaCode).ToList();
                    city.ChildrenList = countyList;
                    AddDefaultCity(city);
                    CustomCity(i, provinceList.Count, province, city);
                    includeList.AddRange(countyList);
                    List<Area> notIncludeList = sortedList.Where(p => p.AreaCode.Substring(0, 2) == province.AreaCode.Substring(0, 2) &&
                                                                 p.AreaCode != province.AreaCode).Where(p => !includeList.Select(a => a.AreaCode).Contains(p.AreaCode)).ToList();
                    city.ChildrenList.AddRange(notIncludeList);
                }
                else
                {
                    List<Area> cityList = sortedList.Where(p => p.AreaCode.Substring(0, 2) == province.AreaCode.Substring(0, 2) &&
                                                           p.AreaCode.Substring(4, 2) == "00" &&
                                                           p.AreaCode != province.AreaCode).ToList();
                    province.ChildrenList = cityList;
                    includeList.AddRange(cityList);
                    foreach (Area city in cityList)
                    {
                        List<Area> countyList = sortedList.Where(p => p.AreaCode.Substring(0, 4) == city.AreaCode.Substring(0, 4) &&
                                             p.AreaCode != city.AreaCode).ToList();
                        city.ChildrenList = countyList;
                        AddDefaultCity(city);
                        CustomCity(i, provinceList.Count, province, city);
                        includeList.AddRange(countyList);
                    }

                    List<Area> notIncludeList = sortedList.Where(p => p.AreaCode.Substring(0, 2) == province.AreaCode.Substring(0, 2) &&
                                                                 p.AreaCode != province.AreaCode).Where(p => !includeList.Select(a => a.AreaCode).Contains(p.AreaCode)).ToList();
                    foreach (Area notInclude in notIncludeList)
                    {
                        AddDefaultCity(notInclude);
                    }
                    province.ChildrenList.AddRange(notIncludeList);
                }
                CustomProvince(province);
            }
            return provinceList;
        }

        /// <summary>
        /// 如果二级市下面没有区县，就在第三级加一个默认的市辖区，AreaCode代码和二级的一样，比如 济源市
        /// </summary>
        /// <param name="city"></param>
        private static void AddDefaultCity(Area city)
        {
            if (city.ChildrenList == null || city.ChildrenList.Count == 0)
            {
                city.ChildrenList = new List<Area>();
                city.ChildrenList.Add(new Area
                {
                    AreaName = "市辖区",
                    AreaCode = city.AreaCode.Substring(0, 3) + "1" + city.AreaCode.Substring(4)
                });
            }
        }

        /// <summary>
        /// 行政区划里面没有台湾、香港和澳门，就手动加入
        /// </summary>
        /// <param name="province"></param>
        private static void CustomProvince(Area province)
        {
            switch (province.AreaName)
            {
                case "台湾省":
                    province.ChildrenList = new List<Area>();
                    #region 台北市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710100",
                        AreaName = "台北市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710101",AreaName = "松山区"},
                           new Area { AreaCode = "710102",AreaName = "信义区"},
                           new Area { AreaCode = "710103",AreaName = "大安区"},
                           new Area { AreaCode = "710104",AreaName = "中山区"},
                           new Area { AreaCode = "710105",AreaName = "中正区"},
                           new Area { AreaCode = "710106",AreaName = "大同区"},
                           new Area { AreaCode = "710107",AreaName = "万华区"},
                           new Area { AreaCode = "710108",AreaName = "文山区"},
                           new Area { AreaCode = "710109",AreaName = "南港区"},
                           new Area { AreaCode = "710110",AreaName = "内湖区"},
                           new Area { AreaCode = "710111",AreaName = "士林区"},
                           new Area { AreaCode = "710112",AreaName = "北投区"}
                        }
                    });
                    #endregion
                    #region 高雄市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710200",
                        AreaName = "高雄市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710201",AreaName = "盐埕区"},
                           new Area { AreaCode = "710202",AreaName = "鼓山区"},
                           new Area { AreaCode = "710203",AreaName = "左营区"},
                           new Area { AreaCode = "710204",AreaName = "楠梓区"},
                           new Area { AreaCode = "710205",AreaName = "三民区"},
                           new Area { AreaCode = "710206",AreaName = "新兴区"},
                           new Area { AreaCode = "710207",AreaName = "前金区"},
                           new Area { AreaCode = "710208",AreaName = "苓雅区"},
                           new Area { AreaCode = "710209",AreaName = "前镇区"},
                           new Area { AreaCode = "710210",AreaName = "旗津区"},
                           new Area { AreaCode = "710211",AreaName = "小港区"},
                           new Area { AreaCode = "710212",AreaName = "凤山区"},
                           new Area { AreaCode = "710213",AreaName = "林园区"},
                           new Area { AreaCode = "710214",AreaName = "大寮区"},
                           new Area { AreaCode = "710215",AreaName = "大树区"},
                           new Area { AreaCode = "710216",AreaName = "大社区"},
                           new Area { AreaCode = "710217",AreaName = "仁武区"},
                           new Area { AreaCode = "710218",AreaName = "鸟松区"},
                           new Area { AreaCode = "710219",AreaName = "冈山区"},
                           new Area { AreaCode = "710220",AreaName = "桥头区"},
                           new Area { AreaCode = "710221",AreaName = "燕巢区"},
                           new Area { AreaCode = "710222",AreaName = "田寮区"},
                           new Area { AreaCode = "710223",AreaName = "阿莲区"},
                           new Area { AreaCode = "710224",AreaName = "路竹区"},
                           new Area { AreaCode = "710225",AreaName = "湖内区"},
                           new Area { AreaCode = "710226",AreaName = "茄萣区"},
                           new Area { AreaCode = "710227",AreaName = "永安区"},
                           new Area { AreaCode = "710228",AreaName = "弥陀区"},
                           new Area { AreaCode = "710229",AreaName = "梓官区"},
                           new Area { AreaCode = "710230",AreaName = "旗山区"},
                           new Area { AreaCode = "710231",AreaName = "美浓区"},
                           new Area { AreaCode = "710232",AreaName = "六龟区"},
                           new Area { AreaCode = "710233",AreaName = "甲仙区"},
                           new Area { AreaCode = "710234",AreaName = "杉林区"},
                           new Area { AreaCode = "710235",AreaName = "内门区"},
                           new Area { AreaCode = "710236",AreaName = "茂林区"},
                           new Area { AreaCode = "710237",AreaName = "桃源区"},
                           new Area { AreaCode = "710238",AreaName = "那玛夏区"}
                        }
                    });
                    #endregion
                    #region 基隆市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710300",
                        AreaName = "基隆市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710301",AreaName = "中正区"},
                           new Area { AreaCode = "710302",AreaName = "七堵区"},
                           new Area { AreaCode = "710303",AreaName = "暖暖区"},
                           new Area { AreaCode = "710304",AreaName = "仁爱区"},
                           new Area { AreaCode = "710305",AreaName = "中山区"},
                           new Area { AreaCode = "710306",AreaName = "安乐区"},
                           new Area { AreaCode = "710307",AreaName = "信义区"}
                        }
                    });
                    #endregion
                    #region 台中市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710400",
                        AreaName = "台中市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710401",AreaName = "中区"},
                           new Area { AreaCode = "710402",AreaName = "东区"},
                           new Area { AreaCode = "710403",AreaName = "南区"},
                           new Area { AreaCode = "710404",AreaName = "西区"},
                           new Area { AreaCode = "710405",AreaName = "北区"},
                           new Area { AreaCode = "710406",AreaName = "西屯区"},
                           new Area { AreaCode = "710407",AreaName = "南屯区"},
                           new Area { AreaCode = "710408",AreaName = "北屯区"},
                           new Area { AreaCode = "710409",AreaName = "丰原区"},
                           new Area { AreaCode = "710410",AreaName = "东势区"},
                           new Area { AreaCode = "710411",AreaName = "大甲区"},
                           new Area { AreaCode = "710412",AreaName = "清水区"},
                           new Area { AreaCode = "710413",AreaName = "沙鹿区"},
                           new Area { AreaCode = "710414",AreaName = "梧栖区"},
                           new Area { AreaCode = "710415",AreaName = "后里区"},
                           new Area { AreaCode = "710416",AreaName = "神冈区"},
                           new Area { AreaCode = "710417",AreaName = "潭子区"},
                           new Area { AreaCode = "710418",AreaName = "大雅区"},
                           new Area { AreaCode = "710419",AreaName = "新社区"},
                           new Area { AreaCode = "710420",AreaName = "石冈区"},
                           new Area { AreaCode = "710421",AreaName = "外埔区"},
                           new Area { AreaCode = "710422",AreaName = "大安区"},
                           new Area { AreaCode = "710423",AreaName = "乌日区"},
                           new Area { AreaCode = "710424",AreaName = "大肚区"},
                           new Area { AreaCode = "710425",AreaName = "龙井区"},
                           new Area { AreaCode = "710426",AreaName = "雾峰区"},
                           new Area { AreaCode = "710427",AreaName = "太平区"},
                           new Area { AreaCode = "710428",AreaName = "大里区"},
                           new Area { AreaCode = "710429",AreaName = "和平区"}
                        }
                    });
                    #endregion
                    #region 台南市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710500",
                        AreaName = "台南市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710501",AreaName = "东区"},
                           new Area { AreaCode = "710502",AreaName = "南区"},
                           new Area { AreaCode = "710504",AreaName = "北区"},
                           new Area { AreaCode = "710506",AreaName = "安南区"},
                           new Area { AreaCode = "710507",AreaName = "安平区"},
                           new Area { AreaCode = "710508",AreaName = "中西区"},
                           new Area { AreaCode = "710509",AreaName = "新营区"},
                           new Area { AreaCode = "710510",AreaName = "盐水区"},
                           new Area { AreaCode = "710511",AreaName = "白河区"},
                           new Area { AreaCode = "710512",AreaName = "柳营区"},
                           new Area { AreaCode = "710513",AreaName = "后壁区"},
                           new Area { AreaCode = "710514",AreaName = "东山区"},
                           new Area { AreaCode = "710515",AreaName = "麻豆区"},
                           new Area { AreaCode = "710516",AreaName = "下营区"},
                           new Area { AreaCode = "710517",AreaName = "六甲区"},
                           new Area { AreaCode = "710518",AreaName = "官田区"},
                           new Area { AreaCode = "710519",AreaName = "大内区"},
                           new Area { AreaCode = "710520",AreaName = "佳里区"},
                           new Area { AreaCode = "710521",AreaName = "学甲区"},
                           new Area { AreaCode = "710522",AreaName = "西港区"},
                           new Area { AreaCode = "710523",AreaName = "七股区"},
                           new Area { AreaCode = "710524",AreaName = "将军区"},
                           new Area { AreaCode = "710525",AreaName = "北门区"},
                           new Area { AreaCode = "710526",AreaName = "新化区"},
                           new Area { AreaCode = "710527",AreaName = "善化区"},
                           new Area { AreaCode = "710528",AreaName = "新市区"},
                           new Area { AreaCode = "710529",AreaName = "安定区"},
                           new Area { AreaCode = "710530",AreaName = "山上区"},
                           new Area { AreaCode = "710531",AreaName = "玉井区"},
                           new Area { AreaCode = "710532",AreaName = "楠西区"},
                           new Area { AreaCode = "710533",AreaName = "南化区"},
                           new Area { AreaCode = "710534",AreaName = "左镇区"},
                           new Area { AreaCode = "710535",AreaName = "仁德区"},
                           new Area { AreaCode = "710536",AreaName = "归仁区"},
                           new Area { AreaCode = "710537",AreaName = "关庙区"},
                           new Area { AreaCode = "710538",AreaName = "龙崎区"},
                           new Area { AreaCode = "710539",AreaName = "永康区"},
                        }
                    });
                    #endregion
                    #region 新竹市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710600",
                        AreaName = "新竹市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710601",AreaName = "东区"},
                           new Area { AreaCode = "710602",AreaName = "北区"},
                           new Area { AreaCode = "710603",AreaName = "香山区"}
                        }
                    });
                    #endregion
                    #region 嘉义市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710700",
                        AreaName = "嘉义市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710701",AreaName = "东区"},
                           new Area { AreaCode = "710702",AreaName = "西区"}
                        }
                    });
                    #endregion
                    #region 新北市
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "710800",
                        AreaName = "新北市",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "710801",AreaName = "板桥区"},
                           new Area { AreaCode = "710802",AreaName = "三重区"},
                           new Area { AreaCode = "710803",AreaName = "中和区"},
                           new Area { AreaCode = "710804",AreaName = "永和区"},
                           new Area { AreaCode = "710805",AreaName = "新庄区"},
                           new Area { AreaCode = "710806",AreaName = "新店区"},
                           new Area { AreaCode = "710807",AreaName = "树林区"},
                           new Area { AreaCode = "710808",AreaName = "莺歌区"},
                           new Area { AreaCode = "710809",AreaName = "三峡区"},
                           new Area { AreaCode = "710810",AreaName = "淡水区"},
                           new Area { AreaCode = "710811",AreaName = "汐止区"},
                           new Area { AreaCode = "710812",AreaName = "瑞芳区"},
                           new Area { AreaCode = "710813",AreaName = "土城区"},
                           new Area { AreaCode = "710814",AreaName = "芦洲区"},
                           new Area { AreaCode = "710815",AreaName = "五股区"},
                           new Area { AreaCode = "710816",AreaName = "泰山区"},
                           new Area { AreaCode = "710817",AreaName = "林口区"},
                           new Area { AreaCode = "710818",AreaName = "深坑区"},
                           new Area { AreaCode = "710819",AreaName = "石碇区"},
                           new Area { AreaCode = "710820",AreaName = "坪林区"},
                           new Area { AreaCode = "710821",AreaName = "三芝区"},
                           new Area { AreaCode = "710822",AreaName = "石门区"},
                           new Area { AreaCode = "710823",AreaName = "八里区"},
                           new Area { AreaCode = "710824",AreaName = "平溪区"},
                           new Area { AreaCode = "710825",AreaName = "双溪区"},
                           new Area { AreaCode = "710826",AreaName = "贡寮区"},
                           new Area { AreaCode = "710827",AreaName = "金山区"},
                           new Area { AreaCode = "710828",AreaName = "万里区"},
                           new Area { AreaCode = "710829",AreaName = "乌来区"},
                        }
                    });
                    #endregion
                    #region 宜兰县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "712200",
                        AreaName = "宜兰县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "712201",AreaName = "宜兰市"},
                           new Area { AreaCode = "712221",AreaName = "罗东镇"},
                           new Area { AreaCode = "712222",AreaName = "苏澳镇"},
                           new Area { AreaCode = "712223",AreaName = "头城镇"},
                           new Area { AreaCode = "712224",AreaName = "礁溪乡"},
                           new Area { AreaCode = "712225",AreaName = "壮围乡"},
                           new Area { AreaCode = "712226",AreaName = "员山乡"},
                           new Area { AreaCode = "712227",AreaName = "冬山乡"},
                           new Area { AreaCode = "712228",AreaName = "五结乡"},
                           new Area { AreaCode = "712229",AreaName = "三星乡"},
                           new Area { AreaCode = "712230",AreaName = "大同乡"},
                           new Area { AreaCode = "712231",AreaName = "南澳乡"}
                        }
                    });
                    #endregion
                    #region 桃园县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "712300",
                        AreaName = "桃园县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "712301",AreaName = "桃园市"},
                           new Area { AreaCode = "712302",AreaName = "中坜市"},
                           new Area { AreaCode = "712303",AreaName = "平镇市"},
                           new Area { AreaCode = "712304",AreaName = "八德市"},
                           new Area { AreaCode = "712305",AreaName = "杨梅市"},
                           new Area { AreaCode = "712321",AreaName = "大溪镇"},
                           new Area { AreaCode = "712323",AreaName = "芦竹乡"},
                           new Area { AreaCode = "712324",AreaName = "大园乡"},
                           new Area { AreaCode = "712325",AreaName = "龟山乡"},
                           new Area { AreaCode = "712327",AreaName = "龙潭乡"},
                           new Area { AreaCode = "712329",AreaName = "新屋乡"},
                           new Area { AreaCode = "712330",AreaName = "观音乡"},
                           new Area { AreaCode = "712331",AreaName = "复兴乡"},
                        }
                    });
                    #endregion
                    #region 新竹县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "712400",
                        AreaName = "新竹县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "712401",AreaName = "竹北市"},
                           new Area { AreaCode = "712421",AreaName = "竹东镇"},
                           new Area { AreaCode = "712422",AreaName = "新埔镇"},
                           new Area { AreaCode = "712423",AreaName = "关西镇"},
                           new Area { AreaCode = "712424",AreaName = "湖口乡"},
                           new Area { AreaCode = "712425",AreaName = "新丰乡"},
                           new Area { AreaCode = "712426",AreaName = "芎林乡"},
                           new Area { AreaCode = "712427",AreaName = "橫山乡"},
                           new Area { AreaCode = "712428",AreaName = "北埔乡"},
                           new Area { AreaCode = "712429",AreaName = "宝山乡"},
                           new Area { AreaCode = "712430",AreaName = "峨眉乡"},
                           new Area { AreaCode = "712431",AreaName = "尖石乡"},
                           new Area { AreaCode = "712432",AreaName = "五峰乡"}
                        }
                    });
                    #endregion
                    #region 苗栗县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "712500",
                        AreaName = "苗栗县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "712501",AreaName = "苗栗市"},
                           new Area { AreaCode = "712521",AreaName = "苑里镇"},
                           new Area { AreaCode = "712522",AreaName = "通霄镇"},
                           new Area { AreaCode = "712523",AreaName = "竹南镇"},
                           new Area { AreaCode = "712524",AreaName = "头份镇"},
                           new Area { AreaCode = "712525",AreaName = "后龙镇"},
                           new Area { AreaCode = "712526",AreaName = "卓兰镇"},
                           new Area { AreaCode = "712527",AreaName = "大湖乡"},
                           new Area { AreaCode = "712528",AreaName = "公馆乡"},
                           new Area { AreaCode = "712529",AreaName = "铜锣乡"},
                           new Area { AreaCode = "712530",AreaName = "南庄乡"},
                           new Area { AreaCode = "712531",AreaName = "头屋乡"},
                           new Area { AreaCode = "712532",AreaName = "三义乡"},
                           new Area { AreaCode = "712533",AreaName = "西湖乡"},
                           new Area { AreaCode = "712534",AreaName = "造桥乡"},
                           new Area { AreaCode = "712535",AreaName = "三湾乡"},
                           new Area { AreaCode = "712536",AreaName = "狮潭乡"},
                           new Area { AreaCode = "712537",AreaName = "泰安乡"}
                        }
                    });
                    #endregion
                    #region 彰化县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "712700",
                        AreaName = "彰化县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "712701",AreaName = "彰化市"},
                           new Area { AreaCode = "712721",AreaName = "鹿港镇"},
                           new Area { AreaCode = "712722",AreaName = "和美镇"},
                           new Area { AreaCode = "712723",AreaName = "线西乡"},
                           new Area { AreaCode = "712724",AreaName = "伸港乡"},
                           new Area { AreaCode = "712725",AreaName = "福兴乡"},
                           new Area { AreaCode = "712726",AreaName = "秀水乡"},
                           new Area { AreaCode = "712727",AreaName = "花坛乡"},
                           new Area { AreaCode = "712728",AreaName = "芬园乡"},
                           new Area { AreaCode = "712729",AreaName = "员林镇"},
                           new Area { AreaCode = "712730",AreaName = "溪湖镇"},
                           new Area { AreaCode = "712731",AreaName = "田中镇"},
                           new Area { AreaCode = "712732",AreaName = "大村乡"},
                           new Area { AreaCode = "712733",AreaName = "埔盐乡"},
                           new Area { AreaCode = "712734",AreaName = "埔心乡"},
                           new Area { AreaCode = "712735",AreaName = "永靖乡"},
                           new Area { AreaCode = "712736",AreaName = "社头乡"},
                           new Area { AreaCode = "712737",AreaName = "二水乡"},
                           new Area { AreaCode = "712738",AreaName = "北斗镇"},
                           new Area { AreaCode = "712739",AreaName = "二林镇"},
                           new Area { AreaCode = "712740",AreaName = "田尾乡"},
                           new Area { AreaCode = "712741",AreaName = "埤头乡"},
                           new Area { AreaCode = "712742",AreaName = "芳苑乡"},
                           new Area { AreaCode = "712743",AreaName = "大城乡"},
                           new Area { AreaCode = "712744",AreaName = "竹塘乡"},
                           new Area { AreaCode = "712745",AreaName = "溪州乡"}
                        }
                    });
                    #endregion
                    #region 南投县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "712800",
                        AreaName = "南投县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "712801",AreaName = "南投市"},
                           new Area { AreaCode = "712821",AreaName = "埔里镇"},
                           new Area { AreaCode = "712822",AreaName = "草屯镇"},
                           new Area { AreaCode = "712823",AreaName = "竹山镇"},
                           new Area { AreaCode = "712824",AreaName = "集集镇"},
                           new Area { AreaCode = "712825",AreaName = "名间乡"},
                           new Area { AreaCode = "712826",AreaName = "鹿谷乡"},
                           new Area { AreaCode = "712827",AreaName = "中寮乡"},
                           new Area { AreaCode = "712828",AreaName = "鱼池乡"},
                           new Area { AreaCode = "712829",AreaName = "国姓乡"},
                           new Area { AreaCode = "712830",AreaName = "水里乡"},
                           new Area { AreaCode = "712831",AreaName = "信义乡"},
                           new Area { AreaCode = "712832",AreaName = "仁爱乡"}
                        }
                    });
                    #endregion
                    #region 云林县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "712900",
                        AreaName = "云林县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "712901",AreaName = "斗六市"},
                           new Area { AreaCode = "712921",AreaName = "斗南镇"},
                           new Area { AreaCode = "712922",AreaName = "虎尾镇"},
                           new Area { AreaCode = "712923",AreaName = "西螺镇"},
                           new Area { AreaCode = "712924",AreaName = "土库镇"},
                           new Area { AreaCode = "712925",AreaName = "北港镇"},
                           new Area { AreaCode = "712926",AreaName = "古坑乡"},
                           new Area { AreaCode = "712927",AreaName = "大埤乡"},
                           new Area { AreaCode = "712928",AreaName = "莿桐乡"},
                           new Area { AreaCode = "712929",AreaName = "林内乡"},
                           new Area { AreaCode = "712930",AreaName = "二仑乡"},
                           new Area { AreaCode = "712931",AreaName = "仑背乡"},
                           new Area { AreaCode = "712932",AreaName = "麦寮乡"},
                           new Area { AreaCode = "712933",AreaName = "东势乡"},
                           new Area { AreaCode = "712934",AreaName = "褒忠乡"},
                           new Area { AreaCode = "712935",AreaName = "台西乡"},
                           new Area { AreaCode = "712936",AreaName = "元长乡"},
                           new Area { AreaCode = "712937",AreaName = "四湖乡"},
                           new Area { AreaCode = "712938",AreaName = "口湖乡"},
                           new Area { AreaCode = "712939",AreaName = "水林乡"}
                        }
                    });
                    #endregion
                    #region 嘉义县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "713000",
                        AreaName = "嘉义县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "713001",AreaName = "太保市"},
                           new Area { AreaCode = "713002",AreaName = "朴子市"},
                           new Area { AreaCode = "713023",AreaName = "布袋镇"},
                           new Area { AreaCode = "713024",AreaName = "大林镇"},
                           new Area { AreaCode = "713025",AreaName = "民雄乡"},
                           new Area { AreaCode = "713026",AreaName = "溪口乡"},
                           new Area { AreaCode = "713027",AreaName = "新港乡"},
                           new Area { AreaCode = "713028",AreaName = "六脚乡"},
                           new Area { AreaCode = "713029",AreaName = "东石乡"},
                           new Area { AreaCode = "713030",AreaName = "义竹乡"},
                           new Area { AreaCode = "713031",AreaName = "鹿草乡"},
                           new Area { AreaCode = "713032",AreaName = "水上乡"},
                           new Area { AreaCode = "713033",AreaName = "中埔乡"},
                           new Area { AreaCode = "713034",AreaName = "竹崎乡"},
                           new Area { AreaCode = "713035",AreaName = "梅山乡"},
                           new Area { AreaCode = "713036",AreaName = "番路乡"},
                           new Area { AreaCode = "713037",AreaName = "大埔乡"},
                           new Area { AreaCode = "713038",AreaName = "阿里山乡"}
                        }
                    });
                    #endregion
                    #region 屏东县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "713300",
                        AreaName = "屏东县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "713301",AreaName = "屏东市"},
                           new Area { AreaCode = "713321",AreaName = "潮州镇"},
                           new Area { AreaCode = "713322",AreaName = "东港镇"},
                           new Area { AreaCode = "713323",AreaName = "恒春镇"},
                           new Area { AreaCode = "713324",AreaName = "万丹乡"},
                           new Area { AreaCode = "713325",AreaName = "长治乡"},
                           new Area { AreaCode = "713326",AreaName = "麟洛乡"},
                           new Area { AreaCode = "713327",AreaName = "九如乡"},
                           new Area { AreaCode = "713328",AreaName = "里港乡"},
                           new Area { AreaCode = "713329",AreaName = "盐埔乡"},
                           new Area { AreaCode = "713330",AreaName = "高树乡"},
                           new Area { AreaCode = "713331",AreaName = "万峦乡"},
                           new Area { AreaCode = "713332",AreaName = "内埔乡"},
                           new Area { AreaCode = "713333",AreaName = "竹田乡"},
                           new Area { AreaCode = "713334",AreaName = "新埤乡"},
                           new Area { AreaCode = "713335",AreaName = "枋寮乡"},
                           new Area { AreaCode = "713336",AreaName = "新园乡"},
                           new Area { AreaCode = "713337",AreaName = "崁顶乡"},
                           new Area { AreaCode = "713338",AreaName = "林边乡"},
                           new Area { AreaCode = "713339",AreaName = "南州乡"},
                           new Area { AreaCode = "713340",AreaName = "佳冬乡"},
                           new Area { AreaCode = "713341",AreaName = "琉球乡"},
                           new Area { AreaCode = "713342",AreaName = "车城乡"},
                           new Area { AreaCode = "713343",AreaName = "满州乡"},
                           new Area { AreaCode = "713344",AreaName = "枋山乡"},
                           new Area { AreaCode = "713345",AreaName = "三地门乡"},
                           new Area { AreaCode = "713346",AreaName = "雾台乡"},
                           new Area { AreaCode = "713347",AreaName = "玛家乡"},
                           new Area { AreaCode = "713348",AreaName = "泰武乡"},
                           new Area { AreaCode = "713349",AreaName = "来义乡"},
                           new Area { AreaCode = "713350",AreaName = "春日乡"},
                           new Area { AreaCode = "713351",AreaName = "狮子乡"},
                           new Area { AreaCode = "713352",AreaName = "牡丹乡"},
                        }
                    });
                    #endregion
                    #region 台东县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "713400",
                        AreaName = "台东县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "713401",AreaName = "台东市"},
                           new Area { AreaCode = "713421",AreaName = "成功镇"},
                           new Area { AreaCode = "713422",AreaName = "关山镇"},
                           new Area { AreaCode = "713423",AreaName = "卑南乡"},
                           new Area { AreaCode = "713424",AreaName = "鹿野乡"},
                           new Area { AreaCode = "713425",AreaName = "池上乡"},
                           new Area { AreaCode = "713426",AreaName = "东河乡"},
                           new Area { AreaCode = "713427",AreaName = "长滨乡"},
                           new Area { AreaCode = "713428",AreaName = "太麻里乡"},
                           new Area { AreaCode = "713429",AreaName = "大武乡"},
                           new Area { AreaCode = "713430",AreaName = "绿岛乡"},
                           new Area { AreaCode = "713431",AreaName = "海端乡"},
                           new Area { AreaCode = "713432",AreaName = "延平乡"},
                           new Area { AreaCode = "713433",AreaName = "金峰乡"},
                           new Area { AreaCode = "713434",AreaName = "达仁乡"},
                           new Area { AreaCode = "713435",AreaName = "兰屿乡"}
                        }
                    });
                    #endregion
                    #region 花莲县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "713500",
                        AreaName = "花莲县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "713501",AreaName = "花莲市"},
                           new Area { AreaCode = "713521",AreaName = "凤林镇"},
                           new Area { AreaCode = "713522",AreaName = "玉里镇"},
                           new Area { AreaCode = "713523",AreaName = "新城乡"},
                           new Area { AreaCode = "713524",AreaName = "吉安乡"},
                           new Area { AreaCode = "713525",AreaName = "寿丰乡"},
                           new Area { AreaCode = "713526",AreaName = "光复乡"},
                           new Area { AreaCode = "713527",AreaName = "丰滨乡"},
                           new Area { AreaCode = "713528",AreaName = "瑞穗乡"},
                           new Area { AreaCode = "713529",AreaName = "富里乡"},
                           new Area { AreaCode = "713530",AreaName = "秀林乡"},
                           new Area { AreaCode = "713531",AreaName = "万荣乡"},
                           new Area { AreaCode = "713532",AreaName = "卓溪乡"}
                        }
                    });
                    #endregion
                    #region 澎湖县
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "713600",
                        AreaName = "澎湖县",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "713601",AreaName = "马公市"},
                           new Area { AreaCode = "713621",AreaName = "湖西乡"},
                           new Area { AreaCode = "713622",AreaName = "白沙乡"},
                           new Area { AreaCode = "713623",AreaName = "西屿乡"},
                           new Area { AreaCode = "713624",AreaName = "望安乡"},
                           new Area { AreaCode = "713625",AreaName = "七美乡"}
                        }
                    });
                    #endregion
                    break;

                case "香港特别行政区":
                    province.ChildrenList = new List<Area>();
                    #region 香港岛
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "810100",
                        AreaName = "香港岛",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "810101",AreaName = "中西区"},
                           new Area { AreaCode = "810102",AreaName = "湾仔区"},
                           new Area { AreaCode = "810103",AreaName = "东区"},
                           new Area { AreaCode = "810104",AreaName = "南区"}
                        }
                    });
                    #endregion
                    #region 九龙
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "810200",
                        AreaName = "九龙",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "810201",AreaName = "油尖旺区"},
                           new Area { AreaCode = "810202",AreaName = "深水埗区"},
                           new Area { AreaCode = "810203",AreaName = "九龙城区"},
                           new Area { AreaCode = "810204",AreaName = "黄大仙区"},
                           new Area { AreaCode = "810205",AreaName = "观塘区"}
                        }
                    });
                    #endregion
                    #region 新界
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "810300",
                        AreaName = "新界",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "810301",AreaName = "荃湾区"},
                           new Area { AreaCode = "810302",AreaName = "屯门区"},
                           new Area { AreaCode = "810303",AreaName = "元朗区"},
                           new Area { AreaCode = "810304",AreaName = "北区"},
                           new Area { AreaCode = "810305",AreaName = "大埔区"},
                           new Area { AreaCode = "810306",AreaName = "西贡区"},
                           new Area { AreaCode = "810307",AreaName = "沙田区"},
                           new Area { AreaCode = "810308",AreaName = "葵青区"},
                           new Area { AreaCode = "810309",AreaName = "离岛区"}
                        }
                    });
                    #endregion
                    break;

                case "澳门特别行政区":
                    province.ChildrenList = new List<Area>();
                    #region 澳门半岛
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "820100",
                        AreaName = "澳门半岛",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "820101",AreaName = "花地玛堂区"},
                           new Area { AreaCode = "820102",AreaName = "圣安多尼堂区"},
                           new Area { AreaCode = "820103",AreaName = "大堂区"},
                           new Area { AreaCode = "820104",AreaName = "望德堂区"},
                           new Area { AreaCode = "820105",AreaName = "风顺堂区"}
                        }
                    });
                    #endregion
                    #region 氹仔岛
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "820200",
                        AreaName = "氹仔岛",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "820201",AreaName = "嘉模堂区"}
                        }
                    });
                    #endregion
                    #region 路环岛
                    province.ChildrenList.Add(new Area
                    {
                        AreaCode = "820300",
                        AreaName = "路环岛",
                        ChildrenList = new List<Area>()
                        {
                           new Area { AreaCode = "820301",AreaName = "圣方济各堂区"}
                        }
                    });
                    #endregion
                    break;
            }
        }

        /// <summary>
        /// 第三级的数据可能不完整，重新到另外一个页面去获取
        /// </summary>
        /// <param name="province"></param>
        /// <param name="city"></param>
        private static void CustomCity(int index, int total, Area province, Area city)
        {
            List<Area> countyList = GetCountyList(index, total, province, city);

            List<Area> notIncludeList = countyList.Where(p => !city.ChildrenList.Select(c => c.AreaCode).Contains(p.AreaCode) && p.AreaName != "市辖区").ToList();
            if (notIncludeList.Count > 0)
            {
                foreach (Area county in notIncludeList)
                {
                    county.AreaName = county.AreaName.Replace(province.AreaName, string.Empty).Replace(city.AreaName, string.Empty);
                    county.AreaName = county.AreaName.Replace(province.AreaName.Replace("省", string.Empty), string.Empty).Replace(city.AreaName.Replace("市", string.Empty), string.Empty);
                }
                city.ChildrenList.AddRange(notIncludeList);
                city.ChildrenList = city.ChildrenList.OrderBy(p => p.AreaCode).ToList();
            }
            //如果第三级有缺的，可以在这里添加
            switch (city.AreaName)
            {
                case "合肥市":
                    city.ChildrenList.Add(new Area
                    {
                        AreaCode = "340174",
                        AreaName = "滨湖新区"
                    });
                    break;
            }
        }

        /// <summary>
        /// 获取第三级的数据
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        private static List<Area> GetCountyList(int index, int total, Area province, Area city)
        {
            WriteLine(string.Format("[{0} {1}/{2}] {3}", province.AreaName, index + 1, total, city.AreaName));

            string url = GlobalConstants.UrlCounty;
            url = string.Format(url, city.AreaCode.Substring(0, 2), city.AreaCode.Substring(0, 4));
            string html = HtmlHelper.GetHtmlContent(url, "GB2312");
            Thread.Sleep(100);

            List<Area> areaList = new List<Area>();

            if (!string.IsNullOrEmpty(html))
            {
                string areaName = string.Empty;
                string areaCode = string.Empty;

                MatchCollection mcTr = GlobalConstants.RegexCountyTr.Matches(html);
                foreach (Match matchTr in mcTr)
                {
                    List<string> oneArea = new List<string>();
                    string tr = matchTr.Value;
                    MatchCollection mcTd = GlobalConstants.RegexCountyTd.Matches(tr);
                    foreach (Match matchTd in mcTd)
                    {
                        string td = matchTd.Groups[1].Value;
                        if (!string.IsNullOrEmpty(td))
                        {
                            oneArea.Add(td);
                        }
                    }
                    if (oneArea.Count == 2)
                    {
                        areaCode = HtmlHelper.ReplaceHtmlTag(oneArea[0]).Trim();
                        areaName = HtmlHelper.ReplaceHtmlTag(oneArea[1]).Trim();
                        if (!string.IsNullOrEmpty(areaCode) && !string.IsNullOrEmpty(areaName))
                        {
                            areaCode = areaCode.Substring(0, 6);
                            // 判断是否是数字
                            if (Regex.IsMatch(areaCode, @"^[+-]?\d*$"))
                            {
                                areaList.Add(new Area
                                {
                                    AreaCode = areaCode,
                                    AreaName = areaName
                                });
                            }
                        }
                    }
                }
            }
            return areaList;
        }

        private static void WriteLine(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
            Console.WriteLine(str);
        }
    }
}
