using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tower of Hanoi: move every disk from the left pole to the right pole,
// never putting a bigger disk on a smaller one. Disks can be dragged,
// or tapped: tap a pole to lift its top disk, tap another pole to drop it.
public class HanoiGameScript : MonoBehaviour
{
    [System.Serializable]
    public class SkyTheme
    {
        public string id;
        public Sprite sky;
        [Tooltip("Multiplied into the board art, trees and floor so they match the light.")]
        public Color tint = Color.white;
    }

    private const string MENU_SCENE = "StartScene";
    private const int TARGET_POLE = 2;
    public const int MAX_DISKS = 5;

    [Header("Board (towerOfHanoi.png pixels, from the bottom-left)")]
    public RectTransform diskContainer;
    [Tooltip("Smallest disk first.")]
    public Sprite[] diskSprites;
    public float[] poleX = { 250f, 674f, 1098f };
    public float floorY = 47f;
    public float liftY = 545f;
    public float diskHeight = 62f;
    public float minDiskWidth = 150f;
    public float maxDiskWidth = 390f;

    [Header("Sky themes (bought in the shop)")]
    public Image skyImage;
    public Graphic[] tintedArt;
    public SkyTheme[] skyThemes;

    [Header("HUD")]
    public TMP_Text timeText;
    public TMP_Text movesText;
    public GameObject messageBox;
    public TMP_Text messageText;
    [Tooltip("Message height from the top; on the win screen it moves up over the HUD, clear of the victory card.")]
    public float messageY = -190f;
    public float winMessageY = -70f;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject winPanel;
    public Image victoryImage;
    [Tooltip("Victory cards with 1, 2 and 3 stars.")]
    public Sprite[] victorySprites = new Sprite[3];
    public TMP_Text winTimeText;
    public TMP_Text winMovesText;
    public TMP_Text winMessageText;
    public GameObject moreDisksButton;
    [Tooltip("Watch an ad on the win screen to halve the time.")]
    public Button rewardedButton;
    [Tooltip("Stars this game added to the shop wallet.")]
    public TMP_Text winStarsText;

    [Header("Star criteria (bottom left)")]
    public GameObject criteriaPanel;
    [Tooltip("Rows for 3, 2 and 1 star.")]
    public TMP_Text[] criteriaTexts = new TMP_Text[3];

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource soundSource;
    public AudioClip pickSound;
    public AudioClip dropSound;
    public AudioClip buttonSound;
    public AudioClip victorySound;

    [Header("Stars")]
    [Tooltip("3 stars = minimum moves and at most this many seconds per move. 2 stars = up to 1.5x the moves and twice the time.")]
    public float secondsPerMove = 3f;

    private readonly List<HanoiDiskScript>[] poles =
    {
        new List<HanoiDiskScript>(),
        new List<HanoiDiskScript>(),
        new List<HanoiDiskScript>()
    };

    private int diskCount = 0;
    private int moves = 0;
    private float gameTime = 0f;
    private bool playing = false;
    private bool won = false;
    private bool rewardUsed = false;
    private int awardedStars = 0;

    private HanoiDiskScript selectedDisk;   // lifted by a tap
    private HanoiDiskScript draggedDisk;
    private Coroutine messageRoutine;

    private int MinMoves => (1 << diskCount) - 1;
    private float ThreeStarTime => MinMoves * secondsPerMove;
    private int TwoStarMoves => Mathf.FloorToInt(MinMoves * 1.5f);

    void Start()
    {
        Time.timeScale = 1f;
        ApplySavedAudioSettings();
        ApplySkyTheme();

        winPanel.SetActive(false);
        startPanel.SetActive(true);
        criteriaPanel.SetActive(false);

        if (messageBox != null)
            messageBox.SetActive(false);

        UpdateHud();
    }

