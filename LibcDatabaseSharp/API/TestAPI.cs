using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.MachO;

namespace LibcDatabaseSharp.API
{
    public class TestAPI
    {
        public static void ShowSpecificSection(IELF elf)
        {
            SymbolTable<ulong>? symbolTable = null;
            symbolTable = elf.GetSection(".dynsym") as SymbolTable<ulong>;

            foreach (var sym in symbolTable.Entries)
            {
                Console.WriteLine(sym.Name);
            }
        }
    }
}