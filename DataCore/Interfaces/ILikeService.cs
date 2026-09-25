using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface ILikeService
    {
        bool IsLiked(string customerTC, string portfolioTC);
        bool AddLike(string customerTC, string portfolioTC);
        bool RemoveLike(string customerTC, string portfolioTC);
        int GetLikeCount(string portfolioTC);
        List<Tbl_Like> GetLikesByCustomerTC(string customerTC);
        List<Tbl_Like> GetLikesByPortfolioTC(string portfolioTC);
    }
}
