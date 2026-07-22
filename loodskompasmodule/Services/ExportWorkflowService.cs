//                               .-') _      .-') _    ('-.    .-. .-')    ('-.                 _ (`-.  
//                              ( OO ) )    ( OO ) )  ( OO ).-.\  ( OO ) _(  OO)               ( (OO  ) 
//   ,----.     .-'),-----. ,--./ ,--,' ,--./ ,--,'   / . --. / ;-----.\(,------. ,--. ,--.   _.`     \ 
//  '  .-./-') ( OO'  .-.  '|   \ |  |\ |   \ |  |\   | \-.  \  | .-.  | |  .---' |  | |  |  (__...--'' 
//  |  |_( O- )/   |  | |  ||    \|  | )|    \|  | ).-'-'  |  | | '-' /_)|  |     |  | | .-') |  /  | | 
//  |  | .--, \\_) |  |\|  ||  .     |/ |  .     |/  \| |_.'  | | .-. `.(|  '--.  |  |_|( OO )|  |_.' | 
// (|  | '. (_/  \ |  | |  ||  |\    |  |  |\    |    |  .-.  | | |  \  ||  .--'  |  | | `-' /|  .___.' 
//  |  '--'  |    `'  '-'  '|  | \   |  |  | \   |    |  | |  | | '--'  /|  `---.('  '-'(_.-' |  |      
//   `------'       `-----' `--'  `--'  `--'  `--'    `--' `--' `------' `------'  `-----'    `--'     

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanKompasExporter.Models;
using LoodsmanKompasExporter.Services;
using LoodsmanKompasExporter.Views;

namespace LoodsmanKompasExporter.Services
{
    public class ExportWorkflowService
    {
        private readonly INetPluginCall _npc;
        private readonly LoodsmanService _loodsmanService;
        private readonly KompasService _kompasService;

        public ExportWorkflowService(INetPluginCall npc)
        {
            _npc = npc ?? throw new ArgumentNullException(nameof(npc));

            _loodsmanService = new LoodsmanService(npc);
            _kompasService = new KompasService();
        }

        public void Run()
        {
            try
            {
                List<int> selectedIds = GetSelectedIds();

                if (selectedIds.Count == 0)
                {
                    MessageBox.Show(
                        "Не выбрано ни одного объекта.",
                        "Экспорт",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }

                List<LoodsmanFile> files = GetFiles(selectedIds);

                if (files.Count == 0)
                {
                    MessageBox.Show(
                        "В выбранных объектах не найдено файлов КОМПАС.",
                        "Экспорт",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    return;
                }

                ExportOptions options = ShowOptions(files);

                if (options == null)
                {
                    return;
                }

                ExportResult result = ExportFiles(files, options);

                ShowResult(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Ошибка экспорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private ExportOptions ShowOptions(IReadOnlyList<LoodsmanFile> files)
        {
            List<KompasDocumentType> documentTypes = files
                .Select(file => ExportFormatService.GetDocumentType(file.ExtractedPath))
                .Where(type => type != KompasDocumentType.Unknown)
                .Distinct()
                .ToList();

            if (documentTypes.Count == 0)
            {
                MessageBox.Show(
                    "Не удалось определить типы файлов КОМПАС.",
                    "Экспорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return null;
            }

            var window = new ExportOptionsWindow(documentTypes);

            bool? result = window.ShowDialog();

            if (result != true)
                return null;

            return window.Options;
        }

        private List<int> GetSelectedIds()
        {
            object result = _npc.RunMethod("CGetTreeSelectedIDs");
            string selectedIdsText = result?.ToString();

            if (string.IsNullOrWhiteSpace(selectedIdsText))
            {
                return new List<int>();
            }

            return selectedIdsText
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Select(ParseId)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        private static int? ParseId(string value)
        {
            int id;

            if (int.TryParse(value, out id))
            {
                return id;
            }

            return null;
        }

        private ExportResult ExportFiles(IReadOnlyList<LoodsmanFile> files, ExportOptions options)
        {
            var result = new ExportResult { TotalCount = files.Count };

            foreach (LoodsmanFile file in files)
            {
                try
                {
                    string sourcePath = file.ExtractedPath;

                    KompasDocumentType documentType = ExportFormatService.GetDocumentType(
                        sourcePath
                    );

                    ExportFormat format = GetFormatForDocumentType(documentType, options);

                    string outputFileName =
                        Path.GetFileNameWithoutExtension(file.FileName)
                        + ExportFormatService.GetExtension(format);

                    string outputPath = Path.Combine(options.OutputDirectory, outputFileName);

                    _kompasService.ExportFile(sourcePath, outputPath, format);

                    if (!File.Exists(outputPath))
                    {
                        throw new InvalidOperationException(
                            $"КОМПАС сообщил об успешном экспорте, "
                                + $"но файл не найден:\n{outputPath}"
                        );
                    }

                    result.ExportedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"{file.FileName}: {ex.Message}");
                }
            }

            return result;
        }

        private static ExportFormat GetFormatForDocumentType(
            KompasDocumentType documentType,
            ExportOptions options
        )
        {
            switch (documentType)
            {
                case KompasDocumentType.Part3D:
                case KompasDocumentType.Assembly3D:
                    return options.Model3DFormat;

                case KompasDocumentType.Drawing2D:
                    return options.Drawing2DFormat;

                case KompasDocumentType.Fragment2D:
                    return options.Fragment2DFormat;

                case KompasDocumentType.Specification:
                    return options.SpecificationFormat;

                case KompasDocumentType.Text:
                    return options.TextFormat;

                default:
                    throw new NotSupportedException(
                        $"Тип документа {documentType} " + "не поддерживается."
                    );
            }
        }

        private static void ShowResult(ExportResult result)
        {
            string message =
                $"Всего файлов: {result.TotalCount}\n"
                + $"Экспортировано: {result.ExportedCount}\n"
                + $"Ошибок: {result.FailedCount}";

            if (result.Errors.Count > 0)
            {
                const int maxDisplayedErrors = 10;

                IEnumerable<string> displayedErrors = result.Errors.Take(maxDisplayedErrors);

                message += "\n\nОшибки:\n" + string.Join("\n", displayedErrors);

                if (result.Errors.Count > maxDisplayedErrors)
                {
                    message +=
                        $"\n\nИ ещё ошибок: " + $"{result.Errors.Count - maxDisplayedErrors}";
                }
            }

            MessageBox.Show(
                message,
                "Экспорт завершён",
                MessageBoxButton.OK,
                result.FailedCount == 0 ? MessageBoxImage.Information : MessageBoxImage.Warning
            );
        }

        private List<LoodsmanFile> GetFiles(IEnumerable<int> selectedIds)
        {
            var files = new List<LoodsmanFile>();

            foreach (int idVersion in selectedIds)
            {
                IReadOnlyList<LoodsmanFile> extractedFiles = _loodsmanService.ExtractKompasFiles(
                    idVersion
                );

                if (extractedFiles != null)
                {
                    files.AddRange(extractedFiles);
                }
            }

            return files
                .Where(file => file != null)
                .GroupBy(file => new { file.IdVersion, FileName = file.FileName?.Trim() })
                .Select(group => group.First())
                .ToList();
        }
    }
}
