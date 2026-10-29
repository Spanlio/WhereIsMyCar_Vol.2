using System;
using UnityEngine;

// Stars earned in both games, spent in the menu shop. Saved in PlayerPrefs.
public static class StarWallet
{
    public const string NO_ADS = "no_ads";
    public const string SKY_DAY = "sky_day";

    private const string STARS_KEY = "Stars";
    private const string OWNED_PREFIX = "Owned_";
    private const string SKY_KEY = "HanoiSky";

    public static event Action OnChanged;

    public static int Stars => PlayerPrefs.GetInt(STARS_KEY, 0);

    public static void Add(int amount)
    {
        if (amount <= 0)
            return;

        PlayerPrefs.SetInt(STARS_KEY, Stars + amount);
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }

    public static bool TrySpend(int amount)
    {
        if (amount > Stars)
            return false;

        PlayerPrefs.SetInt(STARS_KEY, Stars - amount);
        PlayerPrefs.Save();
        OnChanged?.Invoke();
        return true;
    }

    public static bool Owns(string item)
    {
        return item == SKY_DAY || PlayerPrefs.GetInt(OWNED_PREFIX + item, 0) == 1;
    }

    public static void Unlock(string item)
    {
        PlayerPrefs.SetInt(OWNED_PREFIX + item, 1);
        PlayerPrefs.Save();
        OnChanged?.Invoke();
    }

    // Sky theme for the Tower of Hanoi.
    public static string SelectedSky
    {
        get
        {
            string sky = PlayerPrefs.GetString(SKY_KEY, SKY_DAY);
            return Owns(sky) ? sky : SKY_DAY;
        }
        set
        {
            PlayerPrefs.SetString(SKY_KEY, value);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }
}
