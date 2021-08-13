using IOWebApplication.Infrastructure.Data.Models.Nomenclatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Data.Models.Common
{
    /// <summary>
    /// Ключове за интеграция на ниво съд
    /// </summary>
    [Table("common_court_api_key")]
    public class CourtApiKey
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("court_id")]
        public int CourtId { get; set; }

        [Column("integration_type_id")]
        public int IntegrationTypeId { get; set; }

        [Column("key")]
        public string Key { get; set; }

        [Column("secret")]
        public string Secret { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [ForeignKey(nameof(CourtId))]
        public virtual Court Court { get; set; }

        [ForeignKey(nameof(IntegrationTypeId))]
        public virtual IntegrationType IntegrationType { get; set; }
    }
}
