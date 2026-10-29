using UnityEngine;

public class FlyingObjectManager : MonoBehaviour
{
    // Reward for a watched rewarded ad in CityScene.
    void OnEnable()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.rewardedAds.OnRewardEarned += DestroyAllFlyingObjects;
    }

    void OnDisable()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.rewardedAds.OnRewardEarned -= DestroyAllFlyingObjects;
    }

    public void DestroyAllFlyingObjects()
    {
        FlyingObjectControllerScript[] flyingObjects =
        Object.FindObjectsByType<FlyingObjectControllerScript>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        foreach (FlyingObjectControllerScript obj in flyingObjects)
        {
            if(obj == null)
            {
                continue;
            }

            if (obj.CompareTag("Bomb")){
                obj.TriggerExplosion();
            }
            else
            {
                obj.StartAndDestroy(Color.gray);
            }
        }
    }
}
