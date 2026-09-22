using System;
using System.Collections.Generic;
using System.IO;
using Kompas6API5;
using Kompas6Constants3D;
using LoodsmanKompasExporter.Models;

namespace LoodsmanKompasExporter.Services
{
    public class KompasService
    {
        private KompasObject _kompas;

        public KompasObject StartKompas()
        {
            if (_kompas != null)
                return _kompas;

            Type kompasType =
                Type.GetTypeFromProgID("KOMPAS.Application.5")
                ?? throw new InvalidOperationException("COM-класс KOMPAS.Application.5 не найден.");

            _kompas =
                (KompasObject)Activator.CreateInstance(kompasType)
                ?? throw new InvalidOperationException("Не удалось создать объект КОМПАС.");

            _kompas.Visible = true;
            _kompas.ActivateControllerAPI();

            return _kompas;
        }

        public IReadOnlyList<ExportFileResult> ExportFiles(
            IEnumerable<string> sourceFiles,
            string outputFolder,
            ExportFormat format
        )
        {
            if (sourceFiles == null)
                throw new ArgumentNullException(nameof(sourceFiles));

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                throw new ArgumentException("Не указана папка для экспорта.", nameof(outputFolder));
            }

            Directory.CreateDirectory(outputFolder);

            var results = new List<ExportFileResult>();

            foreach (string sourcePath in sourceFiles)
            {
                string outputPath = null;

                try
                {
                    if (string.IsNullOrWhiteSpace(sourcePath))
                    {
                        throw new ArgumentException("Путь к исходному файлу пуст.");
                    }

                    string outputFileName =
                        Path.GetFileNameWithoutExtension(sourcePath)
                        + ExportFormatService.GetExtension(format);

                    outputPath = Path.Combine(outputFolder, outputFileName);

                    ExportFile(sourcePath, outputPath, format);

                    results.Add(
                        new ExportFileResult
                        {
                            SourcePath = sourcePath,
                            OutputPath = outputPath,
                            IsSuccess = true,
                        }
                    );
                }
                catch (Exception ex)
                {
                    results.Add(
                        new ExportFileResult
                        {
                            SourcePath = sourcePath,
                            OutputPath = outputPath,
                            IsSuccess = false,
                            ErrorMessage = ex.Message,
                        }
                    );
                }
            }

            return results;
        }

