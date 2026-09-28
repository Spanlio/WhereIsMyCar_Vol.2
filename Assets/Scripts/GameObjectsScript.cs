using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameObjectsScript : MonoBehaviour
{
    // =========================================================
    // CARS
    // =========================================================

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

    // =========================================================
    // COORDINATES
    // =========================================================

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

    // =========================================================
    // AUDIO
    // =========================================================

    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;
    public AudioSource musicSource;

    // =========================================================
    // DRAGGING
    // =========================================================

    public static GameObject lastDragged = null;
    public static bool isDragging = false;

    [HideInInspector]
    public bool inRightPlace = false;

    // =========================================================
    // RANDOM SPAWN
    // =========================================================

    [Header("Random Spawn Setup")]
    public Transform carsContainer;
    public Transform placesContainer;
    public Transform carsSpawnPointsContainer;
    public Transform placeSpawnPointsContainer;

    public float maxRandomRotation = 5f;

    // =========================================================
    // TIMER
    // =========================================================

    [Header("Game Timer")]
    public TMP_Text timerText;

    private float gameTime = 0f;

    // =========================================================
    // GAME STATE
    // =========================================================

    [Header("Game State")]
    [SerializeField]
    private bool gameFinished = false;

    public bool GameFinished => gameFinished;

    // =========================================================
    // LIVES
    // =========================================================

    [Header("Lives")]
    public int startingLives = 3;
    public int lives = 3;

    public Image life1;
    public Image life2;
    public Image life3;

    // =========================================================
    // WIN SCREEN
    // =========================================================

    [Header("Win Screen")]
    public GameObject winPanel;
    public TMP_Text finalTimeText;
    public TMP_Text scoreText;

    // =========================================================
    // LOSE SCREEN
    // =========================================================

    [Header("Lose Screen")]
    public GameObject losePanel;
    public TMP_Text loseTimeText;

    // =========================================================
    // SCORE
    // =========================================================

    [Header("Score Time Limits")]
    public float threePointTime = 60f;
    public float twoPointTime = 120f;
    public float onePointTime = 180f;

    // =========================================================
    // PLACED CARS
    // =========================================================

    private HashSet<GameObject> correctlyPlacedCars =
        new HashSet<GameObject>();

    private const int TOTAL_CARS = 12;

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        lives = startingLives;

        gameTime = 0f;
        gameFinished = false;

        lastDragged = null;
        isDragging = false;

        inRightPlace = false;

        correctlyPlacedCars.Clear();

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        ResetLivesUI();

        RandomizeGame();
        ApplySavedAudioSettings();

        UpdateTimerDisplay();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (gameFinished)
            return;

        gameTime += Time.deltaTime;

        UpdateTimerDisplay();

        // =========================================================
        // DEBUG KEYS
        // =========================================================

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

    // =========================================================
    // TIMER
    // =========================================================

    void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int hours =
            Mathf.FloorToInt(gameTime / 3600f);

        int minutes =
            Mathf.FloorToInt((gameTime % 3600f) / 60f);

        int seconds =
            Mathf.FloorToInt(gameTime % 60f);

        timerText.text =
            string.Format(
                "{0:00}:{1:00}:{2:00}",
                hours,
                minutes,
                seconds
            );
    }

    // =========================================================
    // REGISTER CORRECT CAR
    // =========================================================

    public void RegisterCorrectPlacement(GameObject car)
    {
        if (gameFinished || car == null)
            return;

        if (!correctlyPlacedCars.Contains(car))
        {
            correctlyPlacedCars.Add(car);

            Debug.Log(
                "Correct cars: " +
                correctlyPlacedCars.Count +
                "/" +
                TOTAL_CARS
            );
        }

        inRightPlace = true;

        if (correctlyPlacedCars.Count >= TOTAL_CARS)
        {
            WinGame();
        }
    }

    // =========================================================
    // CHECK IF CAR IS CORRECT
    // =========================================================

    public bool IsCarCorrectlyPlaced(GameObject car)
    {
        if (car == null)
            return false;

        return correctlyPlacedCars.Contains(car);
    }

    // =========================================================
    // WIN
    // =========================================================

    private void WinGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;

        lastDragged = null;
        isDragging = false;

        int score = CalculateScore();

        Debug.Log(
            "WIN! All cars placed correctly!"
        );

        Debug.Log(
            "Final time: " +
            FormatTime(gameTime)
        );

        if (winPanel != null)
            winPanel.SetActive(true);

        if (finalTimeText != null)
        {
            finalTimeText.text =
                "Time: " + FormatTime(gameTime);
        }

        if (scoreText != null)
        {
            scoreText.text =
                "Score: " + score + " / 3";
        }
    }

    // =========================================================
    // LOSE LIFE
    // =========================================================

    public void LoseLife()
    {
        if (gameFinished)
            return;

        if (lives <= 0)
            return;

        lives--;

        Debug.Log(
            "LIFE LOST! Remaining: " +
            lives
        );

        UpdateLivesUI();

        // Play losing-life sound if assigned.
        if (carSoundSource != null &&
            sounds != null &&
            sounds.Length > 14 &&
            sounds[14] != null)
        {
            carSoundSource.PlayOneShot(
                sounds[14]
            );
        }

        // Last heart lost.
        if (lives <= 0)
        {
            LoseGame();
        }
    }

    // =========================================================
    // RESET HEART UI
    // =========================================================

    private void ResetLivesUI()
    {
        if (life1 != null)
            life1.gameObject.SetActive(true);

        if (life2 != null)
            life2.gameObject.SetActive(true);

        if (life3 != null)
            life3.gameObject.SetActive(true);
    }

    // =========================================================
    // UPDATE HEART UI
    // =========================================================

    private void UpdateLivesUI()
    {
        if (life1 != null)
            life1.gameObject.SetActive(lives >= 1);

        if (life2 != null)
            life2.gameObject.SetActive(lives >= 2);

        if (life3 != null)
            life3.gameObject.SetActive(lives >= 3);
    }

    // =========================================================
    // LOSE GAME
    // =========================================================

    private void LoseGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;

        lastDragged = null;
        isDragging = false;

        Debug.Log(
            "GAME OVER! All hearts lost."
        );

        if (losePanel != null)
            losePanel.SetActive(true);

        if (loseTimeText != null)
        {
            loseTimeText.text =
                "Time: " + FormatTime(gameTime);
        }
    }

    // =========================================================
    // SCORE
    // =========================================================

    private int CalculateScore()
    {
        if (gameTime <= threePointTime)
            return 3;

        if (gameTime <= twoPointTime)
            return 2;

        if (gameTime <= onePointTime)
            return 1;

        return 0;
    }

    // =========================================================
    // FORMAT TIME
    // =========================================================

    private string FormatTime(float time)
    {
        int hours =
            Mathf.FloorToInt(time / 3600f);

        int minutes =
            Mathf.FloorToInt((time % 3600f) / 60f);

        int seconds =
            Mathf.FloorToInt(time % 60f);

        return string.Format(
            "{0:00}:{1:00}:{2:00}",
            hours,
            minutes,
            seconds
        );
    }

    // =========================================================
    // RANDOMIZE GAME
    // =========================================================

    void RandomizeGame()
    {
        List<GameObject> cars =
            new List<GameObject>
            {
                garbageTruck,
                medicine,
                schoolBus,
                b2,
                cementaMasina,
                e46,
                e61,
                ekskavators,
                policija,
                traktors1,
                traktors5,
                ugunsdzeseji
            };

        List<GameObject> places =
            new List<GameObject>();

        if (placesContainer != null)
        {
            for (int i = 0;
                 i < placesContainer.childCount;
                 i++)
            {
                GameObject child =
                    placesContainer
                        .GetChild(i)
                        .gameObject;

                if (child.GetComponent<DropPlaceScript>() != null)
                    places.Add(child);
            }
        }

        List<Transform> carSpawnPoints =
            GetDirectChildren(
                carsSpawnPointsContainer
            );

        List<Transform> placeSpawnPoints =
            GetDirectChildren(
                placeSpawnPointsContainer
            );

        if (cars.Count != TOTAL_CARS)
        {
            Debug.LogError(
                "Expected 12 cars, found " +
                cars.Count
            );

            return;
        }

        if (places.Count != TOTAL_CARS)
        {
            Debug.LogError(
                "Expected 12 places, found " +
                places.Count
            );

            return;
        }

        if (carSpawnPoints.Count < TOTAL_CARS)
        {
            Debug.LogError(
                "Not enough car spawn points!"
            );

            return;
        }

        if (placeSpawnPoints.Count < TOTAL_CARS)
        {
            Debug.LogError(
                "Not enough place spawn points!"
            );

            return;
        }

        Shuffle(carSpawnPoints);
        Shuffle(placeSpawnPoints);

        for (int i = 0; i < cars.Count; i++)
        {
            RectTransform carRect =
                cars[i].GetComponent<RectTransform>();

            if (carRect == null)
                continue;

            carRect.position =
                carSpawnPoints[i].position;

            float rotation =
                Random.Range(
                    -maxRandomRotation,
                    maxRandomRotation
                );

            carRect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation
                );
        }

        for (int i = 0; i < places.Count; i++)
        {
            RectTransform placeRect =
                places[i].GetComponent<RectTransform>();

            if (placeRect == null)
                continue;

            placeRect.position =
                placeSpawnPoints[i].position;

            float rotation =
                Random.Range(
                    -maxRandomRotation,
                    maxRandomRotation
                );

            placeRect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation
                );
        }

        SaveCarStartingPositions();
    }

    // =========================================================
    // CHILDREN
    // =========================================================

    List<Transform> GetDirectChildren(
        Transform parent)
    {
        List<Transform> result =
            new List<Transform>();

        if (parent == null)
            return result;

        for (int i = 0;
             i < parent.childCount;
             i++)
        {
            result.Add(
                parent.GetChild(i)
            );
        }

        return result;
    }

    // =========================================================
    // SHUFFLE
    // =========================================================

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1;
             i > 0;
             i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            T temp =
                list[i];

            list[i] =
                list[randomIndex];

            list[randomIndex] =
                temp;
        }
    }

    // =========================================================
    // SAVE POSITIONS
    // =========================================================

    void SaveCarStartingPositions()
    {
        garbageTruckCoord =
            garbageTruck.GetComponent<RectTransform>().localPosition;

        medicineCoord =
            medicine.GetComponent<RectTransform>().localPosition;

        schoolBusCoord =
            schoolBus.GetComponent<RectTransform>().localPosition;

        b2Coord =
            b2.GetComponent<RectTransform>().localPosition;

        cementaMasinaCoord =
            cementaMasina.GetComponent<RectTransform>().localPosition;

        e46Coord =
            e46.GetComponent<RectTransform>().localPosition;

        e61Coord =
            e61.GetComponent<RectTransform>().localPosition;

        ekskavatorsCoord =
            ekskavators.GetComponent<RectTransform>().localPosition;

        policijaCoord =
            policija.GetComponent<RectTransform>().localPosition;

        traktors1Coord =
            traktors1.GetComponent<RectTransform>().localPosition;

        traktors5Coord =
            traktors5.GetComponent<RectTransform>().localPosition;

        ugunsdzesejiCoord =
            ugunsdzeseji.GetComponent<RectTransform>().localPosition;
    }

    // =========================================================
    // AUDIO SETTINGS
    // =========================================================

    void ApplySavedAudioSettings()
    {
        float soundVolume =
            PlayerPrefs.GetFloat(
                "SoundVolume",
                1f
            );

        float musicVolume =
            PlayerPrefs.GetFloat(
                "MusicVolume",
                0.3f
            );

        if (carSoundSource != null)
            carSoundSource.volume =
                soundVolume;

        if (musicSource != null)
            musicSource.volume =
                musicVolume;
    }
}
