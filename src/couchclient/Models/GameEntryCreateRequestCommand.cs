using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace couchclient.Models
{
    public class GameEntryCreateRequestCommand
    {
        [Required]
        public string name {get; set;}
        [Required]
        public List<string> description { get; set; }
        public List<GameOption> options { get; set; }

        public GameEntry GetGameEntry()
        {
            return new GameEntry
            {
		        Pid = new Guid(),
                __T = "ge",
                name = this.name,
                description = this.description,
                options = this.options,
            };
        }
    }
}
