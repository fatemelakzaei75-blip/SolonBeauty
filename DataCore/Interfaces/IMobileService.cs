using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface IMobileService
    {
        bool IsMobileExists(string mobileNumber);
        Tbl_Mobile Create(string mobileNumber);
    }
}
