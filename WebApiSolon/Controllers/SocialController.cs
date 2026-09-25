using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Social</summary>
[Route("api/Social")]
public class SocialController : BaseCrudController<Tbl_Social>
{
    public SocialController(DatabaseContext db) : base(db) { }
}
