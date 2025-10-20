

using System.Text.Json.Serialization;

namespace Infrastructure.Data;

public class CustomerFile
{
    public Guid Id { get; set; }  // PK, UUID
    public required string File { get; set; }
    public required Guid CustomerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Customer Customer { get; set; }
}