using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectTransformationScript : MonoBehaviour
{

    public float rotationSpeed = 90f;
    public float scaleSpeed = 0.5f;
    public float minScale = 0.3f;
    public float maxScale = 0.9f;
    private bool rotateCW, rotateCCW,
                    scaleUpY, scaleDownY,
                    scaleUpX, scaleDownX;
    public static bool isTransforming = false;

    void Update()
    {
        // If a button's PointerUp never arrives (finger slid off, button hidden,
        // wrong EventTrigger), the flag would stay on forever: the car keeps
        // turning and the camera stays locked. No finger/mouse down = release all.
        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            ReleaseAllButtons();
        }

        if (GameObjectsScript.lastDragged == null)
        {
            ReleaseAllButtons();
            isTransforming = false;
            return;
        }

        // Keyboard controls (desktop) work alongside the on-screen buttons (mobile).
        bool cw = rotateCW || Input.GetKey(KeyCode.X);
        bool ccw = rotateCCW || Input.GetKey(KeyCode.Z);
        bool upY = scaleUpY || Input.GetKey(KeyCode.UpArrow);
        bool downY = scaleDownY || Input.GetKey(KeyCode.DownArrow);
        bool upX = scaleUpX || Input.GetKey(KeyCode.RightArrow);
        bool downX = scaleDownX || Input.GetKey(KeyCode.LeftArrow);

        RectTransform rc = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
        //rotation
        if (cw)
        {
            rc.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }

        if (ccw)
        {
            rc.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }

        Vector3 scale = rc.localScale;
        //===================== y scale ===================
        if (upY)
        {
            scale.y = Mathf.Min(scale.y + scaleSpeed * Time.deltaTime, maxScale);
        }

        if (downY)
        {
            scale.y = Mathf.Max(scale.y - scaleSpeed * Time.deltaTime, minScale);
        }
        // =================== x scale =====================
        // X can be negative when the car is flipped, so scale the magnitude and keep the sign.
        float signX = scale.x < 0f ? -1f : 1f;
        float sizeX = Mathf.Abs(scale.x);

        if (upX)
        {
            sizeX = Mathf.Min(sizeX + scaleSpeed * Time.deltaTime, maxScale);
        }

        if (downX)
        {
            sizeX = Mathf.Max(sizeX - scaleSpeed * Time.deltaTime, minScale);
        }

        if (upX || downX)
        {
            scale.x = sizeX * signX;
        }

        rc.localScale = scale;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Flip();
        }

        isTransforming = cw || ccw || upY || downY || upX || downX;
    }

    void ReleaseAllButtons()
    {
        rotateCW = rotateCCW = false;
        scaleUpY = scaleDownY = false;
        scaleUpX = scaleDownX = false;
    }

    void OnDisable()
    {
        ReleaseAllButtons();
        isTransforming = false;
    }

    public void Flip()
    {
        if (GameObjectsScript.lastDragged == null)
        {
            return;
        }

        RectTransform rc = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
        Vector3 scale = rc.localScale;
        scale.x = -scale.x;
        rc.localScale = scale;
    }

    public void StartRotateCW(BaseEventData eventData){rotateCW = true;}
    public void StopRotateCW(BaseEventData eventData){rotateCW = false;}
    public void StartRotateCCW(BaseEventData eventData){rotateCCW = true;}
    public void StopRotateCCW(BaseEventData eventData){rotateCCW = false;}
    //==================== SCALE ======================
    //Y
    public void StartScaleUpY(BaseEventData eventData) { scaleUpY = true;}
    public void StopScaleUpY(BaseEventData eventData) { scaleUpY = false;}
    public void StartScaleDownY(BaseEventData eventData) { scaleDownY = true;}
    public void StopScaleDownY(BaseEventData eventData) { scaleDownY = false;}
    //X
    public void StartScaleUpX(BaseEventData eventData) { scaleUpX = true; }
    public void StopScaleUpX(BaseEventData eventData) { scaleUpX = false; }
    public void StartScaleDownX(BaseEventData eventData) { scaleDownX = true; }
    public void StopScaleDownX(BaseEventData eventData) { scaleDownX = false; }
}
