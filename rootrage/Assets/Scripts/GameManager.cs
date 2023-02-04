using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public ArenaManager arena;

    public Collectable collectable;
    public List<Player> players;
    public int WinningScore = 10;
    [SerializeField] private LayerMask obstacleLayermask = 1 << 8 | 1 << 7 | 1 << 6; //by default 'player' and 'obstacle'

    private bool initialized = false;

    private void Awake()
    {
        Invoke(nameof(Initialize), 0.5f);
    }

    private void Initialize()
    {
        if (!initialized)
        {
            initialized = true;
            Reset();
        }
    }

    void FixedUpdate()
    {
        if (!initialized) return;

        foreach (var player in players)
        {
            Player.PlayerInfo info = player.GetStats();
            if (info.score >= WinningScore)
            {
                GameFinished(player);
                Reset();
                break;
            }
        }
    }

    protected void GameFinished(Player winner)
    {
        Debug.Log("Game ended!");
    }

    public void Reset()
    {
        Root[] roots = FindObjectsOfType<Root>();
        foreach (Root root in roots)
        {
            Destroy(root.gameObject);
        }

        Collectable[] others = FindObjectsOfType<Collectable>();
        foreach (Collectable other in others)
        {
            Destroy(other.gameObject);
        }

        SpawnCollectables();

        List<GameObject> spawnPoints = new List<GameObject>(arena.ArenaSpawnPoints);
        foreach (var player in players)
        {
            if (spawnPoints.Count > 0)
            {
                int spawnPointIndex = Random.Range(0, spawnPoints.Count - 1);
                var spawnPoint = spawnPoints[spawnPointIndex];
                spawnPoints.RemoveAt(spawnPointIndex);
                player.Reset(spawnPoint);
            }
            else
            {
                player.Reset(null);
            }

        }
    }

    private void SpawnCollectables()
    {
        int collectablesToSpawn = 20;
        float radius = 1.0f;
        for (int i = 0; i < collectablesToSpawn; i++)
        {
            Vector3 spawnPos = new Vector3(Random.Range(0, 32), 0.8f, Random.Range(0, -16));

            Collider[] collisionResults = new Collider[16]; // can this be solved better?
            int hitCount = Physics.OverlapBoxNonAlloc(spawnPos, Vector3.one * 0.45f, collisionResults, transform.rotation, obstacleLayermask, QueryTriggerInteraction.Ignore);
            if (hitCount > 0)
            {
                i--;
            }
            else
            {
                Instantiate(collectable, spawnPos, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), gameObject.transform);
            }
        }
    }
}
