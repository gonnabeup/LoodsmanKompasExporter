using LoodsmanKompasExporter.Models;
using System;
using System.IO;
using System.Collections.Generic;

namespace LoodsmanKompasExporter.Services
{
    public static class ExportFormatService
    {
        public static KompasDocumentType GetDocumentType(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return KompasDocumentType.Unknown;

            string extension = Path.GetExtension(filePath)
                ?.ToLowerInvariant();

            switch (extension)
            {
                case ".m3d":
                    return KompasDocumentType.Part3D;

                case ".a3d":
                    return KompasDocumentType.Assembly3D;

                case ".cdw":
                    return KompasDocumentType.Drawing2D;

                case ".spw":
                    return KompasDocumentType.Specification;

                case ".frw":
                    return KompasDocumentType.Fragment2D;

                case ".kdw":
                    return KompasDocumentType.Text;

                default:
                    return KompasDocumentType.Unknown;
            }
        }

        public static IReadOnlyList<ExportFormat> GetSupportedFormats(
            KompasDocumentType documentType)
        {
            switch (documentType)
            {
                case KompasDocumentType.Part3D:
                    return new[]
                    {
                        ExportFormat.Step,
                        ExportFormat.Iges,
                        ExportFormat.Stl,
                        ExportFormat.Sat,
                        ExportFormat.Xt,
                        ExportFormat.Vrml
                    };

                case KompasDocumentType.Assembly3D:
                    return new[]
                    {
                        ExportFormat.Step,
                        ExportFormat.Iges,
                        ExportFormat.Sat,
                        ExportFormat.Xt,
                        ExportFormat.Vrml
                    };

                case KompasDocumentType.Drawing2D:
                case KompasDocumentType.Fragment2D:
                    return new[]
                    {
                        //база
                        ExportFormat.Pdf,
                        ExportFormat.Dxf,
                        ExportFormat.Dwg,

                        //изображения
                        ExportFormat.Bmp,
                        ExportFormat.Gif,
                        ExportFormat.Jpeg,
                        ExportFormat.Png,
                        ExportFormat.Tga,
                        ExportFormat.Tiff,
                        ExportFormat.Emf
                    };

                case KompasDocumentType.Specification:
                    return new[]
                    {
                        //база
                        ExportFormat.Pdf,

                        //изображения
                        ExportFormat.Bmp,
                        ExportFormat.Gif,
                        ExportFormat.Jpeg,
                        ExportFormat.Png,
                        ExportFormat.Tga,
                        ExportFormat.Tiff,
                        ExportFormat.Emf,

                        //таблицы
                        ExportFormat.Xls,
                        ExportFormat.Xlsx,
                        ExportFormat.Ods
                    };

                case KompasDocumentType.Text:
                    return new[]
                    {
                        //база
                        ExportFormat.Pdf,

                        //изображения
                        ExportFormat.Bmp,
                        ExportFormat.Gif,
                        ExportFormat.Jpeg,
                        ExportFormat.Png,
                        ExportFormat.Tga,
                        ExportFormat.Tiff,
                        ExportFormat.Emf,

                        //текстовые
                        ExportFormat.Txt
                    };

                default:
                    return Array.Empty<ExportFormat>();
            }
        }

        public static bool IsSupported(
            KompasDocumentType documentType,
            ExportFormat exportFormat)
        {
            IReadOnlyList<ExportFormat> formats =
                GetSupportedFormats(documentType);

            foreach (ExportFormat format in formats)
            {
                if (format == exportFormat)
                    return true;
            }

            return false;
        }

        public static string GetExtension(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Step:
                    return ".step";

                case ExportFormat.Iges:
                    return ".igs";

                case ExportFormat.Stl:
                    return ".stl";

                case ExportFormat.Sat:
                    return ".sat";

                case ExportFormat.Xt:
                    return ".x_t";

                case ExportFormat.Vrml:
                    return ".wrl";

                case ExportFormat.Pdf:
                    return ".pdf";

                case ExportFormat.Dxf:
                    return ".dxf";

                case ExportFormat.Dwg:
                    return ".dwg";

                case ExportFormat.Bmp:
                    return ".bmp";

                case ExportFormat.Gif:
                    return ".gif";

                case ExportFormat.Jpeg:
                    return ".jpg";

                case ExportFormat.Png:
                    return ".png";

                case ExportFormat.Tiff:
                    return ".tif";

                case ExportFormat.Tga:
                    return ".tga";

                case ExportFormat.Emf:
                    return ".emf";

                case ExportFormat.Xls:
                    return ".xls";

                case ExportFormat.Xlsx:
                    return ".xlsx";

                case ExportFormat.Ods:
                    return ".ods";

                case ExportFormat.Txt:
                    return ".txt";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(format),
                        format,
                        "Неизвестный формат.");
            }
        }
    }
}
