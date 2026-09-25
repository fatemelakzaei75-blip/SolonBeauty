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
/// پیاده‌سازی سرویس نظرات و امتیازدهی مشتریان سالن
/// </summary>
public class CommentService : BaseRepository<Tbl_Comment>, ICommentService
{
    public CommentService(DatabaseContext db) : base(db)
    {
    }

    public async Task<List<Tbl_Comment>> GetApprovedCommentsAsync(
        Guid? personalTc = null,
        Guid? serviceTc = null,
        CancellationToken ct = default)
    {
        return await QueryNoTracking
            .Where(c => c.IsApproved)
            .OrderByDescending(c => c.RegisterData)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Tbl_Comment>> GetPagedCommentsAsync(
        PaginationParams pagination,
        bool? approvedOnly = null,
        CancellationToken ct = default)
    {
        var query = QueryNoTracking;
        if (approvedOnly.HasValue)
        {
            query = query.Where(c => c.IsApproved == approvedOnly.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.RegisterData)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Tbl_Comment>(items, total, pagination.PageNumber, pagination.PageSize);
    }

    public async Task<Tbl_Comment> AddCommentAsync(CommentCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Tbl_Comment
        {
            Tc = Guid.NewGuid(),
            CustomerName = dto.AuthorName,
            ServiceName = "خدمات سالن",
            CommentText = dto.Content,
            Rating = dto.Rating,
            IsApproved = false,
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        await InsertAsync(entity, ct);
        return entity;
    }

    public async Task<bool> ApproveCommentAsync(Guid tc, CancellationToken ct = default)
    {
        var item = await Set.FirstOrDefaultAsync(c => c.Tc == tc && !c.IsDelete, ct);
        if (item is null)
        {
            return false;
        }

        item.IsApproved = true;
        return await Db.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteCommentAsync(Guid tc, CancellationToken ct = default)
    {
        return await SoftDeleteAsync(tc, ct);
    }
}
