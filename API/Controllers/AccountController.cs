using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username)) return BadRequest("User Already Exists");
            using var hash = new HMACSHA512();

            var user = new AppUser
            {
                Username = registerDTO.Username.ToLower(),
                PasswordHash = hash.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.password)),
                PasswordSalt = hash.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.Username,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(filter => filter.Username == loginDto.Username);
            if (user == null) return Unauthorized("Invalid User Name");

            var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");

            }

            return new UserDto { Username = user.Username, Token = _tokenService.CreateToken(user) };


        }
        [NonAction]
        private async Task<bool> UserExists(string Username)
        {
            return await _context.Users.AnyAsync(filter => filter.Username == Username.ToLower());
        }
    }
}