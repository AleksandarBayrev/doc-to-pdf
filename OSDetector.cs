namespace DocToPdf
{
    public enum OperatingSystemType
    {
        Windows,
        Linux,
        MacOS,
        Unknown
    }

    public static class OSDetector
    {
        public static bool IsWindows()
        {
            return OperatingSystem.IsWindows();
        }

        public static bool IsLinux()
        {
            return OperatingSystem.IsLinux();
        }

        public static bool IsMacOS()
        {
            return OperatingSystem.IsMacOS();
        }

        public static string GetOSName()
        {
            if (IsWindows())
                return OperatingSystemType.Windows.ToString();
            else if (IsLinux())
                return OperatingSystemType.Linux.ToString();
            else if (IsMacOS())
                return OperatingSystemType.MacOS.ToString();
            else
                return OperatingSystemType.Unknown.ToString();
        }
    }
}