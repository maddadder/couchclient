using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserMessageUpdateRequestCommand
    {
        [Required]
        public Guid Pid { get; set;  }
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
                Pid = this.Pid,
                __T = "um",
		        Body = this.Body,
		        To = this.To,
		        From = this.From,
                ApiVersion = this.ApiVersion
            };
	    } 
    }
}
