﻿using AuthLibrary.Data;
using DateTimeService.Areas.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DateTimeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<DateTimeServiceUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IUserService _userService;

        public AuthenticateController(UserManager<DateTimeServiceUser> userManager, RoleManager<IdentityRole> roleManager, IUserService userService)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._userService = userService;
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {


                var response = await _userService.AuthenticateAsync(model, IpAddress());

                if (response == null)
                    return BadRequest();

                //setTokenCookie(response.RefreshToken);

                return Ok(new
                {
                    token = response.JwtToken,
                    expiration = response.JwtValidTo,
                    refresh = response.RefreshToken,
                    expiration_refresh = response.RefreshValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenModel model)
        {
            var refreshToken = model.RefreshToken;
            var response = await _userService.RefreshTokenAsync(refreshToken, IpAddress());

            if (response == null)
                return Unauthorized(new { message = "Invalid token" });

            //setTokenCookie(response.RefreshToken);

            return Ok(new
            {
                token = response.JwtToken,
                expiration = response.JwtValidTo,
                refresh = response.RefreshToken,
                expiration_refresh = response.RefreshValidTo
            });
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RefreshTokenModel model)
        {
            // accept token from request body or cookie
            var token = model.RefreshToken;

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var response = _userService.RevokeToken(token, IpAddress());

            if (!response)
                return NotFound(new { message = "Token not found" });

            return Ok(new { message = "Token revoked" });
        }


        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            DateTimeServiceUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                foreach (var role in model.Roles)
                {
                    if (await roleManager.RoleExistsAsync(role))
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
            }

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again.", Description = result.Errors });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not found!" });

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User deletion failed! Please check user details and try again.", Description = result.Errors });

            return Ok(new Response { Status = "Success", Message = "User deleted successfully!" });
        }


        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {

            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null || !await userManager.CheckPasswordAsync(user, model.OldPassword))
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not found or wrong old password" });

            var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Password change is failed! Please check user details and try again.", Description = result.Errors });

            return Ok(new Response { Status = "Success", Message = "Password changed password successfully!" });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        [Route("modify-roles")]
        public async Task<IActionResult> ModifyRoles([FromBody] ModifyRolesModel model)
        {

            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not found" });

            List<string> successAddRoles = new();
            List<string> successDeleteRoles = new();

            foreach (var role in model.AddRoles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    await userManager.AddToRoleAsync(user, role);
                    successAddRoles.Add(role);
                }
            }

            foreach (var role in model.DeleteRoles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    await userManager.RemoveFromRoleAsync(user, role);
                    successDeleteRoles.Add(role);
                }
            }

            return Ok(new Response { Status = "Success", Message = "Added roles: " + string.Join(", ", successAddRoles) + ", deleted roles: " + string.Join(", ", successDeleteRoles) });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [Route("available-roles")]
        public IActionResult GetAvailableRoles()
        {
            return Ok(roleManager.Roles);
        }


        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
