using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Customer</summary>
[Route("api/Customer")]
public class CustomerController : BaseCrudController<Tbl_Customer>
{
    public CustomerController(DatabaseContext db) : base(db) { }
}
