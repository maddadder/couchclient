using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace couchclient.Models
{
    public class GameOptionCreateRequestCommand
    {
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
		        Pid = new Guid(),
                __T = "go",
                GameEntryRef = this.GameEntryRef,
                description = this.description,
                next = this.next,
            };
        }
    }
}
