using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Bắt buộc phải có để dùng thư viện Database
using MangaAPI.Services;
using MangaAPI.Data;
using MangaAPI.Models;

namespace MangaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MangaController : ControllerBase
    {
        private readonly MangaAiService _mangaAiService;
        private readonly AppDbContext _dbContext;

        // Bơm AppDbContext vào Controller
        public MangaController(MangaAiService mangaAiService, AppDbContext dbContext)
        {
            _mangaAiService = mangaAiService;
            _dbContext = dbContext;
        }

        // ==========================================
        // API GỐC: UPLOAD VÀ LƯU DATABASE
        // ==========================================
        [HttpPost("upload-and-analyze")]
        public async Task<IActionResult> UploadPage(IFormFile mangaPage, [FromForm] long chapterPageVersionId)
        {
            var aiResult = await _mangaAiService.AnalyzePageAsync(mangaPage);

            if (aiResult == null || aiResult.Status != "success" || aiResult.Data == null)
            {
                return BadRequest("AI không thể phân tích trang truyện này.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var region in aiResult.Data)
                {
                    var newRegion = new PageRegion
                    {
                        ChapterPageVersionId = chapterPageVersionId,
                        TypeCode = region.RegionType == "speech-balloon" ? "SPEECH_BUBBLE" : "OTHER",
                        X = (decimal)region.X,
                        Y = (decimal)region.Y,
                        Width = (decimal)region.Width,
                        Height = (decimal)region.Height,
                        ConfidenceScore = (decimal)region.Confidence,
                        SourceType = "AI"
                    };
                    
                    _dbContext.PageRegions.Add(newRegion);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync(); 

                return Ok(new { 
                    Message = "Đã phân tích và lưu Database thành công!", 
                    TotalRegionsSaved = aiResult.Total_Regions, 
                    Regions = aiResult.Data
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); 
                return StatusCode(500, "Lỗi khi lưu Database: " + ex.Message);
            }
        }

        // ==========================================
        // API 1: MANGAKA GIAO TASK CHO ASSISTANT
        // ==========================================
        [HttpPost("assign-task-to-region")]
        public async Task<IActionResult> AssignTask([FromBody] CreateTaskRequest request)
        {
            var regionExists = await _dbContext.PageRegions
                .AnyAsync(r => r.PageRegionId == request.PageRegionId);

            if (!regionExists) return BadRequest("Vùng truyện này chưa được lưu!");

            var newAnnotation = new ChapterPageAnnotation
            {
                ChapterPageVersionId = request.ChapterPageVersionId,
                PageRegionId = request.PageRegionId,
                AnnotatedByUserId = request.MangakaUserId,
                AnnotationText = request.TaskNote
            };

            _dbContext.ChapterPageAnnotations.Add(newAnnotation);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Đã gán task thành công!", TaskId = newAnnotation.AnnotationId });
        }

        // ==========================================
        // API 2: AUTO-TRANSLATE
        // ==========================================
        // ==========================================
        // API 2: AUTO-TRANSLATE (PHIÊN BẢN GỌI AI THẬT)
        // ==========================================
        [HttpPost("auto-translate-regions")]
        public async Task<IActionResult> TriggerAutoTranslate(IFormFile mangaPage, [FromForm] long chapterPageVersionId)
        {
            // 1. Lấy các khung đỏ (Speech Bubble) từ Database
            var speechRegions = await _dbContext.PageRegions
                .Where(r => r.ChapterPageVersionId == chapterPageVersionId && r.TypeCode == "SPEECH_BUBBLE")
                .ToListAsync();

            if (!speechRegions.Any()) return BadRequest("Không tìm thấy bong bóng thoại nào để dịch.");

            // 2. XÓA ĐOẠN DỮ LIỆU GIẢ CŨ VÀ GỌI AI PYTHON DỊCH THẬT
            var translationResult = await _mangaAiService.TranslateRegionsAsync(mangaPage, speechRegions);
            if (translationResult == null) return StatusCode(500, "AI Python gặp lỗi khi dịch thuật.");

            // 3. Đè bản dịch thật từ AI vào Database
            foreach (var region in speechRegions)
            {
                var match = translationResult.FirstOrDefault(t => t.PageRegionId == region.PageRegionId);
                if (match != null)
                {
                    region.OriginalText = match.TranslatedText; 
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { 
                Message = "Auto-Translate chạy thành công!", 
                Regions = speechRegions 
            });
        }
    }

    // ==========================================
    // DTO CLASS (Để ngoài Controller, nhưng vẫn trong Namespace)
    // ==========================================
    // ==========================================
    // DTO CLASS (Để ngoài Controller, nhưng vẫn trong Namespace)
    // ==========================================
    public class CreateTaskRequest
    {
        public long ChapterPageVersionId { get; set; }
        public long PageRegionId { get; set; }
        public int MangakaUserId { get; set; }
        public string TaskNote { get; set; } = string.Empty;
    }
}