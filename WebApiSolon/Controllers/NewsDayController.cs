using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_NewsDay</summary>
[Route("api/NewsDay")]
public class NewsDayController : BaseCrudController<Tbl_NewsDay>
{
    public NewsDayController(DatabaseContext db) : base(db) { }
}
