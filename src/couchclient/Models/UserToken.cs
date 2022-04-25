using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace couchclient.Models
{
    public class UserToken {
        public string Token {
            get;
            set;
        }
        [JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan Validaty {
            get;
            set;
        }
        public string RefreshToken {
            get;
            set;
        }
        public Guid Id {
            get;
            set;
        }
        public string Email {
            get;
            set;
        }
        public Guid GuidId {
            get;
            set;
        }
        public DateTime ExpiredTime {
            get;
            set;
        }
    }
}