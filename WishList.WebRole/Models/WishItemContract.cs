using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace WishList.WebRole.Models
{
    public class WishItemContract
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string brand { get; set; }
        public string no { get; set; }
        public string comment { get; set; }
        public Nullable<int> price { get; set; }
        public string currency { get; set; }
        public string imageId { get; set; }
        [JsonIgnore]
        public byte[] blob { get; set; }
        public string base64 { get; set; }
    }
}