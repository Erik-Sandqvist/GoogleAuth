using System.ComponentModel.DataAnnotations;

namespace Api.Data
{
    public class Credential
    {

        [Key]
        public Guid MyProperty { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string ExpiresInSeconds { get; set; }

        public string IdToken { get; set; }

        public DateTime IssuedUtc  { get; set; }
        public Guid UserId { get; internal set; }
    }
}
