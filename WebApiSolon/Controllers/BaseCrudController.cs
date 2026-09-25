using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataLayer.Context;
using DataLayer.Entity.BaseEntity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApiSolon.Controllers;

/// <summary>
/// کنترلر پایه CRUD برای موجودیت‌های پایه با پشتیبانی کامل از استانداردهای RESTful، HATEOAS و صفحه‌بندی
/// حذف به صورت منطقی (Soft Delete) انجام می‌شود.
/// </summary>
/// <typeparam name="TEntity">موجودیت مشتق شده از Tbl_BaseEntity</typeparam>
[ApiController]
[Authorize(Roles = "Admin")]
public abstract class BaseCrudController<TEntity> : BaseApiController where TEntity : Tbl_BaseEntity
{
    protected readonly DatabaseContext Db;

    protected BaseCrudController(DatabaseContext db)
    {
        Db = db;
    }

    protected DbSet<TEntity> Set => Db.Set<TEntity>();

    protected IQueryable<TEntity> Query => Set.AsNoTracking().Where(x => !x.IsDelete);

    /// <summary>
    /// فهرست کامل رکوردهای فعال (سازگار با ApiDataStore کلاینت)
    /// </summary>
    [HttpGet]
    public virtual async Task<ActionResult<List<TEntity>>> GetAll(CancellationToken ct = default)
    {
        var route = Request.Path.Value ?? "/api";
        Response.Headers.Append("Link", $"<{route}>; rel=\"self\", <{route}/paged>; rel=\"paged\"");

        var list = await Query.ToListAsync(ct);
        return Ok(list);
    }

    /// <summary>
    /// دریافت فهرست با صفحه‌بندی استاندارد سمت سرور
    /// </summary>
    [HttpGet("paged")]
    public virtual async Task<ActionResult<ApiResponse<PagedResult<TEntity>>>> GetPaged(
        [FromQuery] PaginationParams pagination,
        CancellationToken ct = default)
    {
        var total = await Query.CountAsync(ct);
        var items = await Query
            .OrderByDescending(x => x.RegisterData)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        var paged = new PagedResult<TEntity>(items, total, pagination.PageNumber, pagination.PageSize);
        var route = Request.Path.Value ?? "/api";

        return PagedResponse(paged, route);
    }

    /// <summary>
    /// دریافت یک رکورد بر اساس شناسه TC
    /// </summary>
    [HttpGet("{tc:guid}")]
    public virtual async Task<ActionResult<TEntity>> Get(Guid tc, CancellationToken ct = default)
    {
        var item = await Query.FirstOrDefaultAsync(x => x.Tc == tc, ct);
        if (item is null)
        {
            return NotFound(new { Success = false, Message = "رکورد یافت نشد" });
        }

        var route = Request.Path.Value ?? $"/api/{tc}";
        Response.Headers.Append("Link", $"<{route}>; rel=\"self\", <{route}>; rel=\"update\", <{route}>; rel=\"delete\"");

        return Ok(item);
    }

    /// <summary>
    /// ثبت رکورد جدید
    /// </summary>
    [HttpPost]
    public virtual async Task<ActionResult<TEntity>> Create(
        [FromBody] TEntity entity,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        entity.ID = 0;
        if (entity.Tc == Guid.Empty)
        {
            entity.Tc = Guid.NewGuid();
        }

        entity.RegisterData = DateTime.Now;
        entity.IsDelete = false;

        await Set.AddAsync(entity, ct);
        await Db.SaveChangesAsync(ct);

        var route = $"{Request.Path.Value?.TrimEnd('/')}/{entity.Tc}";
        Response.Headers.Append("Link", $"<{route}>; rel=\"self\"");

        return Ok(entity);
    }

    /// <summary>
    /// ویرایش رکورد موجود
    /// </summary>
    [HttpPut("{tc:guid}")]
    public virtual async Task<ActionResult<TEntity>> Update(
        Guid tc,
        [FromBody] TEntity entity,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var current = await Set.FirstOrDefaultAsync(x => x.Tc == tc && !x.IsDelete, ct);
        if (current is null)
        {
            return NotFound(new { Success = false, Message = "رکورد یافت نشد" });
        }

        entity.ID = current.ID;
        entity.Tc = current.Tc;
        entity.RegisterData = current.RegisterData;

        Db.Entry(current).CurrentValues.SetValues(entity);
        await Db.SaveChangesAsync(ct);

        return Ok(current);
    }

    /// <summary>
    /// حذف منطقی (Soft Delete) رکورد
    /// </summary>
    [HttpDelete("{tc:guid}")]
    public virtual async Task<ActionResult> Delete(Guid tc, CancellationToken ct = default)
    {
        var current = await Set.FirstOrDefaultAsync(x => x.Tc == tc && !x.IsDelete, ct);
        if (current is null)
        {
            return NotFound(new { Success = false, Message = "رکورد یافت نشد" });
        }

        current.IsDelete = true;
        current.IsActive = false;
        await Db.SaveChangesAsync(ct);

        return Ok(new { Success = true, Message = "رکورد با موفقیت حذف گردید" });
    }
}
