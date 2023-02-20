using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : GameManager
{
    [System.Serializable]
    public class AgentInfo
    {
        public RootAgent Agent;
        public float ScoreReward = 0.0f;
    }

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
    private int _resetTimer = 0;
    private List<AgentInfo> agentsList = new List<AgentInfo>();

    void Awake()
    {
        agentsList = new List<AgentInfo>(players.Count);
        for (int i = 0; i < players.Count; i++)
        {
            agentsList.Add(new AgentInfo());
            agentsList[i].Agent = players[i].GetComponentsInChildren<RootAgent>()[0];
        }
    }

    protected override void FixedUpdate()
    {
        Initialize();

        if (!initialized) return;
        started = true;

        _resetTimer += 1;
        if (_resetTimer >= MaxEnvironmentSteps)
        {
            Reset();
        }

        for (int i = 0; i < agentsList.Count; i++)
        {
            Player.PlayerInfo info = agentsList[i].Agent.GetPlayerStats();
            float collectableReward = ((float)info.score * ((float)info.score + 1.0f)) / 2.0f;
            collectableReward = collectableReward / (((float)WinningScore * ((float)WinningScore + 1.0f)) / 2.0f);
            collectableReward = collectableReward / 10.0f;

            float scoreRewardDifference = collectableReward - agentsList[i].ScoreReward;
            agentsList[i].ScoreReward = collectableReward;
            if (scoreRewardDifference != 0.0f)
            {
                agentsList[i].Agent.AddReward(scoreRewardDifference);
            }
        }
        base.FixedUpdate();
    }

    protected override void GameFinished(int winner)
    {
        ended = true;
        for (int i = 0; i < agentsList.Count; i++)
        {
            if (i == winner)
            {
                agentsList[i].Agent.AddReward(2.0f - (_resetTimer / MaxEnvironmentSteps));
            }
            else
            {
                agentsList[i].Agent.AddReward(-1.0f);
            }
        }
        Reset();
    }

    public override void Reset()
    {
        _resetTimer = 0;
        for (int i = 0; i < agentsList.Count; i++)
        {
            agentsList[i].Agent.EndEpisode();
            agentsList[i].ScoreReward = 0.0f;
        }
        base.Reset();
    }
}
