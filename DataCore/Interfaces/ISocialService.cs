using DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.Interfaces
{
    public interface ISocialService
    {
        List<Tbl_Social> GetSocialMedias();
        bool AddSocialMedia(Tbl_Social socialMedia);
        bool EditSocialMedia(Tbl_Social socialMedia);
    }
}
