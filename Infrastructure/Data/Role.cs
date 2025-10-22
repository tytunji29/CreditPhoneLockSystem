using System.Text.Json.Serialization;

namespace Infrastructure.Data;

public class Role
{
    public Guid Id { get; set; }  // PK, UUID
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [JsonIgnore]
    public ICollection<AdminUser> Users { get; set; } = new List<AdminUser>();
    }