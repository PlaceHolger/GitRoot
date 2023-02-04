using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ArenaManager arena;
    public List<Player> players;
    public int WinningScore = 10;

    void FixedUpdate()
    {
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
        Root[] others = FindObjectsOfType<Root>();
        foreach (Root other in others)
        {
            Destroy(other.gameObject);
        }
        foreach (var player in players)
        {
            player.Reset();
        }
    }
}
