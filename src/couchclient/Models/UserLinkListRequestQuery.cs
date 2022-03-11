using System;
namespace couchclient.Models
{
    public class UserLinkListRequestQuery
    {
        public string Search { get; set; }
        public int Limit { get; set; } = 100;
        public int Skip { get; set; } 
    }
}
