using Lvy.Models.CrmDB;
using Lvy.NetCore.API.Helpers;
using Lvy.NetCore.API.Models;
using Lvy.Trip.Biz.Crm;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Lvy.NetCore.API.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<CrmAccountModel> GetAll();
        CrmAccountModel GetById(string id);
    }

    public class UserService : IUserService
    {

        private AccountBiz biz = new AccountBiz();


        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = biz.GetByLogin("1611000001", model.Username, model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public IEnumerable<CrmAccountModel> GetAll()
        {
            return biz.GetAllAccount("1812000007");
        }

        public CrmAccountModel GetById(string id)
        {
            return biz.GetAccountCustomer(id);
        }

        // helper methods

        private string generateJwtToken(CrmAccountModel user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Code) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}