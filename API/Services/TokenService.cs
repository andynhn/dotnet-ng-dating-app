using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key; // symmetric encryption is where only one key is used to encrypt and decrypt electronic info (JWT uses this. key only needs to exist on the server). 
        // assymetric encryption is where 1 public and 1 private key is used to encrypt and decrypt (https, ssl, etc.)
        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public string CreateToken(AppUser user)
        {
            // identify what claims you are putting in the token
            // nameId will store the user.UserName
            var claims = new List<Claim>
            {
                // use UniqueName for username and NameId for the user's Id (the int)
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            };

            // create credentials that takes in the key and a security algorithm to sign the token
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // now describe the token. Specify what goes inside the token.
            // Subject has the claims. 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            
            // need a token handler.
            var tokenHandler = new JwtSecurityTokenHandler();

            // create token from token handler using the token descriptor
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // finally, write the token and return it to whoever needs it.
            return tokenHandler.WriteToken(token);
        }
    }
}