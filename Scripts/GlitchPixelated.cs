using System;
using System.IO.Compression;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GlitchPixelated : MonoBehaviour
{
    //**Don't foeget to make sure the image type is set to "Advanced" and "Read/Write Enabled" 
    // is checked in the import settings of the image you want to pixelate.**

    public SpriteRenderer spriteRenderer;

    private Texture2D originalTexture;
    private Texture2D pixelatedTexture;
    private Texture2D copyTexture;
    public int dividePixles = 10;
    public float mouseDragRadius = 10f;
    private Camera cameraMain;

    void SetValueFromHost() //waiting for parameter..
    {

    }

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        originalTexture = spriteRenderer.sprite.texture;
        copyTexture = new Texture2D(originalTexture.width, originalTexture.height);

        cameraMain = Camera.main;
    }

    void ReturenOriginalTexture()
    {
        spriteRenderer.sprite = Sprite.Create(originalTexture, new Rect(0, 0, originalTexture.width, originalTexture.height), new Vector2(0.5f, 0.5f));
    }

    void PixelatedMethod()
    {
        ReturenOriginalTexture();

        pixelatedTexture = new Texture2D(copyTexture.width / dividePixles, copyTexture.height / dividePixles);

        for (int y =0; y <= pixelatedTexture.height; y++)
        {
            for (int x =0; x <= pixelatedTexture.width; x++)
            {
                //color glitchColor > blue (random blue value)
                Color pixelColor = originalTexture.GetPixel(x * dividePixles, y * dividePixles);

                for (int dy = 0; dy < dividePixles; dy++)
                {
                    for (int dx = 0; dx < dividePixles; dx++)
                    {
                        if (x * dividePixles + dx < copyTexture.width && y * dividePixles + dy < copyTexture.height)
                        {
                            copyTexture.SetPixel(x * dividePixles + dx, y * dividePixles + dy, pixelColor);
                        }
                    }
                }
            }
        }
        copyTexture.Apply();
        spriteRenderer.sprite = Sprite.Create(copyTexture,new Rect(0, 0, originalTexture.width, originalTexture.height), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PixelatedMethod();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            ReturenOriginalTexture();
        }
    }

    void OnMouseDrag()
    {
        Debug.Log("Mouse is being dragged");

        Vector3 mouseWorld = cameraMain.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseLocal = transform.InverseTransformPoint(mouseWorld);

        float textureX = (mouseLocal.x + 0.5f) * copyTexture.width;
        float textureY = (mouseLocal.y + 0.5f) * copyTexture.height;

        int texX = Mathf.Clamp(Mathf.RoundToInt(textureX), 0, copyTexture.width - 1);
        int texY = Mathf.Clamp(Mathf.RoundToInt(textureY), 0, copyTexture.height - 1);

        ReturnOriginalPixels(texX, texY);

        Debug.Log($"Mouse Position in Texture: ({texX}, {texY})");
        Debug.Log($"Mouse Position in World: {mouseWorld}");
    }

    void ReturnOriginalPixels(int posX, int posY)
    {
        // dividePixles = Mathf.Clamp(dividePixles,1,dividePixles);
        // dividePixles-=(int)Time.deltaTime;

        for (int y = -Mathf.RoundToInt(mouseDragRadius); y <= mouseDragRadius; y++)
        {
            for (int x = -Mathf.RoundToInt(mouseDragRadius); x <= mouseDragRadius; x++)
            {
                int px = posX + x;
                int py = posY + y;

                if (px < 0 || py < 0 || px >= originalTexture.width || py >= originalTexture.height)
                { continue; }

                Color originalPixelColor = originalTexture.GetPixel(px, py);
                copyTexture.SetPixel(px, py, originalPixelColor);
            }
        }
        copyTexture.Apply();
    }

    void SendValueToHost()//waiting for parameter..
    {

    }
}
