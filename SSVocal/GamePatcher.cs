using System.Diagnostics;

namespace SSVocal;

public static class GamePatcher
{
    public static void LaunchPSArcTool(string args)
    {
        ProcessStartInfo startInfo = new();
        startInfo.UseShellExecute = false;
        startInfo.FileName = Path.GetFullPath("Binaries/psarc.exe");
        startInfo.WorkingDirectory = Path.GetFullPath(".");
        startInfo.Arguments = args;
        startInfo.RedirectStandardOutput = true;

        Console.WriteLine("\n\n-- start psarc tool --\n");
        using (Process proc = Process.Start(startInfo))
        {
            while (!proc.StandardOutput.EndOfStream)
            {
                Console.WriteLine(proc.StandardOutput.ReadLine());
            }

            proc.WaitForExit();
        }
        Console.WriteLine("\n-- end psarc tool --\n\n");
    }

    public static void ExtractPSArc(string inputPath, string outputPath)
    {
        LaunchPSArcTool(
            $" extract" +
            $" --overwrite" +
            $" --input \"{inputPath}\"" +
            $" --to \"{outputPath}\""
        );
    }
}
