using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;
using IPLTicketBooking.Repositories;
using Microsoft.Extensions.Logging;

namespace IPLTicketBooking.Services
{

	public class RoleService : IRoleService
	{
		private readonly IRoleRepository _roleRepository;
		private readonly IUserRepository _userRepository;
		private readonly ILogger<RoleService> _logger;

		public RoleService(
			IRoleRepository roleRepository,
			IUserRepository userRepository,
			ILogger<RoleService> logger)
		{
			_roleRepository = roleRepository;
			_userRepository = userRepository;
			_logger = logger;
		}

		public async Task<RoleDto> CreateRole(RoleDto roleDto)
		{
			try
			{
				var existingRole = await _roleRepository.GetByNameAsync(roleDto.Name);
				if (existingRole != null)
				{
					throw new Exception("Role already exists");
				}

				var role = new Role
				{
					Name = roleDto.Name,
					Permissions = roleDto.Permissions
				};

				await _roleRepository.CreateAsync(role);

				return new RoleDto
				{
					Id = role.Id,
					Name = role.Name,
					Permissions = role.Permissions
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating role");
				throw;
			}
		}

		public async Task<List<RoleDto>> GetAllRoles()
		{
			try
			{
				var roles = await _roleRepository.GetAllAsync();
				return roles.ConvertAll(r => new RoleDto
				{
					Id = r.Id,
					Name = r.Name,
					Permissions = r.Permissions
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all roles");
				throw;
			}
		}

		public async Task AssignRoleToUser(AssignRoleDto assignRoleDto)
		{
			try
			{
				var user = await _userRepository.GetByIdAsync(assignRoleDto.UserId);
				if (user == null)
				{
					throw new Exception("User not found");
				}

				var role = await _roleRepository.GetByIdAsync(assignRoleDto.RoleId);
				if (role == null)
				{
					throw new Exception("Role not found");
				}

				if (!user.RoleIds.Contains(role.Id))
				{
					user.RoleIds.Add(role.Id);
					await _userRepository.UpdateAsync(user);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error assigning role to user");
				throw;
			}
		}
	}
}