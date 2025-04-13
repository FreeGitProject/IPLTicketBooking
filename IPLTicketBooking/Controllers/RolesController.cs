using Microsoft.AspNetCore.Mvc;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IPLTicketBooking.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin")]
	public class RolesController : ControllerBase
	{
		private readonly IRoleService _roleService;
		private readonly ILogger<RolesController> _logger;

		public RolesController(IRoleService roleService, ILogger<RolesController> logger)
		{
			_roleService = roleService;
			_logger = logger;
		}

		[HttpPost]
		public async Task<IActionResult> CreateRole(RoleDto roleDto)
		{
			try
			{
				var role = await _roleService.CreateRole(roleDto);
				return CreatedAtAction(nameof(GetAllRoles), role);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating role");
				return BadRequest(ex.Message);
			}
		}

		[HttpGet]
		public async Task<IActionResult> GetAllRoles()
		{
			try
			{
				var roles = await _roleService.GetAllRoles();
				return Ok(roles);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all roles");
				return StatusCode(500, "An error occurred while retrieving roles");
			}
		}

		[HttpPost("assign")]
		public async Task<IActionResult> AssignRole(AssignRoleDto assignRoleDto)
		{
			try
			{
				await _roleService.AssignRoleToUser(assignRoleDto);
				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error assigning role");
				return BadRequest(ex.Message);
			}
		}
	}
}