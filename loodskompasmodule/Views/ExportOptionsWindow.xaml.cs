//                               .-') _      .-') _    ('-.    .-. .-')    ('-.                 _ (`-.  
//                              ( OO ) )    ( OO ) )  ( OO ).-.\  ( OO ) _(  OO)               ( (OO  ) 
//   ,----.     .-'),-----. ,--./ ,--,' ,--./ ,--,'   / . --. / ;-----.\(,------. ,--. ,--.   _.`     \ 
//  '  .-./-') ( OO'  .-.  '|   \ |  |\ |   \ |  |\   | \-.  \  | .-.  | |  .---' |  | |  |  (__...--'' 
//  |  |_( O- )/   |  | |  ||    \|  | )|    \|  | ).-'-'  |  | | '-' /_)|  |     |  | | .-') |  /  | | 
//  |  | .--, \\_) |  |\|  ||  .     |/ |  .     |/  \| |_.'  | | .-. `.(|  '--.  |  |_|( OO )|  |_.' | 
// (|  | '. (_/  \ |  | |  ||  |\    |  |  |\    |    |  .-.  | | |  \  ||  .--'  |  | | `-' /|  .___.' 
//  |  '--'  |    `'  '-'  '|  | \   |  |  | \   |    |  | |  | | '--'  /|  `---.('  '-'(_.-' |  |      
//   `------'       `-----' `--'  `--'  `--'  `--'    `--' `--' `------' `------'  `-----'    `--'     

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using LoodsmanKompasExporter.Models;
using LoodsmanKompasExporter.Services;
using Forms = System.Windows.Forms;

namespace LoodsmanKompasExporter.Views
{
    public partial class ExportOptionsWindow : Window
    {
        public ExportOptions Options { get; private set; }

        public ExportOptionsWindow(IEnumerable<KompasDocumentType> documentTypes)
        {
            InitializeComponent();

            var types = new HashSet<KompasDocumentType>(documentTypes);

            InitializeFormatRows(types);
        }

        private static List<FormatItem> CreateFormatItems(IEnumerable<ExportFormat> formats)
        {
            var result = new List<FormatItem>();

            foreach (ExportFormat format in formats)
            {
                result.Add(new FormatItem { Format = format, Name = GetFormatName(format) });
            }

            return result;
        }

        private static string GetFormatName(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Step:
                    return "STEP (*.step)";

                case ExportFormat.Iges:
                    return "IGES (*.igs)";

                case ExportFormat.Stl:
                    return "STL (*.stl)";

                case ExportFormat.Sat:
                    return "SAT (*.sat)";

                case ExportFormat.Xt:
                    return "Parasolid (*.x_t)";

                case ExportFormat.Vrml:
                    return "VRML (*.wrl)";

                case ExportFormat.Pdf:
                    return "PDF (*.pdf)";

                case ExportFormat.Dxf:
                    return "DXF (*.dxf)";

                case ExportFormat.Dwg:
                    return "DWG (*.dwg)";

                case ExportFormat.Bmp:
                    return "BMP (*.bmp)";

                case ExportFormat.Tiff:
                    return "TIFF (*.tiff)";

                case ExportFormat.Jpeg:
                    return "JPEG (*.jpeg)";

                case ExportFormat.Png:
                    return "PNG (*.png)";

                case ExportFormat.Tga:
                    return "TGA (*.tga)";

                case ExportFormat.Emf:
                    return "EMF (*.emf)";

                case ExportFormat.Gif:
                    return "Gif (*.gif)";

                case ExportFormat.Xls:
                    return "XLS (*.xls)";

                case ExportFormat.Xlsx:
                    return "XLSX (*.xlsx)";

                case ExportFormat.Ods:
                    return "ODS (*.ods)";

                case ExportFormat.Txt:
                    return "TXT (*.txt)";

                default:
                    return format.ToString();
            }
        }

