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
                throw new Exception("LibreOffice executable not found. Ensure that LibreOffice is installed on your system.");
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

            if (OperatingSystem.IsWindows())
            {
                var pathProgramFiles = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LibreOffice", "program", "soffice.exe");
                if (IsCommandAvailable(pathProgramFiles, "--version"))
                {
                    Console.WriteLine($"Using '{pathProgramFiles}' command for conversion.");
                    return (pathProgramFiles, baseArgs);
                }

                var pathProgramFilesX86 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LibreOffice", "program", "soffice.exe");
                if (IsCommandAvailable(pathProgramFilesX86, "--version"))
                {
                    Console.WriteLine($"Using '{pathProgramFilesX86}' command for conversion.");
                    return (pathProgramFilesX86, baseArgs);
                }
            }

            if (OperatingSystem.IsLinux())
            {
                if (IsCommandAvailable("flatpak", "info org.libreoffice.LibreOffice"))
                {
                    Console.WriteLine("Using 'flatpak run org.libreoffice.LibreOffice' command for conversion.");
                    return ("flatpak", $"run org.libreoffice.LibreOffice {baseArgs}");
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
            }

            if (OperatingSystem.IsMacOS())
            {
                var pathLibreOffice = "/Applications/LibreOffice.app/Contents/MacOS/soffice";
                if (IsCommandAvailable(pathLibreOffice, "--version"))
                {
                    Console.WriteLine($"Using '{pathLibreOffice}' command for conversion.");
                    return (pathLibreOffice, baseArgs);
                }
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
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };

                process.Start();

                if (OperatingSystem.IsWindows())
                {
                    process.StandardInput.Write("\n"); // Send a newline to ensure the command doesn't hang waiting for input
                }

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