    void OnEnable()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.rewardedAds.OnRewardEarned += HalveTime;
    }

    void OnDisable()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.rewardedAds.OnRewardEarned -= HalveTime;
    }

    void Update()
    {
        // Time.deltaTime is 0 while an ad is showing (timeScale = 0).
        if (playing)
            gameTime += Time.deltaTime;

        UpdateHud();
    }

    void UpdateHud()
    {
        if (timeText != null)
            timeText.text = FormatTime(gameTime);

        if (movesText != null)
            movesText.text = diskCount > 0 ? "Gājieni: " + moves + " / " + MinMoves : "Gājieni: 0";

        if (rewardedButton != null)
        {
            rewardedButton.gameObject.SetActive(!rewardUsed);
            rewardedButton.interactable =
                won &&
                AdManager.Instance != null &&
                AdManager.Instance.rewardedAds.IsReady;
        }
    }

    // ---------------------------------------------------------------- game

    public void StartGame(int count)
    {
        diskCount = Mathf.Clamp(count, 1, Mathf.Min(MAX_DISKS, diskSprites.Length));

        foreach (List<HanoiDiskScript> pole in poles)
        {
            foreach (HanoiDiskScript disk in pole)
                DestroyDisk(disk);
            pole.Clear();
        }

        selectedDisk = null;
        draggedDisk = null;

        // Biggest disk at the bottom of the left pole.
        for (int size = diskCount - 1; size >= 0; size--)
        {
            HanoiDiskScript disk = CreateDisk(size);
            disk.pole = 0;
            poles[0].Add(disk);
            PlaceOnPole(disk, false);
        }

        moves = 0;
        gameTime = 0f;
        rewardUsed = false;
        awardedStars = 0;
        won = false;
        playing = true;

        startPanel.SetActive(false);
        winPanel.SetActive(false);
        UpdateCriteria();
        criteriaPanel.SetActive(true);
    }

    HanoiDiskScript CreateDisk(int size)
    {
        GameObject obj = new GameObject(
            "Disk" + size,
            typeof(RectTransform),
            typeof(Image),
            typeof(HanoiDiskScript)
        );
        obj.transform.SetParent(diskContainer, false);

        RectTransform rect = (RectTransform)obj.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;

        float t = diskCount == 1 ? 0f : size / (float)(diskCount - 1);
        rect.sizeDelta = new Vector2(
            Mathf.Lerp(minDiskWidth, maxDiskWidth, t),
            diskHeight + 6f
        );

        Image image = obj.GetComponent<Image>();
        image.sprite = diskSprites[size];
        image.type = Image.Type.Sliced;

        HanoiDiskScript disk = obj.GetComponent<HanoiDiskScript>();
        disk.size = size;
        disk.game = this;
        return disk;
    }

    void DestroyDisk(HanoiDiskScript disk)
    {
        if (Application.isPlaying)
            Destroy(disk.gameObject);
        else
            DestroyImmediate(disk.gameObject);
    }

    HanoiDiskScript Top(int pole)
    {
        List<HanoiDiskScript> stack = poles[pole];
        return stack.Count > 0 ? stack[stack.Count - 1] : null;
    }

    // Puts the disk at its place on its pole: dropped from where it is now
    // (animate), or set straight there (new game).
    void PlaceOnPole(HanoiDiskScript disk, bool animate = true, AudioClip landSound = null)
    {
        int index = poles[disk.pole].IndexOf(disk);
        Vector2 target = new Vector2(
            poleX[disk.pole],
            floorY + diskHeight * (index + 0.5f)
        );

        if (animate)
        {
            disk.FallTo(target, () => PlaySound(landSound));
        }
        else
        {
            disk.StopFall();
            disk.Rect.anchoredPosition = target;
        }
    }

    void Lift(HanoiDiskScript disk)
    {
        disk.StopFall();
        disk.Rect.anchoredPosition = new Vector2(poleX[disk.pole], liftY);
        disk.transform.SetAsLastSibling();
    }

    int NearestPole(float x)
    {
        int nearest = 0;
        for (int i = 1; i < poleX.Length; i++)
        {
            if (Mathf.Abs(poleX[i] - x) < Mathf.Abs(poleX[nearest] - x))
                nearest = i;
        }
        return nearest;
    }

    void TryMove(HanoiDiskScript disk, int to)
    {
        HanoiDiskScript top = Top(to);

        if (top != null && top.size < disk.size)
        {
            PlaceOnPole(disk);
            StartCoroutine(FlashInvalid(disk.GetComponent<Image>()));
            ShowMessage("Lielāku disku nevar likt uz mazāka!");
            return;
        }

        poles[disk.pole].Remove(disk);
        disk.pole = to;
        poles[to].Add(disk);
        PlaceOnPole(disk, true, dropSound);

        moves++;

        if (poles[TARGET_POLE].Count == diskCount)
            Win();
    }

    IEnumerator FlashInvalid(Image image)
    {
        image.color = new Color(1f, 0.45f, 0.45f);
        yield return new WaitForSecondsRealtime(0.3f);
        if (image != null)
            image.color = Color.white;
    }

    // Test key L (DevCheatsScript): moves the whole tower to the target pole
    // in the minimum number of moves.
    public void DebugWin()
    {
        if (!playing)
            return;

        if (selectedDisk != null || draggedDisk != null)
        {
            selectedDisk = null;
            draggedDisk = null;
        }

        List<HanoiDiskScript> all = new List<HanoiDiskScript>();
        foreach (List<HanoiDiskScript> pole in poles)
        {
            all.AddRange(pole);
            pole.Clear();
        }
        all.Sort((a, b) => b.size.CompareTo(a.size));   // biggest at the bottom

        foreach (HanoiDiskScript disk in all)
        {
            disk.pole = TARGET_POLE;
            poles[TARGET_POLE].Add(disk);
            PlaceOnPole(disk, false);
        }

        moves = MinMoves;
        Win();
    }

    // ---------------------------------------------------------------- input

    public void PoleClicked(int pole)
    {
        if (!playing || draggedDisk != null)
            return;

        if (selectedDisk == null)
        {
            selectedDisk = Top(pole);
            if (selectedDisk == null)
                return;

            Lift(selectedDisk);
            PlaySound(pickSound);
        }
        else if (pole == selectedDisk.pole)
        {
            PlaceOnPole(selectedDisk);
            selectedDisk = null;
        }
        else
        {
            HanoiDiskScript disk = selectedDisk;
            selectedDisk = null;
            TryMove(disk, pole);
        }
    }

    public void BeginDrag(HanoiDiskScript disk, PointerEventData eventData)
    {
        if (!playing || disk != Top(disk.pole))
            return;

        if (selectedDisk != null && selectedDisk != disk)
            PlaceOnPole(selectedDisk);
        selectedDisk = null;

        draggedDisk = disk;
        disk.StopFall();
        disk.transform.SetAsLastSibling();
        PlaySound(pickSound);
        Drag(disk, eventData);
    }

    public void Drag(HanoiDiskScript disk, PointerEventData eventData)
    {
        if (disk != draggedDisk)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                diskContainer, eventData.position, eventData.pressEventCamera, out Vector2 local))
        {
            disk.Rect.anchoredPosition = local;
        }
    }

    public void EndDrag(HanoiDiskScript disk, PointerEventData eventData)
    {
        if (disk != draggedDisk)
            return;

        draggedDisk = null;

        int target = NearestPole(disk.Rect.anchoredPosition.x);
        if (target == disk.pole)
            PlaceOnPole(disk);
        else
            TryMove(disk, target);
    }

    // ---------------------------------------------------------------- ads

    public void WatchAdForTime()
    {
        PlayButtonSound();

        if (won && !rewardUsed && AdManager.Instance != null)
            AdManager.Instance.rewardedAds.ShowAd();
    }

    // Reward for a watched rewarded ad on the win screen, once per game.
    // A faster time can earn more stars, so the result is shown again.
    void HalveTime()
    {
        if (!won || rewardUsed)
            return;

        rewardUsed = true;
        gameTime *= 0.5f;
        ShowResult();
        AwardStars();
        ShowMessage("Laiks samazināts uz pusi!");
    }

    // ---------------------------------------------------------------- win

    void Win()
    {
        playing = false;
        won = true;

        ShowResult();
        AwardStars();
        moreDisksButton.SetActive(diskCount < MAX_DISKS);
        winPanel.SetActive(true);

        PlaySound(victorySound);
    }

    void ShowResult()
    {
        int stars = CalculateStars();

        victoryImage.sprite = victorySprites[stars - 1];
        winTimeText.text = FormatTime(gameTime);
        winMovesText.text = "Gājieni: " + moves + "  (minimums " + MinMoves + ")";
        winMessageText.text =
            stars == 3 ? "Perfekti!" :
            stars == 2 ? "Labs darbs!" :
            "Tev izdevās!";
    }

    // Pays the stars into the shop wallet. After the time is halved only
    // the extra stars are added.
    void AwardStars()
    {
        int stars = CalculateStars();
        if (stars > awardedStars)
        {
            StarWallet.Add(stars - awardedStars);
            awardedStars = stars;
        }

        if (winStarsText != null)
            winStarsText.text = "+" + awardedStars;
    }

    // Each star level needs both its moves and its time.
    int CalculateStars()
    {
        if (moves <= MinMoves && gameTime <= ThreeStarTime)
            return 3;

        if (moves <= TwoStarMoves && gameTime <= ThreeStarTime * 2f)
            return 2;

        return 1;
    }

    void UpdateCriteria()
    {
        criteriaTexts[0].text = MinMoves + " gājieni, laiks ≤ " + FormatTime(ThreeStarTime);
        criteriaTexts[1].text = "≤ " + TwoStarMoves + " gājieni, laiks ≤ " + FormatTime(ThreeStarTime * 2f);
        criteriaTexts[2].text = "tornis pārvietots";
    }

    // ---------------------------------------------------------------- buttons

    public void Retry()
    {
        PlayButtonSound();
        StartGame(diskCount);
    }

    public void MoreDisks()
    {
        PlayButtonSound();
        StartGame(diskCount + 1);
    }

    public void ChooseDisks()
    {
        PlayButtonSound();
        playing = false;
        won = false;
        winPanel.SetActive(false);
        criteriaPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MENU_SCENE);
    }

    public void PlayButtonSound()
    {
        PlaySound(buttonSound);
    }

    // ---------------------------------------------------------------- helpers

    void PlaySound(AudioClip clip)
    {
        if (soundSource != null && clip != null)
            soundSource.PlayOneShot(clip);
    }

    void ShowMessage(string text)
    {
        if (messageBox == null || messageText == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text));
    }

    IEnumerator MessageRoutine(string text)
    {
        messageText.text = text;
        ((RectTransform)messageBox.transform).anchoredPosition = new Vector2(0f, won ? winMessageY : messageY);
        messageBox.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        messageBox.SetActive(false);
        messageRoutine = null;
    }

    void ApplySkyTheme()
    {
        string selected = StarWallet.SelectedSky;
        foreach (SkyTheme theme in skyThemes)
        {
            if (theme.id != selected)
                continue;

            skyImage.sprite = theme.sky;
            foreach (Graphic art in tintedArt)
                art.color *= theme.tint;
        }
    }

    void ApplySavedAudioSettings()
    {
        if (musicSource != null)
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.3f);

        if (soundSource != null)
            soundSource.volume = PlayerPrefs.GetFloat("SoundVolume", 1f);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