        public IReadOnlyList<ExportFileResult> ExportFiles(
            IEnumerable<LoodsmanFile> files,
            string outputFolder,
            ExportFormat format
        )
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                throw new ArgumentException("Не указана папка для экспорта.", nameof(outputFolder));
            }

            Directory.CreateDirectory(outputFolder);

            var results = new List<ExportFileResult>();

            foreach (LoodsmanFile file in files)
            {
                string outputPath = null;

                try
                {
                    if (file == null)
                        throw new ArgumentException("Получен пустой элемент файла.");

                    if (string.IsNullOrWhiteSpace(file.ExtractedPath))
                    {
                        throw new ArgumentException(
                            "У файла отсутствует извлечённый локальный путь."
                        );
                    }

                    string outputFileName =
                        Path.GetFileNameWithoutExtension(file.FileName)
                        + ExportFormatService.GetExtension(format);

                    outputPath = GetUniqueOutputPath(outputFolder, outputFileName);

                    ExportFile(file.ExtractedPath, outputPath, format);

                    results.Add(
                        new ExportFileResult
                        {
                            Product = file.Product,
                            SourcePath = file.ExtractedPath,
                            OutputPath = outputPath,
                            IsSuccess = true,
                        }
                    );
                }
                catch (Exception ex)
                {
                    results.Add(
                        new ExportFileResult
                        {
                            Product = file?.Product,
                            SourcePath = file?.ExtractedPath,
                            OutputPath = outputPath,
                            IsSuccess = false,
                            ErrorMessage = ex.Message,
                        }
                    );
                }
            }

            return results;
        }

        public void ExportFile(string sourcePath, string outputPath, ExportFormat format)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Не указан исходный файл.");

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Исходный файл не найден.", sourcePath);

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Не указан путь экспорта.");

            string directory = Path.GetDirectoryName(outputPath);

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException("Не удалось определить папку экспорта.");

            Directory.CreateDirectory(directory);

            KompasDocumentType documentType = ExportFormatService.GetDocumentType(sourcePath);

            if (!ExportFormatService.IsSupported(documentType, format))
            {
                throw new NotSupportedException(
                    $"Формат {format} не поддерживается для {documentType}."
                );
            }

            switch (documentType)
            {
                case KompasDocumentType.Part3D:
                case KompasDocumentType.Assembly3D:
                    Export3DFile(sourcePath, outputPath, format);
                    break;
                case KompasDocumentType.Drawing2D:
                case KompasDocumentType.Fragment2D:
                    Export2DFile(sourcePath, outputPath, format);
                    break;
                case KompasDocumentType.Specification:
                    ExportSpecificationFile(sourcePath, outputPath, format);
                    break;
                case KompasDocumentType.Text:
                    ExportTextFile(sourcePath, outputPath, format);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Тип документа {documentType} не поддерживается."
                    );
            }
        }

        private static string GetUniqueOutputPath(string outputFolder, string outputFileName)
        {
            string outputPath = Path.Combine(outputFolder, outputFileName);

            if (!File.Exists(outputPath))
                return outputPath;

            string name = Path.GetFileNameWithoutExtension(outputFileName);

            string extension = Path.GetExtension(outputFileName);

            int index = 2;

            while (true)
            {
                string candidate = Path.Combine(outputFolder, $"{name} ({index}){extension}");

                if (!File.Exists(candidate))
                    return candidate;

                index++;
            }
        }

        private void Export2DFile(string sourcePath, string outputPath, ExportFormat format)
        {
            string extension = ExportFormatService.GetExtension(format);

            outputPath = Path.ChangeExtension(outputPath, extension);

            string outputDirectory = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            KompasObject kompas = StartKompas();

            ksDocument2D document = null;

            try
            {
                document = (ksDocument2D)kompas.Document2D();

                if (document == null)
                {
                    throw new InvalidOperationException(
                        "КОМПАС не создал объект " + "2D-документа."
                    );
                }

                bool opened = document.ksOpenDocument(sourcePath, false);

                if (!opened)
                {
                    throw new InvalidOperationException(
                        $"Не удалось открыть 2D-файл:\n" + sourcePath
                    );
                }

                bool saved = document.ksSaveDocument(outputPath);

                if (!saved)
                {
                    throw new InvalidOperationException(
                        $"Не удалось экспортировать " + $"2D-документ в {format}:\n" + outputPath
                    );
                }

                if (!File.Exists(outputPath))
                {
                    throw new IOException(
                        $"КОМПАС сообщил об успешном "
                            + $"экспорте, но файл не найден:\n"
                            + outputPath
                    );
                }
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        document.ksCloseDocument();
                    }
                    catch { 
                        // Ошибка закрытия не должна перекрывать
                        // основную ошибку экспорта.
                    }
                }
            }
        }

        private void Export3DFile(string sourcePath, string outputPath, ExportFormat format)
        {
            string outputDirectory = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            KompasObject kompas = StartKompas();

            ksDocument3D document = null;

            try
            {
                document = (ksDocument3D)kompas.Document3D();

                if (document == null)
                {
                    throw new InvalidOperationException("КОМПАС не создал объект 3D-документа.");
                }

                bool opened = document.Open(sourcePath, false);

                if (!opened)
                {
                    throw new InvalidOperationException($"Не удалось открыть файл:\n{sourcePath}");
                }

                var parameters =
                    (ksAdditionFormatParam)document.AdditionFormatParam()
                    ?? throw new InvalidOperationException("КОМПАС не вернул параметры экспорта.");
                parameters.format = (short)GetKompas3DFormat(format);

                bool saved = document.SaveAsToAdditionFormat(outputPath, parameters);

                if (!saved)
                {
                    throw new InvalidOperationException(
                        $"Не удалось сохранить файл:\n{outputPath}"
                    );
                }

                if (!File.Exists(outputPath))
                {
                    throw new IOException(
                        $"КОМПАС сообщил об успешном экспорте, но файл не найден:\n{outputPath}"
                    );
                }
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        document.close();
                    }
                    catch
                    {
                        // Игнорируем пока
                        // Ошибка закрытия не должна перекрывать
                        // основную ошибку экспорта.
                    }
                }
            }
        }

        private void ExportSpecificationFile(
            string sourcePath,
            string outputPath,
            ExportFormat format
        )
        {
            outputPath = Path.ChangeExtension(outputPath, ExportFormatService.GetExtension(format));

            string outputDirectory = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            KompasObject kompas = StartKompas();

            ksSpcDocument document = null;

            try
            {
                document = (ksSpcDocument)kompas.SpcDocument();

                if (document == null)
                {
                    throw new InvalidOperationException("КОМПАС не создал объект спецификации.");
                }

                bool opened = document.ksOpenDocument(sourcePath, 0);

                if (!opened)
                {
                    throw new InvalidOperationException(
                        $"Не удалось открыть спецификацию:\n{sourcePath}"
                    );
                }

                bool saved = document.ksSaveDocument(outputPath);

                if (!saved)
                {
                    throw new InvalidOperationException(
                        $"Не удалось экспортировать спецификацию "
                            + $"в формат {format}:\n{outputPath}"
                    );
                }

                if (!File.Exists(outputPath))
                {
                    throw new IOException(
                        $"КОМПАС сообщил об успешном экспорте, "
                            + $"но файл не найден:\n{outputPath}"
                    );
                }
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        document.ksCloseDocument();
                    }
                    catch {
                        // Ошибка закрытия не должна перекрывать
                        // основную ошибку экспорта.
                    }
                }
            }
        }

        private void ExportTextFile(string sourcePath, string outputPath, ExportFormat format)
        {
            string extension = ExportFormatService.GetExtension(format);

            outputPath = Path.ChangeExtension(outputPath, extension);

            string outputDirectory = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            KompasObject kompas = StartKompas();

            ksDocumentTxt document = null;

            try
            {
                document = (ksDocumentTxt)kompas.DocumentTxt();

                if (document == null)
                {
                    throw new InvalidOperationException(
                        "КОМПАС не создал объект текстового документа."
                    );
                }

                bool opened = document.ksOpenDocument(sourcePath, 0);

                if (!opened)
                {
                    throw new InvalidOperationException(
                        $"Не удалось открыть текстовый документ:\n{sourcePath}"
                    );
                }

                bool saved = document.ksSaveDocument(outputPath);

                if (!saved)
                {
                    throw new InvalidOperationException(
                        $"Не удалось экспортировать текстовый документ "
                            + $"в формат {format}:\n{outputPath}"
                    );
                }

                if (!File.Exists(outputPath))
                {
                    throw new IOException(
                        $"КОМПАС сообщил об успешном экспорте, "
                            + $"но файл не найден:\n{outputPath}"
                    );
                }
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        document.ksCloseDocument();
                    }
                    catch
                    {
                        // Ошибка закрытия не должна перекрывать
                        // основную ошибку экспорта.
                    }
                }
            }
        }

        private static int GetKompas3DFormat(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Step:
                    return (int)D3FormatConvType.format_STEP;

                case ExportFormat.Iges:
                    return (int)D3FormatConvType.format_IGES;

                case ExportFormat.Stl:
                    return (int)D3FormatConvType.format_STL;

                case ExportFormat.Sat:
                    return (int)D3FormatConvType.format_SAT;

                case ExportFormat.Xt:
                    return (int)D3FormatConvType.format_XT;

                case ExportFormat.Vrml:
                    return (int)D3FormatConvType.format_VRML;

                default:
                    throw new NotSupportedException(
                        $"Формат {format} не поддерживается для 3D-документов."
                    );
            }
        }
    }
}
