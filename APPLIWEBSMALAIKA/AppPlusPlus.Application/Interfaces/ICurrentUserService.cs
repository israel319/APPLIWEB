namespace AppPlusPlus.Application.Interfaces;

public interface ICurrentUserService
{
    string? Login { get; }
    int? RoleId { get; }
    string? RoleName { get; }
    bool IsAdmin { get; }
}
