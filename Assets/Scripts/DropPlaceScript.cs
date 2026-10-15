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
        if (gameObjectsScript == null || gameObjectsScript.GameFinished)
            return;

        // Use the event's own button instead of Input.GetMouseButtonUp:
        // on a phone there is no real mouse, so that check is unreliable.
        if (eventData.pointerDrag == null ||
            eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!eventData.pointerDrag.CompareTag(tag))
        {
            gameObjectsScript.inRightPlace = false;
            PlayWrongPlacementSound();
            ReturnCarToStart(eventData.pointerDrag);
            return;
        }

        if (!TryPlaceCar(eventData.pointerDrag))
        {
            // Right spot, wrong rotation/size: leave the car here (it stays selected)
            // so it can be transformed and snaps in once it matches.
            gameObjectsScript.inRightPlace = false;
            PlayWrongPlacementSound();
        }
    }

    void Update()
    {
        if (gameObjectsScript == null || gameObjectsScript.GameFinished)
            return;

        // The selected car was dropped and is being transformed with the buttons.
        GameObject car = GameObjectsScript.lastDragged;
        if (car == null || GameObjectsScript.isDragging ||
            !car.CompareTag(tag) || gameObjectsScript.IsCarCorrectlyPlaced(car))
            return;

        RectTransform placeRect = GetComponent<RectTransform>();
        Vector2 carScreenPosition =
            RectTransformUtility.WorldToScreenPoint(Camera.main, car.transform.position);
        if (!RectTransformUtility.RectangleContainsScreenPoint(placeRect, carScreenPosition, Camera.main))
            return;

        if (TryPlaceCar(car))
        {
            // OnEndDrag already ran, so do its "placed" cleanup here.
            CanvasGroup carCanvasGroup = car.GetComponent<CanvasGroup>();
            if (carCanvasGroup != null)
                carCanvasGroup.blocksRaycasts = false;

            GameObjectsScript.lastDragged = null;
            gameObjectsScript.inRightPlace = false;
        }
    }

    private bool TryPlaceCar(GameObject car)
    {
        RectTransform draggedRect =
            car.GetComponent<RectTransform>();

        RectTransform placeRect =
            GetComponent<RectTransform>();

        if (draggedRect == null || placeRect == null)
            return false;

        placeZRot = placeRect.eulerAngles.z;
        carZRot = draggedRect.eulerAngles.z;

        // DeltaAngle correctly handles 359° vs 1° as a 2° difference.
        diffZRot = Mathf.Abs(Mathf.DeltaAngle(carZRot, placeZRot));

        placeSize = draggedRect.localScale;
        carSize = placeRect.localScale;

        xSizeDiff = Mathf.Abs(placeSize.x - carSize.x);
        ySizeDiff = Mathf.Abs(placeSize.y - carSize.y);

        bool correctRotation = diffZRot <= 7f;
        bool correctSize = xSizeDiff <= 0.08f && ySizeDiff <= 0.08f;

        if (!correctRotation || !correctSize)
            return false;

        Debug.Log("Car placed correctly!");

        gameObjectsScript.inRightPlace = true;
        gameObjectsScript.RegisterCorrectPlacement(car);

        // The final car can trigger WinGame(), so do not start any
        // movement animation after the game has already ended.
        draggedRect.anchoredPosition = placeRect.anchoredPosition;
        draggedRect.localScale = placeRect.localScale;
        draggedRect.localRotation = placeRect.localRotation;

        PlayCorrectPlacementSound(car.tag);

        if (!gameObjectsScript.GameFinished)
            StartCoroutine(CorrectPlacementBounce(draggedRect));

        return true;
    }

    private void PlayWrongPlacementSound()
    {
        if (gameObjectsScript.carSoundSource != null &&
            gameObjectsScript.sounds != null &&
            gameObjectsScript.sounds.Length > 4 &&
            gameObjectsScript.sounds[4] != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(
                gameObjectsScript.sounds[4]
            );
        }
    }

    private void PlayCorrectPlacementSound(string tagName)
    {
        if (gameObjectsScript.carSoundSource == null ||
            gameObjectsScript.sounds == null)
            return;

        int soundIndex = -1;

        switch (tagName)
        {
            case "Garbage": soundIndex = 1; break;
            case "Ambulance": soundIndex = 2; break;
            case "School": soundIndex = 3; break;
            case "b2": soundIndex = 5; break;
            case "CementaMasina": soundIndex = 6; break;
            case "e46": soundIndex = 7; break;
            case "e61": soundIndex = 8; break;
            case "Ekskavators": soundIndex = 9; break;
            case "Policija": soundIndex = 10; break;
            case "Traktors1": soundIndex = 11; break;
            case "Traktors5": soundIndex = 12; break;
            case "Ugunsdzeseji": soundIndex = 13; break;
        }

        if (soundIndex >= 0 && soundIndex < gameObjectsScript.sounds.Length &&
            gameObjectsScript.sounds[soundIndex] != null)
        {
            gameObjectsScript.carSoundSource.PlayOneShot(
                gameObjectsScript.sounds[soundIndex]
            );
        }
    }

    private void ReturnCarToStart(GameObject car)
    {
        switch (car.name)
        {
            case "GarbageTruck":
                gameObjectsScript.garbageTruck.GetComponent<RectTransform>().localPosition = gameObjectsScript.garbageTruckCoord;
                break;
            case "Medicine":
                gameObjectsScript.medicine.GetComponent<RectTransform>().localPosition = gameObjectsScript.medicineCoord;
                break;
            case "SchoolBus":
                gameObjectsScript.schoolBus.GetComponent<RectTransform>().localPosition = gameObjectsScript.schoolBusCoord;
                break;
            case "b2":
                gameObjectsScript.b2.GetComponent<RectTransform>().localPosition = gameObjectsScript.b2Coord;
                break;
            case "CementaMasina":
                gameObjectsScript.cementaMasina.GetComponent<RectTransform>().localPosition = gameObjectsScript.cementaMasinaCoord;
                break;
            case "e46":
                gameObjectsScript.e46.GetComponent<RectTransform>().localPosition = gameObjectsScript.e46Coord;
                break;
            case "e61":
                gameObjectsScript.e61.GetComponent<RectTransform>().localPosition = gameObjectsScript.e61Coord;
                break;
            case "Ekskavators":
                gameObjectsScript.ekskavators.GetComponent<RectTransform>().localPosition = gameObjectsScript.ekskavatorsCoord;
                break;
            case "Policija":
                gameObjectsScript.policija.GetComponent<RectTransform>().localPosition = gameObjectsScript.policijaCoord;
                break;
            case "Traktors1":
                gameObjectsScript.traktors1.GetComponent<RectTransform>().localPosition = gameObjectsScript.traktors1Coord;
                break;
            case "Traktors5":
                gameObjectsScript.traktors5.GetComponent<RectTransform>().localPosition = gameObjectsScript.traktors5Coord;
                break;
            case "Ugunsdzeseji":
                gameObjectsScript.ugunsdzeseji.GetComponent<RectTransform>().localPosition = gameObjectsScript.ugunsdzesejiCoord;
                break;
            default:
                Debug.Log("No matching object found for " + car.name);
                break;
        }
    }

    IEnumerator CorrectPlacementBounce(RectTransform car)
    {
        Vector2 finalPosition = car.anchoredPosition;
        float bounceHeight = 25f;
        float bounceDuration = 0.18f;

        for (int i = 0; i < 2; i++)
        {
            float time = 0f;
            while (time < bounceDuration)
            {
                if (gameObjectsScript == null || gameObjectsScript.GameFinished)
                    yield break;

                time += Time.deltaTime;
                float t = Mathf.Sin((time / bounceDuration) * Mathf.PI * 0.5f);
                car.anchoredPosition = Vector2.Lerp(
                    finalPosition,
                    finalPosition + Vector2.up * bounceHeight,
                    t);
                yield return null;
            }

            time = 0f;
            while (time < bounceDuration)
            {
                if (gameObjectsScript == null || gameObjectsScript.GameFinished)
                    yield break;

                time += Time.deltaTime;
                float t = 1f - Mathf.Cos((time / bounceDuration) * Mathf.PI * 0.5f);
                car.anchoredPosition = Vector2.Lerp(
                    finalPosition + Vector2.up * bounceHeight,
                    finalPosition,
                    t);
                yield return null;
            }
        }

        if (gameObjectsScript != null && !gameObjectsScript.GameFinished)
            car.anchoredPosition = finalPosition;
    }
}
