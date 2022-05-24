using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class GameOptionListRequestQuery
    {
        [Required]
        public string Search { get; set; }
        public int Limit { get; set; } = 100;
        public int Skip { get; set; } 
    }
}
