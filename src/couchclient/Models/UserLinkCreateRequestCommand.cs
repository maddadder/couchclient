using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserLinkCreateRequestCommand
    {
        [Required]
        public string Content { get; set; }
        [Required]
        [RegularExpression(StringHelper.RegexUrl, ErrorMessage = "Must be a valid URL")]
        public string Href { get; set; }
        [RegularExpression(StringHelper.RegexUrl, ErrorMessage = "Must be a valid URL")]
        public string ImgHref { get; set; }

        public string Target { get; set; }

        public UserLink GetUserLink()
        {
            return new UserLink
            {
		        Pid = new Guid(),
                __T = "ul",
                Content = this.Content,
                Href = this.Href,
                ImgHref = this.ImgHref,
                Target = this.Target,
            };
        }
    }
}
