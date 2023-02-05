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
    public int maxCollectables = 15;
    private float LastSpawnCollectable = 0.0f;
    [SerializeField] private float SpawnCollectableCooldown = 4.0f;
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

        LastSpawnCollectable += Time.deltaTime;

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

        if (CountCollectables() < maxCollectables)
        {
            if (SpawnCollectableCooldown < LastSpawnCollectable)
            {
                LastSpawnCollectable = 0.0f;
                SpawnCollectables(1);
            }
        }
    }

    private int CountCollectables()
    {
        int collectablesInGame = 0;
        foreach (var player in players)
        {
            collectablesInGame += player.GetStats().score;
        }
        Collectable[] others = FindObjectsOfType<Collectable>();
        collectablesInGame += others.Length;
        return collectablesInGame;
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

        SpawnCollectables(Mathf.RoundToInt(maxCollectables * 0.25f));

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

    private void SpawnCollectables(int numCollectables)
    {
        for (int i = 0; i < numCollectables; i++)
        {
            Vector3 spawnPos = new Vector3(arena.transform.localScale.x * (Random.Range(0, arena.ArenaGridWidth) + arena.ArenaGridBorder), 0.8f, -arena.transform.localScale.z * (Random.Range(0, arena.ArenaGridLength) + arena.ArenaGridBorder));

            Collider[] collisionResults = new Collider[16]; // can this be solved better?
            int hitCount = Physics.OverlapBoxNonAlloc(spawnPos, Vector3.one * 0.45f, collisionResults, transform.rotation, obstacleLayermask, QueryTriggerInteraction.Ignore);
            if (hitCount > 0)
            {
                i--;
            }
            else
            {
                Instantiate(collectable, spawnPos, gameObject.transform.rotation, gameObject.transform);
            }
        }
    }

    public void DropCollectables(Vector3 position)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 circlePos = Random.insideUnitCircle.normalized * 3.0f;
            Vector3 spawnPos = new Vector3(circlePos.x + position.x, position.y, circlePos.y + position.z);
            Collider[] collisionResults = new Collider[16]; // can this be solved better?
            int hitCount = Physics.OverlapBoxNonAlloc(spawnPos, Vector3.one * 1.0f, collisionResults, transform.rotation, obstacleLayermask, QueryTriggerInteraction.Ignore);
            if (hitCount > -1)
            {
                Instantiate(collectable, spawnPos, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), gameObject.transform);
                break;
            }
        }
    }
}
