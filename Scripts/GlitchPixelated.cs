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
    private RaycastHit2D rayCastHit;
    public float fadeSpeed = 0.15f; 

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

        for (int y = 0; y <= pixelatedTexture.height; y++)
        {
            for (int x = 0; x <= pixelatedTexture.width; x++)
            {
                //random blue value
                float randomBlue = UnityEngine.Random.Range(0.4f, 1f);
                float randomGreen = UnityEngine.Random.Range(0f, 0.3f);
                float randomRed = UnityEngine.Random.Range(0f, 0.2f);

                Color pixelColor = new Color(randomRed, randomGreen, randomBlue);

                for (int dy = 0; dy < dividePixles; dy++)
                {
                    for (int dx = 0; dx < dividePixles; dx++)
                    {
                        int px = x * dividePixles + dx;
                        int py = y * dividePixles + dy;

                        if (px < copyTexture.width && py < copyTexture.height)
                        {
                            copyTexture.SetPixel(px, py, pixelColor);
                        }
                    }
                }
            }
        }

        copyTexture.Apply();
        spriteRenderer.sprite = Sprite.Create(copyTexture,new Rect(0, 0, originalTexture.width, originalTexture.height),new Vector2(0.5f, 0.5f));
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

        rayCastHit = Physics2D.Raycast(cameraMain.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (rayCastHit.collider != null)
        {
            Debug.Log("Raycast hit: " + rayCastHit.collider.name);
            Debug.Log("mouse position: " + rayCastHit.point.x + ", " + rayCastHit.point.y);
        }
    }

    void OnMouseDrag()
    {
        if (rayCastHit.collider == null) return;

        // Convert world hit point to local position relative to the sprite
        Vector3 localPos = transform.InverseTransformPoint(rayCastHit.point);

        // Get sprite data
        Sprite sprite = spriteRenderer.sprite;
        Rect rect = sprite.rect;
        float ppu = sprite.pixelsPerUnit;

        // Convert from local space (Unity units) to texture pixel space
        float pixelX = (localPos.x * ppu) + sprite.pivot.x + rect.x;
        float pixelY = (localPos.y * ppu) + sprite.pivot.y + rect.y;

        // Clamp to valid texture bounds
        int texX = Mathf.Clamp(Mathf.RoundToInt(pixelX), 0, copyTexture.width - 1);
        int texY = Mathf.Clamp(Mathf.RoundToInt(pixelY), 0, copyTexture.height - 1);

        // Restore original pixels under the mouse
        ReturnOriginalPixels(texX, texY);

        Debug.Log($"Mouse (world): {rayCastHit.point} → (texture): ({texX}, {texY})");
    }

    void ReturnOriginalPixels(int posX, int posY)
    {
        for (int y = -Mathf.RoundToInt(mouseDragRadius); y <= mouseDragRadius; y++)
        {
            for (int x = -Mathf.RoundToInt(mouseDragRadius); x <= mouseDragRadius; x++)
            {
                int px = posX + x;
                int py = posY + y;

                if (px < 0 || py < 0 || px >= originalTexture.width || py >= originalTexture.height)
                    continue;

                Color currentColor = copyTexture.GetPixel(px, py);
                Color originalColor = originalTexture.GetPixel(px, py);

                // blend them smoothly
                Color fadedColor = Color.Lerp(currentColor, originalColor, fadeSpeed);

                copyTexture.SetPixel(px, py, fadedColor);
            }
        }
        copyTexture.Apply();
    }

    void SendValueToHost()//waiting for parameter..
    {

    }
}
