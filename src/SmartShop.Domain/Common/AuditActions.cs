namespace SmartShop.Domain.Common;

public static class AuditActions
{
    public const string Login = "LOGIN";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string Logout = "LOGOUT";
    public const string TokenRefreshed = "TOKEN_REFRESHED";
    public const string RefreshTokenFailed = "REFRESH_TOKEN_FAILED";
    public const string OrderPlaced = "ORDER_PLACED";
    public const string OrderCancelled = "ORDER_CANCELLED";
    public const string ReturnApproved = "RETURN_APPROVED";
    public const string ReturnRejected = "RETURN_REJECTED";
    public const string CouponApplied = "COUPON_APPLIED";
}
