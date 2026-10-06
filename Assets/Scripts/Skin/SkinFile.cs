using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// File layout, little endian:
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
public static class SkinFile
{
    public const int MaxBytes = 32 * 1024 * 1024;

    public static bool TryRead(byte[] file, out Dictionary<string, byte[]> entries)
    {
        entries = null;
        try
        {
            if (file == null || file.Length < 16 || file.Length > MaxBytes)
                return false;

            using (var reader = new BinaryReader(new MemoryStream(file)))
            {
                byte[] magic = reader.ReadBytes(4);
                if (magic.Length != 4 || magic[0] != (byte)'P' || magic[1] != (byte)'M' || magic[2] != (byte)'S' || magic[3] != (byte)'K')
                    return false;
                if (reader.ReadUInt16() != 1)
                    return false;

                int payloadLength = reader.ReadInt32();
                if (payloadLength < 0 || payloadLength > file.Length)
                    return false;
                byte[] payload = reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength)
                    return false;

                int signatureLength = reader.ReadInt32();
                if (signatureLength <= 0 || signatureLength > file.Length)
                    return false;
                byte[] signature = reader.ReadBytes(signatureLength);
                if (signature.Length != signatureLength)
                    return false;
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    return false;

                if (!Verify(payload, signature))
                    return false;

                return TryParsePayload(payload, out entries);
            }
        }
        catch (Exception)
        {
            entries = null;
            return false;
        }
    }

    static bool Verify(byte[] payload, byte[] signature)
    {
        if (SkinPublicKey.Modulus == null || SkinPublicKey.Modulus.Length == 0)
            return false;

        using (var rsa = RSA.Create())
        {
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = SkinPublicKey.Modulus,
                Exponent = SkinPublicKey.Exponent
            });
            return rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    static bool TryParsePayload(byte[] payload, out Dictionary<string, byte[]> entries)
    {
        entries = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        using (var reader = new BinaryReader(new MemoryStream(payload)))
        {
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
                if (dataLength < 0 || dataLength > payload.Length)
                    return false;
                byte[] data = reader.ReadBytes(dataLength);
                if (data.Length != dataLength)
                    return false;
                entries[name] = data;
            }

            return reader.BaseStream.Position == reader.BaseStream.Length;
        }
    }
}
