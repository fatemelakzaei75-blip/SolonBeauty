using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Salon</summary>
[Route("api/Salon")]
public class SalonController : BaseCrudController<Tbl_Salon>
{
    public SalonController(DatabaseContext db) : base(db) { }
}
