using ELFSharp;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace SymbolTableExtension
{
    public static class SymbolTableExtensions
    {
        public static void AddEntry<T>(this SymbolTable<T> symbolTable, SymbolEntry<T> symbolEntry) where T : struct
        {
            var entriesList = symbolTable.GetEntriesList();
            entriesList.Add(symbolEntry);
        }

        private static List<SymbolEntry<T>> GetEntriesList<T>(this SymbolTable<T> symbolTable) where T : struct
        {
            var entriesField = typeof(SymbolTable<T>).GetField("entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (entriesField == null)
            {
                throw new System.Exception("Unable to access SymbolTable entries list.");
            }
            return (List<SymbolEntry<T>>)entriesField.GetValue(symbolTable);
        }
    }
}