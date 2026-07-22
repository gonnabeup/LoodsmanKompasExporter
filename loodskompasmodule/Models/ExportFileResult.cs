namespace LoodsmanKompasExporter.Models
{
    public class ExportFileResult
    {
        public string SourcePath { get; set; }
        
        public string OutputPath { get; set; }
        public string Product { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }
    }
}
