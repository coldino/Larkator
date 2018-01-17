using Avalon.Windows.Dialogs;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LarkatorGUI
{
    /// <summary>
    /// Interaction logic for FileEntryBox.xaml
    /// </summary>
    public partial class DirectoryEntryBox : UserControl
    {
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        public static readonly DependencyProperty TooltipProperty =
            DependencyProperty.Register("Tooltip", typeof(string), typeof(DirectoryEntryBox), new PropertyMetadata("Enter path to the directory"));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DirectoryEntryBox), new PropertyMetadata("Select directory"));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(DirectoryEntryBox), new PropertyMetadata(""));

        public DirectoryEntryBox()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog()
            {
                RootType = RootType.Path,
                ValidateResult = true,
                Title = Title,
                SelectedPath = Value,
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                Value = dialog.SelectedPath;
            }
        }
    }
}
