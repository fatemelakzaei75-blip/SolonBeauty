using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IPortfolioViewService
    {
        bool HasUserViewed(string portfolioTC, string userTC);
        bool AddView(string portfolioTC, string userTC);
        int GetViewCount(string portfolioTC);
    }
}
