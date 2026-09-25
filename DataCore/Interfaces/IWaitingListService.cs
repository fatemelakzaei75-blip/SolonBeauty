using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace DataCore.Interfaces;

public interface IWaitingListService
{
    Task<List<Tbl_WaitingList>> GetActiveWaitingListAsync(CancellationToken ct = default);

    Task<PagedResult<Tbl_WaitingList>> GetPagedWaitingListAsync(PaginationParams pagination, CancellationToken ct = default);

    Task<Tbl_WaitingList> AddToWaitingListAsync(WaitingListCreateDto dto, CancellationToken ct = default);

    Task<bool> MarkNotifiedAsync(Guid tc, CancellationToken ct = default);

    Task<bool> DeleteFromWaitingListAsync(Guid tc, CancellationToken ct = default);
}
