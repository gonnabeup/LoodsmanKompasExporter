using System.Collections.Generic;

namespace LoodsmanKompasExporter.Models
{
    public class ExportResult
    {
        public int TotalCount { get; set; }

        public int ExportedCount { get; set; }

        public int FailedCount
        {
            get { return Errors.Count; }
        }

        public List<string> Errors { get; } =
            new List<string>();
    }
}
