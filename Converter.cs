using System.Diagnostics;

namespace DocToPdf
{
    public static class Converter
    {
        public static async Task ConvertDocxToPdf(string inputPath, string outputDir)
        {
            var (fileName, args) = GetLibreOfficeCommand(inputPath, outputDir);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (startInfo.FileName == null)
            {
                throw new Exception("LibreOffice executable not found. Ensure that LibreOffice is installed and accessible via Flatpak.");
            }

            using (Process? process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    throw new Exception("Failed to start LibreOffice process.");
                }

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"LibreOffice conversion failed for {inputPath}.\nExit Code: {process.ExitCode}\nError: {process.StandardError.ReadToEnd()}");
                }
            }
        }
        private static (string FileName, string Arguments) GetLibreOfficeCommand(string inputPath, string outputDir)
        {
            string baseArgs = $"--headless --convert-to pdf \"{inputPath}\" --outdir \"{outputDir}\"";

            if (IsCommandAvailable(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LibreOffice", "program", "soffice.exe"), "--version"))
            {
                Console.WriteLine("Using 'libreoffice' command for conversion.");
                return ("soffice", baseArgs);
            }

            if (IsCommandAvailable(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LibreOffice", "program", "soffice.exe"), "--version"))
            {
                Console.WriteLine("Using 'soffice' command for conversion.");
                return ("soffice", baseArgs);
            }

            if (IsCommandAvailable("libreoffice", "--version"))
            {
                Console.WriteLine("Using 'libreoffice' command for conversion.");
                return ("libreoffice", baseArgs);
            }

            if (IsCommandAvailable("soffice", "--version"))
            {
                Console.WriteLine("Using 'soffice' command for conversion.");
                return ("soffice", baseArgs);
            }

            if (IsCommandAvailable("flatpak", "info org.libreoffice.LibreOffice"))
            {
                Console.WriteLine("Using 'flatpak run org.libreoffice.LibreOffice' command for conversion.");
                return ("flatpak", $"run org.libreoffice.LibreOffice {baseArgs}");
            }

            throw new Exception("LibreOffice executable not found. Ensure that it is installed natively or accessible via Flatpak.");
        }

        private static bool IsCommandAvailable(string fileName, string arguments)
        {
            try
            {
                using Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
                
                // Return true if the command ran successfully
                return process.ExitCode == 0; 
            }
            catch
            {
                // If it throws an exception (e.g., "No such file or directory"), it's not installed
                return false;
            }
        }
    }
}