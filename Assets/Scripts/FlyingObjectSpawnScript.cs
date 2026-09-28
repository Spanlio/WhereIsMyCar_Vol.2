using UnityEngine;

public class FlyingObjectSpawnScript : MonoBehaviour
{
    ScreenBoundariesScript screenBoundariesScript;
    GameObjectsScript gameObjectsScript;

    [Header("Cloud Prefabs")]
    public GameObject[] cloudPrefabs;

    [Header("Plain Prefabs")]
    public GameObject[] plainPrefabs;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Spawn Interval")]
    public float cloudSpawnInterval = 2f;
    public float plainSpawnInterval = 3f;

    private float minY, maxY;

    [Header("Cloud Speed")]
    public float cloudMinSpeed = 1.5f;
    public float cloudMaxSpeed = 150f;

    [Header("Plain Speed")]
    public float plainMinSpeed = 2f;
    public float plainMaxSpeed = 100f;

    void Start()
    {
        screenBoundariesScript = Object.FindFirstObjectByType<ScreenBoundariesScript>();
        gameObjectsScript = Object.FindFirstObjectByType<GameObjectsScript>();

        if (screenBoundariesScript == null)
        {
            Debug.LogError("FlyingObjectSpawnScript could not find ScreenBoundariesScript.");
            return;
        }

        minY = screenBoundariesScript.minY;
        maxY = screenBoundariesScript.maxY;

        InvokeRepeating(nameof(SpawnCloud), 0f, cloudSpawnInterval);
        InvokeRepeating(nameof(SpawnPlain), 0f, plainSpawnInterval);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void SpawnCloud()
    {
        if (gameObjectsScript != null && gameObjectsScript.GameFinished)
            return;

        if (cloudPrefabs == null || cloudPrefabs.Length == 0 || spawnPoint == null)
            return;

        GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        float y = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);
        GameObject cloud = Instantiate(prefab, pos, Quaternion.identity, spawnPoint);
        FlyingObjectControllerScript controller = cloud.GetComponent<FlyingObjectControllerScript>();

        if (controller != null)
            controller.speed = Random.Range(cloudMinSpeed, cloudMaxSpeed);
    }

    void SpawnPlain()
    {
        if (gameObjectsScript != null && gameObjectsScript.GameFinished)
            return;

        if (plainPrefabs == null || plainPrefabs.Length == 0 || spawnPoint == null)
            return;

        GameObject prefab = plainPrefabs[Random.Range(0, plainPrefabs.Length)];
        float y = Random.Range(minY, maxY);
        Vector3 pos = new Vector3(-spawnPoint.position.x, y, spawnPoint.position.z);
        GameObject plain = Instantiate(prefab, pos, Quaternion.identity, spawnPoint);
        FlyingObjectControllerScript controller = plain.GetComponent<FlyingObjectControllerScript>();

        if (controller != null)
            controller.speed = -Random.Range(plainMinSpeed, plainMaxSpeed);
    }
}
