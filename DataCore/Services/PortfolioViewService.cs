using System.Collections.Generic;
using DataCore.Interfaces;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس شمارش بازدیدهای یکتای نمونه‌کارها
/// </summary>
public class PortfolioViewService : IPortfolioViewService
{
    private static readonly Dictionary<string, HashSet<string>> Views = new();

    public bool HasUserViewed(string portfolioTC, string userTC)
    {
        return Views.TryGetValue(portfolioTC, out var users) && users.Contains(userTC);
    }

    public bool AddView(string portfolioTC, string userTC)
    {
        if (!Views.TryGetValue(portfolioTC, out var users))
        {
            Views[portfolioTC] = users = new HashSet<string>();
        }

        return users.Add(userTC);
    }

    public int GetViewCount(string portfolioTC)
    {
        return Views.TryGetValue(portfolioTC, out var users) ? users.Count : 0;
    }
}
