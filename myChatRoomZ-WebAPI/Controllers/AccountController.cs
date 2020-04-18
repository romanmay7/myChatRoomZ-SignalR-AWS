
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using myChatRoomZ_WebAPI.Data.Models;
using myChatRoomZ_WebAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace myChatRoomZ_WebAPI.Controllers
{
    public class AccountController : Controller
    {
        //private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<ChatUser> _signInManager;
        private readonly UserManager<ChatUser> _userManager;
        private readonly IConfiguration _config;
        public AccountController(
            //ILogger<AccountController> logger,
            SignInManager<ChatUser> signInManager,
            UserManager<ChatUser> userManager,
            IConfiguration config
            )
        {
            //_logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        //public IActionResult Login()
        //{

        //    if (this.User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("Index", "App");
        //    }
        //    return View();
        //}

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        RedirectToAction(Request.Query["ReturnUrl"].First());
                    }
                    else
                    {
                        RedirectToAction("Shop", "App");
                    }

                }
            }
            ModelState.AddModelError("", "Failed to Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody]UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ChatUser user = new ChatUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.UserName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("Could not create new user ");
                }


                if (result.Succeeded)
                {
                    if (Request.Query.Keys.Contains("ReturnUrl"))
                    {

                        RedirectToAction(Request.Query["ReturnUrl"].First());
                    }
                    return Ok();

                }
                else
                {
                    return BadRequest();
                    //RedirectToAction("Shop", "App");
                }
            }
            //ModelState.AddModelError("", "Failed to Create New User");
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Home", "App");
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody]LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                    if (result.Succeeded)
                    {
                        //Create the token
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti,new Guid().ToString())
                           // new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName)
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            _config["Tokens:Issuer"],
                            _config["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddMinutes(30),
                            signingCredentials: creds
                            );

                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo

                        };

                        return Created("", results);
                    }
                }
            }
            return BadRequest();
        }
        //----------Authentication with Google-----------------------------------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody]GoogleLoginRequest request)
        {
            Payload payload = null;
            try
            {
                //Validates a Google issued Web Token
                payload = await ValidateAsync(request.IdToken, new ValidationSettings
                {
                    Audience = new[] { "907069961520-utqs3la12ou4s2ptq592du5uc2psesge.apps.googleusercontent.com" }
                });
                // It is important to add your ClientId as an audience in order to make sure
                // that the token is for your application!
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown:" + e);
                // Invalid token
            }

            var user = await GetOrCreateExternalLoginUser("google", payload.Subject, payload.Email, payload.GivenName, payload.FamilyName);
            var token = await GenerateToken(user);
            return Created("", token);
        }

        public async Task<ChatUser> GetOrCreateExternalLoginUser(string provider, string key, string email, string firstName, string lastName)
        {
            // If Login already linked to a user
            var user = await _userManager.FindByLoginAsync(provider, key);
            if (user != null) return user;

            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // No user exists with this email address, we create a new one
                user = new ChatUser
                {
                    Email = email,
                    UserName = email,
                    FirstName = firstName,
                    LastName = lastName
                };

                await _userManager.CreateAsync(user);
            }

            // Link the user to this login
            var info = new UserLoginInfo(provider, key, provider.ToUpperInvariant());
            var result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
                return user;

            //_logger.LogError("Failed add a user linked to a login.");
            // _logger.LogError(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
            return null;
        }

        public async Task<object> GenerateToken(ChatUser user)

        {
            //var claims = await GetUserClaims(user);
            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Sub,user.Email),
               new Claim(JwtRegisteredClaimNames.Jti,new Guid().ToString())
               // new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName)
             };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Tokens:Issuer"],
                _config["Tokens:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
                );

            var results = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo

            };

            return results;
        }
    }

    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
}
