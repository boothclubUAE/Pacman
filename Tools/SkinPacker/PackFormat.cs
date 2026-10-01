using System.Security.Cryptography;
using System.Text;

namespace SkinPacker;

// File layout, little endian. Must match Assets/Scripts/Skin/SkinFile.cs.
// char[4] magic = PMSK
// ushort version = 1
// int payloadLength
// byte[payloadLength] payload
// int signatureLength
// byte[signatureLength] signature  // RSA-SHA256 PKCS#1 over the payload only
//
// Payload:
// int entryCount
// repeat:
//   int nameLength
//   byte[nameLength] name (UTF-8)
//   int dataLength
//   byte[dataLength] data
static class PackFormat
{
    public static byte[] Build(IReadOnlyList<(string Name, byte[] Data)> entries, RSA rsa)
    {
        byte[] payload = WritePayload(entries);
        byte[] signature = rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(new[] { (byte)'P', (byte)'M', (byte)'S', (byte)'K' });
            writer.Write((ushort)1);
            writer.Write(payload.Length);
            writer.Write(payload);
            writer.Write(signature.Length);
            writer.Write(signature);
        }
        return stream.ToArray();
    }

    public static bool TryRead(byte[] file, RSA rsa, out Dictionary<string, byte[]> entries)
    {
        entries = null;
        try
        {
            using var reader = new BinaryReader(new MemoryStream(file));
            byte[] magic = reader.ReadBytes(4);
            if (magic.Length != 4 || magic[0] != (byte)'P' || magic[1] != (byte)'M' || magic[2] != (byte)'S' || magic[3] != (byte)'K')
                return false;
            if (reader.ReadUInt16() != 1)
                return false;

            int payloadLength = reader.ReadInt32();
            byte[] payload = reader.ReadBytes(payloadLength);
            if (payload.Length != payloadLength)
                return false;

            int signatureLength = reader.ReadInt32();
            byte[] signature = reader.ReadBytes(signatureLength);
            if (signature.Length != signatureLength || reader.BaseStream.Position != reader.BaseStream.Length)
                return false;

            RSAParameters pub = rsa.ExportParameters(false);
            using var verify = RSA.Create();
            verify.ImportParameters(pub);
            if (!verify.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                return false;

            return TryParsePayload(payload, out entries);
        }
        catch (Exception)
        {
            entries = null;
            return false;
        }
    }

    static byte[] WritePayload(IReadOnlyList<(string Name, byte[] Data)> entries)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(entries.Count);
        foreach (var entry in entries)
        {
            byte[] name = Encoding.UTF8.GetBytes(entry.Name);
            writer.Write(name.Length);
            writer.Write(name);
            writer.Write(entry.Data.Length);
            writer.Write(entry.Data);
        }
        writer.Flush();
        return stream.ToArray();
    }

    static bool TryParsePayload(byte[] payload, out Dictionary<string, byte[]> entries)
    {
        entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        using var reader = new BinaryReader(new MemoryStream(payload));
        int count = reader.ReadInt32();
        if (count < 0 || count > 64)
            return false;

        for (int i = 0; i < count; i++)
        {
            int nameLength = reader.ReadInt32();
            if (nameLength <= 0 || nameLength > 64)
                return false;
            string name = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));
            int dataLength = reader.ReadInt32();
            if (dataLength < 0)
                return false;
            byte[] data = reader.ReadBytes(dataLength);
            if (data.Length != dataLength)
                return false;
            entries[name] = data;
        }

        return reader.BaseStream.Position == reader.BaseStream.Length;
    }
}
