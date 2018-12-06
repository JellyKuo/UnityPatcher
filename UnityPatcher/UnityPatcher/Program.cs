using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("UnityPatcher");
            Console.WriteLine("Hello! Use -r to perform recovery");
#if DEBUG
            string path = @"DEBUG PATH";
#else
            string path = @"C:\Program Files\Unity\Editor\Unity.exe";
#endif
            while (!File.Exists(path))
            {
                Console.WriteLine($"Cannot find Unity executable at {path}, enter a path here:");
                path = Console.ReadLine();
            }
            var pattern = new byte[] { 0x84, 0xC0, 0x75, 0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 };
            var newValue = new byte[] { 0x74 };
            if (args.Length > 0 && args[0] == "-r")
            {
                Console.WriteLine("Recoverying Unity");
                pattern = new byte[] { 0x84, 0xC0, 0x74, 0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 };
                newValue = new byte[] { 0x75 };
            }
            try
            {
                var result = Patch(path, ref pattern, newValue);
                if (result)
                {
                    Console.WriteLine("Patch successful!");
                }
                else
                {
                    Console.WriteLine("Cannot find pattern, are you sure this is Unity?");
                    Console.WriteLine("If you want to recover your Unity, make sure you have -r in execution parameters");
                }
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("Access denied! Make sure you are running this as Administrator!");
                Console.WriteLine($"Exception Message: {uae.Message}");
            }
            catch (IOException ioe)
            {
                Console.WriteLine("Cannot access file! Make sure Unity is not running!");
                Console.WriteLine($"Exception Message: {ioe.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong during the patch");
                Console.WriteLine($"Exception Message: {e.Message}");
            }
            finally
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        static bool Patch(string path, ref byte[] pattern, byte[] newValue)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite))
            {
                int index = 0;
                int address = 0;

                int offset = 0;
                byte[] buffer = new byte[1];
                int length;
                while ((length = stream.Read(buffer, 0, 1)) > 0)
                {
                    offset++;
                    if (index == pattern.Length - 1)
                    {
                        Console.WriteLine($"Found pattern end at offset 0x{offset - 1:x}, hex address to change 0x{address:x}");
                        stream.Seek(address, SeekOrigin.Begin);
                        stream.Write(newValue, 0, 1);
                        return true;
                    }
                    if (buffer[0] == pattern[index])
                    {
                        if (index == 2)
                            address = offset - 1;
                        index++;
                        continue;
                    }
                    else
                    {
                        index = 0;
                        continue;
                    }
                }
                return false;
            }
        }
    }
}
