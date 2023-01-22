using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MembersInfoApi.Controllers
{
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        public IConfiguration _configuration;
        public MembersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        static readonly string[] scopeRequiredByApi = new string[] { "MembersApi.All" };
        private List<Member> membersList = new List<Member>
        {
            new Member
            {
                FirstName = "Steve",
                LastName = "Robert",
                Address = "1 Infinite Loop, Cupertino, California",
                MemberId = "M9999"
            },
            new Member
            {
                FirstName = "Adam",
                LastName = "Taylor",
                Address = "5 Cherry Springs,Redmond, Washington, United States",
                MemberId = "M8888"
            }
        };
        
        [HttpGet("[action]/{memberId}")]
        [Authorize(Roles = "Members.Readonly")]
        public IActionResult GetMemberInfo(string memberId)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            var authHeader = Convert.ToString(HttpContext.Request.Headers["Authorization"]);
            var member = membersList.FirstOrDefault(x => x.MemberId == memberId);
            var jwttoken = authHeader.Substring(7);
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(jwttoken);
            if (token != null)
            {
                IEnumerable<Claim> claimstocheck = token.Claims;
                var claimClientID = claimstocheck.Where(claim => claim.Type == "appid").ToList();
                member.ClientID = claimClientID.FirstOrDefault().Value;
            }
            member.authHeader = authHeader;
            member.BuildNumber = _configuration["CodeVersion:Num"];

            if(member==null)
                return NotFound();
            return Ok(member);
        }
    }
}