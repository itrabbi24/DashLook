using System.Windows;
using System.Windows.Media.Animation;

namespace DashLook.Windows;

public partial class StartupBannerWindow : Window
{
    public StartupBannerWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        PositionWindow();
        FadeTo(1, 220);
    }

    public void SetReadyState()
    {
        TitleText.Text = "DashLook is ready";
        SubtitleText.Text = "Developed by ARG RABBI  |  ROTEXIT.COM";
        StateText.Text = "Ready";
    }

    public async Task DismissAsync(int delayMs = 700)
    {
        if (delayMs > 0)
            await Task.Delay(delayMs);

        FadeTo(0, 220);
        await Task.Delay(240);
        Close();
    }

    private void PositionWindow()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 22;
        Top = workArea.Bottom - ActualHeight - 22;
    }

    private void FadeTo(double targetOpacity, int durationMs)
    {
        var animation = new DoubleAnimation(targetOpacity, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new QuadraticEase()
        };
        BeginAnimation(OpacityProperty, animation);
    }
}

