﻿using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserLink
    {
        [Required]
        public Guid Pid { get; set; }
        [Required]
        public string __T {get;set;}
        [Required]
        public string Content { get; set; }
        [Required]
        [RegularExpression(StringHelper.RegexUrl, ErrorMessage = "Must be a valid URL")]
        public string Href { get; set; }
        public string Target { get; set; }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }

    }
}
