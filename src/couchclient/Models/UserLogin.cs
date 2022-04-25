using System.ComponentModel.DataAnnotations;
namespace couchclient.Models {
    public class UserLogin {
        [Required]
        public string Email {
            get;
            set;
        }
        [Required]
        public string Password {
            get;
            set;
        }
        public UserLogin() {}
    }
}