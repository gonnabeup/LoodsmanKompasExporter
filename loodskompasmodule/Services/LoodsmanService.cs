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
using System.Data;
using System.IO;
using Ascon.Plm.Loodsman.PluginSDK;
using LoodsmanKompasExporter.Models;

namespace LoodsmanKompasExporter.Services
{
    public class LoodsmanService
    {
        private const string AssemblyLinkTypes =
            "Документы\x01Состоит из ...\x01Изготавливается из ...";

        private readonly INetPluginCall _npc;

        public LoodsmanService(INetPluginCall npc)
        {
            _npc = npc ?? throw new ArgumentNullException(nameof(npc));
        }

        public IReadOnlyList<LoodsmanFile> ExtractKompasFiles(int rootIdVersion)
        {
            if (rootIdVersion <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rootIdVersion),
                    "Некорректный идентификатор версии."
                );
            }

            var files = new List<LoodsmanFile>();
            var visitedVersions = new HashSet<int>();
            var extractedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ExtractFilesRecursive(rootIdVersion, visitedVersions, extractedFiles, files);

            return files;
        }

        private void ExtractFilesRecursive(
            int idVersion,
            HashSet<int> visitedVersions,
            HashSet<string> extractedFiles,
            ICollection<LoodsmanFile> result
        )
        {
            if (!visitedVersions.Add(idVersion))
                return;

            ExtractVersionFiles(idVersion, extractedFiles, result);

            DataTable children = _npc.GetDataTable(
                "GetTree",
                new object[] { "", "", "", idVersion, AssemblyLinkTypes, false }
            );

            if (children == null || children.Rows.Count == 0)
                return;

            foreach (DataRow row in children.Rows)
            {
                if (row["_ID_VERSION"] == DBNull.Value)
                    continue;

                int childIdVersion = Convert.ToInt32(row["_ID_VERSION"]);

                ExtractFilesRecursive(childIdVersion, visitedVersions, extractedFiles, result);
            }
        }

        private void ExtractVersionFiles(
            int idVersion,
            HashSet<string> extractedFiles,
            ICollection<LoodsmanFile> result
        )
        {
            DataTable files = _npc.GetDataTable(
                "GetInfoAboutVersion",
                new object[] { "", "", "", idVersion, 7 }
            );

            if (files == null || files.Rows.Count == 0)
                return;
            string product = GetProduct(idVersion);
            foreach (DataRow row in files.Rows)
            {

                string fileName = row["_NAME"]?.ToString()?.Trim() ?? "";
                
                string localName = row["_LOCALNAME"]?.ToString()?.Trim() ?? "";

                if (!IsKompasFile(fileName))
                    continue;

                string fileKey = idVersion + "|" + fileName;

                if (!extractedFiles.Add(fileKey))
                    continue;

                object extractResult = _npc.RunMethod(
                    "ExtractFile",
                    new object[] { "", "", "", idVersion, fileName, localName, 0 }
                );

                string extractedPath = extractResult?.ToString()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(extractedPath))
                    continue;

                result.Add(
                    new LoodsmanFile
                    {
                        IdVersion = idVersion,
                        FileName = fileName,
                        Product = product,
                        LocalName = localName,
                        ExtractedPath = extractedPath,
                    }
                );
            }
        }

        private static bool IsKompasFile(string fileName)
        {
            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return extension == ".m3d" || extension == ".a3d" || extension == ".cdw" || extension == ".spw" || extension == ".frw" || extension == ".kdw";
        }
        private string GetProduct(int idVersion)
        {
            DataTable versionInfo = _npc.GetDataTable(
                "GetInfoAboutVersion",
                new object[]
                {
            "",
            "",
            "",
            idVersion,
            2
                });

            if (versionInfo == null || versionInfo.Rows.Count == 0)
                return string.Empty;

            DataRow row = versionInfo.Rows[0];

            if (!versionInfo.Columns.Contains("_PRODUCT"))
                return string.Empty;

            return row["_PRODUCT"]?.ToString()?.Trim() ?? string.Empty;
        }
    }
}
