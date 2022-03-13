using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class ProfileUpdateRequestCommand
    {
        [Required]
        public Guid Pid { get; set;  }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
	    public Profile GetProfile()
	    {
	        return new Profile
            {
                Pid = this.Pid,
                __T = "up",
		        FirstName = this.FirstName,
		        LastName = this.LastName,
		        Email = this.Email,
	            Password = this.Password
            };
	    } 
    }
}
