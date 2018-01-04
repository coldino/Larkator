using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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

        private void BrowseArkTools_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "ark-tools.exe",
                DereferenceLinks = true,
                Filter = "ARK Tools Executable|ark-tools.exe",
                Multiselect = false,
                Title = "Locate ark-tools.exe...",
                FileName = ArkToolsPath,
            };

            var result = dialog.ShowDialog();
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
                DefaultExt = ".ark",
                DereferenceLinks = true,
                Filter = "ARK Save File|*.ark",
                Multiselect = false,
                Title = "Locate saved ARK...",
                FileName = SaveFilePath,
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                SaveFilePath = dialog.FileName;
            }
        }

        private void LetsGoButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Let's go!");

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
