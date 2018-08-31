using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChinaArea
{
    public class Area
    {
        [JsonProperty("name")]
        public string AreaName { get; set; }

        [JsonProperty("code")]
        public string AreaCode { get; set; }

        [JsonProperty("sub")]
        public List<Area> ChildrenList { get; set; }
    }

    public enum AreaLevelEnum
    {
        /// <summary>
        /// 省份
        /// </summary>
        Province = 1,

        /// <summary>
        /// 城市
        /// </summary>
        City = 2,

        /// <summary>
        /// 区县
        /// </summary>
        County = 3
    }

}
