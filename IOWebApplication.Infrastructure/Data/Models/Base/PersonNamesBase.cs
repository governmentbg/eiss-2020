using IOWebApplication.Infrastructure.Data.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Base
{
    public abstract class PersonNamesBase : NamesBase
    {
        [Column("person_id")]
        public int? PersonId { get; set; }

        [ForeignKey(nameof(PersonId))]
        public Person Person { get; set; }        
    }
}
