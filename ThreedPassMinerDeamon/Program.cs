using System.Diagnostics;

namespace ThreedPassMinerDeamon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var p = Run(args);

            while (true)
            {
                if (p.HasExited)
                {
                    p = Run(args);
                }

                var wers = Process.GetProcessesByName("WerFault");
                foreach (var wer in wers)
                {
                    if (wer.MainWindowTitle == "ThreedPassMiner")
                    {
                        wer.CloseMainWindow();
                    }
                }
                

                Thread.Sleep(1000);
            }
        }

        static Process Run(string[] args)
        {
            if ((int)System.Environment.OSVersion.Platform <= 3)
                return Process.Start("ThreedPassMiner.exe", String.Join(" ", args));
            else
                return Process.Start("dotnet", "ThreedPassMiner.dll " + String.Join(" ", args));
        }

    }
}