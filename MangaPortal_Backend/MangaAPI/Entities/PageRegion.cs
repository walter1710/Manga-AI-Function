using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MangaAPI.Models
{
    // Chỉ định rõ bảng thuộc schema 'manga'
    [Table("PageRegion", Schema = "manga")] 
    public class PageRegion
    {
        [Key]
        [Column("page_region_id")]
        public long PageRegionId { get; set; }

        [Column("chapter_page_version_id")]
        public long ChapterPageVersionId { get; set; }

        [Column("type_code")]
        public string TypeCode { get; set; } = null!;

        [Column("region_label")]
        public string? RegionLabel { get; set; }

        [Column("x", TypeName = "decimal(10,2)")]
        public decimal X { get; set; }

        [Column("y", TypeName = "decimal(10,2)")]
        public decimal Y { get; set; }

        [Column("width", TypeName = "decimal(10,2)")]
        public decimal Width { get; set; }

        [Column("height", TypeName = "decimal(10,2)")]
        public decimal Height { get; set; }

        [Column("confidence_score", TypeName = "decimal(5,4)")]
        public decimal? ConfidenceScore { get; set; }

        [Column("source_type")]
        public string SourceType { get; set; } = "AI"; // Mặc định do AI tạo

        [Column("original_text")]
        public string? OriginalText { get; set; }

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}