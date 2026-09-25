using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Permission</summary>
[Route("api/Permission")]
public class PermissionController : BaseCrudController<Tbl_Permission>
{
    public PermissionController(DatabaseContext db) : base(db) { }
}
