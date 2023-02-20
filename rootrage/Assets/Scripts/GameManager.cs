using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private UnityEvent OnPlayerCloseToWinningNotEvent;
    [SerializeField]
    private UnityEvent OnPlayerCloseToWinningEvent;
    [FormerlySerializedAs("OnGameOver")]
    [SerializeField]
    private UnityEvent OnGameOverEvent;

    public ArenaManager arena;

    public Collectable collectable;
    public List<Player> players;
    public int WinningScore = 10;
    public int maxCollectables = 15;
    private float LastSpawnCollectable = 0.0f;
    [SerializeField] private float SpawnCollectableCooldown = 4.0f;
    [SerializeField] private LayerMask obstacleLayermask = 1 << 8 | 1 << 7 | 1 << 6; //by default 'player' and 'obstacle'


    private int currentHighestScore = 0;

    private bool arenaReady = false;
    private bool initialize = false;
    protected bool initialized = false;
    protected bool started = false;
    protected bool ended = false;

    private bool canvasAvailable = false;

    [SerializeField] public List<TMP_Text> scoreBoardsUI;
    [SerializeField] public List<TMP_Text> controlsUI;
    [SerializeField] public List<TMP_Text> playerNameUI;

    public void Initialize()
    {
        initialize = true;
        checkAndPrepareField();
    }
    public void ArenaReady()
    {
        arenaReady = true;
        checkAndPrepareField();
    }

    public void checkAndPrepareField()
    {
        if (!initialized && initialize && arenaReady)
        {
            initialized = true;
            canvasAvailable = (controlsUI.Count == players.Count && playerNameUI.Count == players.Count && scoreBoardsUI.Count == players.Count);
            Reset();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!initialized) return;

        LastSpawnCollectable += Time.deltaTime;

        int newHighestScore = 0;
        for (int i = 0; i < players.Count; i++)
        {
            Player.PlayerInfo info = players[i].GetStats();
            if (info.score > 0)
            {
                StartGame();
                if (canvasAvailable)
                {
                    scoreBoardsUI[i].gameObject.SetActive(true);
                    playerNameUI[i].gameObject.SetActive(true);
                    controlsUI[i].gameObject.SetActive(false);
                    if (!ended) scoreBoardsUI[i].SetText(info.score.ToString());
                }
            }
            if (info.score >= WinningScore)
            {
                if (!ended)
                {
                    GameFinished(i);
                    break;
                }
            }

            if (info.score > newHighestScore)
                newHighestScore = info.score;
        }

        if (newHighestScore != currentHighestScore && !ended)
        {
            if (newHighestScore >= WinningScore - 1) //dramatic
                OnPlayerCloseToWinningEvent.Invoke();
            else if (newHighestScore < WinningScore - 3 && currentHighestScore >= WinningScore - 3)  //was dramatic, is now not dramatic anymore
                OnPlayerCloseToWinningNotEvent.Invoke();
            currentHighestScore = newHighestScore;
        }

        if (started && CountCollectables() < maxCollectables)
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
        Collectable[] others = transform.parent.GetComponentsInChildren<Collectable>();
        collectablesInGame += others.Length;
        return collectablesInGame;
    }

    protected virtual void GameFinished(int winner)
    {
        ended = true;
        if (canvasAvailable)
        {
            foreach (var board in scoreBoardsUI)
            {
                board.SetText("");
            }
            scoreBoardsUI[winner].SetText("Won");
        }
        Invoke(nameof(Reset), 5.0f);
        OnGameOverEvent.Invoke();
    }

    public virtual void Reset()
    {
        started = false;
        ended = false;
        if (canvasAvailable)
        {
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
        }

        currentHighestScore = 0;
        OnPlayerCloseToWinningNotEvent.Invoke();

        Root[] roots = transform.parent.GetComponentsInChildren<Root>();
        foreach (Root root in roots)
        {
            Destroy(root.gameObject);
        }

        Collectable[] others = transform.parent.GetComponentsInChildren<Collectable>();
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
            Vector3 spawnPos = new Vector3(
                arena.transform.position.x + (Random.Range(0, arena.ArenaGridLength) + arena.ArenaGridBorderLength),
                arena.transform.position.y + 0.8f,
                arena.transform.position.z + (Random.Range(0, arena.ArenaGridWidth) + arena.ArenaGridBorderWidth)
            );

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

    protected void StartGame()
    {
        started = true;
    }
}
