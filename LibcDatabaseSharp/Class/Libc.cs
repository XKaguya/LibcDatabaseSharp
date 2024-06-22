#pragma warning disable CS8618

using System.Text;
using ELFSharp.ELF.Sections;

namespace LibcDatabaseSharp.Class
{
    public class Libc
    {
        public string Name { get; set; }
        
        public string Arch { get; set; }
        
        public string Version { get; set; }
        
        public bool IsDebug { get; set; }
        
        public bool IsX64 { get; set; }
        
        public SymbolTable<uint> SymbolTable { get; set; }
        
        public SymbolTable<ulong> SymbolTable64 { get; set; }
        
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Libc Name: {Name}");
            stringBuilder.AppendLine($"Libc Arch: {Arch}");
            stringBuilder.AppendLine($"Libc Version: {Version}");
            stringBuilder.AppendLine($"Libc IsDebug: {IsDebug}");

            return stringBuilder.ToString();
        }
    }
}