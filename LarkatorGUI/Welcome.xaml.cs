using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace LarkatorGUI
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        public string ArkToolsPath
        {
            get { return (string)GetValue(ArkToolsPathProperty); }
            set { SetValue(ArkToolsPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ArkToolsPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ArkToolsPathProperty =
            DependencyProperty.Register("ArkToolsPath", typeof(string), typeof(Welcome), new PropertyMetadata(""));

        public string SaveFilePath
        {
            get { return (string)GetValue(SaveFilePathProperty); }
            set { SetValue(SaveFilePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SaveFilePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveFilePathProperty =
            DependencyProperty.Register("SaveFilePath", typeof(string), typeof(Welcome), new PropertyMetadata(""));

        public Welcome()
        {
            InitializeComponent();

            DependencyPropertyDescriptor.FromProperty(ArkToolsPathProperty, GetType()).AddValueChanged(this, (_, __) => UpdateValidation());
            DependencyPropertyDescriptor.FromProperty(SaveFilePathProperty, GetType()).AddValueChanged(this, (_, __) => UpdateValidation());

            ArkToolsPath = Properties.Settings.Default.ArkTools;
            SaveFilePath = Properties.Settings.Default.SaveFile;

            // Skip the Welcome window if we're already configured
            UpdateValidation();
            if (LetsGoButton.IsEnabled) SwitchToMainWindow();
        }

        private void UpdateValidation()
        {
            var toolsGood = File.Exists(ArkToolsPath);
            var saveGood = File.Exists(SaveFilePath) && SaveFilePath.EndsWith(".ark");

            LetsGoButton.IsEnabled = toolsGood && saveGood;
        }

        private string CheckFile(string fullpath)
        {
            if (String.IsNullOrWhiteSpace(fullpath) || !File.Exists(fullpath))
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                return Path.GetDirectoryName(fullpath);
        }

        private void BrowseArkTools_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = false,
                Multiselect = false,
                DefaultExt = "ark-tools.exe",
                Filter = "ARK Tools Executable|ark-tools.exe",
                Title = "Locate ark-tools.exe...",
                FileName = ArkToolsPath,
                InitialDirectory = CheckFile(ArkToolsPath),
            };

            var result = dialog.ShowDialog(this);
            if (result == true)
            {
                ArkToolsPath = dialog.FileName;
            }
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
            Properties.Settings.Default.ArkTools = ArkToolsPath;
            Properties.Settings.Default.SaveFile = SaveFilePath;

            Properties.Settings.Default.Save();

            SwitchToMainWindow();
        }

        private void SwitchToMainWindow()
        {
            new MainWindow().Show();

            Close();
        }

        private void LaunchHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start((sender as Hyperlink).NavigateUri.ToString());
        }
    }
}
