using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MangaAPI.Models
{
    [Table("ChapterPageAnnotation", Schema = "manga")]
    public class ChapterPageAnnotation
    {
        [Key]
        [Column("chapter_page_annotation_id")]
        public long AnnotationId { get; set; }

        [Column("chapter_page_version_id")]
        public long ChapterPageVersionId { get; set; }

        [Column("page_region_id")]
        public long? PageRegionId { get; set; }

        [Column("annotated_by_user_id")]
        public int AnnotatedByUserId { get; set; }

        [Column("annotation_text")]
        public string AnnotationText { get; set; } = null!;

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}