using System.Security.Cryptography;

namespace SkinPacker;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        KeyStore.Status status = KeyStore.Load(out RSA rsa, out string message);
        if (status == KeyStore.Status.Mismatch)
        {
            Console.Error.WriteLine(message);
            if (!HasFlag(args, "--selftest") && !HasFlag(args, "--init"))
                MessageBox.Show(message, "Skin Packer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (HasFlag(args, "--selftest"))
        {
            SelfTest(rsa);
            return;
        }

        if (HasFlag(args, "--init"))
        {
            Console.WriteLine(status == KeyStore.Status.Created ? message : "Signing key is ready.");
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(rsa, status == KeyStore.Status.Created ? message : null));
    }

    static bool HasFlag(string[] args, string flag)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    static void SelfTest(RSA rsa)
    {
        var entries = new List<(string Name, byte[] Data)>
        {
            ("logo", new byte[] { 1, 2, 3, 4 }),
            ("colors", System.Text.Encoding.UTF8.GetBytes("{\"textPrimary\":\"#E40A6D\",\"textSecondary\":\"#0099C4\",\"wall\":\"#FFFFFF\",\"ghosts\":[\"#F65285\"]}"))
        };

        byte[] file = PackFormat.Build(entries, rsa);
        if (!PackFormat.TryRead(file, rsa, out var read))
            throw new InvalidOperationException("A freshly signed pack did not verify.");
        if (!read.TryGetValue("logo", out byte[] logo) || logo.Length != 4 || logo[0] != 1)
            throw new InvalidOperationException("Pack entries did not round-trip.");

        byte[] tampered = (byte[])file.Clone();
        tampered[20] ^= 0xFF;
        if (PackFormat.TryRead(tampered, rsa, out _))
            throw new InvalidOperationException("A modified pack was accepted.");

        Console.WriteLine("Skin pack signature check passed.");
    }
}
