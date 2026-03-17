using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DashLook.Plugin.ImageViewer;

public partial class ImageViewerControl : UserControl
{
    private double _zoom = 1.0;
    private const double ZoomStep = 0.15;
    private const double MinZoom  = 0.1;
    private const double MaxZoom  = 10.0;

    public ImageViewerControl(BitmapSource bitmap)
    {
        InitializeComponent();
        MainImage.Source = bitmap;
    }

    private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        double delta = e.Delta > 0 ? ZoomStep : -ZoomStep;
        SetZoom(_zoom + delta);
        e.Handled = true;
    }

    private void SetZoom(double zoom)
    {
        _zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
        ZoomTransform.ScaleX = _zoom;
        ZoomTransform.ScaleY = _zoom;
        ZoomText.Text = $"{_zoom:P0}";
        ShowZoomIndicator();
    }

    private void ShowZoomIndicator()
    {
        ZoomIndicator.Opacity = 1;
        var fade = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(1.5)));
        fade.BeginTime = TimeSpan.FromSeconds(0.8);
        ZoomIndicator.BeginAnimation(OpacityProperty, fade);
    }
}
