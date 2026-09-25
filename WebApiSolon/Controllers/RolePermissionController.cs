using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_RolePermission</summary>
[Route("api/RolePermission")]
public class RolePermissionController : BaseCrudController<Tbl_RolePermission>
{
    public RolePermissionController(DatabaseContext db) : base(db) { }
}
