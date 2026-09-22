using LoodsmanKompasExporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LoodsmanKompasExporter.Views
{
    /// <summary>
    /// Логика взаимодействия для EasterEgg.xaml
    /// </summary>
    public partial class EasterEgg : Window
    {

        private readonly HashSet<Key> _pressedKeys = new HashSet<Key>();
        IEnumerable<KompasDocumentType> _types;
        public EasterEgg(IEnumerable<KompasDocumentType> documentTypes)
        {
            InitializeComponent();
            _types = documentTypes;
            PreviewKeyDown += Page_PreviewKeyDown;
            PreviewKeyUp += Page_PreviewKeyUp;
            Loaded += (_, __) => Keyboard.Focus(this);
        }
        private bool IsKeyComboPressed()
        {
            return _pressedKeys.Contains(Key.A)
                && _pressedKeys.Contains(Key.D)
                && _pressedKeys.Contains(Key.M)
                && _pressedKeys.Contains(Key.I)
                && _pressedKeys.Contains(Key.N);
        }
        // обработчик события нажатия на клавишу
        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _pressedKeys.Add(e.Key); // добавляем клавишу в список нажатых

            if (IsKeyComboPressed())
            {
                ExportOptionsWindow w = new ExportOptionsWindow(_types);
                w.Show();
                this.Close();
                e.Handled = true;
            }
        }

        // обработчик события отжатия клавишы
        private void Page_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys.Remove(e.Key);
        }
    }
}
