using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserLinkCreateRequestCommand
    {
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
		        Pid = new Guid(),
                __T = "ul",
                Content = this.Content,
                Href = this.Href,
                Target = this.Target,
            };
        }
    }
}
