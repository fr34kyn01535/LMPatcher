using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (args.Length == 0) args = new string[] { "original.apk" };
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
                using(Stream stream = assemblyEntry.Open())
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    assemblyBytes = ms.ToArray();
                }
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
                assemblyEntry.Delete();
                assemblyEntry = archive.CreateEntry("assets/bin/Data/Managed/Assembly-CSharp.dll");

                using (var entryStream = assemblyEntry.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                {
                    streamWriter.Write(assemblyBytes);
                }
            }
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Windows\System32\bash.exe";
            process.StartInfo.Arguments = "./patch.sh";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();
#if DEBUG
            Console.WriteLine("Done...");
            Console.ReadKey();
#endif
        }
    }
}
