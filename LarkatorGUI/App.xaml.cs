using LarkatorGUI.Properties;

using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;

namespace LarkatorGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                Console.WriteLine(config.FilePath);
            }
            catch (ConfigurationErrorsException ex)
            {
                MessageBox.Show("Larkator's settings file is corrupted and cannot be loaded.\n" +
                    "\nAttempting to reset settings to defaults... :(\n" +
                    "\nApp will now exit - please restart it manually!",
                    "Settings Corruption", MessageBoxButton.OK, MessageBoxImage.Error);

                if (ex.Filename != null)
                {
                    System.IO.File.Delete(ex.Filename);
                }
                else
                {
                    Settings.Default.Reset();
                    Settings.Default.Save();
                }

                Environment.Exit(101);
            }

            base.OnStartup(e);

#if !DEBUG
            RegisterExceptionHandlers();
#endif
        }

        private void RegisterExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            LarkatorGUI.Properties.Settings.Default.Save();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnhandledException(e.ExceptionObject as Exception);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleUnhandledException(e.Exception);
        }

        private void HandleUnhandledException(Exception ex)
        {
            if (ex == null)
            {
                MessageBox.Show("Null Exception!");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Sorry, something bad happened and the application must exit.");
            sb.AppendLine();
            sb.AppendLine("If you wish to report this issue please press Ctrl-C now to copy the details and");
            sb.AppendLine("paste it into a new issue along with a description of what was happening");
            sb.AppendLine("at: https://github.com/coldino/Larkator/issues");
            sb.AppendLine("Thanks!");
            sb.AppendLine();
            sb.AppendLine("Exception:");
            sb.AppendLine(ex.ToString());

            MessageBox.Show(sb.ToString(), "Whoops...", MessageBoxButton.OK, MessageBoxImage.Error);

            Environment.Exit(1);
        }
    }
}
