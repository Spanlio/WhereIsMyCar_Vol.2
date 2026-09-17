using UnityEngine;

public class CameraZoomScript : MonoBehaviour
{
    public float maxZoom = 530f;
    public float minZoom = 150f;
    public float panSpeed = 6f;
    public float zoomJump = 50f;
    Vector3 bottomLeft;
    Vector3 topRight;
    float cameraMaxX;
    float cameraMinX;
    float cameraMaxY;
    float cameraMinY;
    float x, y;
    public Camera mainCamera;


    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        topRight = mainCamera.ScreenToWorldPoint(
            new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, -transform.position.z));

        bottomLeft = mainCamera.ScreenToWorldPoint(
            new Vector3(0, 0, -transform.position.z));

        cameraMaxX = topRight.x;
        cameraMinX = bottomLeft.x;
        cameraMaxY = topRight.y;
        cameraMinY = bottomLeft.y;
    }

    void Update()
    {
        x = Input.GetAxis("Mouse X") * panSpeed;
        y = Input.GetAxis("Mouse Y") * panSpeed;
        transform.Translate(x, y, 0);

        if ((Input.GetAxis("Mouse ScrollWheel") > 0) && (mainCamera.orthographicSize > minZoom))
        {
            mainCamera.orthographicSize = mainCamera.orthographicSize - zoomJump;
        }

        if ((Input.GetAxis("Mouse ScrollWheel") < 0) && (mainCamera.orthographicSize < maxZoom))
        {
            mainCamera.orthographicSize = mainCamera.orthographicSize + zoomJump;
        }

        topRight = mainCamera.ScreenToWorldPoint(
            new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, -transform.position.z));
        bottomLeft = mainCamera.ScreenToWorldPoint(
            new Vector3(0, 0, -transform.position.z));

        if (topRight.x > cameraMaxX)
        {
            transform.position = new Vector3(transform.position.x -
                (topRight.x - cameraMaxX), transform.position.y, transform.position.z);
        }

        if (bottomLeft.x < cameraMinX)
        {
            transform.position = new Vector3(transform.position.x +
                (cameraMinX - bottomLeft.x), transform.position.y, transform.position.z);
        }

        if (topRight.y > cameraMaxY)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y -
                (topRight.y - cameraMaxY), transform.position.z);
        }

        if (bottomLeft.y < cameraMinY)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y +
                (cameraMinY - bottomLeft.y), transform.position.z);
        }
    }
}