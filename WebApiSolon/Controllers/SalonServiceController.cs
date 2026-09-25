using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_SalonSerice</summary>
[Route("api/SalonService")]
public class SalonServiceController : BaseCrudController<Tbl_SalonSerice>
{
    public SalonServiceController(DatabaseContext db) : base(db) { }
}
