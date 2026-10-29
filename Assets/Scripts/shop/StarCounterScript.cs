using TMPro;
using UnityEngine;

// Shows how many stars the player has and keeps it up to date.
public class StarCounterScript : MonoBehaviour
{
    public TMP_Text starsText;
    [Tooltip("{0} is replaced by the number of stars.")]
    public string format = "{0}";

    void OnEnable()
    {
        StarWallet.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        StarWallet.OnChanged -= Refresh;
    }

    void Refresh()
    {
        if (starsText != null)
            starsText.text = string.Format(format, StarWallet.Stars);
    }
}
