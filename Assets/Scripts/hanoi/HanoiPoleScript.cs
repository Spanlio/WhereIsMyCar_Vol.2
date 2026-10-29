using UnityEngine;
using UnityEngine.EventSystems;

// Invisible tap area around a pole.
public class HanoiPoleScript : MonoBehaviour, IPointerClickHandler
{
    public int poleIndex;
    public HanoiGameScript game;

    public void OnPointerClick(PointerEventData eventData)
    {
        game.PoleClicked(poleIndex);
    }
}
