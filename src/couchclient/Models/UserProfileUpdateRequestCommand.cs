using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserProfileUpdateRequestCommand
    {
        [Required]
        public Guid Pid { get; set;  }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [EmailAddress]
        public string PreferredUsername { get; set; }
        [Required]
        public string Password { get; set; }
        public bool ReceiveEmailNotificationFromSms { get; set; }
	    public UserProfile GetProfile()
	    {
	        return new UserProfile
            {
                Pid = this.Pid,
                __T = "up",
		        FirstName = this.FirstName,
		        LastName = this.LastName,
                Email = this.Email,
		        PreferredUsername = this.PreferredUsername,
	            Password = this.Password,
                ReceiveEmailNotificationFromSms = this.ReceiveEmailNotificationFromSms
            };
	    } 
    }
}
