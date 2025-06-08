namespace Business.DTOs;
public class ValidatedUserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
