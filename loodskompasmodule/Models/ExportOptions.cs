



namespace LoodsmanKompasExporter.Models
{
    public class ExportOptions
    {
        public string OutputDirectory { get; set; }

        public ExportFormat Model3DFormat { get; set; }

        public ExportFormat Drawing2DFormat { get; set; }

        public ExportFormat Fragment2DFormat { get; set; }

        public ExportFormat SpecificationFormat { get; set; }

        public ExportFormat TextFormat { get; set; }
    }
}

