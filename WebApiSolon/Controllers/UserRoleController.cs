using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_UserRole</summary>
[Route("api/UserRole")]
public class UserRoleController : BaseCrudController<Tbl_UserRole>
{
    public UserRoleController(DatabaseContext db) : base(db) { }
}
