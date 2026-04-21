namespace DoAnCSharp.Services;

public class ScanQuotaService
{
    private const string QuotaKey = "scan_quota_remaining";
    private const int FreeQuota = 3;

    public int GetRemaining()
        => Microsoft.Maui.Storage.Preferences.Default.Get(QuotaKey, FreeQuota);

    // Trả về true nếu còn lượt, false nếu hết
    public bool TryUseOne()
    {
        int remaining = GetRemaining();
        if (remaining <= 0) return false;
        Microsoft.Maui.Storage.Preferences.Default.Set(QuotaKey, remaining - 1);
        return true;
    }

    public void AddScans(int count)
    {
        int current = GetRemaining();
        Microsoft.Maui.Storage.Preferences.Default.Set(QuotaKey, current + count);
    }

#if DEBUG
    public void ResetToFree()
        => Microsoft.Maui.Storage.Preferences.Default.Set(QuotaKey, FreeQuota);
#endif
}
