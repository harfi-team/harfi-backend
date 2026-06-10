using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.Admin;

public class ServiceTypeDto
{
    public int Id { get; set; }

    [Required]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    public string NameEn { get; set; } = string.Empty;

    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CityDto
{
    public int Id { get; set; }

    [Required]
    public string NameAr { get; set; } = string.Empty;

    [Required]
    public string NameEn { get; set; } = string.Empty;

    public string? Governorate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class FeatureFlagDto
{
    public string Key { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateFeatureFlagRequest
{
    public bool IsEnabled { get; set; }
}
