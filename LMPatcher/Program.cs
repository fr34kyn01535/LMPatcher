using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LMPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = args[0];

            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update))
            {
                ZipArchiveEntry assemblyEntry = archive.GetEntry("assets/bin/Data/Managed/Assembly-CSharp.dll");
                archive.GetEntry("assets/bin/Data/Managed/UnityEngine.dll").ExtractToFile("UnityEngine.dll", true);
                foreach (string entry in archive.Entries.Where(e => e.FullName.StartsWith("META-INF")).Select(e => e.FullName).ToList())
                {
                    archive.GetEntry(entry).Delete();
                }
                byte[] assemblyBytes;
                using (var stream = new MemoryStream())
                {
                    assemblyEntry.Open().CopyTo(stream);
                    assemblyBytes = stream.ToArray();

                    byte[] password = assemblyBytes.Skip(32).Take(8).ToArray();
                    int passwordIndex = 0;
                    for (int i = 0; i < assemblyBytes.Length; i++)
                    {
                        if (passwordIndex == 8) passwordIndex = 0;
                        assemblyBytes[i] ^= password[passwordIndex++];
                    }


                    AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(new MemoryStream(assemblyBytes));


                    foreach (var patch in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType.FullName == "LMPatcher.Patch").Select(t => Activator.CreateInstance(t) as Patch))
                    {
                        try
                        {
                            patch.Assembly = assembly;
                            patch.Apply();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error in " + patch.GetType().Name + ":" + ex.ToString());
#if DEBUG
                            Console.ReadLine();
#endif
                            Environment.Exit(1);
                        }
                    }

                    using (var ms = new MemoryStream())
                    {
                        assembly.Write(ms);
                        assemblyBytes = ms.ToArray();
                    }

                    File.WriteAllBytes("Assembly-CSharp.patched.dll", assemblyBytes);

                    passwordIndex = 0;
                    for (int i = 0; i < assemblyBytes.Length; i++)
                    {
                        if (passwordIndex == 8) passwordIndex = 0;
                        assemblyBytes[i] ^= password[passwordIndex++];
                    }

                    stream.SetLength(assemblyBytes.Length);
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(assemblyBytes);
                    }
                }
            }
        }
    }

}
