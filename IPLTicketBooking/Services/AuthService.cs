using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;
using IPLTicketBooking.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IPLTicketBooking.Services
{

	public class AuthService : IAuthService
	{
		private readonly IUserRepository _userRepository;
		private readonly IRoleRepository _roleRepository;
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthService> _logger;

		public AuthService(
			IUserRepository userRepository,
			IRoleRepository roleRepository,
			IConfiguration configuration,
			ILogger<AuthService> logger)
		{
			_userRepository = userRepository;
			_roleRepository = roleRepository;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<AuthResponseDto> Register(RegisterDto registerDto)
		{
			try
			{
				if (await _userRepository.GetByUsernameAsync(registerDto.Username) != null)
				{
					throw new Exception("Username already exists");
				}

				CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

				var user = new User
				{
					Username = registerDto.Username,
					Email = registerDto.Email,
					PasswordHash = passwordHash,
					PasswordSalt = passwordSalt
				};

				// Assign default "User" role
				var userRole = await _roleRepository.GetByNameAsync("User");
				if (userRole != null)
				{
					user.RoleIds.Add(userRole.Id);
				}

				await _userRepository.CreateAsync(user);

				var roles = await GetUserRoles(user.RoleIds);

				return new AuthResponseDto
				{
					Id = user.Id,
					Username = user.Username,
					Email = user.Email,
					Token = GenerateJwtToken(user),
					Roles = roles.ConvertAll(r => r.Name)
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during registration");
				throw;
			}
		}

		public async Task<AuthResponseDto> Login(LoginDto loginDto)
		{
			try
			{
				var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
				if (user == null)
				{
					throw new Exception("User not found");
				}

				if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
				{
					throw new Exception("Wrong password");
				}

				var roles = await GetUserRoles(user.RoleIds);

				return new AuthResponseDto
				{
					Id = user.Id,
					Username = user.Username,
					Email = user.Email,
					Token = GenerateJwtToken(user),
					Roles = roles.ConvertAll(r => r.Name)
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during login");
				throw;
			}
		}

		public string GenerateJwtToken(User user)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

			var roles = GetUserRoles(user.RoleIds).Result;

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Name, user.Username)
			};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role.Name));
			}

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddDays(7),
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256Signature),
				Issuer = _configuration["Jwt:Issuer"],
				Audience = _configuration["Jwt:Audience"]
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		private async Task<List<Role>> GetUserRoles(List<string> roleIds)
		{
			var roles = new List<Role>();
			foreach (var roleId in roleIds)
			{
				var role = await _roleRepository.GetByIdAsync(roleId);
				if (role != null)
				{
					roles.Add(role);
				}
			}
			return roles;
		}

		private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
		{
			using var hmac = new HMACSHA512();
			passwordSalt = hmac.Key;
			passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
		}

		private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
		{
			using var hmac = new HMACSHA512(passwordSalt);
			var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
			return computedHash.SequenceEqual(passwordHash);
		}
	}
}