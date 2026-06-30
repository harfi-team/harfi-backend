namespace Harfi.Models.Constants;

public static class DisputeStatusConstants
{
    public const string Pending = "قيد المراجعة";
    public const string UnderReview = "قيد التحقيق";
    public const string Resolved = "تم الحل";
    public const string Rejected = "مرفوض";

    public static readonly string[] All = [Pending, UnderReview, Resolved, Rejected];
    public static readonly string[] Active = [Pending, UnderReview];
}
