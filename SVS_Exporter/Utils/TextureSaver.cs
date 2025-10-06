using ADV.Commands.Camera;
using LibCpp2IL.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace SVSExporter.Utils
{
    static class TextureSaver
    {
        public static void SaveTexture(Color32[] colors, int textureWidth, int textureHeight, string path)
        {
            PngWriter.SaveRgbaToPng(colors, textureWidth, textureHeight, path);

        }
        public static void SaveTexture(Texture2D texture, string path)
        {
            var a = texture.GetPixels32();
            PngWriter.SaveRgbaToPng(a, texture.width, texture.height, path);
        }
        public static void SaveTexture(Texture texture, string path)
        {
            RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = temporary;
            GL.Clear(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
            Graphics.Blit(texture, temporary);
            Texture2D texture2D = new Texture2D(temporary.width, temporary.height);
            texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
            var a = texture2D.GetPixels32();
            PngWriter.SaveRgbaToPng(a, texture2D.width, texture2D.height, path);
            UnityEngine.Object.Destroy(texture2D);
            //var a = texture2D.GetPixels32();
            //PngWriter.SaveRgbaToPng(a, texture2D.width, texture2D.height, path);
            //UnityEngine.Object.Destroy(texture2D);
        }
    }
    static class PngWriter
    {
        private static uint[] crcTable = GenerateCrcTable();
        private static byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        public static void SaveRgbaToPng(Color32[] rgbaArray, int width, int height, string outputPath)
        {
            using (var stream = new FileStream(outputPath, FileMode.Create))
            {
                stream.Write(pngSignature, 0, pngSignature.Length);

                WriteIHDR(stream, width, height);

                WriteIDAT(stream, rgbaArray, width, height);

                WriteIEND(stream);
            }
        }

        private static void WriteIHDR(Stream stream, int width, int height)
        {
            byte[] ihdrData = new byte[13];
            ihdrData[0] = (byte)(width >> 24);
            ihdrData[1] = (byte)(width >> 16);
            ihdrData[2] = (byte)(width >> 8);
            ihdrData[3] = (byte)width;
            ihdrData[4] = (byte)(height >> 24);
            ihdrData[5] = (byte)(height >> 16);
            ihdrData[6] = (byte)(height >> 8);
            ihdrData[7] = (byte)height;
            ihdrData[8] = 8;
            ihdrData[9] = 6;
            ihdrData[10] = 0;
            ihdrData[11] = 0;
            ihdrData[12] = 0;

            WriteChunk(stream, "IHDR", ihdrData);
        }

        private static void WriteIDAT(Stream stream, Color32[] rgbaArray, int width, int height)
        {
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.WriteByte(0x78);
                memoryStream.WriteByte(0x9C);

                uint adler = 1;
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                {
                    byte[] row = new byte[1 + width * 4];
                    for (int y = height - 1; y >= 0; y--)
                    {
                        row[0] = 0; // filter byte
                        for (int x = 0; x < width; x++)
                        {
                            int pixelIndex = y * width + x;
                            int dataIndex = 1 + x * 4;
                            row[dataIndex] = rgbaArray[pixelIndex].r;
                            row[dataIndex + 1] = rgbaArray[pixelIndex].g;
                            row[dataIndex + 2] = rgbaArray[pixelIndex].b;
                            row[dataIndex + 3] = rgbaArray[pixelIndex].a;
                        }

                        deflateStream.Write(row, 0, row.Length);

                        foreach (byte b in row)
                        {
                            adler = AdlerUpdate(adler, b);
                        }
                    }
                }

                memoryStream.WriteByte((byte)(adler >> 24));
                memoryStream.WriteByte((byte)(adler >> 16));
                memoryStream.WriteByte((byte)(adler >> 8));
                memoryStream.WriteByte((byte)adler);

                WriteChunk(stream, "IDAT", memoryStream.ToArray());
            }
        }

        private static uint AdlerUpdate(uint adler, byte b)
        {
            const uint MOD_ADLER = 65521;
            uint a = adler & 0xFFFF;
            uint d = adler >> 16 & 0xFFFF;

            a = (a + b) % MOD_ADLER;
            d = (d + a) % MOD_ADLER;

            return d << 16 | a;
        }

        private static void WriteIEND(Stream stream)
        {
            WriteChunk(stream, "IEND", new byte[0]);
        }

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            int length = data.Length;
            stream.WriteByte((byte)(length >> 24));
            stream.WriteByte((byte)(length >> 16));
            stream.WriteByte((byte)(length >> 8));
            stream.WriteByte((byte)length);

            byte[] typeBytes = Encoding.ASCII.GetBytes(type);
            stream.Write(typeBytes, 0, 4);

            stream.Write(data, 0, data.Length);

            byte[] crcData = new byte[4 + data.Length];
            Array.Copy(typeBytes, 0, crcData, 0, 4);
            Array.Copy(data, 0, crcData, 4, data.Length);
            uint crc = ComputeCrc32(crcData);
            stream.WriteByte((byte)(crc >> 24));
            stream.WriteByte((byte)(crc >> 16));
            stream.WriteByte((byte)(crc >> 8));
            stream.WriteByte((byte)crc);
        }

        private static uint ComputeCrc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                crc = crc >> 8 ^ crcTable[(crc ^ b) & 0xFF];
            }
            return ~crc;
        }
        private static uint[] GenerateCrcTable()
        {
            uint[] table = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                {
                    if ((c & 1) != 0)
                        c = 0xEDB88320 ^ c >> 1;
                    else
                        c >>= 1;
                }
                table[n] = c;
            }
            return table;
        }
    }
}
