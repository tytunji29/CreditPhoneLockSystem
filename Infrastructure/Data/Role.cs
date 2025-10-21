namespace Infrastructure.Data;

public class Role
{
    public Guid Id { get; set; }  // PK, UUID
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

      // Navigation for all users under this role
        public ICollection<AdminUser> Users { get; set; } = new List<AdminUser>();
    }