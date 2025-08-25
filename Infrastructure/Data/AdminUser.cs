public class AdminUser
{
    public Guid Id { get; set; }  // PK, UUID
    public required string FullName { get; set; } 
    public required string Email { get; set; } 
    public required string PasswordHarsh { get; set; } 
    public required string PasswordSalt { get; set; } 
}