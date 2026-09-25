using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface ISalonServiceService
    {
        List<Tbl_SalonSerice> GetServices();

        Tbl_SalonSerice GetServiceByTC(string tc);

        List<Tbl_SalonSerice> GetServicesByCategoryTC(string categoryTC);

        List<Tbl_SalonSerice> SearchServices(string search);

        bool AddService(Tbl_SalonSerice service);

        bool EditService(Tbl_SalonSerice service);

        bool DeleteService(string tc);
    }
}
