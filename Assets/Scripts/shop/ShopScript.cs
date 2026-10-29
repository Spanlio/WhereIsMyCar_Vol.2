using UnityEngine;

// The shop popup in the start menu.
public class ShopScript : MonoBehaviour
{
    public GameObject shopPopup;

    public void OpenShop()
    {
        shopPopup.SetActive(true);
    }

    public void CloseShop()
    {
        shopPopup.SetActive(false);
    }
}
