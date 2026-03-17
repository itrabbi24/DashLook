using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace DashLook.Plugin.FontViewer;

public partial class FontViewerControl : UserControl
{
    private const string Pangram = "The quick brown fox jumps over the lazy dog";

    public FontViewerControl(string fontPath)
    {
        InitializeComponent();

        try
        {
            // Load the font from file using WPF's font URI scheme
            var fontUri = new Uri($"file:///{fontPath.Replace('\\', '/')}");
            var glyphTypeface = new System.Windows.Media.GlyphTypeface(fontUri);
            // FamilyNames is a dictionary of CultureInfo → name; pick English (1033) or first available
            string fontFamily = glyphTypeface.FamilyNames.TryGetValue(
                System.Globalization.CultureInfo.GetCultureInfo(1033), out var name)
                ? name
                : glyphTypeface.FamilyNames.Values.FirstOrDefault() ?? Path.GetFileNameWithoutExtension(fontPath);

            var family = new FontFamily($"file:///{Path.GetDirectoryName(fontPath)?.Replace('\\', '/')}/" +
                                        $"#{fontFamily}");

            FontNameText.Text   = fontFamily;
            FontDetailText.Text = Path.GetFileName(fontPath);

            ApplyFont(family);
        }
        catch
        {
            FontNameText.Text   = Path.GetFileNameWithoutExtension(fontPath);
            FontDetailText.Text = Path.GetFileName(fontPath);
            // Apply fallback but still show sample text
            foreach (var tb in new[] { Sample48, Sample32, Sample20, Sample14, AlphabetText, NumbersText })
                tb.Text = tb == AlphabetText || tb == NumbersText ? tb.Text : Pangram;
        }
    }

    private void ApplyFont(FontFamily family)
    {
        Sample48.Text = Pangram;
        Sample32.Text = Pangram;
        Sample20.Text = Pangram;
        Sample14.Text = Pangram;

        foreach (var tb in new[] { Sample48, Sample32, Sample20, Sample14, AlphabetText, NumbersText })
            tb.FontFamily = family;
    }
}
