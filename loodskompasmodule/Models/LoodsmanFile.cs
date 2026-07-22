namespace LoodsmanKompasExporter.Models
{
    public class LoodsmanFile
    {
        public int IdVersion { get; set; }

        public string FileName { get; set; }

        public string LocalName { get; set; }

        public string ExtractedPath { get; set; }
        public string Product { get; set; }
    }
}
