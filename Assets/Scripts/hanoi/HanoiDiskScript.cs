using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// One disk of the tower. Input is passed on to HanoiGameScript, which knows the rules.
public class HanoiDiskScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public int size;    // 0 = smallest
    public int pole;
    [HideInInspector] public HanoiGameScript game;

    [Header("Falling (board pixels)")]
    public float gravity = 6000f;
    [Tooltip("Part of the landing speed kept for the bounce.")]
    public float bounce = 0.25f;

    private Coroutine fallRoutine;

    public RectTransform Rect => (RectTransform)transform;

    public void OnBeginDrag(PointerEventData eventData) => game.BeginDrag(this, eventData);
    public void OnDrag(PointerEventData eventData) => game.Drag(this, eventData);
    public void OnEndDrag(PointerEventData eventData) => game.EndDrag(this, eventData);

    // Tapping a disk counts as tapping its pole. Not called after a drag.
    public void OnPointerClick(PointerEventData eventData) => game.PoleClicked(pole);

    // Slides over to the pole and drops onto the stack with a small bounce.
    public void FallTo(Vector2 target, Action landed)
    {
        StopFall();

        // Edit mode (screenshots) has no coroutines.
        if (!Application.isPlaying)
        {
            Rect.anchoredPosition = target;
            return;
        }

        fallRoutine = StartCoroutine(Fall(target, landed));
    }

    public void StopFall()
    {
        if (fallRoutine != null)
        {
            StopCoroutine(fallRoutine);
            fallRoutine = null;
        }
    }

    IEnumerator Fall(Vector2 target, Action landed)
    {
        Vector2 position = Rect.anchoredPosition;

        // Dropped below its place (dragged low): it just pops up into place.
        if (position.y < target.y)
            position.y = target.y;

        float speed = 0f;
        bool bounced = false;

        while (true)
        {
            // Unscaled, so an ad pausing the game can't freeze a disk mid-air.
            float dt = Time.unscaledDeltaTime;
            position.x = Mathf.Lerp(position.x, target.x, 1f - Mathf.Exp(-25f * dt));
            speed += gravity * dt;
            position.y -= speed * dt;

            if (position.y <= target.y)
            {
                position.y = target.y;

                if (!bounced && speed > 600f)
                {
                    if (landed != null)
                        landed();
                    landed = null;
                    bounced = true;
                    speed = -speed * bounce;
                }
                else
                {
                    break;
                }
            }

            Rect.anchoredPosition = position;
            yield return null;
        }

        Rect.anchoredPosition = target;
        fallRoutine = null;

        // A drop too short to bounce lands here.
        if (landed != null)
            landed();
    }
}
