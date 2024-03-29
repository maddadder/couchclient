﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace couchclient.Models
{
    public class UserMessageCreateRequestCommand
    {
        [Required]
        public string Body { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string ApiVersion { get; set; }

        public UserMessage GetUserMessage()
        {
            return new UserMessage
            {
		        Pid = new Guid(),
                __T = "um",
                Body = this.Body,
                To = this.To,
                From = this.From,
                ApiVersion = this.ApiVersion
            };
        }
    }
}
