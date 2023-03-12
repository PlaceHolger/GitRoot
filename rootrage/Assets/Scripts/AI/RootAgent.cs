using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RootAgent : Agent
{
    [SerializeField] public bool UsePlayerInput = false;
    [SerializeField] private GameManager _manager;
    private Player _player;
    private Actions _actions;
    private BufferSensorComponent otherPlayerBuffer;
    private float locationNormalizationFactor = 32.0f;

    public ArenaManager GetArena { get { return _manager.arena; } }

    public override void Initialize()
    {
        var bufferSensors = GetComponentsInChildren<BufferSensorComponent>();
        otherPlayerBuffer = bufferSensors[0];
        _player = GetComponentInParent<Player>();
        _actions = GetComponentInParent<Actions>();
        locationNormalizationFactor = Mathf.Max(_manager.arena.ArenaGridWidth, _manager.arena.ArenaGridLength);
    }

    void FixedUpdate()
    {

    }

    private float[] GetOtherPlayerData(Player obj)
    {
        Player.PlayerInfo stats = obj.GetStats();
        var otherPlayerData = new float[7];
        var relativePosition = transform.InverseTransformPoint(obj.transform.position);
        otherPlayerData[0] = relativePosition.x / locationNormalizationFactor;
        otherPlayerData[1] = relativePosition.z / locationNormalizationFactor;
        otherPlayerData[2] = Vector3.Dot(obj.transform.forward, transform.forward);
        otherPlayerData[3] = Vector3.Dot(obj.transform.right, transform.right);
        otherPlayerData[4] = ((float)stats.score) / (float)_manager.WinningScore;
        otherPlayerData[5] = (stats.isStunned ? 1f : 0f);
        otherPlayerData[6] = (stats.isShooting ? 1f : 0f);
        return otherPlayerData;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        List<Player> playerList = _manager.players;
        foreach (var otherPlayer in playerList)
        {
            if (otherPlayer != _player)
            {
                otherPlayerBuffer.AppendObservation(GetOtherPlayerData(otherPlayer));
            }
        }

        Vector3 relativePosition = _manager.transform.InverseTransformPoint(transform.position) - new Vector3(_manager.arena.ArenaGridBorderLength, 0.0f, _manager.arena.ArenaGridBorderWidth);
        sensor.AddObservation(relativePosition.x / locationNormalizationFactor);
        sensor.AddObservation(relativePosition.z / locationNormalizationFactor);
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);

        Player.PlayerInfo stats = GetPlayerStats();
        sensor.AddObservation(stats.isStunned);
        sensor.AddObservation(stats.isShooting);
        sensor.AddObservation((float)stats.score / (float)_manager.WinningScore);

        sensor.AddObservation(_actions.rootCreator.IsSomethingInMeleeRange());
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (UsePlayerInput) return;

        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;

        float actionV = Mathf.Clamp(continuousActions[0], -1f, 1f);
        float actionH = Mathf.Clamp(continuousActions[1], -1f, 1f);
        int shoot = (int)discreteActions[0];

        _actions.ApplyActions(shoot > 0, new Vector2(actionV, actionH));
    }


    public Player.PlayerInfo GetPlayerStats()
    {
        return _player.GetStats();
    }
}
