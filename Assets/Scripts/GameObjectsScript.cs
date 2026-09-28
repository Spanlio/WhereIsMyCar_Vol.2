using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameObjectsScript : MonoBehaviour
{
    public GameObject garbageTruck;
    public GameObject medicine;
    public GameObject schoolBus;
    public GameObject b2;
    public GameObject cementaMasina;
    public GameObject e46;
    public GameObject e61;
    public GameObject ekskavators;
    public GameObject policija;
    public GameObject traktors1;
    public GameObject traktors5;
    public GameObject ugunsdzeseji;

    [HideInInspector] public Vector2 garbageTruckCoord;
    [HideInInspector] public Vector2 medicineCoord;
    [HideInInspector] public Vector2 schoolBusCoord;
    [HideInInspector] public Vector2 b2Coord;
    [HideInInspector] public Vector2 cementaMasinaCoord;
    [HideInInspector] public Vector2 e46Coord;
    [HideInInspector] public Vector2 e61Coord;
    [HideInInspector] public Vector2 ekskavatorsCoord;
    [HideInInspector] public Vector2 policijaCoord;
    [HideInInspector] public Vector2 traktors1Coord;
    [HideInInspector] public Vector2 traktors5Coord;
    [HideInInspector] public Vector2 ugunsdzesejiCoord;

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;
    public AudioSource musicSource;

    public static GameObject lastDragged = null;
    public static bool isDragging = false;

    [HideInInspector] public bool inRightPlace = false;

    [Header("Random Spawn Setup")]
    public Transform carsContainer;
    public Transform placesContainer;
    public Transform carsSpawnPointsContainer;
    public Transform placeSpawnPointsContainer;
    public float maxRandomRotation = 5f;

    [Header("Game Timer")]
    public TMP_Text timerText;
    private float gameTime = 0f;

    [Header("Game State")]
    [SerializeField] private bool gameFinished = false;
    public bool GameFinished => gameFinished;

    [Header("Lives")]
    public int startingLives = 3;
    public int lives = 3;
    public Image life1;
    public Image life2;
    public Image life3;

    [Header("Shared End Game Panel")]
    public EndGamePanelScript endGamePanel;

    [Tooltip("Maximum number of destroyed cars allowed before DNF. The 4th destroyed car causes DNF.")]
    public int maxCarsBeforeDNF = 3;

    [Header("Score Time Limits")]
    public float threePointTime = 60f;
    public float twoPointTime = 120f;
    public float onePointTime = 180f;

    [Header("Car Destruction Conditions")]
    [SerializeField] private int destroyedCars = 0;
    public int DestroyedCars => destroyedCars;

    private HashSet<GameObject> correctlyPlacedCars = new HashSet<GameObject>();
    private HashSet<GameObject> destroyedCarObjects = new HashSet<GameObject>();
    private const int TOTAL_CARS = 12;

    void Start()
    {
        Time.timeScale = 1f;
        lives = startingLives;
        destroyedCars = 0;
        gameTime = 0f;
        gameFinished = false;
        lastDragged = null;
        isDragging = false;
        inRightPlace = false;
        correctlyPlacedCars.Clear();
        destroyedCarObjects.Clear();

        if (endGamePanel != null)
            endGamePanel.Hide();

        ResetLivesUI();
        RandomizeGame();
        ApplySavedAudioSettings();
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (gameFinished)
            return;

        gameTime += Time.deltaTime;
        UpdateTimerDisplay();

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("DEBUG: Instant Lose");
            LoseGame();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("DEBUG: Instant Win");
            WinGame();
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int hours = Mathf.FloorToInt(gameTime / 3600f);
        int minutes = Mathf.FloorToInt((gameTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void RegisterCorrectPlacement(GameObject car)
    {
        if (gameFinished || car == null)
            return;

        if (!correctlyPlacedCars.Contains(car))
        {
            correctlyPlacedCars.Add(car);
            Debug.Log("Correct cars: " + correctlyPlacedCars.Count + "/" + (TOTAL_CARS - destroyedCars));
        }

        inRightPlace = true;

        int requiredCars = TOTAL_CARS - destroyedCars;

        if (correctlyPlacedCars.Count >= requiredCars)
            WinGame();
    }

    public bool IsCarCorrectlyPlaced(GameObject car)
    {
        return car != null && correctlyPlacedCars.Contains(car);
    }

    public void RegisterCarDestroyed(GameObject car)
    {
        if (gameFinished || car == null)
            return;

        if (!destroyedCarObjects.Add(car))
            return;

        destroyedCars++;

        Debug.Log("CAR DESTROYED: " + car.name + " | Destroyed cars: " + destroyedCars + "/" + maxCarsBeforeDNF);

        if (destroyedCars > maxCarsBeforeDNF)
            DNFGame();
    }

    private void WinGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;
        lastDragged = null;
        isDragging = false;
        FreezeGameplay();

        int score = CalculateScore();
        string formattedTime = FormatTime(gameTime);

        Debug.Log("WIN! All remaining cars placed correctly!");
        Debug.Log("Final time: " + formattedTime);
        Debug.Log("Destroyed cars: " + destroyedCars);
        Debug.Log("Stars: " + score + "/3");

        if (endGamePanel != null)
            endGamePanel.ShowWin(formattedTime, score);
    }

    public void LoseLife()
    {
        if (gameFinished || lives <= 0)
            return;

        lives--;
        Debug.Log("LIFE LOST! Remaining: " + lives);
        UpdateLivesUI();

        if (carSoundSource != null && sounds != null && sounds.Length > 14 && sounds[14] != null)
            carSoundSource.PlayOneShot(sounds[14]);

        if (lives <= 0)
            LoseGame();
    }

    private void ResetLivesUI()
    {
        if (life1 != null) life1.gameObject.SetActive(true);
        if (life2 != null) life2.gameObject.SetActive(true);
        if (life3 != null) life3.gameObject.SetActive(true);
    }

    private void UpdateLivesUI()
    {
        if (life1 != null) life1.gameObject.SetActive(lives >= 1);
        if (life2 != null) life2.gameObject.SetActive(lives >= 2);
        if (life3 != null) life3.gameObject.SetActive(lives >= 3);
    }

    private void LoseGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;
        lastDragged = null;
        isDragging = false;
        FreezeGameplay();

        string formattedTime = FormatTime(gameTime);
        Debug.Log("GAME OVER! All hearts lost.");

        if (endGamePanel != null)
            endGamePanel.ShowLose(formattedTime);
    }

    private void DNFGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;
        lastDragged = null;
        isDragging = false;
        FreezeGameplay();

        string formattedTime = FormatTime(gameTime);

        Debug.Log("DNF! Too many cars were destroyed.");
        Debug.Log("Destroyed cars: " + destroyedCars);

        if (endGamePanel != null)
            endGamePanel.ShowDNF(formattedTime);
    }

    private void FreezeGameplay()
    {
        Animator[] animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (Animator animator in animators)
        {
            if (animator != null)
                animator.enabled = false;
        }

        lastDragged = null;
        isDragging = false;
        inRightPlace = false;
    }

    private int CalculateScore()
    {
        int timeScore;

        if (gameTime <= threePointTime)
            timeScore = 3;
        else if (gameTime <= twoPointTime)
            timeScore = 2;
        else if (gameTime <= onePointTime)
            timeScore = 1;
        else
            timeScore = 0;

        if (destroyedCars >= 1)
            timeScore = Mathf.Min(timeScore, 2);

        if (destroyedCars >= 2)
            timeScore = Mathf.Min(timeScore, 1);

        return Mathf.Clamp(timeScore, 0, 3);
    }

    private string FormatTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    void RandomizeGame()
    {
        List<GameObject> cars = new List<GameObject>
        {
            garbageTruck, medicine, schoolBus, b2, cementaMasina, e46,
            e61, ekskavators, policija, traktors1, traktors5, ugunsdzeseji
        };

        List<GameObject> places = new List<GameObject>();

        if (placesContainer != null)
        {
            for (int i = 0; i < placesContainer.childCount; i++)
            {
                GameObject child = placesContainer.GetChild(i).gameObject;
                if (child.GetComponent<DropPlaceScript>() != null)
                    places.Add(child);
            }
        }

        List<Transform> carSpawnPoints = GetDirectChildren(carsSpawnPointsContainer);
        List<Transform> placeSpawnPoints = GetDirectChildren(placeSpawnPointsContainer);

        if (cars.Count != TOTAL_CARS || places.Count != TOTAL_CARS || carSpawnPoints.Count < TOTAL_CARS || placeSpawnPoints.Count < TOTAL_CARS)
        {
            Debug.LogError("Randomization setup does not contain the expected 12 cars, 12 places and enough spawn points.");
            return;
        }

        Shuffle(carSpawnPoints);
        Shuffle(placeSpawnPoints);

        for (int i = 0; i < cars.Count; i++)
        {
            RectTransform carRect = cars[i].GetComponent<RectTransform>();
            if (carRect == null) continue;
            carRect.position = carSpawnPoints[i].position;
            carRect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-maxRandomRotation, maxRandomRotation));
        }

        for (int i = 0; i < places.Count; i++)
        {
            RectTransform placeRect = places[i].GetComponent<RectTransform>();
            if (placeRect == null) continue;
            placeRect.position = placeSpawnPoints[i].position;
            placeRect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-maxRandomRotation, maxRandomRotation));
        }

        SaveCarStartingPositions();
    }

    List<Transform> GetDirectChildren(Transform parent)
    {
        List<Transform> result = new List<Transform>();
        if (parent == null) return result;
        for (int i = 0; i < parent.childCount; i++) result.Add(parent.GetChild(i));
        return result;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void SaveCarStartingPositions()
    {
        garbageTruckCoord = garbageTruck.GetComponent<RectTransform>().localPosition;
        medicineCoord = medicine.GetComponent<RectTransform>().localPosition;
        schoolBusCoord = schoolBus.GetComponent<RectTransform>().localPosition;
        b2Coord = b2.GetComponent<RectTransform>().localPosition;
        cementaMasinaCoord = cementaMasina.GetComponent<RectTransform>().localPosition;
        e46Coord = e46.GetComponent<RectTransform>().localPosition;
        e61Coord = e61.GetComponent<RectTransform>().localPosition;
        ekskavatorsCoord = ekskavators.GetComponent<RectTransform>().localPosition;
        policijaCoord = policija.GetComponent<RectTransform>().localPosition;
        traktors1Coord = traktors1.GetComponent<RectTransform>().localPosition;
        traktors5Coord = traktors5.GetComponent<RectTransform>().localPosition;
        ugunsdzesejiCoord = ugunsdzeseji.GetComponent<RectTransform>().localPosition;
    }

    void ApplySavedAudioSettings()
    {
        float soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
        if (carSoundSource != null) carSoundSource.volume = soundVolume;
        if (musicSource != null) musicSource.volume = musicVolume;
    }
}
