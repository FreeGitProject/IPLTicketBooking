using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPLTicketBooking.DTOs
{
	public class RoleDto
	{
		public string Id { get; set; }

		[Required]
		public string Name { get; set; }

		public List<string> Permissions { get; set; } = new List<string>();
	}

	public class AssignRoleDto
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		public string RoleId { get; set; }
	}
}