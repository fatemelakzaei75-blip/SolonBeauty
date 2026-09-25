using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IPortfolioService
    {
        List<Tbl_Portfoilo> GetPortfolios();
        Tbl_Portfoilo GetPortfolioByTC(string tc);
        Tbl_Portfoilo GetPortfolioByPortfolioSampleTC(string portfolioSampleTC);
        List<Tbl_Portfoilo> SearchPortfolios(string search);
        List<Tbl_Portfoilo> GetMostLikedPortfolios();
        bool AddPortfolio(Tbl_Portfoilo portfolio);
        bool EditPortfolio(Tbl_Portfoilo portfolio);
        bool DeletePortfolio(string tc);
    }
}
