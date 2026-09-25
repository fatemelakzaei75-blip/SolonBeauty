using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace DataCore.Interfaces;

public interface IFAQService
{
    Task<List<Tbl_FAQ>> GetActiveFAQsAsync(string? category = null, CancellationToken ct = default);

    Task<PagedResult<Tbl_FAQ>> GetPagedFAQsAsync(PaginationParams pagination, CancellationToken ct = default);

    Task<Tbl_FAQ?> GetByTcAsync(Guid tc, CancellationToken ct = default);

    Task<Tbl_FAQ> CreateFAQAsync(FAQCreateDto dto, CancellationToken ct = default);

    Task<bool> UpdateFAQAsync(Guid tc, FAQCreateDto dto, CancellationToken ct = default);

    Task<bool> DeleteFAQAsync(Guid tc, CancellationToken ct = default);
}
