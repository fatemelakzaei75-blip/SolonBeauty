using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Portfoilo</summary>
[Route("api/Portfolio")]
public class PortfolioController : BaseCrudController<Tbl_Portfoilo>
{
    public PortfolioController(DatabaseContext db) : base(db) { }
}
