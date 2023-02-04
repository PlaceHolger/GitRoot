using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Player> players;
    public int WinninScore = 10;

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        foreach (var player in players)
        {
           Player.PlayerInfo info = player.GetStats();
           if (info.score >= WinninScore) {
                GameFinished(player);
                Reset();
                break;
           }
        }
    }

    protected void GameFinished(Player winner) 
    {

    }

    public void Reset()
    {
        
    }
}
