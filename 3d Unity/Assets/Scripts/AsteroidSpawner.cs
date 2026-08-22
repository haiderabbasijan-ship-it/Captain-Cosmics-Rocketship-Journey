using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;
    public float spawnRate = 2f; // Time between spawns in seconds
    public float spawnRadius = 10f; // Radius within which asteroids will spawn

    void Start()
    {
        InvokeRepeating("SpawnAsteroid", 0f, spawnRate);
    }

    void SpawnAsteroid()
    {
        Vector3 spawnPosition = Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = 0; // Keep asteroids on the same plane

        Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Asteroid spawned at: " + spawnPosition);
    }
}
