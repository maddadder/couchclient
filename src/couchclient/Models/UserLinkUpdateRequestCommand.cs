using System;
namespace couchclient.Models
{
    public class UserLinkUpdateRequestCommand
    {
        public Guid Pid { get; set;  }
        public string Content { get; set; }
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
