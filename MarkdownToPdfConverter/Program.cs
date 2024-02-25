using MarkdownSharp;
using PdfSharp.Pdf;

namespace MarkdownToPdfConverter;

class Program
{
    static void Main(string[] args)
    {
        // Check if correct number of arguments provided
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: MarkdownToPdfConverter.exe <input.md> <output.pdf>");
            return;
        }

        string inputMdFile = args[0];
        string outputPdfFile = args[1];

        // Check if input file exists
        if (!File.Exists(inputMdFile))
        {
            Console.WriteLine("Input file not found.");
            return;
        }

        var extension = Path.GetExtension(inputMdFile);

        if (extension != ".md")
        {
            Console.WriteLine("Input file is not of type markdown.");
            return;
        }

        string markdownContent = File.ReadAllText(inputMdFile);

        // Convert markdown to HTML
        Markdown markdown = new Markdown();
        string htmlContent = markdown.Transform(markdownContent);

        // Convert HTML to PDF
        PdfDocument pdf = PdfGenerator.GeneratePdf(htmlContent);

        pdf.Save(outputPdfFile);


        // Check if input file exists
        if (!File.Exists(outputPdfFile))
        {
            Console.WriteLine("Error when saving output file.");
            return;
        }

        Console.WriteLine("Conversion completed successfully!");
    }
}