using System;
using System.ComponentModel.DataAnnotations;

namespace couchclient.Models
{
    public class Profile
    {
        [Required]
        public Guid Pid { get; set; }
        public string __T {get;set;}
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        private string _password;
        [Required]
        public string Password {
            get
            {
                return _password;
            }
            set
            {
                _password = BCrypt.Net.BCrypt.HashPassword(value);
            }
        }
        public DateTime Created { get;set; }
        public DateTime Modified { get;set; }
    }
}
