using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
namespace SSVocal;

public static class URLPatcher
{
    public static void PatchFile(string fileName, Uri serverUrl)
    {
        PatchFile(fileName, serverUrl.ToString());
    }

    public static void PatchFile(string fileName, string serverUrl)
    {
        byte[] patchData = PatchData(File.ReadAllBytes(fileName), serverUrl);
        File.WriteAllBytes(fileName, patchData);
    }

    public static byte[] PatchData(byte[] data, Uri serverUrl)
    {
        return PatchData(data, serverUrl.ToString());
    }

    public static byte[] PatchData(byte[] data, string serverUrl)
    {
        if (serverUrl.EndsWith('/'))
        {
            serverUrl = serverUrl.Remove(serverUrl.Length - 1);
        }

        if (!Uri.TryCreate(serverUrl, UriKind.RelativeOrAbsolute, out _))
        {
            throw new Exception("URL is invalid !!");
        }

        if (serverUrl.Length > data.Length)
        {
            throw new ArgumentException("URL is too long !!");
        }

        string dataAsString = Encoding.ASCII.GetString(data);

        using MemoryStream ms = new(data);
        using BinaryWriter writer = new(ms);

        string serverOtgUrl = $"{serverUrl}/otg";
        byte[] serverUrlAsBytes = Encoding.ASCII.GetBytes(serverUrl);
        byte[] serverOtgUrlAsBytes = Encoding.ASCII.GetBytes(serverOtgUrl);

        bool wroteUrl = false;

        MatchCollection urls = Regex.Matches(dataAsString, "https://soundshapes.psvita.online.scea.com\x00*");
        MatchCollection otgUrls = Regex.Matches(dataAsString, "https://soundshapes.psvita.online.scea.com/otg\x00*");

        foreach (Match urlMatch in urls)
        {
            wroteUrl = PatchURL(urlMatch, serverUrl, serverUrlAsBytes, writer);
        }

        foreach (Match urlMatch in otgUrls)
        {
            wroteUrl = PatchURL(urlMatch, serverOtgUrl, serverOtgUrlAsBytes, writer);
        }

        if (!wroteUrl)
        {
            throw new Exception("No patchable URLs were found");
        }

        writer.Flush();
        writer.Close();

        return data;
    }

    private static bool PatchURL(Match urlMatch, string serverUrl, byte[] serverUrlAsBytes, BinaryWriter writer)
    {
        string url = urlMatch.Value;

        if (serverUrl.Length > url.Length - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(serverUrl), $"Server URL is too long !!\n" + $"{serverUrl.Length - (url.Length - 1)} above maximum length");
        }
        int offset = urlMatch.Index;

        writer.BaseStream.Position = offset;
        for (int i = 0; i < url.Length; i++)
        {
            writer.Write((byte)0x00); // Zero out data
        }

        writer.BaseStream.Position = offset; // Reset position to beginning
        writer.Write(serverUrlAsBytes);

        return true;
    }

    public static void LaunchSCETool(string args)
    {
        ProcessStartInfo startInfo = new();
        startInfo.UseShellExecute = false;
        startInfo.FileName = Path.GetFullPath("Binaries/scetool.exe");
        startInfo.WorkingDirectory = Path.GetFullPath(".");
        startInfo.Arguments = args;
        startInfo.RedirectStandardOutput = true;

        Console.WriteLine("\n\n-- start scetool --\n");
        using (Process proc = Process.Start(startInfo))
        {
            while (!proc.StandardOutput.EndOfStream) Console.WriteLine(proc.StandardOutput.ReadLine());
            proc.WaitForExit();
        }
        Console.WriteLine("\n-- end scetool --\n\n");
    }
}