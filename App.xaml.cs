using System.Windows;

namespace SmartMiniStore
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global unhandled-exception handler so the app never dies silently.
            DispatcherUnhandledException += (_, args) =>
            {
                MessageBox.Show(
                    "An unexpected error occurred:\n" + args.Exception.Message,
                    "Smart Mini Store",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}