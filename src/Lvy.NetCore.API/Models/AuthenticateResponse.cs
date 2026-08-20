
using Lvy.Models.CrmDB;

namespace Lvy.NetCore.API.Models
{
    public class AuthenticateResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(CrmAccountModel user, string token)
        {
            Id = user.Code;
            FirstName = user.Name;
           // LastName = user.LastName;
            Username = user.LoginName;
            Token = token;
        }
    }
}