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

    // Existing coordinates
    [HideInInspector]
    public Vector2 garbageTruckCoord;

    [HideInInspector]
    public Vector2 medicineCoord;

    [HideInInspector]
    public Vector2 schoolBusCoord;

    // New coordinates
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

    [HideInInspector]
    public bool inRightPlace = false;

    public static GameObject lastDragged = null;
    public static bool isDragging = false;


    void Awake()
    {
        // Existing objects
        garbageTruckCoord =
            garbageTruck.GetComponent<RectTransform>().localPosition;

        medicineCoord =
            medicine.GetComponent<RectTransform>().localPosition;

        schoolBusCoord =
            schoolBus.GetComponent<RectTransform>().localPosition;

        // New objects
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
}