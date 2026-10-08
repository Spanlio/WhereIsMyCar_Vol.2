using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectTransformationScript : MonoBehaviour
{

    public float rotationSpeed = 90f;
    public float scaleSpeed = 0.5f;
    private bool rotateCW, rotateCCW, 
                    scaleUpY, scaleDownY,
                    scaleUpX, scaleDownX;
    public static bool isTransforming = false;

    void Awake()
    {
        
    }

    void Update()
    {
        if (GameObjectsScript.lastDragged == null)
        {
            return;
        }

        RectTransform rc = GameObjectsScript.lastDragged.GetComponent<RectTransform>();
        //rotation
        if (rotateCW)
        {
            rc.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }

        if (rotateCCW)
        {
            rc.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        //===================== y scale ===================
        if (scaleUpY && rc.localScale.y < 0.9)
        {
            rc.localScale += new Vector3(0, scaleSpeed * Time.deltaTime, 0);
        }

        if (scaleDownY && rc.localScale.y < 0.9)
        {
            rc.localScale -= new Vector3(0, scaleSpeed * Time.deltaTime, 0);
        }
        // =================== x scale =====================
        if (scaleUpX && rc.localScale.x < 0.9)
        {
            rc.localScale += new Vector3(scaleSpeed * Time.deltaTime, 0, 0);
        }

        if (scaleUpX && rc.localScale.x < 0.9)
        {
            rc.localScale -= new Vector3(scaleSpeed * Time.deltaTime, 0, 0);
        }

        isTransforming =
            rotateCW || rotateCCW || scaleUpY || scaleDownY || scaleUpX || scaleDownX;
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