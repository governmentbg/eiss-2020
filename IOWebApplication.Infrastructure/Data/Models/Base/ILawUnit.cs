using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
   public interface ILawUnit
    {
        int LawUnitId { get; set; }

        LawUnit LawUnit { get; set; }
    }
}
