using IPLTicketBooking.DTOs;

namespace IPLTicketBooking.Services
{
	public interface IRoleService
	{
		Task<RoleDto> CreateRole(RoleDto roleDto);
		Task<List<RoleDto>> GetAllRoles();
		Task AssignRoleToUser(AssignRoleDto assignRoleDto);
	}
}