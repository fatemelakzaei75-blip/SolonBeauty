using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Mobile</summary>
[Route("api/Mobile")]
public class MobileController : BaseCrudController<Tbl_Mobile>
{
    public MobileController(DatabaseContext db) : base(db) { }
}
