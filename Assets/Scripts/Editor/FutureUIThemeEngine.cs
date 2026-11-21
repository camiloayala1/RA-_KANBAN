using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class FutureUIThemeEngine
{
    private const string UI_SPRITE_DIR = "Assets/Images/UI";

    // Paleta de Colores 2027 FUTURE
    public static readonly Color ColorBgVoid = new Color(0.04f, 0.06f, 0.09f, 1f);          // #0A0F17
    public static readonly Color ColorCardGlass = new Color(0.06f, 0.09f, 0.15f, 0.98f);     // #0F1726 (Obsidian Cyber Glass)
    public static readonly Color ColorCardElevated = new Color(0.10f, 0.15f, 0.24f, 0.98f);  // #1A263D
    public static readonly Color ColorInputBg = new Color(0.08f, 0.12f, 0.19f, 0.98f);       // #141F30 (Solid High-Tech Input)
    public static readonly Color ColorCyanNeon = new Color(0.00f, 0.95f, 1.00f, 1f);        // #00F2FE
    public static readonly Color ColorIndigoGlow = new Color(0.39f, 0.40f, 0.95f, 1f);      // #6366F1
    public static readonly Color ColorEmeraldNeon = new Color(0.06f, 0.82f, 0.55f, 1f);     // #10D08C
    public static readonly Color ColorAmberNeon = new Color(0.98f, 0.65f, 0.12f, 1f);       // #F9A61F
    public static readonly Color ColorCoralDanger = new Color(0.95f, 0.26f, 0.26f, 1f);      // #F24242
    public static readonly Color ColorTextWhite = new Color(0.97f, 0.98f, 1.00f, 1f);
    public static readonly Color ColorTextMuted = new Color(0.58f, 0.65f, 0.74f, 1f);
    public static readonly Color ColorBorderSubtle = new Color(0.14f, 0.28f, 0.44f, 0.85f);
    public static readonly Color ColorBorderCyan = new Color(0.00f, 0.95f, 1.00f, 0.55f);

    public static void OpenLoginScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Login.unity", OpenSceneMode.Single);
    }

    public static void OpenMainMenuScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
    }

    public static void OpenARScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/ARScene.unity", OpenSceneMode.Single);
    }

    public static void TestShowModalCrear()
    {
        OpenMainMenuScene();
        var canvas = GameObject.Find("Canvas");
        var modal = canvas != null ? canvas.transform.Find("Modal_Crear")?.gameObject : null;
        if (modal != null) modal.SetActive(true);
    }

    public static void TestShowModalOpciones()
    {
        OpenMainMenuScene();
        var canvas = GameObject.Find("Canvas");
        var modal = canvas != null ? canvas.transform.Find("Modal_OpcionesTablero")?.gameObject : null;
        if (modal != null) modal.SetActive(true);
    }

    public static void TestShowModalEditar()
    {
        OpenMainMenuScene();
        var canvas = GameObject.Find("Canvas");
        var modal = canvas != null ? canvas.transform.Find("Modal_EditarTablero")?.gameObject : null;
        if (modal != null) modal.SetActive(true);
    }

    public static void TestShowModalEliminar()
    {
        OpenMainMenuScene();
        var canvas = GameObject.Find("Canvas");
        var modal = canvas != null ? canvas.transform.Find("Modal_ConfirmarEliminar")?.gameObject : null;
        if (modal != null) modal.SetActive(true);
    }

    public static void TestShowModalCrearNota()
    {
        OpenARScene();
        var hud = GameObject.Find("ScreenHUD");
        var modal = hud != null ? hud.transform.Find("ModalCrearNota")?.gameObject : null;
        if (modal != null) modal.SetActive(true);
    }

    public static void DiagnoseLoginScene()
    {
        var cam = Camera.main;
        Debug.Log($"Cam: {cam != null}, cullingMask={cam?.cullingMask}, clearFlags={cam?.clearFlags}");
        var canvas = Object.FindAnyObjectByType<Canvas>();
        Debug.Log($"Canvas: {canvas != null}, renderMode={canvas?.renderMode}, enabled={canvas?.enabled}");
        if (canvas != null)
        {
            PrintHierarchy(canvas.transform, "");
        }
    }

    private static void PrintHierarchy(Transform t, string indent)
    {
        var rt = t.GetComponent<RectTransform>();
        Debug.Log($"{indent}- {t.name} (active={t.gameObject.activeSelf}, layer={t.gameObject.layer}, size={rt?.rect.size}, anchoredPos={rt?.anchoredPosition})");
        foreach (Transform ch in t)
        {
            PrintHierarchy(ch, indent + "  ");
        }
    }

    [MenuItem("Tools/RA Kanban/Remasterizar Escena Login 2027")]
    public static void RemasterLoginScene()
    {
        GenerateProceduralSprites();
        AssetDatabase.Refresh();
        BuildFuturisticLoginScene();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[FutureUI] ¡Escena Login remasterizada al estilo 2027 FUTURE con éxito!");
    }

    [MenuItem("Tools/RA Kanban/Aplicar Tema 2027 FUTURE a Todo")]
    public static void ExecuteFullOverhaul()
    {
        Debug.Log("[FutureUI] Iniciando transformación visual 2027 FUTURE...");
        GenerateProceduralSprites();
        AssetDatabase.Refresh();

        BuildFuturisticLoginScene();
        ApplyThemeToMainMenu();
        ApplyThemeToARScene();
        ApplyThemeToPrefabs();
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[FutureUI] ¡Transformación 2027 FUTURE completada con éxito!");
    }

    #region 1. GENERACIÓN DE SPRITES PROCEDURALES (9-SLICE)

    public static void GenerateProceduralSprites()
    {
        if (!Directory.Exists(UI_SPRITE_DIR))
            Directory.CreateDirectory(UI_SPRITE_DIR);

        CreateRoundedRectSprite("Frame_Glass_Dark.png", 128, 128, 22, 
            ColorCardGlass, ColorBorderSubtle, 2f, new Vector4(24, 24, 24, 24));

        CreateRoundedRectSprite("Frame_Glass_Cyan.png", 128, 128, 22, 
            ColorCardGlass, ColorBorderCyan, 2.5f, new Vector4(24, 24, 24, 24));

        CreateCapsuleSprite("Button_Capsule_Cyan.png", 128, 64, 
            ColorCyanNeon, new Color(0.05f, 0.65f, 0.95f, 1f), ColorTextWhite, 0f, new Vector4(30, 20, 30, 20));

        CreateCapsuleSprite("Button_Capsule_Glass.png", 128, 64, 
            new Color(0.08f, 0.12f, 0.18f, 0.85f), new Color(0.05f, 0.08f, 0.13f, 0.90f), ColorBorderCyan, 1.5f, new Vector4(30, 20, 30, 20));

        CreateCapsuleSprite("Button_Capsule_Danger.png", 128, 64, 
            new Color(0.85f, 0.20f, 0.20f, 1f), new Color(0.65f, 0.10f, 0.10f, 1f), ColorTextWhite, 0f, new Vector4(30, 20, 30, 20));

        CreateRoundedRectSprite("Input_Field_Glass.png", 128, 64, 14, 
            ColorInputBg, new Color(0.00f, 0.85f, 1.00f, 0.35f), 2f, new Vector4(16, 16, 16, 16));

        CreatePostItSprite("Card_PostIt_Future.png", 128, 128, 18, 
            ColorCardElevated, ColorBorderSubtle, 1.5f, new Vector4(20, 20, 20, 20));

        CreateHoloRingSprite("Ring_Trash_Holo.png", 128, 128, ColorCoralDanger);

        CreateRadialAuraSprite("Aura_Ambient_Cyan.png", 512, 512, ColorCyanNeon, 0.38f);
    }

    private static void CreateRoundedRectSprite(string fileName, int width, int height, float radius, Color fillColor, Color borderColor, float borderWidth, Vector4 border9Slice)
    {
        string fullPath = Path.Combine(UI_SPRITE_DIR, fileName);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = DistanceToRoundedRect(x + 0.5f, y + 0.5f, width, height, radius);
                if (dist <= -borderWidth)
                {
                    tex.SetPixel(x, y, fillColor);
                }
                else if (dist <= 0f)
                {
                    float t = Mathf.Clamp01((dist + borderWidth) / Mathf.Max(borderWidth, 0.001f));
                    Color c = Color.Lerp(fillColor, borderColor, t);
                    tex.SetPixel(x, y, c);
                }
                else if (dist < 1.2f)
                {
                    float alpha = Mathf.Clamp01(1f - (dist / 1.2f));
                    Color c = borderColor;
                    c.a *= alpha;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(fullPath, border9Slice);
    }

    private static void CreateCapsuleSprite(string fileName, int width, int height, Color topColor, Color bottomColor, Color borderColor, float borderWidth, Vector4 border9Slice)
    {
        string fullPath = Path.Combine(UI_SPRITE_DIR, fileName);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float radius = (height - 2) * 0.5f;

        for (int y = 0; y < height; y++)
        {
            float verticalT = (float)y / height;
            Color fillGrad = Color.Lerp(bottomColor, topColor, verticalT);

            for (int x = 0; x < width; x++)
            {
                float dist = DistanceToRoundedRect(x + 0.5f, y + 0.5f, width, height, radius);
                if (dist <= -borderWidth)
                {
                    tex.SetPixel(x, y, fillGrad);
                }
                else if (dist <= 0f)
                {
                    if (borderWidth > 0f)
                    {
                        float t = Mathf.Clamp01((dist + borderWidth) / borderWidth);
                        Color c = Color.Lerp(fillGrad, borderColor, t);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillGrad);
                    }
                }
                else if (dist < 1.2f)
                {
                    float alpha = Mathf.Clamp01(1f - (dist / 1.2f));
                    Color c = borderWidth > 0f ? borderColor : fillGrad;
                    c.a *= alpha;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(fullPath, border9Slice);
    }

    private static void CreatePostItSprite(string fileName, int width, int height, float radius, Color fillColor, Color borderColor, float borderWidth, Vector4 border9Slice)
    {
        string fullPath = Path.Combine(UI_SPRITE_DIR, fileName);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = DistanceToRoundedRect(x + 0.5f, y + 0.5f, width, height, radius);
                if (dist <= -borderWidth)
                {
                    if (y >= height - 16)
                    {
                        tex.SetPixel(x, y, ColorCyanNeon);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                }
                else if (dist <= 0f)
                {
                    Color b = (y >= height - 16) ? ColorCyanNeon : borderColor;
                    tex.SetPixel(x, y, b);
                }
                else if (dist < 1.2f)
                {
                    float alpha = Mathf.Clamp01(1f - (dist / 1.2f));
                    Color c = (y >= height - 16) ? ColorCyanNeon : borderColor;
                    c.a *= alpha;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(fullPath, border9Slice);
    }

    private static void CreateHoloRingSprite(string fileName, int width, int height, Color ringColor)
    {
        string fullPath = Path.Combine(UI_SPRITE_DIR, fileName);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        float radiusOuter = width * 0.46f;
        float radiusInner = width * 0.38f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (dist >= radiusInner && dist <= radiusOuter)
                {
                    tex.SetPixel(x, y, ringColor);
                }
                else if (dist < radiusInner && dist >= radiusInner - 2f)
                {
                    float a = Mathf.Clamp01((dist - (radiusInner - 2f)) / 2f);
                    Color c = ringColor;
                    c.a *= a;
                    tex.SetPixel(x, y, c);
                }
                else if (dist > radiusOuter && dist <= radiusOuter + 2f)
                {
                    float a = Mathf.Clamp01(1f - ((dist - radiusOuter) / 2f));
                    Color c = ringColor;
                    c.a *= a;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(fullPath, Vector4.zero);
    }

    private static void CreateRadialAuraSprite(string fileName, int width, int height, Color auraColor, float maxAlpha)
    {
        string fullPath = Path.Combine(UI_SPRITE_DIR, fileName);
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        float radius = width * 0.48f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (dist < radius)
                {
                    float norm = dist / radius;
                    float falloff = Mathf.Pow(1f - norm, 2.4f);
                    Color c = auraColor;
                    c.a = falloff * maxAlpha;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(fullPath, Vector4.zero);
    }

    private static float DistanceToRoundedRect(float x, float y, float width, float height, float radius)
    {
        float dx = Mathf.Max(radius - x, 0f, x - (width - radius));
        float dy = Mathf.Max(radius - y, 0f, y - (height - radius));
        return Mathf.Sqrt(dx * dx + dy * dy) - radius;
    }

    private static void ConfigureSpriteImporter(string assetPath, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
    }

    public static Sprite LoadUISprite(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(UI_SPRITE_DIR, name));
    }

    public static void ExtractTransparentLogo()
    {
        string srcPath = "Assets/Images/LOGOARKAN.png";
        if (!File.Exists(srcPath))
            srcPath = "Assets/Images/LOGOARKAN 1.png";
        if (!File.Exists(srcPath)) return;

        var importer = AssetImporter.GetAtPath(srcPath) as TextureImporter;
        bool prevReadable = importer != null && importer.isReadable;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Texture2D src = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
        if (src == null) return;

        int w = src.width;
        int h = src.height;
        Color[] pixels = src.GetPixels();

        bool[] isOutside = new bool[w * h];
        Queue<int> queue = new Queue<int>();

        bool IsBg(Color c) => (c.r > 0.88f && c.g > 0.88f && c.b > 0.88f);

        for (int x = 0; x < w; x++)
        {
            int idxBot = x;
            int idxTop = (h - 1) * w + x;
            if (IsBg(pixels[idxBot]) && !isOutside[idxBot]) { isOutside[idxBot] = true; queue.Enqueue(idxBot); }
            if (IsBg(pixels[idxTop]) && !isOutside[idxTop]) { isOutside[idxTop] = true; queue.Enqueue(idxTop); }
        }
        for (int y = 0; y < h; y++)
        {
            int idxLeft = y * w;
            int idxRight = y * w + (w - 1);
            if (IsBg(pixels[idxLeft]) && !isOutside[idxLeft]) { isOutside[idxLeft] = true; queue.Enqueue(idxLeft); }
            if (IsBg(pixels[idxRight]) && !isOutside[idxRight]) { isOutside[idxRight] = true; queue.Enqueue(idxRight); }
        }

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            int curr = queue.Dequeue();
            int cx = curr % w;
            int cy = curr / w;

            for (int i = 0; i < 4; i++)
            {
                int nx = cx + dx[i];
                int ny = cy + dy[i];
                if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                {
                    int nIdx = ny * w + nx;
                    if (!isOutside[nIdx] && IsBg(pixels[nIdx]))
                    {
                        isOutside[nIdx] = true;
                        queue.Enqueue(nIdx);
                    }
                }
            }
        }

        // Extraer únicamente el emblema geométrico de las 3 columnas Kanban (y >= 280)
        int minX = w, maxX = 0, minY = h, maxY = 0;
        for (int y = 280; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int idx = y * w + x;
                if (!isOutside[idx])
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        int pad = 24;
        minX = Mathf.Max(0, minX - pad);
        maxX = Mathf.Min(w - 1, maxX + pad);
        minY = Mathf.Max(0, minY - pad);
        maxY = Mathf.Min(h - 1, maxY + pad);

        int cropW = maxX - minX + 1;
        int cropH = maxY - minY + 1;

        Texture2D cleanTex = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);
        for (int y = 0; y < cropH; y++)
        {
            for (int x = 0; x < cropW; x++)
            {
                int sx = minX + x;
                int sy = minY + y;
                int sIdx = sy * w + sx;

                if (isOutside[sIdx] || sy < 280)
                {
                    cleanTex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    Color c = pixels[sIdx];
                    float bright = (c.r + c.g + c.b) / 3f;
                    if (bright > 0.88f)
                    {
                        bool neighborIsOut = false;
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = sx + dx[i];
                            int ny = sy + dy[i];
                            if (nx >= 0 && nx < w && ny >= 0 && ny < h && isOutside[ny * w + nx])
                            {
                                neighborIsOut = true;
                                break;
                            }
                        }
                        if (neighborIsOut)
                        {
                            c.a = Mathf.Clamp01((1f - bright) / 0.12f);
                        }
                    }
                    cleanTex.SetPixel(x, y, c);
                }
            }
        }

        cleanTex.Apply();
        string outPath = Path.Combine(UI_SPRITE_DIR, "Logo_Brand_Transparent.png");
        File.WriteAllBytes(outPath, cleanTex.EncodeToPNG());
        Object.DestroyImmediate(cleanTex);

        if (importer != null)
        {
            importer.isReadable = prevReadable;
            importer.SaveAndReimport();
        }

        AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(outPath, Vector4.zero);
        Debug.Log("[FutureUI] Logo transparente de marca extraído con éxito.");
    }

    public static void CreateSplashBrandHero()
    {
        string emblemPath = Path.Combine(UI_SPRITE_DIR, "Logo_Brand_Transparent.png");
        if (!File.Exists(emblemPath))
            ExtractTransparentLogo();

        var importer = AssetImporter.GetAtPath(emblemPath) as TextureImporter;
        bool prevReadable = importer != null && importer.isReadable;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Texture2D emblem = AssetDatabase.LoadAssetAtPath<Texture2D>(emblemPath);
        if (emblem == null) return;

        int canvasW = 1024;
        int canvasH = 1024;
        Texture2D splash = new Texture2D(canvasW, canvasH, TextureFormat.RGBA32, false);

        // 1. Fondo transparente con halo de resplandor holográfico sutil
        Vector2 center = new Vector2(canvasW * 0.5f, canvasH * 0.52f);
        float glowRadius = 400f;

        for (int y = 0; y < canvasH; y++)
        {
            for (int x = 0; x < canvasW; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= glowRadius)
                {
                    float t = Mathf.Clamp01(1f - (dist / glowRadius));
                    float glowAlpha = Mathf.Pow(t, 2.2f) * 0.22f;
                    splash.SetPixel(x, y, new Color(0f, 0.95f, 1f, glowAlpha));
                }
                else
                {
                    splash.SetPixel(x, y, Color.clear);
                }
            }
        }

        // 2. Escalar y centrar el emblema dentro del lienzo
        float targetWidth = 640f;
        float scale = targetWidth / emblem.width;
        int dw = Mathf.RoundToInt(emblem.width * scale);
        int dh = Mathf.RoundToInt(emblem.height * scale);

        int startX = (canvasW - dw) / 2;
        int startY = (canvasH - dh) / 2 + 30;

        for (int y = 0; y < dh; y++)
        {
            float v = (float)y / dh;
            int sy = Mathf.Clamp(Mathf.RoundToInt(v * emblem.height), 0, emblem.height - 1);

            for (int x = 0; x < dw; x++)
            {
                float u = (float)x / dw;
                int sx = Mathf.Clamp(Mathf.RoundToInt(u * emblem.width), 0, emblem.width - 1);

                Color ec = emblem.GetPixel(sx, sy);
                if (ec.a > 0.01f)
                {
                    int px = startX + x;
                    int py = startY + y;
                    if (px >= 0 && px < canvasW && py >= 0 && py < canvasH)
                    {
                        Color dst = splash.GetPixel(px, py);
                        Color blended = Color.Lerp(dst, ec, ec.a);
                        blended.a = Mathf.Clamp01(dst.a + ec.a);
                        splash.SetPixel(px, py, blended);
                    }
                }
            }
        }

        splash.Apply();
        string splashPath = Path.Combine(UI_SPRITE_DIR, "Splash_Brand_Hero.png");
        File.WriteAllBytes(splashPath, splash.EncodeToPNG());
        Object.DestroyImmediate(splash);

        if (importer != null)
        {
            importer.isReadable = prevReadable;
            importer.SaveAndReimport();
        }

        AssetDatabase.ImportAsset(splashPath, ImportAssetOptions.ForceUpdate);
        ConfigureSpriteImporter(splashPath, Vector4.zero);
        Debug.Log("[FutureUI] Splash_Brand_Hero.png generado con éxito.");
    }

    [MenuItem("Tools/RA Kanban/Configurar Splash Screen Elegante 2027")]
    public static void ConfigureBrandSplashScreen()
    {
        ExtractTransparentLogo();
        CreateSplashBrandHero();

        Sprite splashHero = AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(UI_SPRITE_DIR, "Splash_Brand_Hero.png"));
        if (splashHero == null)
            splashHero = AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(UI_SPRITE_DIR, "Logo_Brand_Transparent.png"));

        PlayerSettings.SplashScreen.show = true;
        PlayerSettings.SplashScreen.backgroundColor = ColorBgVoid; // #040812 Deep Space
        PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Dolly;
        PlayerSettings.SplashScreen.drawMode = PlayerSettings.SplashScreen.DrawMode.AllSequential;
        PlayerSettings.SplashScreen.overlayOpacity = 1f;
        PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;

        if (splashHero != null)
        {
            var logo = PlayerSettings.SplashScreenLogo.Create(2.5f, splashHero);
            PlayerSettings.SplashScreen.logos = new PlayerSettings.SplashScreenLogo[] { logo };
            Debug.Log("[FutureUI] PlayerSettings.SplashScreen actualizado con logo de marca heroico.");
        }

        BuildFuturisticLoginScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    #endregion

    #region 2. CONSTRUCCIÓN DE LA ESCENA LOGIN 2027 FUTURE DEDICADA

    public static void BuildFuturisticLoginScene()
    {
        string scenePath = "Assets/Scenes/Login.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Sprite frameGlassCyan = LoadUISprite("Frame_Glass_Cyan.png");
        Sprite btnCyan = LoadUISprite("Button_Capsule_Cyan.png");
        Sprite btnGlass = LoadUISprite("Button_Capsule_Glass.png");
        Sprite inputGlass = LoadUISprite("Input_Field_Glass.png");
        Sprite auraHolo = LoadUISprite("Aura_Ambient_Cyan.png");
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/UI/Logo_Brand_Transparent.png");
        if (logoSprite == null)
            logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/LOGOARKAN.png");

        // 1. Configurar Cámara Principal
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ColorBgVoid;
            cam.orthographic = true;
            cam.orthographicSize = 960f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
        }

        // 2. Asegurar Objeto FirebaseManager en escena (si no existe)
        GameObject fbGo = GameObject.Find("FirebaseManager");
        if (fbGo == null)
        {
            fbGo = new GameObject("FirebaseManager");
            fbGo.AddComponent<FirebaseManager>();
            fbGo.AddComponent<FirebaseAuthREST>();
        }

        // 3. Limpiar o configurar Canvas
        GameObject canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>();
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0f; // Match Width (1080 exacto para cualquier celular vertical)

        // 4. Limpiar hijos viejos de Canvas para reconstrucción limpia
        while (canvasGo.transform.childCount > 0)
        {
            Object.DestroyImmediate(canvasGo.transform.GetChild(0).gameObject);
        }

        // 5. Fondo Deep Space (Full Screen)
        GameObject bgGo = CreateUIObject("Fondo_Espacio", canvasGo.transform);
        SetStretch(bgGo.GetComponent<RectTransform>());
        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.color = ColorBgVoid;
        bgImg.raycastTarget = false;

        // 6. Halo Holográfico Ambiental Central (Profundidad Visual 2027)
        if (auraHolo != null)
        {
            GameObject auraGo = CreateUIObject("Fondo_AuraHolo", canvasGo.transform);
            RectTransform aRt = auraGo.GetComponent<RectTransform>();
            aRt.anchorMin = new Vector2(0.5f, 0.5f);
            aRt.anchorMax = new Vector2(0.5f, 0.5f);
            aRt.pivot = new Vector2(0.5f, 0.5f);
            aRt.sizeDelta = new Vector2(1080f, 1400f);
            aRt.anchoredPosition = new Vector2(0f, 50f);
            Image aImg = auraGo.AddComponent<Image>();
            aImg.sprite = auraHolo;
            aImg.color = new Color(0.00f, 0.95f, 1.00f, 0.28f);
            aImg.raycastTarget = false;
        }

        // 7. Contenedor Raíz Adaptativo (Elevación Suave al abrir Teclado y Resguardo de Notch)
        GameObject rootGo = CreateUIObject("Content_Root", canvasGo.transform);
        RectTransform rootRt = rootGo.GetComponent<RectTransform>();
        SetStretch(rootRt);

        SafeAreaFitter saf = rootGo.GetComponent<SafeAreaFitter>();
        if (saf == null) saf = rootGo.AddComponent<SafeAreaFitter>();
        saf.mode = SafeAreaFitter.SafeAreaMode.TopAndBottom;
        saf.extraTopPadding = 25f;
        saf.extraBottomPadding = 20f;

        // --- A. ENCABEZADO SUPERIOR DE MARCA (Brand Hero) ---
        GameObject headerGo = CreateUIObject("Header_Brand", rootGo.transform);
        RectTransform hRt = headerGo.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0.5f, 1f);
        hRt.anchorMax = new Vector2(0.5f, 1f);
        hRt.pivot = new Vector2(0.5f, 1f);
        hRt.anchoredPosition = new Vector2(0f, -65f);
        hRt.sizeDelta = new Vector2(960f, 340f);

        // Logo (Emblema Geométrico Puro)
        if (logoSprite != null)
        {
            GameObject logoGo = CreateUIObject("Logo", headerGo.transform);
            RectTransform lRt = logoGo.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.5f, 1f);
            lRt.anchorMax = new Vector2(0.5f, 1f);
            lRt.pivot = new Vector2(0.5f, 1f);
            lRt.anchoredPosition = new Vector2(0f, 0f);
            lRt.sizeDelta = new Vector2(210f, 210f);
            Image lImg = logoGo.AddComponent<Image>();
            lImg.sprite = logoSprite;
            lImg.preserveAspect = true;
            lImg.raycastTarget = false;
        }

        // Título de la Terminal
        GameObject titleGo = CreateUIObject("Titulo_Terminal", headerGo.transform);
        RectTransform tRt = titleGo.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.5f, 1f);
        tRt.anchorMax = new Vector2(0.5f, 1f);
        tRt.pivot = new Vector2(0.5f, 1f);
        tRt.anchoredPosition = new Vector2(0f, -225f);
        tRt.sizeDelta = new Vector2(960f, 100f);
        TextMeshProUGUI titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "<b><color=#00F2FE>RA</color> // <color=#FFFFFF>KANBAN</color></b>\n<size=42%><color=#64748B>SPATIAL AR // CYBERNETIC TERMINAL 2027</color></size>";
        titleTmp.fontSize = 52;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.raycastTarget = false;

        // --- B. TARJETA CYBER-GLASS CENTRAL (Formulario) ---
        GameObject cardGo = CreateUIObject("Card_Terminal", rootGo.transform);
        RectTransform cardRt = cardGo.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 1f);
        cardRt.anchorMax = new Vector2(0.5f, 1f);
        cardRt.pivot = new Vector2(0.5f, 1f);
        cardRt.anchoredPosition = new Vector2(0f, -430f);
        cardRt.sizeDelta = new Vector2(960f, 1180f);

        Image cardImg = cardGo.AddComponent<Image>();
        cardImg.sprite = frameGlassCyan;
        cardImg.type = Image.Type.Sliced;
        cardImg.color = Color.white;
        cardImg.raycastTarget = false;

        // --- BARRA DE TABS DE NAVEGACIÓN ---
        GameObject tabsContainer = CreateUIObject("Tabs_Nav", cardGo.transform);
        RectTransform tabsRt = tabsContainer.GetComponent<RectTransform>();
        tabsRt.anchorMin = new Vector2(0.5f, 1f);
        tabsRt.anchorMax = new Vector2(0.5f, 1f);
        tabsRt.pivot = new Vector2(0.5f, 1f);
        tabsRt.anchoredPosition = new Vector2(0f, -35f);
        tabsRt.sizeDelta = new Vector2(880f, 96f);
        Image tabsBg = tabsContainer.AddComponent<Image>();
        tabsBg.sprite = inputGlass;
        tabsBg.type = Image.Type.Sliced;
        tabsBg.color = new Color(0.05f, 0.08f, 0.13f, 0.95f);
        tabsBg.raycastTarget = false;

        // Tab Login
        GameObject tabLoginGo = CreateUIObject("Tab_Login", tabsContainer.transform);
        RectTransform tabLRt = tabLoginGo.GetComponent<RectTransform>();
        tabLRt.anchoredPosition = new Vector2(-220f, 0f);
        tabLRt.sizeDelta = new Vector2(430f, 80f);
        Image tabLImg = tabLoginGo.AddComponent<Image>();
        tabLImg.sprite = btnCyan;
        tabLImg.type = Image.Type.Sliced;
        Button tabLBtn = tabLoginGo.AddComponent<Button>();

        GameObject tabLTxtGo = CreateUIObject("Text", tabLoginGo.transform);
        SetStretch(tabLTxtGo.GetComponent<RectTransform>());
        TextMeshProUGUI tabLTmp = tabLTxtGo.AddComponent<TextMeshProUGUI>();
        tabLTmp.text = "<b>INICIAR SESIÓN</b>";
        tabLTmp.fontSize = 28;
        tabLTmp.alignment = TextAlignmentOptions.Center;
        tabLTmp.color = ColorCyanNeon;

        // Tab Registro
        GameObject tabRegGo = CreateUIObject("Tab_Registro", tabsContainer.transform);
        RectTransform tabRRt = tabRegGo.GetComponent<RectTransform>();
        tabRRt.anchoredPosition = new Vector2(220f, 0f);
        tabRRt.sizeDelta = new Vector2(430f, 80f);
        Image tabRImg = tabRegGo.AddComponent<Image>();
        tabRImg.sprite = btnGlass;
        tabRImg.type = Image.Type.Sliced;
        Button tabRBtn = tabRegGo.AddComponent<Button>();

        GameObject tabRTxtGo = CreateUIObject("Text", tabRegGo.transform);
        SetStretch(tabRTxtGo.GetComponent<RectTransform>());
        TextMeshProUGUI tabRTmp = tabRTxtGo.AddComponent<TextMeshProUGUI>();
        tabRTmp.text = "REGISTRARSE";
        tabRTmp.fontSize = 28;
        tabRTmp.alignment = TextAlignmentOptions.Center;
        tabRTmp.color = ColorTextMuted;

        // --- SUBPANEL LOGIN ---
        GameObject subLoginGo = CreateUIObject("SubPanel_Login", cardGo.transform);
        RectTransform subLRt = subLoginGo.GetComponent<RectTransform>();
        subLRt.anchorMin = new Vector2(0.5f, 1f);
        subLRt.anchorMax = new Vector2(0.5f, 1f);
        subLRt.pivot = new Vector2(0.5f, 1f);
        subLRt.anchoredPosition = new Vector2(0f, -155f);
        subLRt.sizeDelta = new Vector2(880f, 980f);

        // Prompt
        GameObject promptLGo = CreateUIObject("Prompt_Login", subLoginGo.transform);
        RectTransform pLRt = promptLGo.GetComponent<RectTransform>();
        pLRt.anchorMin = new Vector2(0.5f, 1f);
        pLRt.anchorMax = new Vector2(0.5f, 1f);
        pLRt.pivot = new Vector2(0.5f, 0.5f);
        pLRt.anchoredPosition = new Vector2(0f, -25f);
        pLRt.sizeDelta = new Vector2(880f, 36f);
        TextMeshProUGUI pLTmp = promptLGo.AddComponent<TextMeshProUGUI>();
        pLTmp.text = "<color=#64748B>// ACCESO SEGURO DE OPERADOR AR</color>";
        pLTmp.fontSize = 24;
        pLTmp.alignment = TextAlignmentOptions.Center;

        // Inputs Login (anchos 880px, altos 120px para dedos en móvil)
        TMP_InputField inLoginEmail = CreateInputField("Input_Email", subLoginGo.transform, new Vector2(0f, -115f), new Vector2(880f, 120f), inputGlass, "Correo electrónico institucional...");
        TMP_InputField inLoginPass = CreateInputField("Input_Password", subLoginGo.transform, new Vector2(0f, -270f), new Vector2(880f, 120f), inputGlass, "Contraseña de acceso...", true);

        // Error Login
        GameObject errLGo = CreateUIObject("Texto_Error", subLoginGo.transform);
        RectTransform errLRt = errLGo.GetComponent<RectTransform>();
        errLRt.anchorMin = new Vector2(0.5f, 1f);
        errLRt.anchorMax = new Vector2(0.5f, 1f);
        errLRt.pivot = new Vector2(0.5f, 0.5f);
        errLRt.anchoredPosition = new Vector2(0f, -370f);
        errLRt.sizeDelta = new Vector2(880f, 45f);
        TextMeshProUGUI errLTmp = errLGo.AddComponent<TextMeshProUGUI>();
        errLTmp.color = ColorCoralDanger;
        errLTmp.fontSize = 24;
        errLTmp.alignment = TextAlignmentOptions.Center;
        errLTmp.text = "";

        // Botón Iniciar Sesión (Alto 120px, táctil y visible)
        GameObject btnEntrarGo = CreateUIObject("Btn_Ingresar", subLoginGo.transform);
        RectTransform bERt = btnEntrarGo.GetComponent<RectTransform>();
        bERt.anchorMin = new Vector2(0.5f, 1f);
        bERt.anchorMax = new Vector2(0.5f, 1f);
        bERt.pivot = new Vector2(0.5f, 0.5f);
        bERt.anchoredPosition = new Vector2(0f, -475f);
        bERt.sizeDelta = new Vector2(880f, 120f);
        Image bEImg = btnEntrarGo.AddComponent<Image>();
        bEImg.sprite = btnCyan;
        bEImg.type = Image.Type.Sliced;
        Button bEBtn = btnEntrarGo.AddComponent<Button>();

        GameObject bETxtGo = CreateUIObject("Text", btnEntrarGo.transform);
        SetStretch(bETxtGo.GetComponent<RectTransform>());
        TextMeshProUGUI bETmp = bETxtGo.AddComponent<TextMeshProUGUI>();
        bETmp.text = "<b>INGRESAR AL ESPACIO AR</b>";
        bETmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
        bETmp.fontSize = 32;
        bETmp.alignment = TextAlignmentOptions.Center;

        // Switch a Registro
        GameObject switchRegGo = CreateUIObject("Btn_IrARegistro", subLoginGo.transform);
        RectTransform sRRt = switchRegGo.GetComponent<RectTransform>();
        sRRt.anchorMin = new Vector2(0.5f, 1f);
        sRRt.anchorMax = new Vector2(0.5f, 1f);
        sRRt.pivot = new Vector2(0.5f, 0.5f);
        sRRt.anchoredPosition = new Vector2(0f, -625f);
        sRRt.sizeDelta = new Vector2(880f, 60f);
        Button sRBtn = switchRegGo.AddComponent<Button>();
        TextMeshProUGUI sRTmp = switchRegGo.AddComponent<TextMeshProUGUI>();
        sRTmp.text = "<color=#64748B>¿No tienes cuenta de operador?</color> <color=#00F2FE><b><u>Regístrate aquí</u></b></color>";
        sRTmp.fontSize = 28;
        sRTmp.alignment = TextAlignmentOptions.Center;

        // --- SUBPANEL REGISTRO ---
        GameObject subRegGo = CreateUIObject("SubPanel_Registro", cardGo.transform);
        RectTransform subRRt = subRegGo.GetComponent<RectTransform>();
        subRRt.anchorMin = new Vector2(0.5f, 1f);
        subRRt.anchorMax = new Vector2(0.5f, 1f);
        subRRt.pivot = new Vector2(0.5f, 1f);
        subRRt.anchoredPosition = new Vector2(0f, -155f);
        subRRt.sizeDelta = new Vector2(880f, 980f);
        subRegGo.SetActive(false); // Inactivo por defecto

        // Prompt
        GameObject promptRGo = CreateUIObject("Prompt_Registro", subRegGo.transform);
        RectTransform pRRt = promptRGo.GetComponent<RectTransform>();
        pRRt.anchorMin = new Vector2(0.5f, 1f);
        pRRt.anchorMax = new Vector2(0.5f, 1f);
        pRRt.pivot = new Vector2(0.5f, 0.5f);
        pRRt.anchoredPosition = new Vector2(0f, -20f);
        pRRt.sizeDelta = new Vector2(880f, 36f);
        TextMeshProUGUI pRTmp = promptRGo.AddComponent<TextMeshProUGUI>();
        pRTmp.text = "<color=#64748B>// REGISTRO DE NUEVO OPERADOR EN PLATAFORMA</color>";
        pRTmp.fontSize = 24;
        pRTmp.alignment = TextAlignmentOptions.Center;

        // Inputs Registro
        TMP_InputField inRegEmail = CreateInputField("Input_Email", subRegGo.transform, new Vector2(0f, -95f), new Vector2(880f, 110f), inputGlass, "Nuevo correo electrónico...");
        TMP_InputField inRegPass = CreateInputField("Input_Password", subRegGo.transform, new Vector2(0f, -235f), new Vector2(880f, 110f), inputGlass, "Contraseña (mínimo 6 caracteres)...", true);
        TMP_InputField inRegConfirm = CreateInputField("Input_ConfirmPassword", subRegGo.transform, new Vector2(0f, -375f), new Vector2(880f, 110f), inputGlass, "Repetir contraseña...", true);

        // Error / Exito Registro
        GameObject errRGo = CreateUIObject("Texto_Error", subRegGo.transform);
        RectTransform errRRt = errRGo.GetComponent<RectTransform>();
        errRRt.anchorMin = new Vector2(0.5f, 1f);
        errRRt.anchorMax = new Vector2(0.5f, 1f);
        errRRt.pivot = new Vector2(0.5f, 0.5f);
        errRRt.anchoredPosition = new Vector2(0f, -460f);
        errRRt.sizeDelta = new Vector2(880f, 45f);
        TextMeshProUGUI errRTmp = errRGo.AddComponent<TextMeshProUGUI>();
        errRTmp.color = ColorCoralDanger;
        errRTmp.fontSize = 24;
        errRTmp.alignment = TextAlignmentOptions.Center;
        errRTmp.text = "";

        // Botón Registrarse
        GameObject btnRegGo = CreateUIObject("Btn_Registrar", subRegGo.transform);
        RectTransform bRRt = btnRegGo.GetComponent<RectTransform>();
        bRRt.anchorMin = new Vector2(0.5f, 1f);
        bRRt.anchorMax = new Vector2(0.5f, 1f);
        bRRt.pivot = new Vector2(0.5f, 0.5f);
        bRRt.anchoredPosition = new Vector2(0f, -560f);
        bRRt.sizeDelta = new Vector2(880f, 120f);
        Image bRImg = btnRegGo.AddComponent<Image>();
        bRImg.sprite = btnCyan;
        bRImg.type = Image.Type.Sliced;
        Button bRBtn = btnRegGo.AddComponent<Button>();

        GameObject bRTxtGo = CreateUIObject("Text", btnRegGo.transform);
        SetStretch(bRTxtGo.GetComponent<RectTransform>());
        TextMeshProUGUI bRTmp = bRTxtGo.AddComponent<TextMeshProUGUI>();
        bRTmp.text = "<b>CREAR CUENTA // REGISTRAR</b>";
        bRTmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
        bRTmp.fontSize = 32;
        bRTmp.alignment = TextAlignmentOptions.Center;

        // Switch a Login
        GameObject switchLogGo = CreateUIObject("Btn_IrALogin", subRegGo.transform);
        RectTransform sLRt = switchLogGo.GetComponent<RectTransform>();
        sLRt.anchorMin = new Vector2(0.5f, 1f);
        sLRt.anchorMax = new Vector2(0.5f, 1f);
        sLRt.pivot = new Vector2(0.5f, 0.5f);
        sLRt.anchoredPosition = new Vector2(0f, -700f);
        sLRt.sizeDelta = new Vector2(880f, 60f);
        Button sLBtn = switchLogGo.AddComponent<Button>();
        TextMeshProUGUI sLTmp = switchLogGo.AddComponent<TextMeshProUGUI>();
        sLTmp.text = "<color=#64748B>¿Ya tienes credenciales?</color> <color=#00F2FE><b><u>Inicia sesión</u></b></color>";
        sLTmp.fontSize = 28;
        sLTmp.alignment = TextAlignmentOptions.Center;

        // --- C. PIE DE PÁGINA HUD SEGURIDAD ---
        GameObject footerGo = CreateUIObject("Footer_HUD", rootGo.transform);
        RectTransform footRt = footerGo.GetComponent<RectTransform>();
        footRt.anchorMin = new Vector2(0.5f, 0f);
        footRt.anchorMax = new Vector2(0.5f, 0f);
        footRt.pivot = new Vector2(0.5f, 0f);
        footRt.anchoredPosition = new Vector2(0f, 45f);
        footRt.sizeDelta = new Vector2(960f, 65f);
        TextMeshProUGUI footTmp = footerGo.AddComponent<TextMeshProUGUI>();
        footTmp.text = "<color=#00F2FE>●</color> <color=#475569>SISTEMA AR OPERATIVO // CONEXIÓN SEGURA FIREBASE</color>\n<size=80%><color=#334155>RA KANBAN OS 2027 // PROTOCOLO AUTÓNOMO AES-256</color></size>";
        footTmp.fontSize = 20;
        footTmp.alignment = TextAlignmentOptions.Center;

        // --- D. CONFIGURAR GESTOR ADAPTATIVO DE TECLADO MÓVIL ---
        MobileAuthLayoutManager layoutMgr = rootGo.AddComponent<MobileAuthLayoutManager>();
        layoutMgr.containerToShift = rootRt;
        layoutMgr.keyboardShiftAmount = 380f;
        layoutMgr.inputFields = new TMP_InputField[] { inLoginEmail, inLoginPass, inRegEmail, inRegPass, inRegConfirm };
        layoutMgr.interactiveButtons = new Button[] { tabLBtn, tabRBtn, bEBtn, bRBtn, sRBtn, sLBtn };

        // Asegurar EventSystem compatible con New Input System
        EnsureEventSystemWithInputSystem();

        // --- E. ENLAZAR COMPONENTE AUTHCONTROLLER ---
        GameObject authManagerGo = GameObject.Find("AuthManager");
        if (authManagerGo == null)
            authManagerGo = new GameObject("AuthManager");

        AuthController authController = authManagerGo.GetComponent<AuthController>();
        if (authController == null)
            authController = authManagerGo.AddComponent<AuthController>();

        SerializedObject so = new SerializedObject(authController);
        so.FindProperty("subPanelLogin").objectReferenceValue = subLoginGo;
        so.FindProperty("subPanelRegistro").objectReferenceValue = subRegGo;
        so.FindProperty("tabBtnLogin").objectReferenceValue = tabLBtn;
        so.FindProperty("tabBtnRegistro").objectReferenceValue = tabRBtn;
        so.FindProperty("tabTextoLogin").objectReferenceValue = tabLTmp;
        so.FindProperty("tabTextoRegistro").objectReferenceValue = tabRTmp;
        so.FindProperty("tabFondoLogin").objectReferenceValue = tabLImg;
        so.FindProperty("tabFondoRegistro").objectReferenceValue = tabRImg;
        so.FindProperty("inputLoginEmail").objectReferenceValue = inLoginEmail;
        so.FindProperty("inputLoginPassword").objectReferenceValue = inLoginPass;
        so.FindProperty("txtErrorLogin").objectReferenceValue = errLTmp;
        so.FindProperty("btnSubmitLogin").objectReferenceValue = bEBtn;
        so.FindProperty("btnIrARegistroAbajo").objectReferenceValue = sRBtn;
        so.FindProperty("inputRegEmail").objectReferenceValue = inRegEmail;
        so.FindProperty("inputRegPassword").objectReferenceValue = inRegPass;
        so.FindProperty("inputRegConfirmPassword").objectReferenceValue = inRegConfirm;
        so.FindProperty("txtErrorRegistro").objectReferenceValue = errRTmp;
        so.FindProperty("btnSubmitRegistro").objectReferenceValue = bRBtn;
        so.FindProperty("btnIrALoginAbajo").objectReferenceValue = sLBtn;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FutureUI] Login.unity reconstruida con distribución equilibrada y soporte de teclado móvil.");
    }

    public static void EnsureEventSystemWithInputSystem()
    {
        var esGo = GameObject.Find("EventSystem");
        if (esGo == null)
            esGo = new GameObject("EventSystem", typeof(EventSystem));

        var standalone = esGo.GetComponent<StandaloneInputModule>();
        if (standalone != null)
            Object.DestroyImmediate(standalone);

        var inputModule = esGo.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        inputModule.AssignDefaultActions();
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = 5; // Layer UI
        return go;
    }

    private static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static TMP_InputField CreateInputField(string name, Transform parent, Vector2 pos, Vector2 size, Sprite bgSprite, string placeholderText, bool isPassword = false)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = bgSprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        TMP_InputField inputField = go.AddComponent<TMP_InputField>();

        // Text Area
        GameObject textArea = CreateUIObject("Text Area", go.transform);
        RectTransform taRt = textArea.GetComponent<RectTransform>();
        taRt.anchorMin = Vector2.zero;
        taRt.anchorMax = Vector2.one;
        taRt.offsetMin = new Vector2(24f, 10f);
        taRt.offsetMax = new Vector2(-24f, -10f);
        textArea.AddComponent<RectMask2D>();

        // Placeholder
        GameObject phGo = CreateUIObject("Placeholder", textArea.transform);
        SetStretch(phGo.GetComponent<RectTransform>());
        TextMeshProUGUI phTmp = phGo.AddComponent<TextMeshProUGUI>();
        phTmp.text = placeholderText;
        phTmp.fontSize = 30;
        phTmp.fontStyle = FontStyles.Italic;
        phTmp.color = new Color(0.58f, 0.70f, 0.85f, 0.90f);
        phTmp.alignment = TextAlignmentOptions.MidlineLeft;
        phTmp.raycastTarget = false;

        // Text
        GameObject textGo = CreateUIObject("Text", textArea.transform);
        SetStretch(textGo.GetComponent<RectTransform>());
        TextMeshProUGUI textTmp = textGo.AddComponent<TextMeshProUGUI>();
        textTmp.fontSize = 34;
        textTmp.fontStyle = FontStyles.Bold;
        textTmp.color = ColorTextWhite;
        textTmp.alignment = TextAlignmentOptions.MidlineLeft;
        textTmp.raycastTarget = false;

        inputField.textViewport = taRt;
        inputField.textComponent = textTmp;
        inputField.placeholder = phTmp;

        if (isPassword)
        {
            inputField.contentType = TMP_InputField.ContentType.Password;
            inputField.inputType = TMP_InputField.InputType.Password;
        }

        return inputField;
    }

    public static void UpdateBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Login.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/ARScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[FutureUI] Build Settings actualizadas: [0] Login, [1] MainMenu, [2] ARScene.");
    }

    #endregion

    #region 3. APLICAR TEMA A MAINMENU.UNITY

    public static void ApplyThemeToMainMenu()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Sprite frameGlassDark = LoadUISprite("Frame_Glass_Dark.png");
        Sprite frameGlassCyan = LoadUISprite("Frame_Glass_Cyan.png");
        Sprite btnCyan = LoadUISprite("Button_Capsule_Cyan.png");
        Sprite btnGlass = LoadUISprite("Button_Capsule_Glass.png");
        Sprite btnDanger = LoadUISprite("Button_Capsule_Danger.png");
        Sprite inputGlass = LoadUISprite("Input_Field_Glass.png");

        // 1. Configurar Fondo General y Adaptación a Notch (Safe Area)
        GameObject fondoGo = GameObject.Find("Canvas/Fondo");
        if (fondoGo != null)
        {
            Image fondoImg = fondoGo.GetComponent<Image>();
            if (fondoImg != null)
            {
                fondoImg.color = ColorBgVoid;
            }

            SafeAreaFitter saf = fondoGo.GetComponent<SafeAreaFitter>();
            if (saf == null) saf = fondoGo.AddComponent<SafeAreaFitter>();
            saf.mode = SafeAreaFitter.SafeAreaMode.TopAndBottom;
            saf.extraTopPadding = 25f;
            saf.extraBottomPadding = 25f;
        }

        // 2. Eliminar completamente el panel de login viejo en MainMenu (ahora reside en su escena dedicada Login.unity)
        GameObject panelLoginViejo = GameObject.Find("Canvas/Panel_LoginGoogle");
        if (panelLoginViejo != null)
        {
            Object.DestroyImmediate(panelLoginViejo);
            Debug.Log("[FutureUI] Panel_LoginGoogle eliminado definitivamente de MainMenu.unity.");
        }

        // 3. Configurar Canvas Root para Móviles
        GameObject canvasGo = GameObject.Find("Canvas");
        if (canvasGo != null)
        {
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0f; // Match width
            }
        }

        // 4. Limpiar Canvases anidados redundantes en los modales para unificar el raycasting
        string[] modalNames = new string[] { "Modal_Crear", "Modal_OpcionesTablero", "Modal_EditarTablero", "Modal_ConfirmarEliminar" };
        foreach (var mName in modalNames)
        {
            GameObject mGo = FindModal(mName);
            if (mGo != null)
            {
                var gr = mGo.GetComponent<GraphicRaycaster>();
                if (gr != null) Object.DestroyImmediate(gr);
                var cs = mGo.GetComponent<CanvasScaler>();
                if (cs != null) Object.DestroyImmediate(cs);
                var c = mGo.GetComponent<Canvas>();
                if (c != null) Object.DestroyImmediate(c);
            }
        }

        // 5. Configurar Título Principal (Más grande y posicionado de forma armónica)
        GameObject tituloAppGo = GameObject.Find("Canvas/Fondo/Titulo_App");
        if (tituloAppGo != null)
        {
            RectTransform tRt = tituloAppGo.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.5f, 1f);
            tRt.anchorMax = new Vector2(0.5f, 1f);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.anchoredPosition = new Vector2(0f, -210f);
            tRt.sizeDelta = new Vector2(960f, 150f);

            TextMeshProUGUI tmp = tituloAppGo.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "<b><color=#00F2FE>RA</color> // <color=#FFFFFF>KANBAN</color></b>\n<size=38%><color=#64748B>SPATIAL WORKSPACE OS 2027</color></size>";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 78;
            }
        }

        // 5.1 Estilizar Texto de Usuario / Operador Conectado
        GameObject userGo = GameObject.Find("Canvas/Fondo/Textuser_name");
        if (userGo != null)
        {
            RectTransform uRt = userGo.GetComponent<RectTransform>();
            uRt.anchorMin = new Vector2(0.5f, 1f);
            uRt.anchorMax = new Vector2(0.5f, 1f);
            uRt.pivot = new Vector2(0.5f, 1f);
            uRt.anchoredPosition = new Vector2(0f, -390f);
            uRt.sizeDelta = new Vector2(960f, 65f);

            TextMeshProUGUI uTmp = userGo.GetComponent<TextMeshProUGUI>();
            if (uTmp != null)
            {
                uTmp.fontSize = 34;
                uTmp.alignment = TextAlignmentOptions.Center;
                uTmp.color = ColorTextWhite;
            }
        }

        // 6. Estilizar Botón Nuevo Tablero
        GameObject btnNuevoGo = GameObject.Find("Canvas/Fondo/Btn_NuevoTablero");
        Button btnNuevo = null;
        if (btnNuevoGo != null)
        {
            RectTransform bnRt = btnNuevoGo.GetComponent<RectTransform>();
            bnRt.anchorMin = new Vector2(0.5f, 1f);
            bnRt.anchorMax = new Vector2(0.5f, 1f);
            bnRt.pivot = new Vector2(0.5f, 1f);
            bnRt.anchoredPosition = new Vector2(0f, -490f);
            bnRt.sizeDelta = new Vector2(880f, 130f);

            btnNuevo = btnNuevoGo.GetComponent<Button>();
            Image btnImg = btnNuevoGo.GetComponent<Image>();
            if (btnImg != null && btnCyan != null)
            {
                btnImg.sprite = btnCyan;
                btnImg.type = Image.Type.Sliced;
                btnImg.color = Color.white;
            }

            TextMeshProUGUI tmp = btnNuevoGo.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "<b>+ INICIALIZAR TABLERO</b>";
                tmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
                tmp.fontSize = 34;
            }
        }

        // 7. Estilizar Botón Cerrar Sesión (Esquina superior derecha protegida de barra de estado y notch)
        GameObject btnCerrarSesionGo = GameObject.Find("Canvas/Fondo/Btn_CerrarSesion");
        if (btnCerrarSesionGo != null)
        {
            RectTransform csRt = btnCerrarSesionGo.GetComponent<RectTransform>();
            csRt.anchorMin = new Vector2(1f, 1f);
            csRt.anchorMax = new Vector2(1f, 1f);
            csRt.pivot = new Vector2(1f, 1f);
            csRt.anchoredPosition = new Vector2(-45f, -125f);
            csRt.sizeDelta = new Vector2(210f, 85f);

            Image btnImg = btnCerrarSesionGo.GetComponent<Image>();
            if (btnImg != null && btnGlass != null)
            {
                btnImg.sprite = btnGlass;
                btnImg.type = Image.Type.Sliced;
                btnImg.color = Color.white;
            }

            TextMeshProUGUI tmp = btnCerrarSesionGo.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "<b>SALIR</b>";
                tmp.color = ColorCyanNeon;
                tmp.fontSize = 26;
            }
        }

        // 8. Estilizar ScrollView y Lista de Tableros (No tan arriba, distribuida hacia abajo)
        GameObject scrollGo = GameObject.Find("Canvas/Fondo/ListaTableros_Scroll");
        if (scrollGo != null)
        {
            RectTransform sRt = scrollGo.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0f);
            sRt.anchorMax = new Vector2(0.5f, 1f);
            sRt.pivot = new Vector2(0.5f, 0.5f);
            sRt.offsetMin = new Vector2(-460f, 60f);
            sRt.offsetMax = new Vector2(460f, -660f);

            Image scrollImg = scrollGo.GetComponent<Image>();
            if (scrollImg != null && frameGlassDark != null)
            {
                scrollImg.sprite = frameGlassDark;
                scrollImg.type = Image.Type.Sliced;
                scrollImg.color = new Color(0.07f, 0.10f, 0.16f, 0.70f);
            }

            // Ajustar espaciado interno del Content
            Transform contentT = scrollGo.transform.Find("Viewport/Content");
            if (contentT != null)
            {
                var vlg = contentT.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    vlg.spacing = 25f;
                    vlg.padding = new RectOffset(20, 20, 25, 25);
                }
            }
        }

        // 9. Estilizar Modales
        EstilizarModalCrearTablero(frameGlassCyan, btnCyan, btnGlass, inputGlass);
        EstilizarModalOpciones(frameGlassCyan, btnCyan, btnGlass, btnDanger);
        EstilizarModalEditar(frameGlassDark, btnCyan, btnGlass, inputGlass);
        EstilizarModalEliminar(frameGlassDark, btnDanger, btnGlass);

        // 10. Re-enlazar campos de MainMenuController para que todas las funcionalidades funcionen al 100%
        ReenlazarMainMenuController(btnNuevo);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FutureUI] MainMenu.unity actualizado con éxito.");
    }

    private static GameObject FindModal(string modalName)
    {
        Transform canvasT = GameObject.Find("Canvas")?.transform;
        if (canvasT != null)
        {
            Transform m = canvasT.Find(modalName);
            if (m != null) return m.gameObject;
        }
        return GameObject.Find("Canvas/" + modalName);
    }

    private static void EstilizarModalCrearTablero(Sprite frameGlass, Sprite btnPrimary, Sprite btnSecondary, Sprite inputGlass)
    {
        GameObject modal = FindModal("Modal_Crear");
        if (modal == null) return;

        // Overlay a pantalla completa que bloquea toques debajo
        RectTransform mRt = modal.GetComponent<RectTransform>();
        SetStretch(mRt);

        Image modalImg = modal.GetComponent<Image>();
        if (modalImg == null) modalImg = modal.AddComponent<Image>();
        modalImg.color = new Color(0.02f, 0.04f, 0.07f, 0.88f);
        modalImg.raycastTarget = true;

        // Comprobar o crear Caja_Crear centrada
        Transform caja = modal.transform.Find("Caja_Crear");
        if (caja == null)
        {
            GameObject cajaGo = CreateUIObject("Caja_Crear", modal.transform);
            caja = cajaGo.transform;
            RectTransform cRt = cajaGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(960f, 640f);

            Image cImg = cajaGo.AddComponent<Image>();
            cImg.sprite = frameGlass;
            cImg.type = Image.Type.Sliced;
            cImg.color = Color.white;

            // Mover hijos a Caja_Crear
            var chList = new System.Collections.Generic.List<Transform>();
            foreach (Transform ch in modal.transform)
            {
                if (ch != caja) chList.Add(ch);
            }
            foreach (var ch in chList)
            {
                ch.SetParent(caja, false);
            }
        }
        else
        {
            Image cImg = caja.GetComponent<Image>();
            if (cImg != null && frameGlass != null)
            {
                cImg.sprite = frameGlass;
                cImg.type = Image.Type.Sliced;
                cImg.color = Color.white;
            }
        }

        // Título del modal
        foreach (var tmp in caja.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (tmp.gameObject.name.Contains("Titulo") || tmp.text.Contains("Nombre del Tablero"))
            {
                tmp.text = "<b><color=#00F2FE>//</color> NUEVO TABLERO KANBAN</b>";
                tmp.fontSize = 38;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = ColorTextWhite;
                RectTransform tRt = tmp.GetComponent<RectTransform>();
                if (tRt != null)
                {
                    tRt.anchoredPosition = new Vector2(0f, 200f);
                    tRt.sizeDelta = new Vector2(860f, 70f);
                }
            }
        }

        Transform inputNombre = caja.Find("Input_NombreTablero");
        if (inputNombre != null)
        {
            RectTransform inRt = inputNombre.GetComponent<RectTransform>();
            inRt.anchoredPosition = new Vector2(0f, 20f);
            inRt.sizeDelta = new Vector2(840f, 115f);

            if (inputGlass != null)
            {
                Image img = inputNombre.GetComponent<Image>();
                if (img != null) { img.sprite = inputGlass; img.type = Image.Type.Sliced; }
            }

            TMP_InputField inField = inputNombre.GetComponent<TMP_InputField>();
            if (inField != null)
            {
                if (inField.textComponent != null)
                {
                    inField.textComponent.fontSize = 30;
                    inField.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                }
                if (inField.placeholder != null)
                {
                    var phTmp = inField.placeholder.GetComponent<TextMeshProUGUI>();
                    if (phTmp != null)
                    {
                        phTmp.text = "Nombre del tablero (ej. Sprint 2027)...";
                        phTmp.fontSize = 28;
                        phTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    }
                }
            }
        }

        Transform btnConfirmar = caja.Find("Btn_Confirmar");
        if (btnConfirmar != null && btnPrimary != null)
        {
            RectTransform bRt = btnConfirmar.GetComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(-220f, -180f);
            bRt.sizeDelta = new Vector2(390f, 105f);

            Image img = btnConfirmar.GetComponent<Image>();
            if (img != null) { img.sprite = btnPrimary; img.type = Image.Type.Sliced; }
            TextMeshProUGUI tmp = btnConfirmar.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "<b>CREAR TABLERO</b>";
                tmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
                tmp.fontSize = 28;
            }
        }

        Transform btnCancelar = caja.Find("Btn_Cancelar");
        if (btnCancelar != null && btnSecondary != null)
        {
            RectTransform bRt = btnCancelar.GetComponent<RectTransform>();
            bRt.anchoredPosition = new Vector2(220f, -180f);
            bRt.sizeDelta = new Vector2(390f, 105f);

            Image img = btnCancelar.GetComponent<Image>();
            if (img != null) { img.sprite = btnSecondary; img.type = Image.Type.Sliced; }
            TextMeshProUGUI tmp = btnCancelar.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "CANCELAR";
                tmp.color = ColorTextWhite;
                tmp.fontSize = 26;
            }
        }

        // Adaptabilidad al Teclado Móvil (Elevación dinámica para que los botones nunca queden tapados)
        ModalKeyboardShift shift = modal.GetComponent<ModalKeyboardShift>();
        if (shift == null) shift = modal.AddComponent<ModalKeyboardShift>();
        shift.containerToShift = caja.GetComponent<RectTransform>();
        shift.shiftAmount = 400f;
        shift.shiftSmoothTime = 0.16f;
        shift.restY = 0f;

        modal.SetActive(false);
    }

    private static void EstilizarModalOpciones(Sprite frameGlass, Sprite btnPrimary, Sprite btnSecondary, Sprite btnDanger)
    {
        GameObject modal = FindModal("Modal_OpcionesTablero");
        if (modal == null) return;

        SetStretch(modal.GetComponent<RectTransform>());

        Image modalImg = modal.GetComponent<Image>();
        if (modalImg == null) modalImg = modal.AddComponent<Image>();
        modalImg.color = new Color(0.02f, 0.04f, 0.07f, 0.88f);
        modalImg.raycastTarget = true;

        Transform caja = modal.transform.Find("Caja_Opciones");
        if (caja != null)
        {
            RectTransform cRt = caja.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(960f, 800f);

            Image cajaImg = caja.GetComponent<Image>();
            if (cajaImg != null && frameGlass != null) 
            { 
                cajaImg.sprite = frameGlass; 
                cajaImg.type = Image.Type.Sliced; 
                cajaImg.color = Color.white; 
            }

            foreach (var t in caja.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (t.gameObject.name.Contains("Titulo") || t.text.Contains("Pepito"))
                {
                    t.color = ColorCyanNeon;
                    t.fontSize = 40;
                    t.alignment = TextAlignmentOptions.Center;
                    RectTransform tRt = t.GetComponent<RectTransform>();
                    if (tRt != null)
                    {
                        tRt.anchoredPosition = new Vector2(0f, 280f);
                        tRt.sizeDelta = new Vector2(860f, 70f);
                    }
                }
            }

            Transform btnAbrir = caja.Find("Btn_OpcionAbrir");
            if (btnAbrir != null && btnPrimary != null)
            {
                RectTransform bRt = btnAbrir.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(0f, 85f);
                bRt.sizeDelta = new Vector2(840f, 115f);

                Image img = btnAbrir.GetComponent<Image>();
                if (img != null) { img.sprite = btnPrimary; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnAbrir.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<b>ABRIR EN REALIDAD AUMENTADA</b>";
                    tmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
                    tmp.fontSize = 30;
                }
            }

            Transform btnEditar = caja.Find("Btn_OpcionEditar");
            if (btnEditar != null && btnSecondary != null)
            {
                RectTransform bRt = btnEditar.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(0f, -50f);
                bRt.sizeDelta = new Vector2(840f, 105f);

                Image img = btnEditar.GetComponent<Image>();
                if (img != null) { img.sprite = btnSecondary; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnEditar.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "EDITAR NOMBRE";
                    tmp.color = ColorTextWhite;
                    tmp.fontSize = 26;
                }
            }

            Transform btnEliminar = caja.Find("Btn_OpcionEliminar");
            if (btnEliminar != null && btnDanger != null)
            {
                RectTransform bRt = btnEliminar.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(0f, -175f);
                bRt.sizeDelta = new Vector2(840f, 105f);

                Image img = btnEliminar.GetComponent<Image>();
                if (img != null) { img.sprite = btnDanger; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnEliminar.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "ELIMINAR TABLERO";
                    tmp.color = ColorTextWhite;
                    tmp.fontSize = 26;
                }
            }

            Transform btnCerrar = caja.Find("Btn_CerrarOpciones");
            if (btnCerrar != null && btnSecondary != null)
            {
                RectTransform bRt = btnCerrar.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(0f, -295f);
                bRt.sizeDelta = new Vector2(840f, 90f);

                Image img = btnCerrar.GetComponent<Image>();
                if (img != null) { img.sprite = btnSecondary; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnCerrar.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "VOLVER";
                    tmp.color = ColorTextMuted;
                    tmp.fontSize = 24;
                }
            }
        }

        // Adaptabilidad al Teclado Móvil (Elevación dinámica para que los botones nunca queden tapados)
        ModalKeyboardShift shift = modal.GetComponent<ModalKeyboardShift>();
        if (shift == null) shift = modal.AddComponent<ModalKeyboardShift>();
        shift.containerToShift = caja.GetComponent<RectTransform>();
        shift.shiftAmount = 400f;
        shift.shiftSmoothTime = 0.16f;
        shift.restY = 0f;

        modal.SetActive(false);
    }

    private static void EstilizarModalEditar(Sprite frameGlass, Sprite btnPrimary, Sprite btnSecondary, Sprite inputGlass)
    {
        GameObject modal = FindModal("Modal_EditarTablero");
        if (modal == null) return;

        SetStretch(modal.GetComponent<RectTransform>());

        Image modalImg = modal.GetComponent<Image>();
        if (modalImg == null) modalImg = modal.AddComponent<Image>();
        modalImg.color = new Color(0.02f, 0.04f, 0.07f, 0.88f);
        modalImg.raycastTarget = true;

        Transform caja = modal.transform.Find("Caja_Editar");
        if (caja != null)
        {
            RectTransform cRt = caja.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(960f, 640f);

            Image cajaImg = caja.GetComponent<Image>();
            if (cajaImg != null && frameGlass != null) { cajaImg.sprite = frameGlass; cajaImg.type = Image.Type.Sliced; }

            foreach (var tmp in caja.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (tmp.gameObject.name.Contains("Titulo") || tmp.text.Contains("Editar"))
                {
                    tmp.text = "<b><color=#00F2FE>//</color> EDITAR NOMBRE DEL TABLERO</b>";
                    tmp.fontSize = 36;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = ColorTextWhite;
                    RectTransform tRt = tmp.GetComponent<RectTransform>();
                    if (tRt != null)
                    {
                        tRt.anchoredPosition = new Vector2(0f, 200f);
                        tRt.sizeDelta = new Vector2(860f, 70f);
                    }
                }
            }

            Transform input = caja.Find("Input_EditarNombre");
            if (input != null)
            {
                RectTransform inRt = input.GetComponent<RectTransform>();
                inRt.anchoredPosition = new Vector2(0f, 20f);
                inRt.sizeDelta = new Vector2(840f, 115f);

                if (inputGlass != null)
                {
                    Image img = input.GetComponent<Image>();
                    if (img != null) { img.sprite = inputGlass; img.type = Image.Type.Sliced; }
                }

                TMP_InputField inField = input.GetComponent<TMP_InputField>();
                if (inField != null)
                {
                    if (inField.textComponent != null)
                    {
                        inField.textComponent.fontSize = 30;
                        inField.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    }
                }
            }

            Transform btnConfirmar = caja.Find("Btn_ConfirmarEdicion");
            if (btnConfirmar != null && btnPrimary != null)
            {
                RectTransform bRt = btnConfirmar.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(-220f, -180f);
                bRt.sizeDelta = new Vector2(390f, 105f);

                Image img = btnConfirmar.GetComponent<Image>();
                if (img != null) { img.sprite = btnPrimary; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnConfirmar.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<b>GUARDAR CAMBIOS</b>";
                    tmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
                    tmp.fontSize = 28;
                }
            }

            Transform btnCancelar = caja.Find("Btn_CancelarEdicion");
            if (btnCancelar != null && btnSecondary != null)
            {
                RectTransform bRt = btnCancelar.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(220f, -180f);
                bRt.sizeDelta = new Vector2(390f, 105f);

                Image img = btnCancelar.GetComponent<Image>();
                if (img != null) { img.sprite = btnSecondary; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnCancelar.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "CANCELAR";
                    tmp.color = ColorTextWhite;
                    tmp.fontSize = 26;
                }
            }
        }

        // Adaptabilidad al Teclado Móvil (Elevación dinámica para que los botones nunca queden tapados)
        ModalKeyboardShift shift = modal.GetComponent<ModalKeyboardShift>();
        if (shift == null) shift = modal.AddComponent<ModalKeyboardShift>();
        shift.containerToShift = caja.GetComponent<RectTransform>();
        shift.shiftAmount = 400f;
        shift.shiftSmoothTime = 0.16f;
        shift.restY = 0f;

        modal.SetActive(false);
    }

    private static void EstilizarModalEliminar(Sprite frameGlass, Sprite btnDanger, Sprite btnSecondary)
    {
        GameObject modal = FindModal("Modal_ConfirmarEliminar");
        if (modal == null) return;

        SetStretch(modal.GetComponent<RectTransform>());

        Image modalImg = modal.GetComponent<Image>();
        if (modalImg == null) modalImg = modal.AddComponent<Image>();
        modalImg.color = new Color(0.02f, 0.04f, 0.07f, 0.88f);
        modalImg.raycastTarget = true;

        Transform caja = modal.transform.Find("Caja_Opciones");
        if (caja != null)
        {
            RectTransform cRt = caja.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(960f, 640f);

            Image cajaImg = caja.GetComponent<Image>();
            if (cajaImg != null && frameGlass != null) { cajaImg.sprite = frameGlass; cajaImg.type = Image.Type.Sliced; }

            foreach (var tmp in caja.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (tmp.gameObject.name.Contains("Titulo") || tmp.text.Contains("Eliminar") || tmp.text.Contains("ADVERTENCIA"))
                {
                    tmp.text = "<b><color=#F24242>//</color> CONFIRMAR ELIMINACIÓN</b>";
                    tmp.fontSize = 38;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = ColorCoralDanger;
                    RectTransform tRt = tmp.GetComponent<RectTransform>();
                    if (tRt != null)
                    {
                        tRt.anchoredPosition = new Vector2(0f, 200f);
                        tRt.sizeDelta = new Vector2(860f, 70f);
                    }
                }
                else if (tmp.gameObject.name.Contains("Advertencia") || tmp.text.Contains("seguro"))
                {
                    tmp.fontSize = 26;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = ColorTextWhite;
                    RectTransform tRt = tmp.GetComponent<RectTransform>();
                    if (tRt != null)
                    {
                        tRt.anchoredPosition = new Vector2(0f, 30f);
                        tRt.sizeDelta = new Vector2(840f, 150f);
                    }
                }
            }

            Transform btnSi = caja.Find("Btn_ConfirmarEliminarSi");
            if (btnSi != null && btnDanger != null)
            {
                RectTransform bRt = btnSi.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(-220f, -180f);
                bRt.sizeDelta = new Vector2(390f, 105f);

                Image img = btnSi.GetComponent<Image>();
                if (img != null) { img.sprite = btnDanger; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnSi.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) { tmp.text = "<b>SÍ, ELIMINAR</b>"; tmp.color = ColorTextWhite; tmp.fontSize = 28; }
            }

            Transform btnNo = caja.Find("Btn_ConfirmarEliminarNo");
            if (btnNo != null && btnSecondary != null)
            {
                RectTransform bRt = btnNo.GetComponent<RectTransform>();
                bRt.anchoredPosition = new Vector2(220f, -180f);
                bRt.sizeDelta = new Vector2(390f, 105f);

                Image img = btnNo.GetComponent<Image>();
                if (img != null) { img.sprite = btnSecondary; img.type = Image.Type.Sliced; }
                TextMeshProUGUI tmp = btnNo.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) { tmp.text = "CANCELAR"; tmp.color = ColorTextWhite; tmp.fontSize = 26; }
            }
        }

        // Adaptabilidad al Teclado Móvil (Elevación dinámica para que los botones nunca queden tapados)
        ModalKeyboardShift shift = modal.GetComponent<ModalKeyboardShift>();
        if (shift == null) shift = modal.AddComponent<ModalKeyboardShift>();
        shift.containerToShift = caja.GetComponent<RectTransform>();
        shift.shiftAmount = 400f;
        shift.shiftSmoothTime = 0.16f;
        shift.restY = 0f;

        modal.SetActive(false);
    }

    private static void ReenlazarMainMenuController(Button btnNuevoTablero)
    {
        MainMenuController mm = Object.FindAnyObjectByType<MainMenuController>();
        if (mm == null) return;

        SerializedObject so = new SerializedObject(mm);

        // Modal Crear
        GameObject mCrear = FindModal("Modal_Crear");
        if (mCrear != null)
        {
            so.FindProperty("modalCrear").objectReferenceValue = mCrear;
            Transform cCrear = mCrear.transform.Find("Caja_Crear");
            if (cCrear != null)
            {
                var inNom = cCrear.Find("Input_NombreTablero")?.GetComponent<TMP_InputField>();
                if (inNom != null) so.FindProperty("inputNombre").objectReferenceValue = inNom;

                var bConf = cCrear.Find("Btn_Confirmar")?.GetComponent<Button>();
                if (bConf != null)
                {
                    so.FindProperty("btnConfirmarCrear").objectReferenceValue = bConf;
                    SerializedObject bSo = new SerializedObject(bConf);
                    var pCalls = bSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                    if (pCalls != null && pCalls.arraySize > 0) { pCalls.ClearArray(); bSo.ApplyModifiedProperties(); }
                }

                var bCanc = cCrear.Find("Btn_Cancelar")?.GetComponent<Button>();
                if (bCanc != null)
                {
                    so.FindProperty("btnCancelarCrear").objectReferenceValue = bCanc;
                    SerializedObject bSo = new SerializedObject(bCanc);
                    var pCalls = bSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                    if (pCalls != null && pCalls.arraySize > 0) { pCalls.ClearArray(); bSo.ApplyModifiedProperties(); }
                }
            }
        }

        if (btnNuevoTablero != null)
        {
            so.FindProperty("btnNuevoTablero").objectReferenceValue = btnNuevoTablero;
            SerializedObject bSo = new SerializedObject(btnNuevoTablero);
            var pCalls = bSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            if (pCalls != null && pCalls.arraySize > 0) { pCalls.ClearArray(); bSo.ApplyModifiedProperties(); }
        }

        // Modal Opciones
        GameObject mOpc = FindModal("Modal_OpcionesTablero");
        if (mOpc != null)
        {
            so.FindProperty("modalOpciones").objectReferenceValue = mOpc;
            Transform cOpc = mOpc.transform.Find("Caja_Opciones");
            if (cOpc != null)
            {
                foreach (var t in cOpc.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (t.gameObject.name.Contains("Titulo") || t.text.Contains("Pepito"))
                        so.FindProperty("textoTituloTableroSeleccionado").objectReferenceValue = t;
                }
                var bAbrir = cOpc.Find("Btn_OpcionAbrir")?.GetComponent<Button>();
                if (bAbrir != null) so.FindProperty("btnOpcionAbrir").objectReferenceValue = bAbrir;

                var bEdit = cOpc.Find("Btn_OpcionEditar")?.GetComponent<Button>();
                if (bEdit != null) so.FindProperty("btnOpcionEditar").objectReferenceValue = bEdit;

                var bElim = cOpc.Find("Btn_OpcionEliminar")?.GetComponent<Button>();
                if (bElim != null) so.FindProperty("btnOpcionEliminar").objectReferenceValue = bElim;

                var bCerr = cOpc.Find("Btn_CerrarOpciones")?.GetComponent<Button>();
                if (bCerr != null) so.FindProperty("btnCerrarOpciones").objectReferenceValue = bCerr;
            }
        }

        // Modal Editar
        GameObject mEdit = FindModal("Modal_EditarTablero");
        if (mEdit != null)
        {
            so.FindProperty("modalEditar").objectReferenceValue = mEdit;
            Transform cEdit = mEdit.transform.Find("Caja_Editar");
            if (cEdit != null)
            {
                var inEdit = cEdit.Find("Input_EditarNombre")?.GetComponent<TMP_InputField>();
                if (inEdit != null) so.FindProperty("inputEditarNombre").objectReferenceValue = inEdit;

                var bConfEdit = cEdit.Find("Btn_ConfirmarEdicion")?.GetComponent<Button>();
                if (bConfEdit != null) so.FindProperty("btnConfirmarEdicion").objectReferenceValue = bConfEdit;

                var bCancEdit = cEdit.Find("Btn_CancelarEdicion")?.GetComponent<Button>();
                if (bCancEdit != null) so.FindProperty("btnCancelarEdicion").objectReferenceValue = bCancEdit;
            }
        }

        // Modal Confirmar Eliminar
        GameObject mElim = FindModal("Modal_ConfirmarEliminar");
        if (mElim != null)
        {
            so.FindProperty("modalConfirmarEliminar").objectReferenceValue = mElim;
            Transform cElim = mElim.transform.Find("Caja_Opciones");
            if (cElim != null)
            {
                foreach (var t in cElim.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (t.gameObject.name.Contains("Advertencia") || t.text.Contains("seguro"))
                        so.FindProperty("textoAdvertenciaEliminar").objectReferenceValue = t;
                }
                var bSi = cElim.Find("Btn_ConfirmarEliminarSi")?.GetComponent<Button>();
                if (bSi != null) so.FindProperty("btnConfirmarEliminarSi").objectReferenceValue = bSi;

                var bNo = cElim.Find("Btn_ConfirmarEliminarNo")?.GetComponent<Button>();
                if (bNo != null) so.FindProperty("btnConfirmarEliminarNo").objectReferenceValue = bNo;
            }
        }

        // Panel Login eliminado
        so.FindProperty("panelLogin").objectReferenceValue = null;

        so.ApplyModifiedProperties();
        Debug.Log("[FutureUI] MainMenuController re-enlazado con éxito.");
    }

    #endregion

    #region 4. APLICAR TEMA A ARSCENE.UNITY

    public static void ApplyThemeToARScene()
    {
        string scenePath = "Assets/Scenes/ARScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Sprite frameGlassCyan = LoadUISprite("Frame_Glass_Cyan.png");
        Sprite frameGlassDark = LoadUISprite("Frame_Glass_Dark.png");
        Sprite btnCyan = LoadUISprite("Button_Capsule_Cyan.png");
        Sprite btnGlass = LoadUISprite("Button_Capsule_Glass.png");
        Sprite btnDanger = LoadUISprite("Button_Capsule_Danger.png");
        Sprite inputGlass = LoadUISprite("Input_Field_Glass.png");
        Sprite ringTrash = LoadUISprite("Ring_Trash_Holo.png");

        GameObject hudGo = GameObject.Find("ScreenHUD");
        if (hudGo == null)
        {
            Debug.LogError("[FutureUI] No se encontró ScreenHUD en ARScene.");
            return;
        }
        Transform hudT = hudGo.transform;

        // 1. Configurar CanvasScaler y Adaptación Safe Area para Mobile Portrait
        CanvasScaler scaler = hudGo.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f; // Match width
        }

        SafeAreaFitter saf = hudGo.GetComponent<SafeAreaFitter>();
        if (saf == null) saf = hudGo.AddComponent<SafeAreaFitter>();
        saf.mode = SafeAreaFitter.SafeAreaMode.TopAndBottom;
        saf.extraTopPadding = 25f;
        saf.extraBottomPadding = 25f;

        // 2. Estilizar Panel de Instrucciones (Banner Flotante Holográfico)
        Transform fondoInstruccionesT = hudT.Find("Fondo_Instrucciones");
        if (fondoInstruccionesT != null)
        {
            RectTransform fiRt = fondoInstruccionesT.GetComponent<RectTransform>();
            fiRt.anchorMin = new Vector2(0.5f, 1f);
            fiRt.anchorMax = new Vector2(0.5f, 1f);
            fiRt.pivot = new Vector2(0.5f, 1f);
            fiRt.anchoredPosition = new Vector2(0f, -265f);
            fiRt.sizeDelta = new Vector2(920f, 100f);

            Image img = fondoInstruccionesT.GetComponent<Image>();
            if (img != null && frameGlassCyan != null)
            {
                img.sprite = frameGlassCyan;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            TextMeshProUGUI tmp = fondoInstruccionesT.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = ColorCyanNeon;
                tmp.fontSize = 26;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        // 3. Estilizar Botón Regresar al Menú (Top Left, Protegido de la barra de notificaciones y notch)
        Transform btnMenuT = hudT.Find("Btn_RegresarMenu");
        if (btnMenuT != null)
        {
            RectTransform bmRt = btnMenuT.GetComponent<RectTransform>();
            bmRt.anchorMin = new Vector2(0f, 1f);
            bmRt.anchorMax = new Vector2(0f, 1f);
            bmRt.pivot = new Vector2(0f, 1f);
            bmRt.anchoredPosition = new Vector2(45f, -135f);
            bmRt.sizeDelta = new Vector2(270f, 105f);

            Image img = btnMenuT.GetComponent<Image>();
            if (img != null && btnGlass != null)
            {
                img.sprite = btnGlass;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            TextMeshProUGUI tmp = btnMenuT.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "<b>← MENÚ</b>";
                tmp.color = ColorTextWhite;
                tmp.fontSize = 28;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        // 4. Estilizar Panel de Botones de Acción (Centrados simétricamente y debajo de la barra de estado)
        Transform panelBotonesT = hudT.Find("Panel_BotonesAccion");
        if (panelBotonesT != null)
        {
            Transform btnNueva = panelBotonesT.Find("Btn_NuevaNota");
            if (btnNueva != null && btnCyan != null)
            {
                RectTransform bnRt = btnNueva.GetComponent<RectTransform>();
                bnRt.anchorMin = new Vector2(0.5f, 1f);
                bnRt.anchorMax = new Vector2(0.5f, 1f);
                bnRt.pivot = new Vector2(1f, 1f);
                bnRt.anchoredPosition = new Vector2(-15f, -135f);
                bnRt.sizeDelta = new Vector2(270f, 105f);

                Image img = btnNueva.GetComponent<Image>();
                if (img != null) { img.sprite = btnCyan; img.type = Image.Type.Sliced; img.color = Color.white; }
                TextMeshProUGUI tmp = btnNueva.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<b>+ NOTA</b>";
                    tmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
                    tmp.fontSize = 30;
                    tmp.alignment = TextAlignmentOptions.Center;
                }
            }

            Transform btnReubicar = panelBotonesT.Find("Btn_Reubicar");
            if (btnReubicar != null && btnGlass != null)
            {
                RectTransform brRt = btnReubicar.GetComponent<RectTransform>();
                brRt.anchorMin = new Vector2(0.5f, 1f);
                brRt.anchorMax = new Vector2(0.5f, 1f);
                brRt.pivot = new Vector2(0f, 1f);
                brRt.anchoredPosition = new Vector2(15f, -135f);
                brRt.sizeDelta = new Vector2(270f, 105f);

                Image img = btnReubicar.GetComponent<Image>();
                if (img != null) { img.sprite = btnGlass; img.type = Image.Type.Sliced; img.color = Color.white; }
                TextMeshProUGUI tmp = btnReubicar.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<b>REUBICAR</b>";
                    tmp.color = ColorTextWhite;
                    tmp.fontSize = 28;
                    tmp.alignment = TextAlignmentOptions.Center;
                }
            }

            // Papelera y círculo de eliminación en la parte inferior
            Transform circulo = panelBotonesT.Find("CirculoRojo");
            if (circulo != null && ringTrash != null)
            {
                RectTransform cRt = circulo.GetComponent<RectTransform>();
                cRt.anchorMin = new Vector2(0.5f, 0f);
                cRt.anchorMax = new Vector2(0.5f, 0f);
                cRt.pivot = new Vector2(0.5f, 0f);
                cRt.anchoredPosition = new Vector2(0f, 40f);

                Image cImg = circulo.GetComponent<Image>();
                if (cImg != null)
                {
                    cImg.sprite = ringTrash;
                    cImg.type = Image.Type.Simple;
                }
            }
        }

        // 5. Estilizar Aviso de Nota Borrada
        Transform avisoBorradaT = hudT.Find("Aviso_NotaBorrada");
        if (avisoBorradaT != null)
        {
            Image img = avisoBorradaT.GetComponent<Image>();
            if (img != null && frameGlassDark != null)
            {
                img.sprite = frameGlassDark;
                img.type = Image.Type.Sliced;
                img.color = new Color(0.07f, 0.10f, 0.16f, 0.85f);
            }
            TextMeshProUGUI tmp = avisoBorradaT.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "<color=#F24242>✔</color> Nota eliminada del espacio AR";
                tmp.color = ColorTextWhite;
                tmp.fontSize = 24;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        // 6. Estilizar Modal Crear Nota (Estilo 2027 FUTURE Cyber-Glass)
        EstilizarModalCrearNotaAR(hudT, frameGlassCyan, btnCyan, btnGlass, inputGlass);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FutureUI] ARScene.unity actualizada con estilo 2027 FUTURE.");
    }

    private static void EstilizarModalCrearNotaAR(Transform hudT, Sprite frameGlassCyan, Sprite btnCyan, Sprite btnGlass, Sprite inputGlass)
    {
        Transform modalT = hudT.Find("ModalCrearNota");
        if (modalT == null) return;
        GameObject modal = modalT.gameObject;

        // Modal siempre al frente de todos los elementos HUD
        modalT.SetAsLastSibling();

        // 1. Overlay a pantalla completa que bloquea toques sobre el espacio AR
        RectTransform mRt = modal.GetComponent<RectTransform>();
        SetStretch(mRt);

        Image modalImg = modal.GetComponent<Image>();
        if (modalImg == null) modalImg = modal.AddComponent<Image>();
        modalImg.color = new Color(0.02f, 0.04f, 0.07f, 0.92f);
        modalImg.raycastTarget = true;

        // 2. Caja contenedora centrada de estilo Cyber-Glass
        Transform caja = modalT.Find("Caja_CrearNota");
        if (caja == null)
        {
            GameObject cajaGo = CreateUIObject("Caja_CrearNota", modalT);
            caja = cajaGo.transform;
            RectTransform cRt = cajaGo.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(960f, 760f);

            Image cImg = cajaGo.AddComponent<Image>();
            cImg.sprite = frameGlassCyan;
            cImg.type = Image.Type.Sliced;
            cImg.color = Color.white;

            // Mover hijos existentes a Caja_CrearNota
            var chList = new System.Collections.Generic.List<Transform>();
            foreach (Transform ch in modalT)
            {
                if (ch != caja) chList.Add(ch);
            }
            foreach (var ch in chList)
            {
                ch.SetParent(caja, false);
            }
        }
        else
        {
            RectTransform cRt = caja.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(960f, 760f);

            Image cImg = caja.GetComponent<Image>();
            if (cImg != null && frameGlassCyan != null)
            {
                cImg.sprite = frameGlassCyan;
                cImg.type = Image.Type.Sliced;
                cImg.color = Color.white;
            }
        }

        // 3. Título del Modal de Nota
        Transform tituloT = caja.Find("Titulo_ModalNota");
        TextMeshProUGUI tituloTmp = null;
        if (tituloT == null)
        {
            GameObject tGo = CreateUIObject("Titulo_ModalNota", caja);
            tituloT = tGo.transform;
            tituloTmp = tGo.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            tituloTmp = tituloT.GetComponent<TextMeshProUGUI>();
        }

        RectTransform tRt = tituloT.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.5f, 0.5f);
        tRt.anchorMax = new Vector2(0.5f, 0.5f);
        tRt.pivot = new Vector2(0.5f, 0.5f);
        tRt.anchoredPosition = new Vector2(0f, 260f);
        tRt.sizeDelta = new Vector2(860f, 100f);

        if (tituloTmp != null)
        {
            tituloTmp.text = "<b><color=#00F2FE>//</color> NUEVA NOTA ESPACIAL</b>\n<size=50%><color=#64748B>POST-IT HOLOGRÁFICO EN TABLERO AR</color></size>";
            tituloTmp.fontSize = 38;
            tituloTmp.alignment = TextAlignmentOptions.Center;
            tituloTmp.color = ColorTextWhite;
        }

        // 4. InputTexto (Campo para escribir el texto de la nota)
        Transform inputT = caja.Find("InputTexto");
        if (inputT != null)
        {
            RectTransform inRt = inputT.GetComponent<RectTransform>();
            inRt.anchorMin = new Vector2(0.5f, 0.5f);
            inRt.anchorMax = new Vector2(0.5f, 0.5f);
            inRt.pivot = new Vector2(0.5f, 0.5f);
            inRt.anchoredPosition = new Vector2(0f, 40f);
            inRt.sizeDelta = new Vector2(860f, 280f);

            if (inputGlass != null)
            {
                Image img = inputT.GetComponent<Image>();
                if (img != null) { img.sprite = inputGlass; img.type = Image.Type.Sliced; }
            }

            TMP_InputField inField = inputT.GetComponent<TMP_InputField>();
            if (inField != null)
            {
                inField.lineType = TMP_InputField.LineType.MultiLineNewline;
                inField.characterLimit = 200;

                if (inField.textComponent != null)
                {
                    inField.textComponent.fontSize = 30;
                    inField.textComponent.alignment = TextAlignmentOptions.TopLeft;
                    inField.textComponent.color = ColorTextWhite;
                }

                if (inField.placeholder != null)
                {
                    var phTmp = inField.placeholder.GetComponent<TextMeshProUGUI>();
                    if (phTmp != null)
                    {
                        phTmp.text = "Escribe aquí la tarea o recordatorio para tu tablero AR...";
                        phTmp.fontSize = 28;
                        phTmp.alignment = TextAlignmentOptions.TopLeft;
                        phTmp.color = ColorTextMuted;
                    }
                }
            }
        }

        // 5. Botón Crear (Colocar Nota en AR)
        Transform btnCrearT = caja.Find("Btn_Crear");
        if (btnCrearT != null)
        {
            RectTransform bRt = btnCrearT.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.pivot = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = new Vector2(-220f, -240f);
            bRt.sizeDelta = new Vector2(390f, 105f);

            if (btnCyan != null)
            {
                Image img = btnCrearT.GetComponent<Image>();
                if (img != null) { img.sprite = btnCyan; img.type = Image.Type.Sliced; img.color = Color.white; }
            }

            TextMeshProUGUI tmp = btnCrearT.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "<b>+ COLOCAR NOTA</b>";
                tmp.color = new Color(0.04f, 0.08f, 0.14f, 1f);
                tmp.fontSize = 26;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        // 6. Botón Cancelar (Cerrar modal)
        Transform btnCancT = caja.Find("Btn_Cancelar");
        if (btnCancT != null)
        {
            RectTransform bRt = btnCancT.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.pivot = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = new Vector2(220f, -240f);
            bRt.sizeDelta = new Vector2(390f, 105f);

            if (btnGlass != null)
            {
                Image img = btnCancT.GetComponent<Image>();
                if (img != null) { img.sprite = btnGlass; img.type = Image.Type.Sliced; img.color = Color.white; }
            }

            TextMeshProUGUI tmp = btnCancT.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "CANCELAR";
                tmp.color = ColorTextWhite;
                tmp.fontSize = 26;
                tmp.alignment = TextAlignmentOptions.Center;
            }
        }

        // 7. Asegurar que KanbanBoardManager tenga las referencias asignadas
        var kbm = Object.FindAnyObjectByType<KanbanBoardManager>();
        if (kbm != null)
        {
            SerializedObject so = new SerializedObject(kbm);
            so.FindProperty("modalCrearNota").objectReferenceValue = modal;
            if (inputT != null)
            {
                var inf = inputT.GetComponent<TMP_InputField>();
                if (inf != null) so.FindProperty("inputTextoNota").objectReferenceValue = inf;
            }
            so.ApplyModifiedProperties();
        }

        // Adaptabilidad al Teclado Móvil (Elevación dinámica para que los botones nunca queden tapados)
        ModalKeyboardShift shift = modal.GetComponent<ModalKeyboardShift>();
        if (shift == null) shift = modal.AddComponent<ModalKeyboardShift>();
        shift.containerToShift = caja.GetComponent<RectTransform>();
        shift.shiftAmount = 400f;
        shift.shiftSmoothTime = 0.16f;
        shift.restY = 0f;

        modal.SetActive(false);
    }

    #endregion

    #region 5. APLICAR TEMA A PREFABS

    public static void ApplyThemeToPrefabs()
    {
        Sprite frameGlassDark = LoadUISprite("Frame_Glass_Dark.png");
        Sprite cardPostIt = LoadUISprite("Card_PostIt_Future.png");

        // 1. Prefab ItemTablero_Btn
        string pathItem = "Assets/Prefabs/ItemTablero_Btn.prefab";
        if (File.Exists(pathItem))
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(pathItem);
            RectTransform rt = prefab.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 140f);
                EditorUtility.SetDirty(rt);
            }

            Image img = prefab.GetComponent<Image>();
            if (img != null && frameGlassDark != null)
            {
                img.sprite = frameGlassDark;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            TMP_Text tmp = prefab.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = ColorTextWhite;
                tmp.fontSize = 36;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                EditorUtility.SetDirty(tmp);
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, pathItem);
            PrefabUtility.UnloadPrefabContents(prefab);
            Debug.Log("[FutureUI] ItemTablero_Btn.prefab actualizado.");
        }

        // 2. Prefabs PostIt y PostIt_Prefab (Ambos actualizados con alto contraste blanco)
        string[] postItPaths = new string[] { "Assets/Prefabs/PostIt.prefab", "Assets/Prefabs/PostIt_Prefab.prefab" };
        foreach (var pathPostIt in postItPaths)
        {
            if (!File.Exists(pathPostIt)) continue;

            GameObject prefab = PrefabUtility.LoadPrefabContents(pathPostIt);
            Image img = prefab.GetComponent<Image>();
            if (img != null && cardPostIt != null)
            {
                img.sprite = cardPostIt;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            TMP_Text tmp = prefab.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = Color.white;
                tmp.fontStyle = FontStyles.Bold;
                tmp.fontSize = 36;
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 22;
                tmp.fontSizeMax = 46;
                EditorUtility.SetDirty(tmp);
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, pathPostIt);
            PrefabUtility.UnloadPrefabContents(prefab);
            Debug.Log($"[FutureUI] {pathPostIt} actualizado con texto blanco de alto contraste.");
        }

        // 3. Prefab KanbanBoard (3D AR Board)
        string pathBoard = "Assets/Prefabs/KanbanBoard.prefab";
        if (File.Exists(pathBoard))
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(pathBoard);

            Transform fondo = prefab.transform.Find("Fondo");
            if (fondo != null && frameGlassDark != null)
            {
                Image fImg = fondo.GetComponent<Image>();
                if (fImg != null)
                {
                    fImg.sprite = frameGlassDark;
                    fImg.type = Image.Type.Sliced;
                    fImg.color = new Color(0.06f, 0.09f, 0.14f, 0.94f);
                }
            }

            Transform colPend = prefab.transform.Find("ColumnasContainer/Columna_Pendiente");
            if (colPend != null)
            {
                TextMeshProUGUI tmp = colPend.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<b><color=#F9A61F>// PENDIENTE</color></b>";
                    tmp.fontSize = 28;
                }
            }

            Transform colProc = prefab.transform.Find("ColumnasContainer/Columna_EnProceso");
            if (colProc != null)
            {
                TextMeshProUGUI tmp = colProc.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<b><color=#00F2FE>// EN PROCESO</color></b>";
                    tmp.fontSize = 28;
                }
            }

            Transform colTerm = prefab.transform.Find("ColumnasContainer/Columna_Terminado");
            if (colTerm != null)
            {
                TextMeshProUGUI tmp = colTerm.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = "<b><color=#10D08C>// COMPLETADO</color></b>";
                    tmp.fontSize = 28;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, pathBoard);
            PrefabUtility.UnloadPrefabContents(prefab);
            Debug.Log("[FutureUI] KanbanBoard.prefab actualizado con estilo 2027 FUTURE.");
        }
    }

    #endregion
}
