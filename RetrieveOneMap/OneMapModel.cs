using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetrieveOneMap
{
    public class OneMapTokenResponse
    {
        public string access_token { get; set; }
        public string expiry_timestamp { get; set; }
    }

    public class OneMapSearchResult
    {
        public int found { get; set; }
        public int totalNumPages { get; set; }
        public int pageNum { get; set; }
        public List<OneMapSearchItem> results { get; set; }
    }

    public class OneMapSearchItem
    {
        public string SEARCHVAL { get; set; }
        public string BLK_NO { get; set; }
        public string ROAD_NAME { get; set; }
        public string BUILDING { get; set; }
        public string ADDRESS { get; set; }
        public string POSTAL { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string LATITUDE { get; set; }
        public string LONGITUDE { get; set; }
    }
}
