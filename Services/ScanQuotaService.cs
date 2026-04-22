namespace DoAnCSharp.Services;

public class ScanQuotaService
{
    private const string QuotaKey = "scan_quota_remaining";
    private const int FreeQuota = 3;

    private string GetUserQuotaKey()
    {
        string email = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "guest");
        return $"{QuotaKey}_{email}";
    }

    public int GetRemaining()
        => Microsoft.Maui.Storage.Preferences.Default.Get(GetUserQuotaKey(), FreeQuota);

    // Trả về true nếu còn lượt, false nếu hết
    public bool TryUseOne()
    {
        int remaining = GetRemaining();
        if (remaining <= 0) return false;
        Microsoft.Maui.Storage.Preferences.Default.Set(GetUserQuotaKey(), remaining - 1);
        return true;
    }

    public void AddScans(int count)
    {
        int current = GetRemaining();
        Microsoft.Maui.Storage.Preferences.Default.Set(GetUserQuotaKey(), current + count);
    }

#if DEBUG
    public void ResetToFree()
        => Microsoft.Maui.Storage.Preferences.Default.Set(GetUserQuotaKey(), FreeQuota);
#endif
}
