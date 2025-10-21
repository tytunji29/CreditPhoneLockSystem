using Infrastructure.Data;

public class AdminUser
{
    public Guid Id { get; set; }  // PK, UUID
    public required string FullName { get; set; } 
    public required string Email { get; set; } 
    public required bool IsSuperAdmin { get; set; } = false;
    public required string PasswordHash { get; set; } 
    public required string PasswordSalt { get; set; } 
 // Foreign key for Role
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
    }