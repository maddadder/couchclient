using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class UserProfileCreateRequestCommand
    {
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
        public NewUserProfile GetProfile()
        {
            return new NewUserProfile
            {
		        Pid = new Guid(),
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
