using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using TMPro;
using Unity.Netcode;
using UnityEngine.Serialization;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private UnityEvent OnPlayerCloseToWinningNotEvent;
    [SerializeField]
    private UnityEvent OnPlayerCloseToWinningEvent;
    [FormerlySerializedAs("OnGameOver")] [SerializeField]
    private UnityEvent OnGameOverEvent;

    public ArenaManager arena;

    public Collectable collectable;
    private List<Player> players;
    public int WinningScore = 10;
    //public int maxCollectables = 15;
    public int maxSimultanCollectables = 4;
    private float LastSpawnCollectable = 0.0f;
    [SerializeField] private float SpawnCollectableCooldown = 4.0f;
    [SerializeField] private LayerMask obstacleLayermask = 1 << 8 | 1 << 7 | 1 << 6; //by default 'player' and 'obstacle'

    private bool initialized = false;
    private int currentHighestScore = 0;

    //private bool started = false;
    private bool ended = false;
    private int winner = 0;
    [SerializeField] public List<TMP_Text> scoreBoardsUI;
    [SerializeField] public List<TMP_Text> controlsUI;
    [SerializeField] public List<TMP_Text> playerNameUI;

    [ClientRpc]
    private void OnPlayerCloseToWinningNotEventClientRpc() => OnPlayerCloseToWinningNotEvent.Invoke();
    [ClientRpc]
    private void OnPlayerCloseToWinningEventClientRpc() => OnPlayerCloseToWinningEvent.Invoke();


    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartNewGameServerRpc()
    {
        initialized = false;
        Initialize();            
    }

    private void Initialize()
    {
        players = FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();
        
        if (!initialized)
        {
            initialized = true;
            Reset();
        }
    }

    private void Update()
    {
        if (!initialized) return;

        LastSpawnCollectable += Time.deltaTime;
        
        int newHighestScore = 0;
        for (int i = 0; i < players.Count; i++)
        {
            Player.PlayerInfo info = players[i].GetStats();
            if (info.score >= WinningScore)
            {
                if (!ended)
                {
                    ended = true;
                    winner = i;
                    GameFinished();
                    break;
                }
            }

            if (info.score > newHighestScore)
                newHighestScore = info.score;
        }
        
        if(!ended)
            UpdateUiClientRpc();

        if (newHighestScore != currentHighestScore && !ended)
        {
            if (newHighestScore >= WinningScore - 1) //dramatic
                OnPlayerCloseToWinningEventClientRpc();
            else if (newHighestScore < WinningScore - 2 && currentHighestScore >= WinningScore - 2)  //was dramatic, is now not dramatic anymore
                OnPlayerCloseToWinningNotEventClientRpc();
            currentHighestScore = newHighestScore;
        }

            if (SpawnCollectableCooldown < LastSpawnCollectable)
            {
                if (CountCollectables() < maxSimultanCollectables)
                {
                    SpawnCollectables(1);
                }
                LastSpawnCollectable = 0.0f;
            }
    }

    [ClientRpc]
    private void UpdateUiClientRpc()
    {
        if(players == null || players.Count == 0)
            players = FindObjectsByType<Player>(FindObjectsSortMode.None).ToList();
        
        for (int i = 0; i < controlsUI.Count; i++)
        {
            if (i < players.Count)
            {
                scoreBoardsUI[i].transform.parent.gameObject.SetActive(true);
                scoreBoardsUI[i].gameObject.SetActive(true);
                playerNameUI[i].gameObject.SetActive(true);
                controlsUI[i].gameObject.SetActive(false);
                if (!ended)
                {
                    scoreBoardsUI[i].SetText(players[i].GetStats().score.ToString());
                }
            }
            else
            {
                scoreBoardsUI[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private int CountCollectables()
    {
        return FindObjectsByType<Collectable>(FindObjectsSortMode.None).Length;
    }

    [ClientRpc(Delivery = RpcDelivery.Reliable)]
    private void GameFinishedClientRpc()
    {
        for (int i = 0; i < players.Count && i < scoreBoardsUI.Count; i++)
        {
            scoreBoardsUI[i].SetText(players[i].GetStats().score >= WinningScore ? "Won" : "Lost");
        }
        OnGameOverEvent.Invoke();
    }

    private void GameFinished()
    {
        GameFinishedClientRpc();
        Invoke(nameof(Reset), 7.0f);
    }
    
    public void Reset()
    {
        //started = false;
        ended = false;
        foreach (var board in scoreBoardsUI)
        {
            board.gameObject.SetActive(false);
            board.SetText("0");
        }
        foreach (var control in controlsUI)
        {
            control.gameObject.SetActive(true);
        }
        foreach (var name in playerNameUI)
        {
            name.gameObject.SetActive(false);
        }

        currentHighestScore = 0;
        OnPlayerCloseToWinningNotEventClientRpc();

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

        List<GameObject> spawnPoints = new List<GameObject>(arena.ArenaSpawnPoints);
        foreach (var player in players)
        {
            if (spawnPoints.Count > 0)
            {
                int spawnPointIndex = Random.Range(0, spawnPoints.Count - 1);
                var spawnPoint = spawnPoints[spawnPointIndex];
                spawnPoints.RemoveAt(spawnPointIndex);
                player.ResetServerRpc(spawnPoint.transform.position + Vector3.up);
            }
            else
            {
                player.ResetServerRpc(player.transform.position);
            }
        }

        //for faster game-start, we directly spawn many
        for (int i = 0; i < maxSimultanCollectables; i++)
        {
            SpawnCollectables(1);            
        }
    }

    static Collider[] m_CollisionResultsCache = new Collider[16];

    private void SpawnCollectables(int numCollectables)
    {
        for (int i = 0; i < numCollectables; )
        {
            //Vector3 spawnPos = new Vector3(arena.transform.localScale.x * (Random.Range(0, arena.ArenaGridWidth) + arena.ArenaGridBorder), 0.8f, -arena.transform.localScale.z * (Random.Range(0, arena.ArenaGridLength) + arena.ArenaGridBorder));
            Vector3 potentialSpawnPos = arena.GetRandomGridElement().groundGameObject.transform.position;

            int hitCount = Physics.OverlapBoxNonAlloc(potentialSpawnPos, Vector3.one * 2, m_CollisionResultsCache, transform.rotation, obstacleLayermask, QueryTriggerInteraction.Ignore);
            if (hitCount == 0)
            {
                var newCollectable = Instantiate(collectable, potentialSpawnPos, Quaternion.identity);
                newCollectable.GetComponent<NetworkObject>().Spawn();
                ++i;
            }
        }
    }

    public void DropCollectables(Vector3 position)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 circlePos = Random.insideUnitCircle.normalized * 4.0f;
            Vector3 spawnPos = new Vector3(circlePos.x + position.x, position.y, circlePos.y + position.z);
            int hitCount = Physics.OverlapBoxNonAlloc(spawnPos, Vector3.one, m_CollisionResultsCache, transform.rotation, obstacleLayermask, QueryTriggerInteraction.Ignore);
            if (hitCount == 0)
            {
                var droppedCollectable = Instantiate(collectable, spawnPos, Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)), gameObject.transform);
                droppedCollectable.GetComponent<NetworkObject>().Spawn();
                droppedCollectable.GetComponent<Collectable>().MarkAsDroppedClientRpc();
                break;
            }
        }
    }
}
