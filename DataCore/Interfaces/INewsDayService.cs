using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface INewsDayService
    {
        Tbl_NewsDay GetNewsDay();
        bool AddNewsDay(Tbl_NewsDay newsDay);
        bool EditNewsDay(Tbl_NewsDay newsDay);
    }
}
