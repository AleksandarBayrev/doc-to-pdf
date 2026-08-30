using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace DocToPdf
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MergeDocxToPdf <input_folder> <output_folder>");
                return;
            }

            string inputFolder = Path.GetFullPath(args[0]);
            string outputFolder = Path.GetFullPath(args[1]);

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine($"The specified input folder does not exist: {inputFolder}");
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            string[] docxFiles = Directory.GetFiles(inputFolder, "*.docx");

            // Sort files alphabetically so they merge in a predictable order
            Array.Sort(docxFiles);

            if (docxFiles.Length == 0)
            {
                Console.WriteLine("No DOCX files found in the specified folder.");
                return;
            }

            string outputFilePath = Path.Combine(outputFolder, "MergedDocument.pdf");

            string tempFolder = Path.Combine(outputFolder, "TempPdfs_" + Guid.NewGuid().ToString().Substring(0, 8));
            Directory.CreateDirectory(tempFolder);

            try
            {
                Console.WriteLine($"Found {docxFiles.Length} files. Converting to PDF via LibreOffice...");

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