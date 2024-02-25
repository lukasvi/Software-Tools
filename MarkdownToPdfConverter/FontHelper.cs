using System.Reflection;

namespace MarkdownToPdfConverter;

/// <summary>
/// Helper class that reads font data from embedded resources.
/// </summary>
public static class FontHelper
{
    public static byte[] Arial
    {
        get { return LoadFontData("MarkdownToPdfConverter.resources.fonts.arial.Arial.ttf"); }
    }

    public static byte[] ArialBold
    {
        get { return LoadFontData("MarkdownToPdfConverter.resources.fonts.arial.Arial_Bold.ttf"); }
    }

    public static byte[] ArialItalic
    {
        get { return LoadFontData("MarkdownToPdfConverter.resources.fonts.arial.Arial_Bold_Italic.ttf"); }
    }

    public static byte[] ArialBoldItalic
    {
        get { return LoadFontData("MarkdownToPdfConverter.resources.fonts.arial.Arial_Italic.ttf"); }
    }

    /// <summary>
    /// Returns the specified font from an embedded resource.
    /// </summary>
    static byte[] LoadFontData(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Test code to find the names of embedded fonts
        //var ourResources = assembly.GetManifestResourceNames();

        using (Stream stream = assembly.GetManifestResourceStream(name))
        {
            if (stream == null)
                throw new ArgumentException("No resource with name " + name);

            int count = (int)stream.Length;
            byte[] data = new byte[count];
            stream.Read(data, 0, count);
            return data;
        }
    }
}
