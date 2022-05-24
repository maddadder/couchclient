using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class GameOption
    {
        [Required]
        public Guid Pid { get; set; }
        public string __T {get;set;}
        [Required]
        public Guid GameEntryRef {get; set;}
        [Required]
        public string description { get; set; }
        [Required]
        public string next { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }

    }
}
