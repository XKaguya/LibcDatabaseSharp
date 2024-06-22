using System.Text.RegularExpressions;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using LibcDatabaseSharp.Class;
using LibcDatabaseSharp.Generic;
using Newtonsoft.Json;

namespace LibcDatabaseSharp.API
{
    public class Handler
    {
        public static string? GetFuncOffset(Libc libc, string funcName)
        {
            if (libc.IsX64)
            {
                foreach (var func in libc.SymbolTable64.Entries)
                {
                    if (func.Name == funcName)
                    {
                        Logger.LogInfo($"{func.Name}: 0x{func.Value.ToString("X")}");
                        return func.Value.ToString("X");
                    }
                }
            }
            else
            {
                foreach (var func in libc.SymbolTable.Entries)
                {
                    if (func.Name == funcName)
                    {
                        Logger.LogInfo($"{func.Name}: 0x{func.Value.ToString("X")}");
                        return func.Value.ToString("X");
                    }
                }
            }

            return null;
        }
        
        public static string? GetFuncOffset(string libcName, string funcName)
        {
            foreach (Libc libc in GlobalVariables.Libcs)
            {
                if (libc.Name == libcName)
                {
                    if (libc.IsX64)
                    {
                        foreach (var func in libc.SymbolTable64.Entries)
                        {
                            if (func.Name == funcName)
                            {
                                Logger.LogInfo($"{func.Name}: 0x{func.Value.ToString("X")}");
                                return ("0x" + func.Value.ToString("X"));
                            }
                        }
                    }
                    else
                    {
                        foreach (var func in libc.SymbolTable.Entries)
                        {
                            if (func.Name == funcName)
                            {
                                Logger.LogInfo($"{func.Name}: 0x{func.Value.ToString("X")}");
                                return ("0x" + func.Value.ToString("X"));
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static string GetLibcDetails(string libcName)
        {
            List<object> details = new List<object>();

            foreach (Libc libc in GlobalVariables.Libcs)
            {
                if (libc.Name == libcName)
                {
                    if (libc.IsX64)
                    {
                        foreach (var func in libc.SymbolTable64.Entries)
                        {
                            var functionDetails = new
                            {
                                FunctionName = func.Name,
                                FunctionValueHex = $"0x{func.Value:X}",
                                FunctionSizeHex = $"0x{func.Size:X}"
                            };

                            details.Add(functionDetails);
                        }
                    }
                    else
                    {
                        foreach (var func in libc.SymbolTable.Entries)
                        {
                            var functionDetails = new
                            {
                                FunctionName = func.Name,
                                FunctionValueHex = $"0x{func.Value:X}",
                                FunctionSizeHex = $"0x{func.Size:X}"
                            };

                            details.Add(functionDetails);
                        }
                    }
                }
            }
            
            return JsonConvert.SerializeObject(details);
        }
        
        public static List<Libc>? GetMatchingLibc(IEnumerable<string> funcNames, IEnumerable<string> offsets)
        {
            try
            {
                var parsedOffsets = offsets.Select(offset => offset.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? offset.Substring(2) : offset).ToList();
                
                List<Libc> matchingLibcs = new List<Libc>();

                foreach (Libc libc in GlobalVariables.Libcs)
                {
                    if (libc.SymbolTable != null || libc.SymbolTable64 != null)
                    {
                        bool allMatch = true;

                        foreach (var (funcName, parsedOffset) in funcNames.Zip(parsedOffsets, (f, o) => (f, o)))
                        {
                            bool matchFound = false;

                            if (libc.IsX64)
                            {
                                foreach (var sym in libc.SymbolTable64.Entries)
                                {
                                    if (sym.Name == funcName && sym.Value.ToString("X").EndsWith(parsedOffset, StringComparison.OrdinalIgnoreCase))
                                    {
                                        matchFound = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var sym in libc.SymbolTable.Entries)
                                {
                                    if (sym.Name == funcName && sym.Value.ToString("X").EndsWith(parsedOffset, StringComparison.OrdinalIgnoreCase))
                                    {
                                        matchFound = true;
                                        break;
                                    }
                                }
                            }

                            if (!matchFound)
                            {
                                allMatch = false;
                                break;
                            }
                        }

                        if (allMatch)
                        {
                            Logger.LogInfo($"Found matching libc: {libc.Name}", false);
                            matchingLibcs.Add(libc);
                        }
                    }
                }

                if (matchingLibcs.Count == 0)
                {
                    Logger.LogError("No matching libc found.");
                }

                return matchingLibcs;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message + ex.StackTrace);
                throw;
            }
        }
        
        public static List<Libc>? GetMatchingLibc(string funcName, string offset)
        {
            return GetMatchingLibc(new List<string> { funcName }, new List<string> { offset });
        }
        
        private static SymbolTable<T> GetSymbolTable<T>(IELF elf) where T : struct
        {
            bool isLoaded = false;
            
            SymbolTable<ulong>? symbolTable64 = null;
            SymbolTable<uint>? symbolTable = null;

            if (elf.Sections.Any(section => section.Name == ".symtab"))
            {
                if (elf.Class == ELFSharp.ELF.Class.Bit64)
                {
                    try
                    {
                        symbolTable64 = elf.GetSection(".symtab") as SymbolTable<ulong>;
                    
                        isLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Section .symtab not found in the libc file.");
                        return null;
                    }
                }
                else
                {
                    try
                    {
                        symbolTable = elf.GetSection(".symtab") as SymbolTable<uint>;
                    
                        isLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Section .symtab not found in the libc file.");
                        return null;
                    }
                }
            }
            else if (elf.Sections.Any(section => section.Name == ".dynsym") && !isLoaded)
            {
                if (elf.Class == ELFSharp.ELF.Class.Bit64)
                {
                    try
                    {
                        symbolTable64 = elf.GetSection(".dynsym") as SymbolTable<ulong>;
                    
                        isLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Section .dynsym not found in the libc file.");
                        return null;
                    }
                }
                else
                {
                    try
                    {
                        symbolTable = elf.GetSection(".dynsym") as SymbolTable<uint>;
                    
                        isLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Section .dynsym not found in the libc file.");
                        return null;
                    }
                }
            }
            else
            {
                Logger.LogError($"ELF has no such section. {elf.Sections}");
            }

            if (symbolTable == null && symbolTable64 == null)
            {
                return null;
            }

            switch (elf.Class)
            {
                case ELFSharp.ELF.Class.Bit32:
                    return symbolTable as SymbolTable<T>;;
                
                case ELFSharp.ELF.Class.Bit64:
                    return symbolTable64 as SymbolTable<T>;;
            }

            return null;
        }

        private static bool LoadLibcFile()
        {
            foreach (string libcPath in GlobalVariables.LibcPaths)
            {
                IELF elf = ELFReader.Load(libcPath);

                // Read Libc Version
                // The version detection based on .rodata section was abandoned due to the database mostly have the accure libc name.
                
                /*ISection rodataSection = null;
                try
                {
                    rodataSection = elf.GetSection(".rodata");
                }
                catch (Exception ex)
                {
                    Logger.LogError(".rodata Section is null.");
                    Logger.LogError($"Skipping {libcPath}.");
                    GlobalVariables.LoadFailedLibcPaths.Add(libcPath);
                    continue;
                }*/

                /*var reader = new BinaryReader(new MemoryStream(rodataSection.GetContents()));
                var rodataContent = reader.ReadBytes(rodataSection.GetContents().Length);
                var rodataString = System.Text.Encoding.UTF8.GetString(rodataContent);*/

                var patterns = new[]
                {
                    @"libc-([\d.]+)",
                    @"glibc-(\d+\.\d+-[\w.]+)\.(\w+)\.",
                    @"(musl)_(\d+\.\d+\.\d+-\d+)",
                    @"([a-zA-Z0-9]+)_(\d+\.\d+)-([\w\.]+)_"
                };

                Match match = null;
                foreach (var pattern in patterns)
                {
                    match = Regex.Match(libcPath, pattern);
                    if (match.Success)
                    {
                        break;
                    }
                }

                if (!match.Success)
                {
                    Logger.LogError($"This arch is not implemented yet.");
                    Logger.LogError($"Skipping {libcPath}");
                    GlobalVariables.LoadFailedLibcPaths.Add(libcPath);
                    continue;
                }

                Libc libc = new Libc();

                if (match.Value.Contains("musl"))
                {
                    if (!match.Value.Contains("amd64") && !match.Value.Contains("i386"))
                    {
                        if (elf.Class == ELFSharp.ELF.Class.Bit64)
                        {
                            libc.Name = $"{match.Groups[1].Value}-{match.Groups[2].Value}-amd64";
                        }
                        else if (elf.Class == ELFSharp.ELF.Class.Bit32)
                        {
                            libc.Name = $"{match.Groups[1].Value}-{match.Groups[2].Value}-i386";
                        }
                    }
                    else
                    {
                        libc.Name = $"{match.Groups[1].Value}-{match.Groups[2].Value}";
                    }

                    libc.Arch = match.Groups[1].Value;
                    libc.Version = match.Groups[2].Value;
                }
                else if (match.Groups.Count == 4)
                {
                    if (match.Groups[1].Value == "dietlibc")
                    {
                        libc.Version = $"{match.Groups[2].Value}-{match.Groups[3].Value}";
                        libc.Arch = match.Groups[1].Value;
                        
                        if (!match.Value.Contains("amd64") && !match.Value.Contains("i386"))
                        {
                            if (elf.Class == ELFSharp.ELF.Class.Bit64)
                            {
                                libc.Name = $"{libc.Arch}-{libc.Version}-amd64";
                            }
                            else if (elf.Class == ELFSharp.ELF.Class.Bit32)
                            {
                                libc.Name = $"{libc.Arch}-{libc.Version}-i386";
                            }
                        }
                        else
                        {
                            libc.Name = $"{libc.Arch}-{libc.Version}";
                        }
                    }
                    else if (match.Groups[3].Value.Contains("ubuntu"))
                    {
                        libc.Version = match.Groups[2].Value;
                        libc.Arch = "libc6";

                        if (!match.Groups[1].Value.Contains("libc6"))
                        {
                            libc.Name = $"{libc.Arch}-{match.Groups[3].Value}-{match.Groups[1].Value}";
                        }
                        else if (!match.Value.Contains("amd64") && !match.Value.Contains("i386"))
                        {
                            if (elf.Class == ELFSharp.ELF.Class.Bit64)
                            {
                                libc.Name = $"{libc.Arch}-{match.Groups[3].Value}-amd64";
                            }
                            else if (elf.Class == ELFSharp.ELF.Class.Bit32)
                            {
                                libc.Name = $"{libc.Arch}-{match.Groups[3].Value}-i386";
                            }
                        }
                        else
                        {
                            libc.Name = $"{libc.Arch}-{match.Groups[3].Value}";
                        }
                    }
                    else
                    {
                        libc.Version = $"{match.Groups[2].Value}-{match.Groups[3].Value}";
                        libc.Arch = "GNU Libc";
                        
                        if (!match.Value.Contains("amd64") && !match.Value.Contains("i386"))
                        {
                            if (elf.Class == ELFSharp.ELF.Class.Bit64)
                            {
                                libc.Name = $"{libc.Arch}-{libc.Version}-amd64";
                            }
                            else if (elf.Class == ELFSharp.ELF.Class.Bit32)
                            {
                                libc.Name = $"{libc.Arch}-{libc.Version}-i386";
                            }
                        }
                        else
                        {
                            libc.Name = $"{libc.Arch}-{libc.Version}";
                        }
                    }
                }
                else if (match.Groups.Count == 3)
                {
                    string arch = match.Groups[1].Value;
                    string version = match.Groups[2].Value;
                    
                    if (!match.Value.Contains("amd64") && !match.Value.Contains("i386"))
                    {
                        if (elf.Class == ELFSharp.ELF.Class.Bit64)
                        {
                            libc.Name = $"{arch}-{version}-amd64";
                        }
                        else if (elf.Class == ELFSharp.ELF.Class.Bit32)
                        {
                            libc.Name = $"{arch}-{version}-i386";
                        }
                    }
                    else
                    {
                        libc.Name = $"{arch}-{version}";
                    }
                    
                    libc.Name = arch + version;
                    libc.Version = version;
                    libc.Arch = arch;
                }
                else if (match.Groups.Count == 2)
                {
                    var random = new Random();
                    int randomNumber = random.Next(1000, 9999);
                    
                    string arch = "libc";
                    string version = $"{match.Groups[1].Value}_{randomNumber}";
                    
                    if (!match.Value.Contains("amd64") && !match.Value.Contains("i386"))
                    {
                        if (elf.Class == ELFSharp.ELF.Class.Bit64)
                        {
                            libc.Name = $"{arch}-{version}-amd64";
                        }
                        else if (elf.Class == ELFSharp.ELF.Class.Bit32)
                        {
                            libc.Name = $"{arch}-{version}-i386";
                        }
                    }
                    else
                    {
                        libc.Name = $"{arch}-{version}";
                    }
                    
                    libc.Version = version;
                    libc.Arch = arch;
                }

                // Get Function Table
                SymbolTable<ulong> symbolTable64 = null;
                SymbolTable<uint> symbolTable = null;

                if (elf.Class == ELFSharp.ELF.Class.Bit32)
                {
                    libc.IsX64 = false;
                    symbolTable = GetSymbolTable<uint>(elf);
                }
                else if (elf.Class == ELFSharp.ELF.Class.Bit64)
                {
                    libc.IsX64 = true;
                    symbolTable64 = GetSymbolTable<ulong>(elf);
                }

                if (symbolTable == null && symbolTable64 == null)
                {
                    Logger.LogError($"{libc.Name} Symbol Table is null!");
                    GlobalVariables.LoadFailedLibcPaths.Add(libcPath);
                    continue;
                }

                libc.SymbolTable = symbolTable;
                libc.SymbolTable64 = symbolTable64;

                // Get Debug Section
                foreach (var section in elf.Sections)
                {
                    if (section.Name.Contains("debug"))
                    {
                        libc.IsDebug = true;
                    }
                }

                GlobalVariables.Libcs.Add(libc);
            }

            return true;
        }

        public static bool ReadAllLibcFile()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string dbDirectory = Path.Combine(currentDirectory, "db");

            if (!Directory.Exists(dbDirectory))
            {
                Logger.LogFatal("Database directory not exists !");
                return false;
            }
            
            GlobalVariables.LibcPaths = Directory.GetFiles(dbDirectory, "*.so", SearchOption.AllDirectories).ToList();
            
            LoadLibcFile();
            
            // Try load as TODO
            if (GlobalVariables.LoadFailedLibcPaths.Count != 0)
            {
                Logger.LogInfo("TODO feature.");
            }

            if (GlobalVariables.Libcs.Count == 0)
            {
                Logger.LogFatal("No libc were loaded. Please check logs.");
                return false;
            }
            
            Logger.LogInfo($"Load Complete. {GlobalVariables.Libcs.Count} were loaded successful and {GlobalVariables.LoadFailedLibcPaths.Count} were failed.");
            
            return true;
        }
    }
}