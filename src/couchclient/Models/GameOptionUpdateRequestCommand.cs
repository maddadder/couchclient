using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class GameOptionUpdateRequestCommand
    {
        [Required]
        public Guid Pid { get; set;  }
        [Required]
        public Guid GameEntryRef {get; set;}
        [Required]
        public string description { get; set; }
        [Required]
        public string next { get; set; }

	    public GameOption GetGameOption()
	    {
	        return new GameOption
            {
                Pid = this.Pid,
                __T = "go",
		        GameEntryRef = this.GameEntryRef,
                description = this.description,
                next = this.next,
            };
	    }
    }
}
