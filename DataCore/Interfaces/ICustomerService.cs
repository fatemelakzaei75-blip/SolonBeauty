using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface ICustomerService
    {
        List<Tbl_Customer> GetCustomers();
        Tbl_Customer GetCustomerByToken(Guid token);
        bool IsCustomerExists(Guid token);
        List<Tbl_Customer> SearchCustomers(string search);
        bool AddCustomer(Tbl_Customer customer);
        bool EditCustomer(Tbl_Customer customer);
        bool DeleteCustomer(string token);
    }
}
