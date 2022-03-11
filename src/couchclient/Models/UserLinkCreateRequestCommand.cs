using System;
namespace couchclient.Models
{
    public class UserLinkCreateRequestCommand
    {
        public string Content { get; set; }
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
