namespace Harfi.DTOs.Admin;

public class ServiceTypeDto
{
    public string NameAr { get; set; } = string.Empty;
}

public class CityDto
{
    public string NameAr { get; set; } = string.Empty;
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
