using UnityEngine;
using UnityEngine.EventSystems;

// Test cheat for phones without a keyboard: 5 quick taps give 50 stars
// (same as the G key in DevCheatsScript).
public class SecretTapsScript : MonoBehaviour, IPointerClickHandler
{
    public int tapsNeeded = 5;
    [Tooltip("Longest pause between two taps, in seconds.")]
    public float maxGap = 0.6f;

    private int taps = 0;
    private float lastTap = -10f;

    public void OnPointerClick(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        taps = now - lastTap <= maxGap ? taps + 1 : 1;
        lastTap = now;

        if (taps < tapsNeeded)
            return;

        taps = 0;
        StarWallet.Add(DevCheatsScript.GIFT_STARS);
        Debug.Log("Cheat: +" + DevCheatsScript.GIFT_STARS + " stars (now " + StarWallet.Stars + ")");
    }
}
