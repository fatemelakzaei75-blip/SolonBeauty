using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataLayer.Context;
using DataLayer.Entity.BaseEntity;
using Microsoft.EntityFrameworkCore;

namespace DataCore.Services;

/// <summary>
/// ریپازیتوری جنریک پایه برای دسترسی استاندارد، ایمن و بهینه به دیتابیس
/// پشتیبانی از AsNoTracking، حذف منطقی، Pagination و Async/Await
/// </summary>
public abstract class BaseRepository<TEntity> where TEntity : Tbl_BaseEntity
{
    protected readonly DatabaseContext Db;

    protected BaseRepository(DatabaseContext db)
    {
        Db = db;
    }

    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    protected IQueryable<TEntity> Query => Set.Where(x => !x.IsDelete);

    protected IQueryable<TEntity> QueryNoTracking => Query.AsNoTracking();

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await QueryNoTracking.ToListAsync(ct);
    }

    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = QueryNoTracking;
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.RegisterData)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<TEntity>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public virtual async Task<TEntity?> GetByTcAsync(Guid tc, CancellationToken ct = default)
    {
        return await QueryNoTracking.FirstOrDefaultAsync(x => x.Tc == tc, ct);
    }

    public virtual async Task<bool> InsertAsync(TEntity entity, CancellationToken ct = default)
    {
        try
        {
            if (entity.Tc == Guid.Empty)
            {
                entity.Tc = Guid.NewGuid();
            }

            if (entity.RegisterData == default)
            {
                entity.RegisterData = DateTime.Now;
            }

            entity.IsDelete = false;
            await Set.AddAsync(entity, ct);
            return await Db.SaveChangesAsync(ct) > 0;
        }
        catch
        {
            return false;
        }
    }

    public virtual async Task<bool> ModifyAsync(TEntity entity, CancellationToken ct = default)
    {
        try
        {
            var current = await Set.FirstOrDefaultAsync(x => x.Tc == entity.Tc && !x.IsDelete, ct);
            if (current is null)
            {
                return false;
            }

            entity.ID = current.ID;
            entity.RegisterData = current.RegisterData;
            Db.Entry(current).CurrentValues.SetValues(entity);
            return await Db.SaveChangesAsync(ct) > 0;
        }
        catch
        {
            return false;
        }
    }

    public virtual async Task<bool> SoftDeleteAsync(Guid tc, CancellationToken ct = default)
    {
        try
        {
            var current = await Set.FirstOrDefaultAsync(x => x.Tc == tc && !x.IsDelete, ct);
            if (current is null)
            {
                return false;
            }

            current.IsDelete = true;
            current.IsActive = false;
            return await Db.SaveChangesAsync(ct) > 0;
        }
        catch
        {
            return false;
        }
    }

    // متدهای همگام برای سازگاری عقب‌رو
    protected List<TEntity> GetAll() => QueryNoTracking.ToList();

    protected TEntity? GetByTc(Guid tc) => QueryNoTracking.FirstOrDefault(x => x.Tc == tc);

    protected TEntity? GetByTc(string tc) => Guid.TryParse(tc, out var g) ? GetByTc(g) : null;

    protected bool Insert(TEntity entity)
    {
        try
        {
            if (entity.Tc == Guid.Empty) entity.Tc = Guid.NewGuid();
            if (entity.RegisterData == default) entity.RegisterData = DateTime.Now;
            entity.IsDelete = false;
            Set.Add(entity);
            return Db.SaveChanges() > 0;
        }
        catch { return false; }
    }

    protected bool Modify(TEntity entity)
    {
        try
        {
            var current = Set.FirstOrDefault(x => x.Tc == entity.Tc && !x.IsDelete);
            if (current is null) return false;

            entity.ID = current.ID;
            entity.RegisterData = current.RegisterData;
            Db.Entry(current).CurrentValues.SetValues(entity);
            return Db.SaveChanges() > 0;
        }
        catch { return false; }
    }

    protected bool SoftDelete(string tc) => Guid.TryParse(tc, out var g) && SoftDelete(g);

    protected bool SoftDelete(Guid tc)
    {
        try
        {
            var current = Set.FirstOrDefault(x => x.Tc == tc && !x.IsDelete);
            if (current is null) return false;
            current.IsDelete = true;
            current.IsActive = false;
            return Db.SaveChanges() > 0;
        }
        catch { return false; }
    }
}
