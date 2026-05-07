namespace SmartShop.WebAPI.Options;

public class RateLimitOptions
{
    public bool Enabled { get; set; } = true;
    public Dictionary<string, RateLimitRule> Rules { get; set; } = new();
}

public class RateLimitRule
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}
