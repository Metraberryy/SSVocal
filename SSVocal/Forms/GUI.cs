using Eto.Drawing;
using Eto.Forms;

namespace SSVocal.Forms;

public static class Gui
{
    public static void Show()
    {
        new Application().Run(new PatchForm());
    }

    public static void CreateOkDialog(string title, string errorMessage)
    {
        MessageBox.Show(errorMessage, title, MessageBoxButtons.OK, MessageBoxType.Information);
    }
    public static bool CreateConfirmationDialog(string title, string errorMessage)
    {
        DialogResult result = MessageBox.Show(errorMessage, title, MessageBoxButtons.YesNo, MessageBoxType.Question);
        return result == DialogResult.Yes;
    }
}