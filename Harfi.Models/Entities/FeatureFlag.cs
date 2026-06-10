using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Harfi.Models.Entities;

public class FeatureFlag
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
