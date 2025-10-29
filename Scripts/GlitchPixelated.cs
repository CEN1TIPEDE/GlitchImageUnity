using System.Collections.Generic;
using UnityEngine;

public class GlitchPixelated : MonoBehaviour
{
    //**Make sure the image type in "Advanced" is set "Read/Write Enabled" in window project setting**

    public SpriteRenderer spriteRenderer;

    private Texture2D originalTexture;
    private Texture2D copyTexture;

    public int dividePixles = 10;
    public float mouseDragRadius = 10f;
    public float fadeSpeed = 0.15f;

    private Camera cameraMain;
    private RaycastHit2D rayCastHit;

    public bool isFirstPicture = false;
    public bool isSecondPicture = false;

    // Track changed pixels for syncing between pictures
    public HashSet<Vector2Int> changedPixels = new HashSet<Vector2Int>();

    // Reference to the other picture (set in Inspector)
    public GlitchPixelated otherPicture;

    //var for pixel changed int pos x and y >> pixleated
    //var for float alpha : 0f-1f changed (on the right picture alpha(left) - alpha(right)) >> pixelated

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalTexture = spriteRenderer.sprite.texture;
        copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
        copyTexture.SetPixels(originalTexture.GetPixels());
        copyTexture.Apply();

        cameraMain = Camera.main;
    }

    void ReturnOriginalTexture()
    {
        spriteRenderer.sprite = Sprite.Create(originalTexture,
            new Rect(0, 0, originalTexture.width, originalTexture.height),
            new Vector2(0.5f, 0.5f));
    }

    void PixelatedMethod()
    {
        for (int y = 0; y < copyTexture.height; y += dividePixles)
        {
            for (int x = 0; x < copyTexture.width; x += dividePixles)
            {
                float r = Random.Range(0.2f, 0.6f);
                float g = Random.Range(0f, 0.3f);
                float b = Random.Range(0.4f, 1f);
                Color pixelColor = new Color(r, g, b);

                for (int dy = 0; dy < dividePixles; dy++)
                {
                    for (int dx = 0; dx < dividePixles; dx++)
                    {
                        int px = x + dx;
                        int py = y + dy;
                        if (px < copyTexture.width && py < copyTexture.height)
                        {
                            copyTexture.SetPixel(px, py, pixelColor);
                            changedPixels.Add(new Vector2Int(px, py));
                        }
                    }
                }
            }
        }
        copyTexture.Apply();
        UpdateSprite();
    }

    void UpdateSprite()
    {
        spriteRenderer.sprite = Sprite.Create(copyTexture,
            new Rect(0, 0, copyTexture.width, copyTexture.height),
            new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            PixelatedMethod();

        if (Input.GetKeyDown(KeyCode.E))
            ReturnOriginalTexture();

        rayCastHit = Physics2D.Raycast(cameraMain.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (otherPicture != null)
                CombineFromOther(otherPicture);
        }
    }

    void OnMouseDrag()
    {
        if (rayCastHit.collider == null) return;

        Vector3 localPos = transform.InverseTransformPoint(rayCastHit.point);
        Sprite sprite = spriteRenderer.sprite;
        Rect rect = sprite.rect;
        float ppu = sprite.pixelsPerUnit;

        float pixelX = (localPos.x * ppu) + sprite.pivot.x + rect.x;
        float pixelY = (localPos.y * ppu) + sprite.pivot.y + rect.y;

        int texX = Mathf.Clamp(Mathf.RoundToInt(pixelX), 0, copyTexture.width - 1);
        int texY = Mathf.Clamp(Mathf.RoundToInt(pixelY), 0, copyTexture.height - 1);

        ReturnOriginalPixels(texX, texY);
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
                Color fadedColor = Color.Lerp(currentColor, originalColor, fadeSpeed);

                copyTexture.SetPixel(px, py, fadedColor);
                changedPixels.Add(new Vector2Int(px, py));
            }
        }
        copyTexture.Apply();
    }

    //combine only changed pixels from the other picture
    public void CombineFromOther(GlitchPixelated other)
    {
        if (other == null || other.copyTexture == null) return;

        int w = copyTexture.width;
        int h = copyTexture.height;

        // Merge only pixels changed in either texture
        HashSet<Vector2Int> allChanged = new HashSet<Vector2Int>(changedPixels);
        foreach (var pos in other.changedPixels)
            allChanged.Add(pos);

        foreach (Vector2Int pos in allChanged)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= w || pos.y >= h)
                continue;

            Color myColor = copyTexture.GetPixel(pos.x, pos.y);
            Color otherColor = other.copyTexture.GetPixel(pos.x, pos.y);

            // If both changed, blend them
            Color finalColor;
            if (changedPixels.Contains(pos) && other.changedPixels.Contains(pos))
                finalColor = Color.Lerp(myColor, otherColor, 0.5f);
            else if (other.changedPixels.Contains(pos))
                finalColor = otherColor;
            else
                finalColor = myColor;

            copyTexture.SetPixel(pos.x, pos.y, finalColor);
        }

        copyTexture.Apply();
        UpdateSprite();

        // Merge change history so next combine keeps track correctly
        changedPixels = allChanged;
        other.changedPixels.Clear();
    }

}
