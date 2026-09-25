using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataCore.Models.Common;
using DataCore.Models.ViewModels;
using DataLayer.Entity;

namespace DataCore.Interfaces;

public interface ICommentService
{
    Task<List<Tbl_Comment>> GetApprovedCommentsAsync(Guid? personalTc = null, Guid? serviceTc = null, CancellationToken ct = default);

    Task<PagedResult<Tbl_Comment>> GetPagedCommentsAsync(PaginationParams pagination, bool? approvedOnly = null, CancellationToken ct = default);

    Task<Tbl_Comment> AddCommentAsync(CommentCreateDto dto, CancellationToken ct = default);

    Task<bool> ApproveCommentAsync(Guid tc, CancellationToken ct = default);

    Task<bool> DeleteCommentAsync(Guid tc, CancellationToken ct = default);
}
