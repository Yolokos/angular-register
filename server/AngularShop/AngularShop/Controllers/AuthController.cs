using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngularShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using AngularShop.Models.QueryModels.Auth;
using AngularShop.Models.QueryModels;

namespace AngularShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }


        //Login User
        [HttpPost("/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]LoginUserModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Email);
                return Ok(new { userName = appUser.UserName, email = appUser.Email, token = GenerateJwtToken(model.Email, appUser) });
            }
            else
            {
                return BadRequest("Email/password is incorrect");
            }
        }

        //Register User
        [HttpPost("/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody]RegisterUserModel model)
        {
            if (!IsValidEmail(model.Email))
            {
                return BadRequest("Not valid email");
            }
            var user = new User
            {
                UserName = model.Email,
                Email = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);

                return Ok(new { userName = user.UserName, email = user.Email, token = GenerateJwtToken(model.Email, user), status = true });
            }
            else
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }
        }

        private object GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}