using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, carZRot, diffZRot;
    private Vector3 placeSize, carSize;
    private float xSizeDiff, ySizeDiff;

    public GameObjectsScript gameObjectsScript;

    void Awake()
    {
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if ((eventData.pointerDrag != null) &&
            Input.GetMouseButtonUp(0) &&
            (!Input.GetMouseButton(2)))
        {
            if (eventData.pointerDrag.tag.Equals(tag))
            {
                placeZRot =
                    eventData.pointerDrag.GetComponent<RectTransform>().transform.eulerAngles.z;

                carZRot =
                    GetComponent<RectTransform>().transform.eulerAngles.z;

                diffZRot = Mathf.Abs(placeZRot - carZRot);

                Debug.Log("Diff Z Rot: " + diffZRot);

                placeSize =
                    eventData.pointerDrag.GetComponent<RectTransform>().localScale;

                carSize =
                    GetComponent<RectTransform>().localScale;

                xSizeDiff = Mathf.Abs(placeSize.x - carSize.x);
                ySizeDiff = Mathf.Abs(placeSize.y - carSize.y);

                Debug.Log("Diff X Size: " + xSizeDiff);
                Debug.Log("Diff Y Size: " + ySizeDiff);

                if ((diffZRot <= 7 ||
                    (diffZRot >= 353 && diffZRot <= 360)) &&
                    (xSizeDiff <= 0.08f && ySizeDiff <= 0.08f))
                {
                    Debug.Log("Car placed correctly!");

                    gameObjectsScript.inRightPlace = true;

                    gameObjectsScript.RegisterCorrectPlacement(eventData.pointerDrag);

                    // Get the dragged object's RectTransform
                    RectTransform draggedObject =
                        eventData.pointerDrag.GetComponent<RectTransform>();

                    // Move object to correct position
                    draggedObject.anchoredPosition =
                        GetComponent<RectTransform>().anchoredPosition;

                    // Match size
                    draggedObject.localScale =
                        GetComponent<RectTransform>().localScale;

                    // Match rotation
                    draggedObject.localRotation =
                        GetComponent<RectTransform>().localRotation;

                    // BOUNCE TWICE
                    StartCoroutine(CorrectPlacementBounce(draggedObject));

                    // CORRECT PLACEMENT SOUNDS
                    switch (eventData.pointerDrag.tag)
                    {
                        // Existing objects
                        case "Garbage":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[1]);
                            break;

                        case "Ambulance":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[2]);
                            break;

                        case "School":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[3]);
                            break;

                        // New objects
                        case "B2":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[5]);
                            break;

                        case "CementaMasina":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[6]);
                            break;

                        case "E46":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[7]);
                            break;

                        case "E61":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[8]);
                            break;

                        case "Ekskavators":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[9]);
                            break;

                        case "Policija":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[10]);
                            break;

                        case "Traktors1":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[11]);
                            break;

                        case "Traktors5":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[12]);
                            break;

                        case "Ugunsdzeseji":
                            gameObjectsScript.carSoundSource.PlayOneShot(
                                gameObjectsScript.sounds[13]);
                            break;

                        default:
                            Debug.Log(
                                "No matching tag found for the dropped object.");
                            break;
                    }
                }
            }
            else
            {
                gameObjectsScript.inRightPlace = false;

                // WRONG PLACEMENT SOUND
                gameObjectsScript.carSoundSource.PlayOneShot(
                    gameObjectsScript.sounds[4]);

                // RETURN OBJECT TO STARTING POSITION
                switch (eventData.pointerDrag.name)
                {
                    // Existing objects
                    case "GarbageTruck":
                        gameObjectsScript.garbageTruck
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.garbageTruckCoord;
                        break;

                    case "Medicine":
                        gameObjectsScript.medicine
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.medicineCoord;
                        break;

                    case "SchoolBus":
                        gameObjectsScript.schoolBus
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.schoolBusCoord;
                        break;

                    // New objects
                    case "b2":
                        gameObjectsScript.b2
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.b2Coord;
                        break;

                    case "CementaMasina":
                        gameObjectsScript.cementaMasina
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.cementaMasinaCoord;
                        break;

                    case "e46":
                        gameObjectsScript.e46
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.e46Coord;
                        break;

                    case "e61":
                        gameObjectsScript.e61
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.e61Coord;
                        break;

                    case "Ekskavators":
                        gameObjectsScript.ekskavators
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.ekskavatorsCoord;
                        break;

                    case "Policija":
                        gameObjectsScript.policija
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.policijaCoord;
                        break;

                    case "Traktors1":
                        gameObjectsScript.traktors1
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.traktors1Coord;
                        break;

                    case "Traktors5":
                        gameObjectsScript.traktors5
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.traktors5Coord;
                        break;

                    case "Ugunsdzeseji":
                        gameObjectsScript.ugunsdzeseji
                            .GetComponent<RectTransform>()
                            .localPosition =
                            gameObjectsScript.ugunsdzesejiCoord;
                        break;

                    default:
                        Debug.Log(
                            "No matching object found for the dropped object.");
                        break;
                }
            }
        }
    }

    IEnumerator CorrectPlacementBounce(RectTransform car)
    {
        Vector2 finalPosition = car.anchoredPosition;

        float bounceHeight = 25f;
        float bounceDuration = 0.18f;

        // Two bounces
        for (int i = 0; i < 2; i++)
        {
            // BOUNCE UP
            float time = 0f;

            while (time < bounceDuration)
            {
                time += Time.deltaTime;

                float t = time / bounceDuration;

                // Smooth movement
                t = Mathf.Sin(t * Mathf.PI * 0.5f);

                car.anchoredPosition = Vector2.Lerp(
                    finalPosition,
                    finalPosition + Vector2.up * bounceHeight,
                    t
                );

                yield return null;
            }

            // BOUNCE DOWN
            time = 0f;

            while (time < bounceDuration)
            {
                time += Time.deltaTime;

                float t = time / bounceDuration;

                // Smooth movement
                t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

                car.anchoredPosition = Vector2.Lerp(
                    finalPosition + Vector2.up * bounceHeight,
                    finalPosition,
                    t
                );

                yield return null;
            }
        }

        // Make absolutely sure the car ends at its correct position
        car.anchoredPosition = finalPosition;
    }
}