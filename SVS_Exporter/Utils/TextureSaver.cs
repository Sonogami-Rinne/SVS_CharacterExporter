using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace SVSExporter.Utils
{
    internal class TextureSaver
    {
        public static void SaveTexture(Color32[] colors, int textureWidth, int textureHeight, string path)
        {
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
            texture.SetPixels32(colors);
            texture.Apply();
            var data = texture.EncodeToPNG();
            new Thread((ThreadStart)delegate
            {
                Thread.CurrentThread.IsBackground = false;
                File.WriteAllBytes(path, data);
            }).Start();
            Texture2D.Destroy(texture);

        }
        public static void SaveTexture(Texture2D texture, string path)
        {
            var data = texture.EncodeToPNG();
            new Thread((ThreadStart)delegate
            {
                Thread.CurrentThread.IsBackground = false;
                File.WriteAllBytes(path, data);
            }).Start();
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
            var data = texture2D.EncodeToPNG();
            new Thread((ThreadStart)delegate
            {
                Thread.CurrentThread.IsBackground = false;
                File.WriteAllBytes(path, data);
            }).Start();
            Texture2D.Destroy(texture2D);
            //var a = texture2D.GetPixels32();
            //PngWriter.SaveRgbaToPng(a, texture2D.width, texture2D.height, path);
            //UnityEngine.Object.Destroy(texture2D);
        }
    }
}
