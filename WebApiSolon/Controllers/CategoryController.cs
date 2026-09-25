using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Category</summary>
[Route("api/Category")]
public class CategoryController : BaseCrudController<Tbl_Category>
{
    public CategoryController(DatabaseContext db) : base(db) { }
}
