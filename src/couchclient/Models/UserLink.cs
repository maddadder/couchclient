using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserLink
    {
        [Required]
        public Guid Pid { get; set; }
        public string __T {get;set;}
        [Required]
        public string Content { get; set; }
        [Required]
        [RegularExpression(StringHelper.RegexUrl)]
        public string Href { get; set; }
        public string Target { get; set; }

    }
}
