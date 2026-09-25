using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace DataCore.Interfaces;

public interface IDiscountService
{
    Task<DiscountValidationResultDto> ValidateDiscountAsync(string code, decimal amount, CancellationToken ct = default);

    Task<List<Tbl_Discount>> GetAllDiscountsAsync(CancellationToken ct = default);

    Task<PagedResult<Tbl_Discount>> GetPagedDiscountsAsync(PaginationParams pagination, CancellationToken ct = default);

    Task<Tbl_Discount?> GetByTcAsync(Guid tc, CancellationToken ct = default);

    Task<Tbl_Discount?> CreateDiscountAsync(DiscountCreateDto dto, CancellationToken ct = default);

    Task<bool> DeleteDiscountAsync(Guid tc, CancellationToken ct = default);
}
