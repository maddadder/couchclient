using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserMessage
    {
        [Required]
        public Guid Pid { get; set; }
        [Required]
        public string __T {get;set;}
        [Required]
        public string Body { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string ApiVersion { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }

    }
}
