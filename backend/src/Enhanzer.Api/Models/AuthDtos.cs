using System.ComponentModel.DataAnnotations;

namespace Enhanzer.Api.Models;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserInfoDto? User { get; set; }
    public int LocationsCount { get; set; }
}

public class LocationDto
{
    public int Id { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public int? StockHandle { get; set; }
}

