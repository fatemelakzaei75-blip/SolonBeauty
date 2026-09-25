using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface ICategoryService
    {
        List<Tbl_Category> GetCategories();
        Tbl_Category GetCategoryByTC(string tc);
        List<Tbl_Category> SearchCategories(string search);
        List<Tbl_Category> GetCategoriesBySalonTC(string salonTC);
        bool AddCategory(Tbl_Category category);
        bool EditCategory(Tbl_Category category);
        bool DeleteCategory(string tc);
    }
}
