using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Confirm</summary>
[Route("api/Confirm")]
public class ConfirmController : BaseCrudController<Tbl_Confirm>
{
    public ConfirmController(DatabaseContext db) : base(db) { }
}
