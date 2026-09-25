using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Personal</summary>
[Route("api/Personal")]
public class PersonalController : BaseCrudController<Tbl_Personal>
{
    public PersonalController(DatabaseContext db) : base(db) { }
}
