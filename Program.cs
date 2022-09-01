using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MemTestPlain
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine($"Overcommit mode {MemoryUtils.GetOvercommitMemoryMode()}");
            var i = 0;
            while (i++ < 256)
            {
                //MemoryUtils.LogProcMemInfo();
                MemoryUtils.LogMemoryInfo();
                Console.WriteLine($"Allocating next 10Mb, i = {i} (total: {i*10}Mb)");
                MemoryUtils.AllocateMemory(10);
            }
            Console.WriteLine(MemoryUtils.UseBuffers());
        }
    }

    internal static class MemoryUtils
    {
        private static readonly List<byte[]> Buffers = new();

        public static string GetMemInfo()
        {
            GC.Collect();
            var info = GC.GetGCMemoryInfo();
            using var proc = System.Diagnostics.Process.GetCurrentProcess();

            return $"ENV: {f(info.MemoryLoadBytes)}/{f(info.TotalAvailableMemoryBytes)}. PrivateMemorySize {f(proc.PrivateMemorySize64)}";
            string f(long i) => i.HumanReadableSize();
        }

        public static string HumanReadableSize(this long size)
        {
            var sizes = new[] { "B", "KB", "MB" };// "GB", "TB" };
            var order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        public static void LogMemoryInfo()
        {
            Console.WriteLine(GetMemInfo());
            Console.WriteLine("SWAP: " + File.ReadAllText("/proc/swaps"));
        }

        public static void AllocateMemory(int sizeInMb)
        {
            try
            {
                var buf = new byte[sizeInMb * 1024 * 1024];
                //Array.Clear(buf, 0, buf.Length);
                //buf[buf.Length / 2] = (byte)sizeInMb;
                Array.Fill<byte>(buf, 16);
                Buffers.Add(buf);
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine("Okay, got OutOfMemoryException!");
                throw;
            }
        }

        public static int UseBuffers() => Buffers.Sum(b => b[b.Length / 2]);

        //public static void LogProcMemInfo()
        //{
        //    var memInfo = ParseProcMemInfo();
        //    Console.WriteLine($"MEM: {get("MemFree")}/{get("MemTotal")}. SWAP: {get("SwapFree")}/{get("SwapTotal")}");
        //    Console.WriteLine($"Private Bytes: {GetProcStatPrivateMemorySize().HumanReadableSize()}");
        //    string get(string key) => memInfo.ContainsKey(key) ? memInfo[key] : "<->";
        //}

        //public static string GetOvercommitMemoryMode()
        //{
        //    const string path = @"/proc/sys/vm/overcommit_memory";
        //    if (!File.Exists(path))
        //    {
        //        return "<unknown>";
        //    }

        //    return File.ReadAllText(path).Trim();
        //}
        //public static Dictionary<string, string> ParseProcMemInfo()
        //{
        //    const string fileName = "/proc/meminfo";
        //    if (!File.Exists(fileName))
        //    {
        //        return new();
        //    }

        //    return File.ReadAllLines(fileName)
        //        .Select(line => line.Split(':'))
        //        .Where(parts => parts.Length == 2)
        //        .ToDictionary(x => x[0].Trim(), x => x[1].Trim());
        //}

        //public static long GetProcStatPrivateMemorySize()
        //{
        //    const string fileName = "/proc/self/stat";
        //    if (!File.Exists(fileName))
        //    {
        //        return default;
        //    }

        //    var content = File.ReadAllText(fileName)
        //        .Split(' ');
            
        //    Console.WriteLine(String.Join(' ' ,content));
        //    var vsize = content
        //        .ElementAt(22);
        //    return long.Parse(vsize);
        //}
    }

    
}
