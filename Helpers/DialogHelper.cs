using System.Windows;

namespace SmartMiniStore.Helpers
{
    /// <summary>
    /// One place for all MessageBox calls.
    /// Keeps ViewModels tidy and lets us change the title consistently.
    /// </summary>
    public static class DialogHelper
    {
        private const string AppTitle = "Smart Mini Store";

        public static void Info(string message)
        {
            MessageBox.Show(message, AppTitle,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Warn(string message)
        {
            MessageBox.Show(message, AppTitle,
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Error(string message)
        {
            MessageBox.Show(message, AppTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool Confirm(string message)
        {
            var result = MessageBox.Show(message, AppTitle,
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}