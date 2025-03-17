using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Normalize : MonoBehaviour
{

    [SerializeField]
    Texture2D debugTexture;

    [SerializeField]
    Material normalizeMaterial;
    [SerializeField]
    Material blurMaterial;

    Texture2D ingestedTexture = null;
    string fileName = "";
    string[] arguments;

    // Options - Default is Native Png Regular
    EncodeFormat encodeFormat = EncodeFormat.png;

    enum EncodeFormat { png, jpeg, tga };


    void Start()

    {

        normalizeMaterial = new Material(Shader.Find("Normalize"));
        blurMaterial = new Material(Shader.Find("Blur"));

        StartCoroutine(FetchTexture());

        Invoke("Exit", 10f);
    }

    IEnumerator FetchTexture()
    {

        arguments = System.Environment.GetCommandLineArgs();

        // Ingest texture file as a texture2D - foreaches aren't always sorted so we do this once
        foreach (string argument in arguments)
        {

            // debugText.text += argument + System.Environment.NewLine;

            if (argument.EndsWith(".png") || argument.EndsWith(".jpg") || argument.EndsWith(".jpeg"))
            {
                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + argument))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result == UnityWebRequest.Result.Success)
                    {
                        ingestedTexture = DownloadHandlerTexture.GetContent(uwr);

                        string[] splitFilename = argument.Split(new string[] { "\\", ".", "_" }, System.StringSplitOptions.RemoveEmptyEntries);
                        fileName = "N_" + splitFilename[splitFilename.Length - 2];

                    }
                }

            }
        }
        Process();
    }



    void Process()
    {




        if (ingestedTexture == null)
        {
            // This isn't actually a texture? Early exit.
            Exit();
            // ingestedTexture = debugTexture;
            //    targetSize = ingestedTexture.height;
        }

        int targetTextureSize = ingestedTexture.height;

        foreach (string argument in arguments)
        {

            //      debugText.text += argument + System.Environment.NewLine;

            // Format
            if (argument.Contains("encodejpeg"))
            {
                encodeFormat = EncodeFormat.jpeg;
            }
            if (argument.Contains("encodetga"))
            {
                encodeFormat = EncodeFormat.tga;
            }

            // Normal Scale
            if (argument.Contains("light"))
            {
                normalizeMaterial.SetFloat("_NormalScale", 0.05f);
            }
            if (argument.Contains("strong"))
            {
                normalizeMaterial.SetFloat("_NormalScale", 0.15f);
            }

            // Size

            //   int targetTextureSize = 512;

            if (argument.Contains("smallsize"))
            {
                targetTextureSize = 512;

            }
            if (argument.Contains("mediumsize"))
            {
                targetTextureSize = 1024;
            }
            if (argument.Contains("largesize"))
            {
                targetTextureSize = 2048;
            }
            if (argument.Contains("extrasize"))
            {
                targetTextureSize = 4096;
            }

            // Handedness
            if (argument.Contains("opengl"))
            {
                normalizeMaterial.SetFloat("_OpenGL", 1f);
            }
        }



        // Okay cool, we attached to a texture. Let's initialize a render texture the same size
        int superSampledTextureSize = targetTextureSize * 2;
        RenderTexture normalizedRenderTexture = new(superSampledTextureSize, superSampledTextureSize, 0, RenderTextureFormat.ARGB32);
        RenderTexture blurredRenderTexture = new(targetTextureSize, targetTextureSize, 0, RenderTextureFormat.ARGB32);

        // We blit the render texture onto our material to compute the normal map
        Graphics.Blit(ingestedTexture, normalizedRenderTexture, normalizeMaterial);
        Graphics.Blit(normalizedRenderTexture, blurredRenderTexture, blurMaterial);

        // We write the render texture to our output Texture2d
        Texture2D outputTexture = new(targetTextureSize, targetTextureSize, TextureFormat.ARGB32, false, false);
        Rect region = new(0, 0, targetTextureSize, targetTextureSize);
        outputTexture.ReadPixels(region, 0, 0, false);
        outputTexture.Apply();

        //  ouput the file to the target destination
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        if (encodeFormat == EncodeFormat.png)
        {
            System.IO.File.WriteAllBytes(System.Environment.CurrentDirectory + "/" + fileName + ".png", outputTexture.EncodeToPNG());
        }
        if (encodeFormat == EncodeFormat.jpeg)
        {
            System.IO.File.WriteAllBytes(System.Environment.CurrentDirectory + "/" + fileName + ".jpeg", outputTexture.EncodeToPNG());
        }
        if (encodeFormat == EncodeFormat.tga)
        {
            System.IO.File.WriteAllBytes(System.Environment.CurrentDirectory + "/" + fileName + ".tga", outputTexture.EncodeToPNG());
        }



        Exit();
        //    }
    }

    void Exit()
    {
        Application.Quit();
    }
}
