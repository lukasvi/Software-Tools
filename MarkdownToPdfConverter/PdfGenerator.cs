using PdfSharp.Drawing.Layout;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MarkdownToPdfConverter;

internal static class PdfGenerator
{
    private static int _verticalAlignment = 30;
    private static int _horizontalAlignment = 30;

    private static int _verticalSpacingAfterH1 = 30;
    private static int _verticalSpacingAfterH2 = 30;
    private static int _verticalSpacingAfterText = 30;

    private static XFont _fontText = new XFont("Arial", 12);
    private static XFont _fontH1 = new XFont("Arial", 20, XFontStyleEx.Bold);
    private static XFont _fontH2 = new XFont("Arial", 16, XFontStyleEx.Bold);

    internal static PdfDocument GeneratePdf(string htmlContent)
    {
        CustomFontResolver.Apply();

        // Initialize PDF document
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();

        // Create a graphics object for the page
        XGraphics gfx = XGraphics.FromPdfPage(page);

        // Draw the HTML content onto the PDF page
        XRect layoutRect = new XRect(0, 0, page.Width, page.Height);

        XTextFormatter textFormatter = new XTextFormatter(gfx);

        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        foreach (HtmlNode node in htmlDoc.DocumentNode.ChildNodes)
        {
            HandleNode(gfx, textFormatter, node, layoutRect, page);
        }

        return document;
    }

    static void HandleNode(XGraphics gfx,XTextFormatter formatter, HtmlNode node, XRect rect, PdfPage page)
    {
        switch (node.NodeType)
        {
            case HtmlNodeType.Element:
                switch (node.Name.ToLower())
                {
                    case "h1":
                        formatter.DrawString(node.InnerText, _fontH1, XBrushes.Black, new XRect(_horizontalAlignment, _verticalAlignment, page.Width - (2 * _horizontalAlignment), page.Height), XStringFormats.TopLeft);
                        _verticalAlignment += _verticalSpacingAfterH1;
                        break;
                    case "h2":
                        formatter.DrawString(node.InnerText, _fontH2, XBrushes.Black, new XRect(_horizontalAlignment, _verticalAlignment, page.Width - (2 * _horizontalAlignment), page.Height), XStringFormats.TopLeft);
                        _verticalAlignment += _verticalSpacingAfterH2;
                        break;
                    case "p":
                        formatter.DrawString(node.InnerText, _fontText, XBrushes.Black, new XRect(_horizontalAlignment, _verticalAlignment, page.Width - (2 * _horizontalAlignment), page.Height), XStringFormats.TopLeft);

                        var paragraphHeight = CalculateRequiredHeightOfTextBlock(gfx, node.InnerText, _fontText, page.Width - (2 * _horizontalAlignment));
                        _verticalAlignment += (int)paragraphHeight + _verticalSpacingAfterText;
                        break;

                    case "ul":
                        var verticalAlignmentAfterList = DrawList(gfx, node.InnerHtml, _verticalAlignment, _fontText, formatter, BulletPointType.Dot);
                        _verticalAlignment = (int)verticalAlignmentAfterList + _verticalSpacingAfterText;
                        break;

                    case "ol":
                        verticalAlignmentAfterList = DrawList(gfx, node.InnerHtml, _verticalAlignment, _fontText, formatter, BulletPointType.Numbered);
                        _verticalAlignment = (int)verticalAlignmentAfterList + _verticalSpacingAfterText;
                        break;

                    default:
                        foreach (HtmlNode childNode in node.ChildNodes)
                        {
                            HandleNode(gfx, formatter, childNode, rect, page);
                        }
                        break;
                }
                break;
            case HtmlNodeType.Text:
                // Handle text nodes if necessary
                break;
                // Add more cases as needed for other HTML node types
        }
    }

    static double DrawList(XGraphics gfx, string listHtml, double startY, XFont font, XTextFormatter formatter, BulletPointType bulletPointType)
    {
        double currentY = startY;
        string[] items = listHtml.Split(new[] { "<li>" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < items.Count(); i++)
        {
            if (items[i].StartsWith("</ul>") || items[i].StartsWith("</ol>"))
            {
                continue;
            }

            // Strip any remaining HTML tags
            string listItem = Regex.Replace(items[i], @"<[^>]+>|&nbsp;", "").Trim();

            if (string.IsNullOrWhiteSpace(listItem) || string.IsNullOrEmpty(listItem))
            {
                continue;
            }

            var symbol = "x ";

            switch (bulletPointType)
            {
                case BulletPointType.Dot:
                    symbol = "• ";
                    break;
                case BulletPointType.Dash:
                    symbol = "- ";
                    break;
                case BulletPointType.Numbered:
                    symbol = $"{i}. ";
                    break;
                default:
                    throw new Exception("Unknown bullet point type");
            }

            // Draw bullet or number
            formatter.DrawString(symbol, font, XBrushes.Black, new XRect(_horizontalAlignment, currentY, 20, gfx.MeasureString(listItem, font).Height), XStringFormats.TopLeft);

            // Draw list item
            formatter.DrawString(listItem, font, XBrushes.Black, new XRect(_horizontalAlignment + 15, currentY, gfx.PageSize.Width - (2 * _horizontalAlignment), gfx.MeasureString(listItem, font).Height), XStringFormats.TopLeft);

            // Update Y position for the next list item
            currentY += CalculateRequiredHeightOfTextBlock(gfx, listItem, _fontText, gfx.PageSize.Width - (2 * _horizontalAlignment));
        }

        return currentY;
    }

    private static double CalculateRequiredHeightOfTextBlock(XGraphics gfx, string text, XFont font, double width)
    {
        XSize size = gfx.MeasureString(text, font);
        double lines = Math.Ceiling(size.Width / width);
        double lineHeight = size.Height;

        Console.WriteLine($"lines: {lines}");
        Console.WriteLine(text);

        return lineHeight * lines;
    }
}
