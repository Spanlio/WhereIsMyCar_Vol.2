using System.Collections;
using UnityEngine;

// Locks the scene to landscape and gives free rotation back when it's left.
public class LandscapeOnlyScript : MonoBehaviour
{
    IEnumerator Start()
    {
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Force a landscape orientation first, then let it flip between the two landscapes.
        if (Screen.orientation != ScreenOrientation.LandscapeRight)
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        yield return null;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }

    void OnDestroy()
    {
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }
}
