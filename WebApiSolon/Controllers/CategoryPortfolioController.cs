using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_CategoryPortfolio</summary>
[Route("api/CategoryPortfolio")]
public class CategoryPortfolioController : BaseCrudController<Tbl_CategoryPortfolio>
{
    public CategoryPortfolioController(DatabaseContext db) : base(db) { }
}
