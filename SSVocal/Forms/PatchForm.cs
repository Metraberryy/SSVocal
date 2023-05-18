using Eto.Forms;
using Eto.Drawing;
using Eto;
using System.Text.RegularExpressions;
using System;

namespace SSVocal.Forms;

public class PatchForm : Form
{
    private readonly FilePicker filePicker;
    private readonly FilePicker licencePicker;
    private readonly TextBox serverUrl;
    private readonly CheckBox urlPatch;
    private readonly CheckBox mlPatch;

    public Control CreatePatchButton(int tabIndex = 0)
    {
        Button control = new()
        {
            Text = "Patch!",
            TabIndex = tabIndex,
        };

        control.Click += (o, e) => Patch();

        return control;
    }

    private void Patch()
    {
        // validate the form first, then attempt patching

        if (string.IsNullOrWhiteSpace(filePicker.FilePath))
        {
            Gui.CreateOkDialog("Form Error", "No folder specified!");
            return;
        }

        if ((bool)urlPatch.Checked)
        {
            if (string.IsNullOrWhiteSpace(licencePicker.FilePath))
            {
                Gui.CreateOkDialog("Form Error", "No licence specified!");
                return;
            }

            string lower = licencePicker.FilePath.ToLower();
            if (!(lower.EndsWith(".rap") || lower.EndsWith(".rif")))
            {
                Gui.CreateOkDialog("Form Error", "Invalid licence!");
                return;
            }

            if (string.IsNullOrWhiteSpace(serverUrl.Text))
            {
                Gui.CreateOkDialog("Form Error", "No server URL specified!");
                return;
            }
            PatchEBOOT(lower.Substring(lower.Length - 3));
        }

        if ((bool)mlPatch.Checked)
        {
            PatchGame();
        }
    }

    public void PatchEBOOT(string extension)
    {
        string binPath = Path.Combine(filePicker.FilePath, "USRDIR", "EBOOT.BIN");
        string elfPath = Path.Combine(filePicker.FilePath, "USRDIR", "EBOOT.elf");

        int pos = licencePicker.FilePath.LastIndexOf("\\") + 1;
        string licenceName = licencePicker.FilePath.Substring(pos, licencePicker.FilePath.Length - pos);
        Directory.CreateDirectory(Path.GetFullPath(@$"{extension}s"));
        try
        {
            File.Copy(licencePicker.FilePath, Path.GetFullPath(@$"{extension}s/{licenceName}"));
        }
        catch { } // if it's already copied, ignore

        if (File.Exists($"{binPath}.bak"))
        {
            File.Move($"{binPath}.bak", binPath);
        }
        URLPatcher.LaunchSCETool($" -v -d \"{binPath}\" \"{elfPath}\"");
        File.Move(binPath, $"{binPath}.bak");

        try
        {
            URLPatcher.PatchFile(elfPath, serverUrl.Text);
        }
        catch (Exception e)
        {
            Gui.CreateOkDialog("Error occurred while patching", "An error occured while patching:\n" + e);
            return;
        }

        string contentID = licenceName.Substring(0, licenceName.Length - 4);

        URLPatcher.LaunchSCETool($"--verbose " +
                      $"--sce-type=SELF" +
                      $" --skip-sections=FALSE" +
                      $" --self-add-shdrs=TRUE" +
                      $" --compress-data=TRUE" +
                      $" --key-revision=0A" +
                      $" --self-app-version=0001000000000000" +
                      $" --self-auth-id=1010000001000003" +
                      $" --self-vendor-id=01000002" +
                      $" --self-ctrl-flags=0000000000000000000000000000000000000000000000000000000000000000" +
                      $" --self-cap-flags=00000000000000000000000000000000000000000000003B0000000100040000" +
                      $" --self-type=NPDRM" +
                      $" --self-fw-version=0003005500000000" +
                      $" --np-license-type=FREE" +
                      $" --np-app-type=SPRX" +
                      $" --np-content-id={contentID}" +
                      $" --np-real-fname=EBOOT.BIN" +
                      $" --encrypt {elfPath} {binPath}");

        Gui.CreateOkDialog("Success!", "The Server URL has been patched to " + serverUrl.Text);
    }

    public void PatchGame()
    {
        string inputPath;
        string outputPath = Path.Combine(filePicker.FilePath, "USRDIR", "game_data");
        IEnumerable<string> files = Directory.GetFiles(Path.Combine(filePicker.FilePath, "USRDIR")).Where(file => file.EndsWith(".psarc"));

        foreach (string file in files)
        {
            inputPath = file;
            GamePatcher.ExtractPSArc(file, outputPath);
        }
    }

    public PatchForm()
    {
        Title = "SSVocal Patcher";
        ClientSize = new Size(512, -1);
        Content = new TableLayout
        {
            Spacing = new Size(5, 5),
            Padding = new Padding(10, 10, 10, 10),
            Rows = {
                new TableRow(
                    new TableCell(new Label { Text = "Game folder: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(filePicker = new FilePicker { TabIndex = 0, FileAction = FileAction.SelectFolder})
                ),
                new TableRow(
                    new TableCell(new Label { Text = "RAP or RIF file: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(licencePicker = new FilePicker { TabIndex = 1, FileAction = FileAction.OpenFile })
                ),
                new TableRow(
                    new TableCell(new Label { Text = "Server URL: ", VerticalAlignment = VerticalAlignment.Center }),
                    new TableCell(serverUrl = new TextBox { TabIndex = 2 })
                ),
                new TableRow(
                    new TableCell(urlPatch = new CheckBox() { Text = "URL Patch?", TabIndex = 3, Checked = true }),
                    new TableCell(mlPatch = new CheckBox() { Text = "ModLoader Patch?", TabIndex = 4, Checked = true })
                ),
                new TableRow(
                    new TableCell(CreatePatchButton(5))
                ),
            },
        };
    }
}
