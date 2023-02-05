using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnParticleHelper : MonoBehaviour
{
    [SerializeField]
    private GameObject particleToSpawn;
    [SerializeField]
    private float lifeTime;
    
    [ContextMenu("Spawn")]
    public void Spawn()
    {
        var hitParticle = Instantiate(particleToSpawn);
        hitParticle.transform.position = transform.position;
        Destroy(hitParticle.gameObject, lifeTime);
    }
}
