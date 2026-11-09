using Microsoft.IdentityModel.Tokens;
using System;

namespace ASI.Basecode.WebApp.Authentication
{
    /// <summary>
    /// Token provider
    /// </summary>
    public class TokenProviderOptions
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// Gets or sets the expiration.
        /// </summary>
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
        /// <summary>
        /// Gets or sets the signing credentials.
        /// </summary>
        public SigningCredentials SigningCredentials { get; set; }
    }
}
