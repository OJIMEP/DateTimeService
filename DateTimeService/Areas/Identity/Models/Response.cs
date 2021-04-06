using DateTimeService.Areas.Identity.Data;
using System;
using System.Collections;

namespace DateTimeService.Areas.Identity.Models
{
    public class Response
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public IEnumerable Description { get; set; }
    }

    public class AuthenticateResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public DateTime JwtValidTo { get; set; }

        //[JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }
        public DateTime RefreshValidTo { get; set; }

        public AuthenticateResponse(DateTimeServiceUser user, string jwtToken, string refreshToken, DateTime _jwtValidTo, DateTime _refreshValidTo)
        {
            Id = user.Id;
            //FirstName = user.FirstName;
            //LastName = user.LastName;
            Username = user.UserName;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
            JwtValidTo = _jwtValidTo;
            RefreshValidTo = _refreshValidTo;
        }
    }
}
