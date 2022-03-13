using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserLinkUpdateRequestCommand
    {
        [Required]
        public Guid Pid { get; set;  }
        [Required]
        public string Content { get; set; }
        [Required]
        [RegularExpression(StringHelper.RegexUrl)]
        public string Href { get; set; }
        public string Target { get; set; }

	    public UserLink GetUserLink()
	    {
	        return new UserLink
            {
                Pid = this.Pid,
                __T = "ul",
		        Content = this.Content,
		        Href = this.Href,
		        Target = this.Target
            };
	    } 
    }
}
