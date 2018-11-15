using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace LarkatorGUI
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        public string SaveFilePath
        {
            get { return (string)GetValue(SaveFilePathProperty); }
            set { SetValue(SaveFilePathProperty, value); }
        }

        public static readonly DependencyProperty SaveFilePathProperty =
            DependencyProperty.Register("SaveFilePath", typeof(string), typeof(Welcome), new PropertyMetadata(""));

        public Welcome()
        {
            InitializeComponent();

            DependencyPropertyDescriptor.FromProperty(SaveFilePathProperty, GetType()).AddValueChanged(this, (_, __) => UpdateValidation());

            SaveFilePath = Properties.Settings.Default.SaveFile;

            // Skip the Welcome window if we're already configured
            UpdateValidation();
            if (LetsGoButton.IsEnabled) SwitchToMainWindow();
        }

        private void UpdateValidation()
        {
            var saveGood = File.Exists(SaveFilePath) && SaveFilePath.EndsWith(".ark");

            LetsGoButton.IsEnabled = saveGood;
        }

        private string CheckFile(string fullpath)
        {
            if (String.IsNullOrWhiteSpace(fullpath) || !File.Exists(fullpath))
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                return Path.GetDirectoryName(fullpath);
        }

        private void BrowseSaveFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = false,
                Multiselect = false,
                DefaultExt = ".ark",
                Filter = "ARK Save File|*.ark",
                Title = "Locate saved ARK...",
                FileName = SaveFilePath,
                InitialDirectory = CheckFile(SaveFilePath),
            };

            var result = dialog.ShowDialog(this);
            if (result == true)
            {
                SaveFilePath = dialog.FileName;
            }
        }

        private void LetsGoButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveFile = SaveFilePath;
            Properties.Settings.Default.Save();

            SwitchToMainWindow();
        }

        private void SwitchToMainWindow()
        {
            new MainWindow().Show();

            Close();
        }
    }
}
