using LibcDatabaseSharp.Class;

namespace LibcDatabaseSharp.Generic
{
    public class GlobalVariables
    {
        public static List<string> LibcPaths = new List<string>();

        public static List<string> LoadFailedLibcPaths = new List<string>();

        public static List<Libc> Libcs = new List<Libc>();
    }
}