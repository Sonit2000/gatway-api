using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace gatway_clone.Objects
{
    public class HandshakeRequest
    {
        [SwaggerSchema("Client public key")]
        [Required]
        public string PublicKey { get; set; }
    }
    public class HandshakeResponse
    {
        [SwaggerSchema("Salt encrypt by shared key")]
        [Required]
        public string Salt { get; set; }

        [SwaggerSchema("Session key encrypt by shared key")]
        [Required]
        public string SessionKey { get; set; }

        [SwaggerSchema("Server public key")]
        [Required]
        public string PublicKey { get; set; }
    }
}
