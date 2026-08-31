using System.Text.Json;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace DocToPdf
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 1 || string.IsNullOrEmpty(args[0]) || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: MergeDocxToPdf <path_to_config.json>. Example: MergeDocxToPdf C:\\config.json");
                return;
            }

            var adjustedPath = Path.GetFullPath(args[0]);

            var config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(adjustedPath));

            if (config == null)
            {
                Console.WriteLine("Failed to read configuration from config.json.");
                return;
            }

            if (string.IsNullOrWhiteSpace(config.InputFolder) || string.IsNullOrWhiteSpace(config.OutputFolder))
            {
                Console.WriteLine("Both inputFolder and outputFolder must be specified in the config.json.");
                return;
            }

            if (!Directory.Exists(config.InputFolder))
            {
                Console.WriteLine($"The specified input folder does not exist: {config.InputFolder}");
                return;
            }

            if (!Directory.Exists(config.OutputFolder))
            {
                Directory.CreateDirectory(config.OutputFolder);
            }

            List<string> docxFiles = Directory.GetFiles(config.InputFolder, "*.docx").ToList();
            docxFiles.AddRange(Directory.GetFiles(config.InputFolder, "*.doc").ToList());

            // Sort files alphabetically so they merge in a predictable order
            docxFiles.Sort();

            if (docxFiles.Count == 0)
            {
                Console.WriteLine("No DOCX files found in the specified folder.");
                return;
            }

            string outputFilePath = Path.Combine(config.OutputFolder, "MergedDocument.pdf");

            string tempFolder = Path.Combine(config.OutputFolder, "TempPdfs_" + Guid.NewGuid().ToString().Substring(0, 8));
            Directory.CreateDirectory(tempFolder);

            try
            {
                Console.WriteLine($"Found {docxFiles.Count} files. Converting to PDF via LibreOffice...");

                foreach (string docx in docxFiles)
                {
                    Console.WriteLine($"Converting: {Path.GetFileName(docx)}");
                    await Converter.ConvertDocxToPdf(docx, tempFolder);
                }

                Console.WriteLine("Merging PDFs...");

                string[] tempPdfFiles = Directory.GetFiles(tempFolder, "*.pdf");

                Array.Sort(tempPdfFiles);

                using (PdfDocument outPdf = new PdfDocument())
                {
                    foreach (string pdfFile in tempPdfFiles)
                    {
                        using (PdfDocument inputPdf = PdfReader.Open(pdfFile, PdfDocumentOpenMode.Import))
                        {
                            for (int i = 0; i < inputPdf.PageCount; i++)
                            {
                                outPdf.AddPage(inputPdf.Pages[i]);
                            }
                        }
                    }
                    outPdf.Save(outputFilePath);
                }

                Console.WriteLine($"Successfully merged files into a single PDF at: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }

    }
}