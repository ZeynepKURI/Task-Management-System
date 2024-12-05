using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWT_Authentication_Authorization.Context;
using JWT_Authentication_Authorization.Interfaces;
using JWT_Authentication_Authorization.Models;
using Microsoft.IdentityModel.Tokens;

namespace JWT_Authentication_Authorization.Services
{
	public class AuthService: IAuthService
    {
		private readonly JwtContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(JwtContext context, IConfiguration configuration)
		{
            _context = context;
            _configuration = configuration;
		}

        public Role AddRole(Role role)
        {
            var addedRole = _context.roles.Add(role);
            _context.SaveChanges();
            return addedRole.Entity;
        }

        public User AddUser(User user)
        {
            var addedUser = _context.users.Add(user);
            _context.SaveChanges();
            return addedUser.Entity;
        }

        public bool AssignRoleToUser(AddUserRole obj)
        {
            try
            {
                var addRoles = new List<UserRole>();
                var user = _context.users.SingleOrDefault(s => s.Id == obj.UserId);
                if (user == null)
                    throw new Exception("user is not valid");
                foreach (var role in obj.RoleIds)
                {
                    var userRole = new UserRole();
                    userRole.RoleId = role;
                    userRole.UserId = user.Id;
                    addRoles.Add(userRole);


                }
                _context.userRoles.AddRange(addRoles);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
           
        }

        public string Login(LoginRequest loginRequest)
        {
           if (loginRequest.Username != null && loginRequest.Password != null)
            {
                var user = _context.users.SingleOrDefault(s => s.Username == loginRequest.Username && s.Password == loginRequest.Password);
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim("Id", user.Id.ToString()),
                        new Claim("Username", user.Name)
                    };
                    var userRoles = _context.userRoles.Where(u => u.UserId == user.Id).ToList();
                    var roleIds = userRoles.Select(s => s.RoleId).ToList();
                    var roles = _context.roles.Where(r => roleIds.Contains(r.Id)).ToList();
                    foreach(var role in roles)
                    {
                        claims.Add(new Claim("Role", role.Name));
                    }
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));  // Burada hatalıydı
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],  
                        _configuration["Jwt:Audience"],  
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(10),
                        signingCredentials: signIn
                    );
                    var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);  // Düzgün şekilde oluşturulmuş token
                    return jwtToken;


                }
                else
                {
                    throw new Exception("user is not valid");
                }

            }
           else
            {
                throw new Exception("credentials are not valid");
            }
        }
    }
}

