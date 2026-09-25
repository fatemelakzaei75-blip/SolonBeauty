using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Roles</summary>
[Route("api/Role")]
public class RoleController : BaseCrudController<Tbl_Roles>
{
    public RoleController(DatabaseContext db) : base(db) { }
}
