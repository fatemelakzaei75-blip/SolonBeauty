using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Interfaces;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس پرسش و پاسخ‌های متداول سالن زیبایی
/// </summary>
public class FAQService : BaseRepository<Tbl_FAQ>, IFAQService
{
    public FAQService(DatabaseContext db) : base(db)
    {
    }

    public async Task<List<Tbl_FAQ>> GetActiveFAQsAsync(string? category = null, CancellationToken ct = default)
    {
        var query = QueryNoTracking.Where(f => f.IsPublished);

        if (!string.IsNullOrWhiteSpace(category) && category != "همه")
        {
            query = query.Where(f => f.Category == category);
        }

        return await query
            .OrderBy(f => f.OrderIndex)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Tbl_FAQ>> GetPagedFAQsAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        return await GetPagedAsync(pagination, ct);
    }

    public override async Task<Tbl_FAQ?> GetByTcAsync(Guid tc, CancellationToken ct = default)
    {
        return await base.GetByTcAsync(tc, ct);
    }

    public async Task<Tbl_FAQ> CreateFAQAsync(FAQCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Tbl_FAQ
        {
            Tc = Guid.NewGuid(),
            Question = dto.Question,
            Answer = dto.Answer,
            Category = dto.Category,
            OrderIndex = dto.DisplayOrder,
            IsPublished = true,
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        await InsertAsync(entity, ct);
        return entity;
    }

    public async Task<bool> UpdateFAQAsync(Guid tc, FAQCreateDto dto, CancellationToken ct = default)
    {
        var item = await Set.FirstOrDefaultAsync(f => f.Tc == tc && !f.IsDelete, ct);
        if (item is null)
        {
            return false;
        }

        item.Question = dto.Question;
        item.Answer = dto.Answer;
        item.Category = dto.Category;
        item.OrderIndex = dto.DisplayOrder;

        return await Db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteFAQAsync(Guid tc, CancellationToken ct = default)
    {
        return await SoftDeleteAsync(tc, ct);
    }
}
