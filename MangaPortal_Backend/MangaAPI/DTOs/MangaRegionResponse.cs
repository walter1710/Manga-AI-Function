namespace MangaAPI.DTOs
{
    public class MangaRegionResponse
    {
        // Thêm dấu ? vào sau string và List
        public string? Status { get; set; } 
        public int Total_Regions { get; set; }
        public List<RegionData>? Data { get; set; } 
    }

    public class RegionData
    {
        // Thêm dấu ? vào sau string
        public string? RegionType { get; set; } 
        public double Confidence { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}