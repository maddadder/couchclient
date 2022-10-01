using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class GameEntryListRequestQuery
    {
        public string Search { get; set; }
        public int Limit { get; set; } = 100;
        public int Skip { get; set; } 
    }
}
