using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in the shop. Unlocks are bought once; sky themes are bought once
// and can then be picked (only one is in use at a time).
public class ShopItemScript : MonoBehaviour
{
    public enum Kind { Unlock, SkyTheme }

    public string id;
    public int price;
    public Kind kind;

    public Button button;
    public Image buttonImage;
    public TMP_Text buttonText;
    [Tooltip("Shown on the buy button next to the price.")]
    public GameObject priceStar;

    public Color buyColor = new Color(0.996f, 1f, 0f);
    public Color useColor = new Color(0.55f, 0.85f, 0.25f);
    public Color doneColor = new Color(0.7f, 0.7f, 0.7f);

    void OnEnable()
    {
        StarWallet.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        StarWallet.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        bool owned = StarWallet.Owns(id);
        bool selected = kind == Kind.SkyTheme && StarWallet.SelectedSky == id;

        // The price sits right of the star icon; other labels are centred.
        priceStar.SetActive(!owned);
        buttonText.margin = new Vector4(owned ? 0f : 56f, 0f, 0f, 0f);

        if (!owned)
        {
            buttonText.text = price.ToString();
            buttonImage.color = buyColor;
            button.interactable = StarWallet.Stars >= price;
        }
        else if (kind == Kind.Unlock)
        {
            buttonText.text = "Nopirkts";
            buttonImage.color = doneColor;
            button.interactable = false;
        }
        else
        {
            buttonText.text = selected ? "Izvēlēts" : "Izvēlēties";
            buttonImage.color = selected ? doneColor : useColor;
            button.interactable = !selected;
        }
    }

    public void Click()
    {
        if (!StarWallet.Owns(id))
        {
            if (!StarWallet.TrySpend(price))
                return;
            StarWallet.Unlock(id);
        }

        if (kind == Kind.SkyTheme)
            StarWallet.SelectedSky = id;
    }
}
