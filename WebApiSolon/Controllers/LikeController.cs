using DataLayer.Context;
using DataLayer.Entity;
using Microsoft.AspNetCore.Mvc;

namespace WebApiSolon.Controllers;

/// <summary>مدیریت اطلاعات پایه: Tbl_Like</summary>
[Route("api/Like")]
public class LikeController : BaseCrudController<Tbl_Like>
{
    public LikeController(DatabaseContext db) : base(db) { }
}
