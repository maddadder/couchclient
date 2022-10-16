using System.ComponentModel.DataAnnotations;
namespace couchclient.Models {
    public class UserLogin {
        [Required]
        public string PreferredUsername {
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