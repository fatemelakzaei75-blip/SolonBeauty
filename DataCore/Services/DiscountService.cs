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
/// سرویس اعتبارسنجی و مدیریت کدهای تخفیف با پشتیبانی از کدهای پیش‌فرض دمو و قوانین مالی
/// </summary>
public class DiscountService : BaseRepository<Tbl_Discount>, IDiscountService
{
    public DiscountService(DatabaseContext db) : base(db)
    {
    }

    public async Task<DiscountValidationResultDto> ValidateDiscountAsync(
        string code,
        decimal amount,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new DiscountValidationResultDto
            {
                Valid = false,
                Message = "کد تخفیف را وارد کنید."
            };
        }

        code = code.Trim().ToUpperInvariant();
        var disc = await QueryNoTracking
            .FirstOrDefaultAsync(d => d.Code.ToUpper() == code && d.IsActive, ct);

        // پشتیبانی از کدهای تخفیف دمو
        if (disc is null)
        {
            if (code == "HADIS20")
            {
                var discountAmt = Math.Min(300_000m, amount * 0.20m);
                return new DiscountValidationResultDto
                {
                    Valid = true,
                    Code = code,
                    Title = "۲۰٪ تخفیف افتتاحیه آنلاین",
                    Percent = 20,
                    DiscountAmount = discountAmt,
                    FinalAmount = Math.Max(0, amount - discountAmt),
                    Message = "کد تخفیف ۲۰٪ با موفقیت اعمال شد."
                };
            }

            if (code == "BEAUTY10")
            {
                var discountAmt = Math.Min(150_000m, amount * 0.10m);
                return new DiscountValidationResultDto
                {
                    Valid = true,
                    Code = code,
                    Title = "۱۰٪ تخفیف همراهان سالن",
                    Percent = 10,
                    DiscountAmount = discountAmt,
                    FinalAmount = Math.Max(0, amount - discountAmt),
                    Message = "کد تخفیف ۱۰٪ با موفقیت اعمال شد."
                };
            }

            return new DiscountValidationResultDto
            {
                Valid = false,
                Message = "کد تخفیف وارد شده نامعتبر یا منقضی شده است."
            };
        }

        if (disc.ExpireDate.HasValue && disc.ExpireDate.Value < DateTime.Now)
        {
            return new DiscountValidationResultDto
            {
                Valid = false,
                Message = "مهلت استفاده از این کد تخفیف به پایان رسیده است."
            };
        }

        if (disc.MinPurchase > 0 && amount < disc.MinPurchase)
        {
            return new DiscountValidationResultDto
            {
                Valid = false,
                Message = $"حداقل مبلغ برای این کد تخفیف {disc.MinPurchase:#,0} تومان است."
            };
        }

        decimal calculatedDiscount = amount * disc.Percent / 100m;
        if (disc.MaxDiscount > 0 && calculatedDiscount > disc.MaxDiscount)
        {
            calculatedDiscount = disc.MaxDiscount;
        }

        return new DiscountValidationResultDto
        {
            Valid = true,
            Code = disc.Code,
            Title = disc.Title,
            Percent = disc.Percent,
            DiscountAmount = calculatedDiscount,
            FinalAmount = Math.Max(0, amount - calculatedDiscount),
            Message = $"کد تخفیف {disc.Percent}٪ با موفقیت اعمال شد."
        };
    }

    public async Task<List<Tbl_Discount>> GetAllDiscountsAsync(CancellationToken ct = default)
    {
        return await GetAllAsync(ct);
    }

    public async Task<PagedResult<Tbl_Discount>> GetPagedDiscountsAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        return await GetPagedAsync(pagination, ct);
    }

    public async Task<Tbl_Discount?> CreateDiscountAsync(DiscountCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Tbl_Discount
        {
            Tc = Guid.NewGuid(),
            Code = dto.Code.Trim().ToUpperInvariant(),
            Title = dto.Title,
            Percent = dto.Percent,
            MaxDiscount = dto.MaxDiscount,
            MinPurchase = dto.MinPurchase,
            ExpireDate = dto.ExpireDate,
            UsageCount = 0,
            IsActive = true,
            RegisterData = DateTime.Now,
            IsDelete = false
        };

        var inserted = await InsertAsync(entity, ct);
        return inserted ? entity : null;
    }

    public async Task<bool> DeleteDiscountAsync(Guid tc, CancellationToken ct = default)
    {
        return await SoftDeleteAsync(tc, ct);
    }
}
