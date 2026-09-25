using System;
using System.Collections.Generic;
using System.Linq;
using DataCore.Interfaces;
using DataLayer.Context;
using DataLayer.Entity;

namespace DataCore.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت مشتریان
/// </summary>
public class CustomerService : BaseRepository<Tbl_Customer>, ICustomerService
{
    public CustomerService(DatabaseContext db) : base(db)
    {
    }

    public List<Tbl_Customer> GetCustomers()
    {
        return GetAll();
    }

    public Tbl_Customer GetCustomerByToken(Guid token)
    {
        return GetByTc(token)!;
    }

    public bool IsCustomerExists(Guid token)
    {
        return QueryNoTracking.Any(x => x.Tc == token);
    }

    public List<Tbl_Customer> SearchCustomers(string search)
    {
        return QueryNoTracking
            .Where(x => x.FullName.Contains(search) || (x.CodeMoaref ?? "").Contains(search))
            .ToList();
    }

    public bool AddCustomer(Tbl_Customer customer)
    {
        return Insert(customer);
    }

    public bool EditCustomer(Tbl_Customer customer)
    {
        return Modify(customer);
    }

    public bool DeleteCustomer(string token)
    {
        return SoftDelete(token);
    }
}
