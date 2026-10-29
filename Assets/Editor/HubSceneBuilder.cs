using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds towerScene, turns the start menu into a game picker and moves the
// ads out of CityScene. Run from Tools > Hub, or in batch mode with
// -executeMethod HubSceneBuilder.BuildAll. Safe to run again.
public static class HubSceneBuilder
{
    const string START_SCENE = "Assets/Scenes/StartScene.unity";
    const string CITY_SCENE = "Assets/Scenes/CityScene.unity";
    const string TOWER_SCENE = "Assets/Scenes/towerScene.unity";
    const string HANOI = "Assets/Sprites/hanojas tornis/";
    const string HANOI_SOUNDS = "Assets/Sounds/hanojas tornis/";
    const string FONT = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    static readonly Color Yellow = new Color(0.996f, 1f, 0f);
    static readonly Color Red = new Color(0.80f, 0.155f, 0.155f);
    static readonly Color DarkText = new Color(0.196f, 0.196f, 0.196f);
    static readonly Color CardDark = new Color(0.118f, 0.118f, 0.118f, 0.97f);
    static readonly Color Dim = new Color(0f, 0f, 0f, 0.6f);

    static TMP_FontAsset font;
    static Sprite uiSprite;

    [MenuItem("Tools/Hub/Build All")]
    public static void BuildAll()
    {
        ImportArt();
        LoadShared();
        BuildTowerScene();
        RebuildMenu();
        UpdateCityScene();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("HubSceneBuilder: done");
    }

    [MenuItem("Tools/Hub/Give 50 Stars")]
    public static void GiveStars()
    {
        StarWallet.Add(DevCheatsScript.GIFT_STARS);
        Debug.Log("Stars: " + StarWallet.Stars);
    }

