using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class MatingPartnerAgent : Agent
{
    private TrainingArena _arena;
    private Actions _actions;
    public BufferSensorComponent otherPlayerBuffer;
    public float locationNormalizationFactor = 32.0f;

    public override void Initialize()
    {
        _arena = GameObject.FindObjectsOfType<TrainingArena>()[0];
        var bufferSensors = GetComponentsInChildren<BufferSensorComponent>();
        otherPlayerBuffer = bufferSensors[0];
        _actions = GetComponentInParent<Actions>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private float[] GetOtherPlayerData(Actions obj)
    {
        var otherPlayerData = new float[6];
        var relativePosition = transform.InverseTransformPoint(obj.transform.position);
        otherPlayerData[0] = relativePosition.x / locationNormalizationFactor;
        otherPlayerData[1] = relativePosition.z / locationNormalizationFactor;
        otherPlayerData[2] = Vector3.Dot(obj.transform.forward, transform.forward);
        otherPlayerData[3] = Vector3.Dot(obj.transform.right, transform.right);
        return otherPlayerData;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        List<Actions> playerList = _arena.players;
        foreach (var otherPlayer in playerList)
        {
            if (otherPlayer != _actions)
            {
                otherPlayerBuffer.AppendObservation(GetOtherPlayerData(otherPlayer));
            }
        }
        Vector3 relativePosition = _arena.transform.InverseTransformPoint(transform.position);
        sensor.AddObservation(relativePosition.x);
        sensor.AddObservation(relativePosition.z);
    }
}
