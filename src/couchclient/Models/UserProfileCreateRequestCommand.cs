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
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public UserProfile GetProfile()
        {
            return new UserProfile
            {
		        Pid = new Guid(),
                __T = "up",
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Password = this.Password
            };
        }
    }
}
