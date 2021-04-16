using IOWebApplication.Infrastructure.Data.Models.Identity;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Data.Models.Messages
{
    [Table("common_news")]
    public class News
    {
        public News()
        {
            NewsUsers = new List<NewsUser>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Required]
        [StringLength(50)]
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("publish_date")]
        public DateTime PublishDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser Author { get; set; }

        public List<NewsUser> NewsUsers { get; set; }
    }
}
