using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingArena : MonoBehaviour
{
    public List<Player> players;
    private ArenaManager _arena;

    private bool _initialized;
    private int _resetTimer;
    public int maxEnvironmentSteps = 5000;

    void Awake()
    {
        _arena = GetComponent<ArenaManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Initialize()
    {
        foreach (var player in players)
        {
            //player.Agent.Initialize(); TODO init agent
            //player.Agent.HitPointsRemaining = PlayerMaxHitPoints;
            //player.Agent.NumberOfTimesPlayerCanBeHit = PlayerMaxHitPoints;
        }
        _initialized = true;
        ResetScene();
    }

    void ResetScene()
    {
        _resetTimer = 0;
        foreach (var player in players)
        {
            //player.hitPointsRemaining = playerMaxHitPoints;
            //player.Agent.ResetAgent(); TODO reset agent
        }
    }

    void FixedUpdate()
    {
        if (!_initialized) return;

        //RESET SCENE IF WE MaxEnvironmentSteps
        _resetTimer += 1;
        if (_resetTimer >= maxEnvironmentSteps)
        {
            ResetScene();
        }
    }

    void EndEpisode()
    {
        foreach (var player in players)
        {
            //player.Agent.EndEpisode(); TODO reset agent
        }
    }

    public Vector2 GetArenaPosition()
    {
        Vector2 arenaSize = new Vector2((_arena.transform.position.x + _arena.ArenaGridWidth / 2.0f + _arena.ArenaGridBorder) * _arena.transform.localScale.x, (_arena.transform.position.z - _arena.ArenaGridLength / 2.0f - _arena.ArenaGridBorder) * _arena.transform.localScale.z);
        return arenaSize;
    }
}
