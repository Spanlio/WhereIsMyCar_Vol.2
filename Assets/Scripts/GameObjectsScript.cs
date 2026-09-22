using System.Collections.Generic;
using UnityEngine;

public class GameObjectsScript : MonoBehaviour
{
    // Existing objects
    public GameObject garbageTruck;
    public GameObject medicine;
    public GameObject schoolBus;

    // New objects
    public GameObject b2;
    public GameObject cementaMasina;
    public GameObject e46;
    public GameObject e61;
    public GameObject ekskavators;
    public GameObject policija;
    public GameObject traktors1;
    public GameObject traktors5;
    public GameObject ugunsdzeseji;


    // Starting coordinates
    [HideInInspector]
    public Vector2 garbageTruckCoord;

    [HideInInspector]
    public Vector2 medicineCoord;

    [HideInInspector]
    public Vector2 schoolBusCoord;

    [HideInInspector]
    public Vector2 b2Coord;

    [HideInInspector]
    public Vector2 cementaMasinaCoord;

    [HideInInspector]
    public Vector2 e46Coord;

    [HideInInspector]
    public Vector2 e61Coord;

    [HideInInspector]
    public Vector2 ekskavatorsCoord;

    [HideInInspector]
    public Vector2 policijaCoord;

    [HideInInspector]
    public Vector2 traktors1Coord;

    [HideInInspector]
    public Vector2 traktors5Coord;

    [HideInInspector]
    public Vector2 ugunsdzesejiCoord;


    public Canvas canvas;
    public AudioSource carSoundSource;
    public AudioClip[] sounds;
    public AudioSource musicSource;


    [HideInInspector]
    public bool inRightPlace = false;

    public static GameObject lastDragged = null;
    public static bool isDragging = false;


    [Header("Random Spawn Setup")]

    public Transform carsContainer;
    public Transform placesContainer;
    public Transform carsSpawnPointsContainer;
    public Transform placeSpawnPointsContainer;

    [Tooltip("Maximum random rotation added to cars and places.")]
    public float maxRandomRotation = 5f;


    void Start()
    {
        RandomizeGame();

        ApplySavedAudioSettings();
    }


    void RandomizeGame()
    {
        // Get the 12 actual cars
        List<GameObject> cars = new List<GameObject>
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


        // Get the 12 destination objects.
        // Only objects with DropPlaceScript count,
        // so BackgroundImage is automatically ignored.
        List<GameObject> places = new List<GameObject>();

        if (placesContainer != null)
        {
            for (int i = 0; i < placesContainer.childCount; i++)
            {
                GameObject child = placesContainer.GetChild(i).gameObject;

                if (child.GetComponent<DropPlaceScript>() != null)
                {
                    places.Add(child);
                }
            }
        }


        // Get the 17 car spawn points
        List<Transform> carSpawnPoints =
            GetDirectChildren(carsSpawnPointsContainer);

        // Get the 17 place spawn points
        List<Transform> placeSpawnPoints =
            GetDirectChildren(placeSpawnPointsContainer);


        // Validate setup
        if (cars.Count != 12)
        {
            Debug.LogError(
                "Expected 12 cars, but found " + cars.Count + "."
            );
            return;
        }

        if (places.Count != 12)
        {
            Debug.LogError(
                "Expected 12 Places, but found " + places.Count + "."
            );
            return;
        }

        if (carSpawnPoints.Count != 17)
        {
            Debug.LogError(
                "Expected 17 car spawn points, but found " +
                carSpawnPoints.Count + "."
            );
            return;
        }

        if (placeSpawnPoints.Count != 17)
        {
            Debug.LogError(
                "Expected 17 place spawn points, but found " +
                placeSpawnPoints.Count + "."
            );
            return;
        }


        // Shuffle both lists independently
        Shuffle(carSpawnPoints);
        Shuffle(placeSpawnPoints);


        // --------------------------------------------------
        // RANDOMIZE CARS
        // --------------------------------------------------

        for (int i = 0; i < cars.Count; i++)
        {
            RectTransform carRect =
                cars[i].GetComponent<RectTransform>();

            if (carRect == null)
            {
                Debug.LogError(
                    cars[i].name + " does not have a RectTransform."
                );
                continue;
            }


            // Move car to its randomly selected spawn point
            carRect.position = carSpawnPoints[i].position;


            // Add a small random rotation
            float randomRotation =
                Random.Range(
                    -maxRandomRotation,
                    maxRandomRotation
                );

            carRect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    randomRotation
                );
        }


        // --------------------------------------------------
        // RANDOMIZE PLACES
        // --------------------------------------------------

        for (int i = 0; i < places.Count; i++)
        {
            RectTransform placeRect =
                places[i].GetComponent<RectTransform>();

            if (placeRect == null)
            {
                Debug.LogError(
                    places[i].name +
                    " does not have a RectTransform."
                );
                continue;
            }


            // Move destination to its randomly selected point
            placeRect.position = placeSpawnPoints[i].position;


            // Add a small random rotation
            float randomRotation =
                Random.Range(
                    -maxRandomRotation,
                    maxRandomRotation
                );

            placeRect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    randomRotation
                );
        }


        // Save the randomized starting positions
        SaveCarStartingPositions();


        Debug.Log("Randomization complete.");
    }


    List<Transform> GetDirectChildren(Transform parent)
    {
        List<Transform> children = new List<Transform>();

        if (parent == null)
        {
            return children;
        }


        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
        }


        return children;
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

    void ApplySavedAudioSettings()
    {
        float savedSoundVolume =
            PlayerPrefs.GetFloat("SoundVolume", 1f);

        float savedMusicVolume =
            PlayerPrefs.GetFloat("MusicVolume", 0.3f);


        if (carSoundSource != null)
        {
            carSoundSource.volume = savedSoundVolume;
        }


        if (musicSource != null)
        {
            musicSource.volume = savedMusicVolume;
        }
    }
}