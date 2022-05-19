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
        [RegularExpression(StringHelper.RegexUrl, ErrorMessage = "Must be a valid URL")]
        public string Href { get; set; }
        [RegularExpression(StringHelper.RegexUrl, ErrorMessage = "Must be a valid URL")]
        public string ImgHref { get; set; }
        public string Target { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }

    }
}
