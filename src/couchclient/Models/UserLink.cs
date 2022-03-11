using System;
namespace couchclient.Models
{
    public class UserLink
    {
        public Guid Pid { get; set; }
        public string __T {get;set;}
        public string Content { get; set; }
        public string Href { get; set; }
        public string Target { get; set; }

    }
}