        private void InitializeFormatRows(HashSet<KompasDocumentType> documentTypes)
        {
            bool has3D =
                documentTypes.Contains(KompasDocumentType.Part3D)
                || documentTypes.Contains(KompasDocumentType.Assembly3D);

            Models3DRow.Visibility = has3D ? Visibility.Visible : Visibility.Collapsed;

            DrawingsRow.Visibility = documentTypes.Contains(KompasDocumentType.Drawing2D)
                ? Visibility.Visible
                : Visibility.Collapsed;

            FragmentsRow.Visibility = documentTypes.Contains(KompasDocumentType.Fragment2D)
                ? Visibility.Visible
                : Visibility.Collapsed;

            SpecificationsRow.Visibility = documentTypes.Contains(KompasDocumentType.Specification)
                ? Visibility.Visible
                : Visibility.Collapsed;

            TextRow.Visibility = documentTypes.Contains(KompasDocumentType.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (has3D)
            {
                Models3DFormatComboBox.ItemsSource = CreateFormatItems(
                    new[]
                    {
                        ExportFormat.Step,
                        ExportFormat.Iges,
                        ExportFormat.Stl,
                        ExportFormat.Sat,
                        ExportFormat.Xt,
                        ExportFormat.Vrml,
                    }
                );

                Models3DFormatComboBox.SelectedIndex = 0;
            }

            if (DrawingsRow.Visibility == Visibility.Visible)
            {
                DrawingFormatComboBox.ItemsSource = CreateFormatItems(
                    ExportFormatService.GetSupportedFormats(KompasDocumentType.Drawing2D)
                );

                DrawingFormatComboBox.SelectedIndex = 0;
            }

            if (FragmentsRow.Visibility == Visibility.Visible)
            {
                FragmentFormatComboBox.ItemsSource = CreateFormatItems(
                    ExportFormatService.GetSupportedFormats(KompasDocumentType.Fragment2D)
                );

                FragmentFormatComboBox.SelectedIndex = 0;
            }

            if (SpecificationsRow.Visibility == Visibility.Visible)
            {
                SpecificationFormatComboBox.ItemsSource = CreateFormatItems(
                    ExportFormatService.GetSupportedFormats(KompasDocumentType.Specification)
                );

                SpecificationFormatComboBox.SelectedIndex = 0;
            }

            if (TextRow.Visibility == Visibility.Visible)
            {
                TextFormatComboBox.ItemsSource = CreateFormatItems(
                    ExportFormatService.GetSupportedFormats(KompasDocumentType.Text)
                );

                TextFormatComboBox.SelectedIndex = 0;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку для экспортированных файлов";

                dialog.ShowNewFolderButton = true;

                if (Directory.Exists(DirectoryTextBox.Text))
                {
                    dialog.SelectedPath = DirectoryTextBox.Text;
                }

                Forms.DialogResult result = dialog.ShowDialog();

                if (result == Forms.DialogResult.OK)
                {
                    DirectoryTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            string directory = DirectoryTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(directory))
            {
                MessageBox.Show(
                    "Выберите папку назначения.",
                    "Экспорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            if (!Directory.Exists(directory))
            {
                MessageBox.Show(
                    "Выбранная папка не существует.",
                    "Экспорт",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            Options = new ExportOptions
            {
                OutputDirectory = directory,

                Model3DFormat = GetSelectedFormat(Models3DFormatComboBox, ExportFormat.Step),

                Drawing2DFormat = GetSelectedFormat(DrawingFormatComboBox, ExportFormat.Pdf),

                Fragment2DFormat = GetSelectedFormat(FragmentFormatComboBox, ExportFormat.Pdf),

                SpecificationFormat = GetSelectedFormat(SpecificationFormatComboBox, ExportFormat.Pdf),

                TextFormat = GetSelectedFormat(TextFormatComboBox, ExportFormat.Pdf),
            };

            DialogResult = true;
        }

        private static ExportFormat GetSelectedFormat(
            System.Windows.Controls.ComboBox comboBox,
            ExportFormat fallback
        )
        {
            var item = comboBox.SelectedItem as FormatItem;

            return item != null ? item.Format : fallback;
        }

        private class FormatItem
        {
            public ExportFormat Format { get; set; }

            public string Name { get; set; }
        }
    }
}
