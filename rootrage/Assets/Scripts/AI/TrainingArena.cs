using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingArena : MonoBehaviour
{
    public List<Actions> players;

    private bool _initialized;
    private int _resetTimer;
    public int maxEnvironmentSteps = 5000;

    // Start is called before the first frame update
    void Start()
    {
        
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
}
