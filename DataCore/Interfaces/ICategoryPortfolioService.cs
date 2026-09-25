using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface ICategoryPortfolioService
    {
        List<Tbl_CategoryPortfolio> GetCategoryPortfolios();

        Tbl_CategoryPortfolio GetCategoryPortfolioByTC(string tc);

        List<Tbl_CategoryPortfolio> GetCategoryPortfoliosBySalonTC(string salonTC);

        List<Tbl_CategoryPortfolio> GetCategoryPortfoliosByCategoryTC(string categoryTC);

        bool AddCategoryPortfolio(Tbl_CategoryPortfolio categoryPortfolio);

        bool EditCategoryPortfolio(Tbl_CategoryPortfolio categoryPortfolio);

        bool DeleteCategoryPortfolio(string tc);
    }
}
    

