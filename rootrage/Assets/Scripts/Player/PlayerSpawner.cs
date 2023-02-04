using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviour
{
    public float spawnRadius = 1f;
    public GameObject prefabToSpawn;

    private GameObject _player;

    public void SpawnPlayer()
    {
        Vector3 initialPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        initialPosition.y = transform.position.y;

        _player = Instantiate(prefabToSpawn, initialPosition, Quaternion.identity);
        _player.name = "[local] Player";
    }

    public void DespawnPlayer()
    {
        Destroy(_player);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
