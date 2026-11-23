using System.Windows;
using System.Windows.Controls;

namespace PrismCrosshair.Views;

public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void Editor_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainWindow shell)
            shell.NavigateTo("editor");
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainWindow shell)
            shell.NavigateTo("settings");
    }
}