    static void LoadShared()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT);
        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    // ---------------------------------------------------------------- import

    static void ImportArt()
    {
        string[] singles =
        {
            HANOI + "towerOfHanoi_board.png", HANOI + "hanoi_floor.png", HANOI + "hanoi_sky.png",
            HANOI + "hanoi_preview.png", HANOI + "star.png", "Assets/Sprites/menu_background.png",
            HANOI + "hanoi_sky_sunset.png", HANOI + "hanoi_sky_night.png",
            HANOI + "hanoi_tree_left.png", HANOI + "hanoi_tree_right.png"
        };
        foreach (string path in singles)
            SetSprite(path, Vector4.zero);

        // Disk ends stay round when the disk is stretched (sliced sprite).
        for (int i = 0; i < 6; i++)
            SetSprite(HANOI + i + "block.png", new Vector4(30, 0, 30, 0));
    }

    static void SetSprite(string path, Vector4 border)
    {
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = border;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();
    }

    static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (sprite == null)
            throw new Exception("Sprite not found: " + path);
        return sprite;
    }

    static AudioClip LoadClip(string path)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null)
            throw new Exception("Audio not found: " + path);
        return clip;
    }

    // ---------------------------------------------------------------- UI helpers

    static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = (RectTransform)obj.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return rect;
    }

    static RectTransform Centered(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        return Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
    }

    static RectTransform Stretch(string name, Transform parent)
    {
        return Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    static Image AddImage(RectTransform rect, Sprite sprite, Color color, bool raycast = true)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycast;
        if (sprite == uiSprite)
            image.type = Image.Type.Sliced;
        return image;
    }

    static TextMeshProUGUI AddText(RectTransform rect, string text, float size, Color color,
        bool bold = true, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    static void AutoSize(TMP_Text text, float min)
    {
        text.enableAutoSizing = true;
        text.fontSizeMax = text.fontSize;
        text.fontSizeMin = min;
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }

    static TextMeshProUGUI Label(string name, Transform parent, Vector2 pos, Vector2 size, string text,
        float fontSize, Color color, bool bold = true)
    {
        return AddText(Centered(name, parent, pos, size), text, fontSize, color, bold);
    }

    static Button TextButton(string name, Transform parent, Vector2 pos, Vector2 size, string text,
        Color color, float fontSize = 46)
    {
        RectTransform rect = Centered(name, parent, pos, size);
        Image image = AddImage(rect, uiSprite, color);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        Color textColor = color == Red ? Color.white : DarkText;
        AddText(Stretch("Text", rect), text, fontSize, textColor);
        return button;
    }

    static Button IconButton(string name, Transform parent, Vector2 pos, Vector2 size, Sprite sprite)
    {
        RectTransform rect = Centered(name, parent, pos, size);
        Image image = AddImage(rect, sprite, Color.white);
        image.preserveAspect = true;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    static void OnClick(Button button, UnityAction action)
    {
        UnityEventTools.AddPersistentListener(button.onClick, action);
    }

    static void Shadow(GameObject obj)
    {
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.3f);
        shadow.effectDistance = new Vector2(6f, -6f);
    }

    static Canvas CreateCanvas(string name, float match)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        obj.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = obj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = obj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = match;
        return canvas;
    }

    // ---------------------------------------------------------------- towerScene

    static void BuildTowerScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObj.tag = "MainCamera";
        Camera camera = cameraObj.GetComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.43f, 0.71f, 0.91f);
        cameraObj.transform.position = new Vector3(0, 0, -10);

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        GameObject gameObj = new GameObject("HanoiGame");
        HanoiGameScript game = gameObj.AddComponent<HanoiGameScript>();
        gameObj.AddComponent<LandscapeOnlyScript>();

        AudioSource music = gameObj.AddComponent<AudioSource>();
        music.clip = LoadClip(HANOI_SOUNDS + "theTower.mp3");
        music.loop = true;
        music.playOnAwake = true;
        music.volume = 0.3f;
        AudioSource sounds = gameObj.AddComponent<AudioSource>();
        sounds.playOnAwake = false;

        game.musicSource = music;
        game.soundSource = sounds;
        game.pickSound = LoadClip(HANOI_SOUNDS + "click.wav");
        game.dropSound = LoadClip(HANOI_SOUNDS + "drop.wav");
        game.buttonSound = LoadClip(HANOI_SOUNDS + "onButton.wav");
        game.victorySound = LoadClip(HANOI_SOUNDS + "victory.mp3");

        // Landscape only, so the canvas always scales by height.
        Canvas canvas = CreateCanvas("Canvas", 1f);
        Transform root = canvas.transform;

        // --- Background and board
        RectTransform sky = Centered("Sky", root, Vector2.zero, new Vector2(2560, 1080));
        game.skyImage = AddImage(sky, LoadSprite(HANOI + "hanoi_sky.png"), Color.white, false);
        AspectRatioFitter skyFit = sky.gameObject.AddComponent<AspectRatioFitter>();
        skyFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        skyFit.aspectRatio = 2560f / 1080f;

        // Leaves room for the HUD on top and the banner ad at the bottom.
        RectTransform boardArea = Stretch("BoardArea", root);
        boardArea.offsetMin = new Vector2(20, 150);
        boardArea.offsetMax = new Vector2(-20, -140);

        RectTransform board = Rect("Board", boardArea, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            Vector2.zero, new Vector2(1280, 600), new Vector2(0.5f, 0f));
        board.gameObject.AddComponent<HanoiBoardFitScript>();

        // Ground and floor reach past the board's edges on wide screens.
        RectTransform ground = Rect("Ground", board, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            Vector2.zero, new Vector2(8000, 2000), new Vector2(0.5f, 1f));
        Image groundImage = AddImage(ground, null, new Color(0.62f, 0.52f, 0.31f), false);

        RectTransform floor = Rect("Floor", board, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            Vector2.zero, new Vector2(4096, 47), new Vector2(0.5f, 0f));
        Image floorImage = AddImage(floor, LoadSprite(HANOI + "hanoi_floor.png"), Color.white, false);

        Image boardArt = AddImage(Stretch("BoardArt", board), LoadSprite(HANOI + "towerOfHanoi_board.png"), Color.white, false);

        // Cherry trees hide the buildings cut off at the art's edges and fill
        // the extra width on wide phones (660x640 board px, see gen_art.py).
        RectTransform treeLeft = Rect("TreeLeft", board, Vector2.zero, Vector2.zero,
            new Vector2(-480, 40), new Vector2(660, 640), Vector2.zero);
        Image treeLeftImage = AddImage(treeLeft, LoadSprite(HANOI + "hanoi_tree_left.png"), Color.white, false);
        RectTransform treeRight = Rect("TreeRight", board, Vector2.zero, Vector2.zero,
            new Vector2(1100, 40), new Vector2(660, 640), Vector2.zero);
        Image treeRightImage = AddImage(treeRight, LoadSprite(HANOI + "hanoi_tree_right.png"), Color.white, false);

        game.tintedArt = new Graphic[] { groundImage, floorImage, boardArt, treeLeftImage, treeRightImage };
        game.skyThemes = new[]
        {
            new HanoiGameScript.SkyTheme { id = StarWallet.SKY_DAY, sky = LoadSprite(HANOI + "hanoi_sky.png"), tint = Color.white },
            new HanoiGameScript.SkyTheme { id = "sky_sunset", sky = LoadSprite(HANOI + "hanoi_sky_sunset.png"), tint = new Color(1f, 0.88f, 0.82f) },
            new HanoiGameScript.SkyTheme { id = "sky_night", sky = LoadSprite(HANOI + "hanoi_sky_night.png"), tint = new Color(0.62f, 0.68f, 0.9f) },
        };

        for (int i = 0; i < 3; i++)
        {
            RectTransform poleArea = Rect("Pole" + i, board, Vector2.zero, Vector2.zero,
                new Vector2(game.poleX[i], 0), new Vector2(420, 640), new Vector2(0.5f, 0f));
            AddImage(poleArea, null, Color.clear);
            HanoiPoleScript pole = poleArea.gameObject.AddComponent<HanoiPoleScript>();
            pole.poleIndex = i;
            pole.game = game;
        }

        RectTransform disks = Stretch("Disks", board);
        disks.pivot = Vector2.zero;
        game.diskContainer = disks;
        game.diskSprites = Enumerable.Range(0, 6).Select(i => LoadSprite(HANOI + i + "block.png")).ToArray();

        // --- HUD
        RectTransform hud = Stretch("HUD", root);

        Button menuButton = TextButton("MenuButton", hud, Vector2.zero, new Vector2(240, 90), "Izvēlne", Yellow, 44);
        SetCorner((RectTransform)menuButton.transform, new Vector2(0, 1), new Vector2(150, -70));
        OnClick(menuButton, game.PlayButtonSound);
        OnClick(menuButton, game.MainMenu);

        // Sized so the HUD still fits on a 4:3 tablet (canvas 1440 wide).
        RectTransform info = Centered("Info", hud, Vector2.zero, new Vector2(520, 100));
        SetCorner(info, new Vector2(0.5f, 1), new Vector2(0, -70));
        AddImage(info, uiSprite, new Color(0.118f, 0.118f, 0.118f, 0.8f), false);
        game.timeText = Label("Time", info, new Vector2(-140, 0), new Vector2(200, 90), "00:00", 56, Color.white);
        game.movesText = Label("Moves", info, new Vector2(110, 0), new Vector2(280, 90), "Gājieni: 0", 40, Yellow);
        AutoSize(game.movesText, 24);

        Button restartButton = IconButton("RestartButton", hud, Vector2.zero, new Vector2(100, 100), LoadSprite(HANOI + "retry1.png"));
        SetCorner((RectTransform)restartButton.transform, new Vector2(1, 1), new Vector2(-70, -70));
        OnClick(restartButton, game.Retry);

        // --- Start panel: rules and disk count
        RectTransform startPanel = Stretch("StartPanel", root);
        AddImage(startPanel, null, Dim);
        RectTransform startCard = Centered("Card", startPanel, new Vector2(0, 40), new Vector2(940, 700));
        AddImage(startCard, uiSprite, CardDark);
        Label("Title", startCard, new Vector2(0, 265), new Vector2(860, 100), "HANOJAS TORNIS", 72, Yellow);
        Label("Rules", startCard, new Vector2(0, 120), new Vector2(840, 170),
            "Pārvieto visu torni uz labo stabu.\nVienā gājienā drīkst pārvietot vienu disku, un lielāku disku nedrīkst likt uz mazāka.",
            34, Color.white, false);
        Label("Choose", startCard, new Vector2(0, -15), new Vector2(840, 60), "Izvēlies disku skaitu:", 40, Color.white);
        for (int count = 3; count <= HanoiGameScript.MAX_DISKS; count++)
        {
            Button countButton = TextButton("Disks" + count, startCard, new Vector2((count - 4) * 210, -125),
                new Vector2(170, 120), count.ToString(), Yellow, 70);
            OnClick(countButton, game.PlayButtonSound);
            UnityEventTools.AddIntPersistentListener(countButton.onClick, game.StartGame, count);
        }
        Button startMenuButton = TextButton("MenuButton", startCard, new Vector2(0, -268), new Vector2(320, 80), "Izvēlne", Red, 40);
        OnClick(startMenuButton, game.PlayButtonSound);
        OnClick(startMenuButton, game.MainMenu);
        game.startPanel = startPanel.gameObject;

        // --- Win panel
        RectTransform winPanel = Stretch("WinPanel", root);
        AddImage(winPanel, null, Dim);
        RectTransform victory = Centered("Victory", winPanel, new Vector2(0, 150), new Vector2(500, 498));
        game.victoryImage = AddImage(victory, LoadSprite(HANOI + "victory3.png"), Color.white, false);
        game.victoryImage.preserveAspect = true;
        game.victorySprites = new[]
        {
            LoadSprite(HANOI + "victory1.png"), LoadSprite(HANOI + "victory2.png"), LoadSprite(HANOI + "victory3.png")
        };
        // Next to the "Laiks:" printed on the card.
        RectTransform winTime = Rect("Time", victory, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-30, -156), new Vector2(240, 70), new Vector2(0f, 0.5f));
        game.winTimeText = AddText(winTime, "00:00", 56, new Color(0.55f, 0.3f, 0.12f), true, TextAlignmentOptions.Left);
        game.winMessageText = Label("Message", winPanel, new Vector2(0, -150), new Vector2(900, 70), "Perfekti!", 60, Yellow);
        game.winMovesText = Label("Moves", winPanel, new Vector2(0, -210), new Vector2(900, 50), "Gājieni: 7", 38, Color.white, false);

        Button winRetry = IconButton("RetryButton", winPanel, new Vector2(-470, -300), new Vector2(110, 110), LoadSprite(HANOI + "retry1.png"));
        OnClick(winRetry, game.Retry);
        Button moreDisks = TextButton("MoreDisksButton", winPanel, new Vector2(-195, -300), new Vector2(330, 100), "Vairāk disku", Yellow, 40);
        OnClick(moreDisks, game.MoreDisks);
        Button chooseDisks = TextButton("ChooseDisksButton", winPanel, new Vector2(140, -300), new Vector2(300, 100), "Disku skaits", Yellow, 40);
        OnClick(chooseDisks, game.ChooseDisks);
        Button winMenu = TextButton("MenuButton", winPanel, new Vector2(420, -300), new Vector2(220, 100), "Izvēlne", Red, 40);
        OnClick(winMenu, game.PlayButtonSound);
        OnClick(winMenu, game.MainMenu);
        game.moreDisksButton = moreDisks.gameObject;

        // Watch an ad to halve the time, next to the victory card.
        RectTransform rewardRect = Centered("RewardedAdButton", winPanel, new Vector2(440, 150), new Vector2(340, 130));
        Image rewardImage = AddImage(rewardRect, uiSprite, new Color(0.55f, 0.85f, 0.25f));
        Button rewardButton = rewardRect.gameObject.AddComponent<Button>();
        rewardButton.targetGraphic = rewardImage;
        ColorBlock colors = rewardButton.colors;
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        rewardButton.colors = colors;
        RectTransform adIcon = Rect("Icon", rewardRect, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(64, 0), new Vector2(100, 100));
        AddImage(adIcon, LoadSprite(HANOI + "Ads.png"), Color.white, false).preserveAspect = true;
        TextMeshProUGUI rewardTitle = AddText(Rect("Title", rewardRect, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(60, 20), new Vector2(-130, 60)),
            "Laiks ÷ 2", 46, DarkText);
        AutoSize(rewardTitle, 24);
        TextMeshProUGUI rewardHint = AddText(Rect("Hint", rewardRect, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(60, -30), new Vector2(-130, 40)),
            "skaties reklāmu", 28, DarkText, false);
        AutoSize(rewardHint, 16);
        OnClick(rewardButton, game.WatchAdForTime);
        game.rewardedButton = rewardButton;

        // Stars paid into the shop wallet, left of the victory card.
        RectTransform earned = Centered("StarsEarned", winPanel, new Vector2(-440, 150), new Vector2(340, 130));
        AddImage(earned, uiSprite, CardDark, false);
        RectTransform earnedIcon = Rect("Icon", earned, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(70, 0), new Vector2(100, 100));
        AddImage(earnedIcon, LoadSprite(HANOI + "star.png"), Color.white, false);
        game.winStarsText = AddText(Rect("Earned", earned, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(65, 20), new Vector2(-150, 60)),
            "+3", 56, Yellow);
        TextMeshProUGUI total = AddText(Rect("Total", earned, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(65, -32), new Vector2(-150, 40)),
            "kopā: 0", 28, Color.white, false);
        AutoSize(total, 16);
        StarCounterScript totalCounter = earned.gameObject.AddComponent<StarCounterScript>();
        totalCounter.starsText = total;
        totalCounter.format = "kopā: {0}";

        game.winPanel = winPanel.gameObject;
        winPanel.gameObject.SetActive(false);

        // --- Star criteria, bottom left (beside the banner). Above the panels,
        // so it can be read on the win screen too.
        RectTransform criteria = Rect("StarCriteria", root, Vector2.zero, Vector2.zero,
            new Vector2(20, 12), new Vector2(470, 128), Vector2.zero);
        AddImage(criteria, uiSprite, new Color(0.118f, 0.118f, 0.118f, 0.8f), false);
        Sprite star = LoadSprite(HANOI + "star.png");
        game.criteriaTexts = new TMP_Text[3];
        for (int row = 0; row < 3; row++)
        {
            RectTransform line = Rect("Stars" + (3 - row), criteria, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(12, -7 - 38 * row), new Vector2(446, 38), new Vector2(0, 1));
            for (int i = 0; i < 3 - row; i++)
            {
                RectTransform icon = Rect("Star", line, new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                    new Vector2(16 + i * 30, 0), new Vector2(30, 30));
                AddImage(icon, star, Color.white, false);
            }
            TextMeshProUGUI text = AddText(Rect("Text", line, Vector2.zero, Vector2.one, new Vector2(52, 0), new Vector2(-104, 0)),
                "", 25, Color.white, false, TextAlignmentOptions.Left);
            AutoSize(text, 14);
            game.criteriaTexts[row] = text;
        }
        game.criteriaPanel = criteria.gameObject;

        // Messages go on top of everything (the reward message shows on the win screen).
        RectTransform messageBox = Centered("MessageBox", root, Vector2.zero, new Vector2(1000, 110));
        SetCorner(messageBox, new Vector2(0.5f, 1), new Vector2(0, -190));
        AddImage(messageBox, uiSprite, new Color(0.1f, 0.1f, 0.1f, 0.95f), false);
        game.messageBox = messageBox.gameObject;
        game.messageText = AddText(Stretch("Text", messageBox), "", 40, Color.white);

        EditorSceneManager.SaveScene(scene, TOWER_SCENE);
    }

    static void SetCorner(RectTransform rect, Vector2 anchor, Vector2 pos)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = pos;
    }

    // ---------------------------------------------------------------- menu

    static void RebuildMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(START_SCENE, OpenSceneMode.Single);
        Transform canvas = GameObject.Find("Canvas").transform;

        foreach (string name in new[] { "PlayButton", "SpinningTires", "CityCard", "HanoiCard", "shopButton", "StarCounter", "ShopPopup" })
        {
            Transform old = canvas.Find(name);
            if (old != null)
                UnityEngine.Object.DestroyImmediate(old.gameObject);
        }

        // No flying bomb in the game picker.
        foreach (GameObject obj in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Select(t => t.gameObject).ToArray())
        {
            if (obj != null && PrefabUtility.IsOutermostPrefabInstanceRoot(obj) &&
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj) == "Assets/Prefabs/FlyingBombMenu.prefab")
                UnityEngine.Object.DestroyImmediate(obj);
        }

        // Neutral background (square, so it covers portrait and landscape).
        RectTransform background = (RectTransform)canvas.Find("menuBackground");
        background.GetComponent<Image>().sprite = LoadSprite("Assets/Sprites/menu_background.png");
        AspectRatioFitter bgFit = background.GetComponent<AspectRatioFitter>();
        if (bgFit == null)
            bgFit = background.gameObject.AddComponent<AspectRatioFitter>();
        bgFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        bgFit.aspectRatio = 1f;

        StartMenuScript menu = UnityEngine.Object.FindFirstObjectByType<StartMenuScript>();
        ButtonSoundScript buttonSound = UnityEngine.Object.FindFirstObjectByType<ButtonSoundScript>();

        RectTransform cityCard = GameCard("CityCard", canvas, LoadSprite("Assets/Fonts/images.png"), "Where is my car?");
        OnClick(cityCard.GetComponent<Button>(), buttonSound.PlayButtonSound);
        OnClick(cityCard.GetComponent<Button>(), menu.PlayCity);

        RectTransform hanoiCard = GameCard("HanoiCard", canvas, LoadSprite(HANOI + "hanoi_preview.png"), "Hanojas tornis");
        OnClick(hanoiCard.GetComponent<Button>(), buttonSound.PlayButtonSound);
        OnClick(hanoiCard.GetComponent<Button>(), menu.PlayHanoi);

        RectTransform shopButton = BuildShop(canvas, menu, buttonSound);

        // Popups must stay on top of the cards.
        canvas.Find("SettingsPopup").SetAsLastSibling();
        canvas.Find("ShopPopup").SetAsLastSibling();

        RectTransform title = (RectTransform)canvas.Find("title");
        RectTransform settings = (RectTransform)canvas.Find("settingsButton");
        RectTransform exit = (RectTransform)canvas.Find("exitButton");

        OrientationLayoutScript layout = canvas.GetComponent<OrientationLayoutScript>();
        layout.entries = new[]
        {
            Entry(title, new Vector2(0, 380), new Vector2(1200, 130), new Vector2(0, 730), new Vector2(900, 160)),
            Entry(cityCard, new Vector2(-320, 20), new Vector2(580, 420), new Vector2(0, 360), new Vector2(700, 490)),
            Entry(hanoiCard, new Vector2(320, 20), new Vector2(580, 420), new Vector2(0, -170), new Vector2(700, 490)),
            Entry(settings, new Vector2(-420, -275), new Vector2(380, 90), new Vector2(0, -650), new Vector2(600, 110)),
            Entry(shopButton, new Vector2(0, -275), new Vector2(380, 90), new Vector2(0, -520), new Vector2(600, 110)),
            Entry(exit, new Vector2(420, -275), new Vector2(380, 90), new Vector2(0, -780), new Vector2(600, 110)),
        };

        // The scene shows the landscape layout until the script runs.
        foreach (OrientationLayoutScript.Entry entry in layout.entries)
        {
            entry.target.anchoredPosition = entry.landscapePosition;
            entry.target.sizeDelta = entry.landscapeSize;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Shop button (a copy of the settings button), star counter in the top
    // right corner and the shop popup (a copy of the settings popup).
    static RectTransform BuildShop(Transform canvas, StartMenuScript menu, ButtonSoundScript buttonSound)
    {
        ShopScript shop = menu.GetComponent<ShopScript>();
        if (shop == null)
            shop = menu.gameObject.AddComponent<ShopScript>();
        Sprite star = LoadSprite(HANOI + "star.png");

        Transform settingsButton = canvas.Find("settingsButton");
        RectTransform shopButton = (RectTransform)UnityEngine.Object.Instantiate(settingsButton, canvas).transform;
        shopButton.name = "shopButton";
        shopButton.SetSiblingIndex(settingsButton.GetSiblingIndex() + 1);
        shopButton.GetComponentInChildren<TMP_Text>().text = "Veikals";
        Button shopClick = shopButton.GetComponent<Button>();
        shopClick.onClick = new Button.ButtonClickedEvent();
        OnClick(shopClick, buttonSound.PlayButtonSound);
        OnClick(shopClick, shop.OpenShop);

        RectTransform counter = Centered("StarCounter", canvas, Vector2.zero, new Vector2(230, 90));
        SetCorner(counter, new Vector2(1, 1), new Vector2(-140, -70));
        AddImage(counter, uiSprite, new Color(0.118f, 0.118f, 0.118f, 0.8f));   // takes taps
        counter.gameObject.AddComponent<SecretTapsScript>();
        AddImage(Rect("Icon", counter, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 0), new Vector2(66, 66)), star, Color.white, false);
        TextMeshProUGUI counterText = AddText(Rect("Stars", counter, Vector2.zero, Vector2.one, new Vector2(45, 0), new Vector2(-110, 0)),
            "0", 50, Yellow);
        counter.gameObject.AddComponent<StarCounterScript>().starsText = counterText;

        // Popup: keep the settings card's look, title and close button.
        Transform settingsPopup = canvas.Find("SettingsPopup");
        GameObject popup = UnityEngine.Object.Instantiate(settingsPopup.gameObject, canvas);
        popup.name = "ShopPopup";
        popup.SetActive(false);
        shop.shopPopup = popup;

        RectTransform card = (RectTransform)popup.transform.GetChild(0);
        card.name = "ShopCard";
        card.anchoredPosition = new Vector2(0, 30);
        card.sizeDelta = new Vector2(900, 740);
        // The settings card is see-through; the shop has too much text for that.
        card.GetComponent<Image>().color = new Color(0.118f, 0.118f, 0.118f, 1f);
        foreach (Transform child in card.Cast<Transform>().ToArray())
        {
            if (child.name != "SettingsTitle" && child.name != "CloseSettingsButton")
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        RectTransform shopTitle = (RectTransform)card.Find("SettingsTitle");
        shopTitle.name = "ShopTitle";
        shopTitle.anchoredPosition = new Vector2(-120, 300);
        shopTitle.sizeDelta = new Vector2(560, 90);
        shopTitle.GetComponent<TMP_Text>().text = "VEIKALS";

        RectTransform close = (RectTransform)card.Find("CloseSettingsButton");
        close.name = "CloseShopButton";
        close.anchoredPosition = new Vector2(0, -290);
        Button closeClick = close.GetComponent<Button>();
        closeClick.onClick = new Button.ButtonClickedEvent();
        OnClick(closeClick, buttonSound.PlayButtonSound);
        OnClick(closeClick, shop.CloseShop);

        // Balance, top right of the card
        RectTransform balance = Centered("Balance", card, new Vector2(300, 300), new Vector2(230, 84));
        AddImage(balance, uiSprite, new Color(0f, 0f, 0f, 0.35f), false);
        AddImage(Rect("Icon", balance, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(48, 0), new Vector2(62, 62)), star, Color.white, false);
        TextMeshProUGUI balanceText = AddText(Rect("Stars", balance, Vector2.zero, Vector2.one, new Vector2(40, 0), new Vector2(-100, 0)),
            "0", 46, Yellow);
        balance.gameObject.AddComponent<StarCounterScript>().starsText = balanceText;

        Sprite adIcon = LoadSprite(HANOI + "Ads.png");
        ShopRow(card, 0, "Bez reklāmām", "Nav reklāmas, mainot ainu", adIcon, true,
            StarWallet.NO_ADS, 30, ShopItemScript.Kind.Unlock, star, buttonSound);
        ShopRow(card, 1, "Diena", "Hanojas torņa fons", LoadSprite(HANOI + "hanoi_sky.png"), false,
            StarWallet.SKY_DAY, 0, ShopItemScript.Kind.SkyTheme, star, buttonSound);
        ShopRow(card, 2, "Saulriets", "Hanojas torņa fons", LoadSprite(HANOI + "hanoi_sky_sunset.png"), false,
            "sky_sunset", 10, ShopItemScript.Kind.SkyTheme, star, buttonSound);
        ShopRow(card, 3, "Nakts", "Hanojas torņa fons", LoadSprite(HANOI + "hanoi_sky_night.png"), false,
            "sky_night", 15, ShopItemScript.Kind.SkyTheme, star, buttonSound);

        return shopButton;
    }

    static void ShopRow(Transform card, int index, string title, string description, Sprite icon, bool crossedOut,
        string id, int price, ShopItemScript.Kind kind, Sprite star, ButtonSoundScript buttonSound)
    {
        RectTransform row = Centered("Item_" + id, card, new Vector2(0, 200 - 115 * index), new Vector2(820, 105));
        AddImage(row, uiSprite, new Color(1f, 1f, 1f, 0.08f), false);

        RectTransform iconRect = Rect("Icon", row, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(90, 0), new Vector2(150, 85));
        AddImage(iconRect, icon, Color.white, false).preserveAspect = true;
        if (crossedOut)
        {
            RectTransform slash = Centered("Slash", iconRect, Vector2.zero, new Vector2(100, 12));
            slash.localRotation = Quaternion.Euler(0, 0, 40);
            AddImage(slash, null, Red, false);
        }

        TextMeshProUGUI name = AddText(Rect("Title", row, Vector2.zero, Vector2.one, new Vector2(-37, 18), new Vector2(-445, -50)),
            title, 40, Color.white, true, TextAlignmentOptions.Left);
        AutoSize(name, 24);
        TextMeshProUGUI info = AddText(Rect("Description", row, Vector2.zero, Vector2.one, new Vector2(-37, -24), new Vector2(-445, -60)),
            description, 26, new Color(0.8f, 0.8f, 0.8f), false, TextAlignmentOptions.Left);
        AutoSize(info, 16);

        RectTransform buttonRect = Rect("Button", row, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-130, 0), new Vector2(230, 80));
        Image buttonImage = AddImage(buttonRect, uiSprite, Yellow);
        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        RectTransform priceStar = Rect("Star", buttonRect, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(42, 0), new Vector2(50, 50));
        AddImage(priceStar, star, Color.white, false);
        TextMeshProUGUI label = AddText(Stretch("Text", buttonRect), "", 38, DarkText);
        AutoSize(label, 20);

        ShopItemScript item = row.gameObject.AddComponent<ShopItemScript>();
        item.id = id;
        item.price = price;
        item.kind = kind;
        item.button = button;
        item.buttonImage = buttonImage;
        item.buttonText = label;
        item.priceStar = priceStar.gameObject;
        OnClick(button, buttonSound.PlayButtonSound);
        OnClick(button, item.Click);
    }

    static RectTransform GameCard(string name, Transform parent, Sprite preview, string title)
    {
        RectTransform card = Centered(name, parent, Vector2.zero, new Vector2(580, 420));
        Image image = AddImage(card, uiSprite, Color.white);
        Button button = card.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        Shadow(card.gameObject);

        RectTransform picture = Stretch("Preview", card);
        picture.offsetMin = new Vector2(16, 96);
        picture.offsetMax = new Vector2(-16, -16);
        AddImage(picture, preview, Color.white, false);

        RectTransform label = Rect("Label", card, Vector2.zero, new Vector2(1, 0), new Vector2(0, 48), new Vector2(-20, 90));
        AddText(label, title, 50, DarkText);
        return card;
    }

    static OrientationLayoutScript.Entry Entry(RectTransform target, Vector2 landPos, Vector2 landSize, Vector2 portPos, Vector2 portSize)
    {
        return new OrientationLayoutScript.Entry
        {
            target = target,
            landscapePosition = landPos,
            landscapeSize = landSize,
            portraitPosition = portPos,
            portraitSize = portSize
        };
    }

    // ---------------------------------------------------------------- CityScene

    // AdManager now creates itself and lives for the whole app, so the scene's
    // copy goes. FlyingObjectManager (the rewarded ad's reward) stays.
    static void UpdateCityScene()
    {
        Scene scene = EditorSceneManager.OpenScene(CITY_SCENE, OpenSceneMode.Single);

        GameObject holder = GameObject.Find("AdsScriptHolder");
        if (holder != null)
        {
            foreach (Type type in new[] { typeof(AdManager), typeof(BannerAd), typeof(RewardedAds), typeof(InterstitialAds), typeof(AdsInitializer) })
            {
                Component component = holder.GetComponent(type);
                if (component != null)
                    UnityEngine.Object.DestroyImmediate(component);
            }
        }

        // Banner is always on now: no toggle button.
        GameObject bannerButton = GameObject.Find("bannerAd");
        if (bannerButton != null)
            UnityEngine.Object.DestroyImmediate(bannerButton);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == TOWER_SCENE))
            scenes.Add(new EditorBuildSettingsScene(TOWER_SCENE, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------------------------------------------------------------- screenshots

    // Renders the scenes to PNGs (for checking layouts without opening them).
    // Output folder comes from the HUB_CAPTURE_DIR environment variable.
    public static void CaptureAll()
    {
        string dir = Environment.GetEnvironmentVariable("HUB_CAPTURE_DIR") ?? "Temp/HubCaptures";
        Directory.CreateDirectory(dir);

        Capture(START_SCENE, 2340, 1080, dir + "/menu_landscape.png", null);
        Capture(START_SCENE, 1080, 2340, dir + "/menu_portrait.png", null);
        Capture(START_SCENE, 2340, 1080, dir + "/shop_landscape.png", null, OpenShopForCapture);
        Capture(START_SCENE, 1080, 2340, dir + "/shop_portrait.png", null, OpenShopForCapture);
        Capture(TOWER_SCENE, 2340, 1080, dir + "/tower_sunset.png", game => { ShowTheme(game, 1); game.StartGame(5); });
        Capture(TOWER_SCENE, 2520, 1080, dir + "/tower_night_wide.png", game => { ShowTheme(game, 2); game.StartGame(3); });
        Capture(TOWER_SCENE, 2340, 1080, dir + "/tower_start.png", null);
        Capture(TOWER_SCENE, 2340, 1080, dir + "/tower_play.png", game =>
        {
            game.StartGame(4);
            game.PoleClicked(0); game.PoleClicked(1);   // smallest to the middle
            game.PoleClicked(0); game.PoleClicked(2);   // next one to the right
            game.PoleClicked(0);                         // lift the third
            game.messageBox.SetActive(true);
            game.messageText.text = "Laiks samazināts uz pusi!";
        });
        Capture(TOWER_SCENE, 2048, 1536, dir + "/tower_play_tablet.png", game =>
        {
            game.StartGame(5);
            SetPrivate(game, "moves", 31);
            SetPrivate(game, "gameTime", 754f);
        });
        Capture(TOWER_SCENE, 2340, 1080, dir + "/tower_win.png", game =>
        {
            game.StartGame(3);
            SetPrivate(game, "moves", 9);
            SetPrivate(game, "gameTime", 34.2f);
            CallPrivate(game, "Win");
        });
        Capture(TOWER_SCENE, 2340, 1080, dir + "/tower_win_rewarded.png", game =>
        {
            game.StartGame(3);
            SetPrivate(game, "moves", 7);
            SetPrivate(game, "gameTime", 30f);
            CallPrivate(game, "Win");
            CallPrivate(game, "HalveTime");
            game.messageBox.SetActive(true);
            game.messageText.text = "Laiks samazināts uz pusi!";
        });
        Debug.Log("HubSceneBuilder: captures in " + dir);
    }

    // Play-mode scripts don't run here, so fill in what OnEnable would show.
    static void OpenShopForCapture()
    {
        GameObject.Find("Canvas").transform.Find("ShopPopup").gameObject.SetActive(true);
        foreach (StarCounterScript counter in UnityEngine.Object.FindObjectsByType<StarCounterScript>(FindObjectsSortMode.None))
            counter.starsText.text = "27";
        foreach (ShopItemScript item in UnityEngine.Object.FindObjectsByType<ShopItemScript>(FindObjectsSortMode.None))
            item.Refresh();
    }

    static void ShowTheme(HanoiGameScript game, int index)
    {
        game.skyImage.sprite = game.skyThemes[index].sky;
        foreach (Graphic art in game.tintedArt)
            art.color *= game.skyThemes[index].tint;
    }

    static void SetPrivate(object target, string field, object value)
    {
        target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    }

    static void CallPrivate(object target, string method)
    {
        target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    }

    static void Capture(string scenePath, int width, int height, string outPath, Action<HanoiGameScript> setup, Action sceneSetup = null)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        LoadShared();
        sceneSetup?.Invoke();

        HanoiGameScript game = UnityEngine.Object.FindFirstObjectByType<HanoiGameScript>();
        if (game != null)
        {
            game.messageBox.SetActive(false);
            setup?.Invoke(game);
            CallPrivate(game, "UpdateHud");
        }

        RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        GameObject camObj = new GameObject("CaptureCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        cam.targetTexture = target;
        camObj.transform.position = new Vector3(0, 0, -10);

        foreach (Camera other in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (other != cam)
                other.enabled = false;
        }

        Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
            .Where(c => c.isRootCanvas).ToArray();
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
            canvases[i].worldCamera = cam;
            canvases[i].planeDistance = 5f + i;
        }

        // Edit mode in batch mode never runs Update, so do the layout passes by hand.
        for (int pass = 0; pass < 3; pass++)
        {
            foreach (CanvasScaler scaler in UnityEngine.Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None))
                typeof(CanvasScaler).GetMethod("Handle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(scaler, null);
            Canvas.ForceUpdateCanvases();
            foreach (AspectRatioFitter fitter in UnityEngine.Object.FindObjectsByType<AspectRatioFitter>(FindObjectsSortMode.None))
                typeof(AspectRatioFitter).GetMethod("UpdateRect", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(fitter, null);
            foreach (OrientationLayoutScript layout in UnityEngine.Object.FindObjectsByType<OrientationLayoutScript>(FindObjectsSortMode.None))
                typeof(OrientationLayoutScript).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(layout, null);
            foreach (HanoiBoardFitScript fit in UnityEngine.Object.FindObjectsByType<HanoiBoardFitScript>(FindObjectsSortMode.None))
                fit.Fit();
            Canvas.ForceUpdateCanvases();
        }

        cam.Render();

        RenderTexture.active = target;
        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(outPath, image.EncodeToPNG());
    }
}
