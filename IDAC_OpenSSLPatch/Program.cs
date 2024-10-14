using System;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;

class Program
{
    static void Main(string[] args)
    {
        if (!IsAdministrator())
        {
            Console.WriteLine("Error: You need to run this program as an administrator.");
            Console.WriteLine("Press enter...");
            Console.ReadLine();
            return;
        }

        string cpuname = GetCpuName();
        Console.WriteLine($"CPU Detected: {cpuname}");

        if (CheckCpu(cpuname))
        {
            OpenSSLPatch();
            Console.WriteLine("Patch successfully.");
            Console.WriteLine("Press enter...");
            Console.ReadLine();
        }

        else
        {
            Console.WriteLine("Error: No patch required (AMD or Intel < 9th gen or older CPU detected).");
            Console.WriteLine("Press enter...");
            Console.ReadLine ();
        }
    }

    static string GetCpuName()
    {
        string cpuname = "";
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_Processor");

        foreach (ManagementObject obj in searcher.Get())
        {
            cpuname = obj["Name"].ToString();
        }

        return cpuname;
    }

    static bool IsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static bool CheckCpu(string cpuname)
    {
        if (cpuname.Contains("Core 2 Duo") || cpuname.Contains("Core 2 Quad") || (cpuname.Contains("Pentium") && !cpuname.Contains("G")) ||cpuname.Contains("Celeron"))
        {
            Console.WriteLine("Trash detected. No patch needed.");
            return false;
        }

        if (cpuname.Contains("Intel"))
        {
            string[] nameparts = cpuname.Split(' ');

            foreach (string part in nameparts)
            {
                if (part.StartsWith("i") && part.Length >= 4 && int.TryParse(part.Substring(2, 3), out int gen1))
                {
                    if (gen1 < 1000)
                    {
                        Console.WriteLine("Trash detected. No patch needed.");
                        return false;
                    }
                }

                if (part.StartsWith("G") && part.Length >= 4 && int.TryParse(part.Substring(1, 4), out int pentium))
                {
                    int pendigit = pentium / 1000;  
                    if (pendigit >= 6)
                    {
                        Console.WriteLine("Founded Pentium Gen 10 or better.");
                        return true;  
                    }
                    else
                    {
                        return false; 
                    }
                }

                if (part.Length >= 4 && int.TryParse(part.Substring(0, 2), out int gen10andup))
                {   
                    if (gen10andup >= 10)
                    {
                        Console.WriteLine("Founded Core Gen 10 or better.");
                        return true;
                    }
                } 

                else if (part.Length >= 3 && int.TryParse(part.Substring(0, 1), out int gen1digit))
                {
                    if (gen1digit >= 2 && gen1digit <= 9)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        return false;
    }

    static void OpenSSLPatch()
    {
        const string variablename = "OPENSSL_ia32cap";
        const string variablevalue = "~0x20000000";

        try
        {
            Environment.SetEnvironmentVariable(variablename, variablevalue, EnvironmentVariableTarget.Machine);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error:"+ex);
        }
    }
}