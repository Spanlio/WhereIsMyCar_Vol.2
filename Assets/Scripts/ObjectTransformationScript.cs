using UnityEngine;

public class ObjectTransformationScript : MonoBehaviour
{
    public GameObjectsScript gameObjectsScript;

    void Awake()
    {
        gameObjectsScript = FindFirstObjectByType<GameObjectsScript>();
    }

    void Update()
    {
        if (GameObjectsScript.lastDragged == null)
            return;

        RectTransform rect =
            GameObjectsScript.lastDragged.GetComponent<RectTransform>();

        if (rect == null)
            return;

        // Rotate counter-clockwise
        if (Input.GetKey(KeyCode.Z))
        {
            rect.Rotate(
                0f,
                0f,
                Time.deltaTime * 12f
            );
        }

        // Rotate clockwise
        if (Input.GetKey(KeyCode.X))
        {
            rect.Rotate(
                0f,
                0f,
                -Time.deltaTime * 12f
            );
        }

        // Increase/decrease HEIGHT
        // Mathf.Abs() makes this work whether the object
        // is normally oriented or flipped.
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (Mathf.Abs(rect.localScale.y) < 0.9f)
            {
                float sign = Mathf.Sign(rect.localScale.y);

                if (sign == 0f)
                    sign = 1f;

                rect.localScale = new Vector3(
                    rect.localScale.x,
                    sign * (Mathf.Abs(rect.localScale.y) + 0.001f),
                    rect.localScale.z
                );
            }
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (Mathf.Abs(rect.localScale.y) > 0.3f)
            {
                float sign = Mathf.Sign(rect.localScale.y);

                if (sign == 0f)
                    sign = 1f;

                rect.localScale = new Vector3(
                    rect.localScale.x,
                    sign * (Mathf.Abs(rect.localScale.y) - 0.001f),
                    rect.localScale.z
                );
            }
        }

        // Increase/decrease WIDTH
        // This specifically handles the negative X produced
        // by the Spacebar flip.
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Mathf.Abs(rect.localScale.x) > 0.3f)
            {
                float sign = Mathf.Sign(rect.localScale.x);

                if (sign == 0f)
                    sign = 1f;

                rect.localScale = new Vector3(
                    sign * (Mathf.Abs(rect.localScale.x) - 0.001f),
                    rect.localScale.y,
                    rect.localScale.z
                );
            }
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Mathf.Abs(rect.localScale.x) < 0.9f)
            {
                float sign = Mathf.Sign(rect.localScale.x);

                if (sign == 0f)
                    sign = 1f;

                rect.localScale = new Vector3(
                    sign * (Mathf.Abs(rect.localScale.x) + 0.001f),
                    rect.localScale.y,
                    rect.localScale.z
                );
            }
        }

        // Flip horizontally.
        // KEEPING YOUR EXISTING BEHAVIOR.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rect.localScale = new Vector3(
                -rect.localScale.x,
                rect.localScale.y,
                rect.localScale.z
            );
        }
    }
}