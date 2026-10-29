using UnityEngine;
using UnityEngine.InputSystem;

// Test keys, in the editor and in builds (needs a keyboard on the device):
//   L - win the current game instantly (City or Tower of Hanoi)
//   G - give 50 stars for the shop
public class DevCheatsScript : MonoBehaviour
{
    public const int GIFT_STARS = 50;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        GameObject obj = new GameObject("DevCheats");
        obj.AddComponent<DevCheatsScript>();
        DontDestroyOnLoad(obj);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.gKey.wasPressedThisFrame)
        {
            StarWallet.Add(GIFT_STARS);
            Debug.Log("Cheat: +" + GIFT_STARS + " stars (now " + StarWallet.Stars + ")");
        }

        if (keyboard.lKey.wasPressedThisFrame)
        {
            HanoiGameScript hanoi = FindFirstObjectByType<HanoiGameScript>();
            if (hanoi != null)
                hanoi.DebugWin();

            GameObjectsScript city = FindFirstObjectByType<GameObjectsScript>();
            if (city != null)
                city.DebugWin();
        }
    }
}
