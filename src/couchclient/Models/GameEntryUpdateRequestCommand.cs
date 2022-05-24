using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class GameEntryUpdateRequestCommand
    {
        [Required]
        public Guid Pid { get; set;  }
        [Required]
        public string name {get; set;}
        [Required]
        public List<string> description { get; set; }
        [Required]
        public List<GameOption> options { get; set; }

	    public GameEntry GetGameEntry()
	    {
	        return new GameEntry
            {
                Pid = this.Pid,
                __T = "ge",
		        name = this.name,
                description = this.description,
                options = this.options,
            };
	    }
    }
}
