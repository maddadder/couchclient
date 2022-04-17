using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace couchclient.Models
{
    public class UserMessageCreateRequestCommandTwilio
    {
        [Required]
        [FromForm(Name = "Body")]
        public string Body { get; set; }
        [Required]
        [FromForm(Name = "To")]
        public string To { get; set; }
        [Required]
        [FromForm(Name = "From")]
        public string From { get; set; }
        [Required]
        [FromForm(Name = "ApiVersion")]
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
