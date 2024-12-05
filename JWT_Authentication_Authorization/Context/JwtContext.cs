using System;
using JWT_Authentication_Authorization.Models;
using Microsoft.EntityFrameworkCore;

namespace JWT_Authentication_Authorization.Context
{
	public class JwtContext: DbContext
	{
		public JwtContext(DbContextOptions<JwtContext> options) : base(options)
		{

		}

		public DbSet<User> users { get; set; }
		public DbSet<Role> roles { get; set; }
		public DbSet<UserRole> userRoles { get; set; }
		public DbSet<Employee> employees { get; set; }
		

	}
}

