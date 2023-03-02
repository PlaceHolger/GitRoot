using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    private Allocation m_RelayAllocation;

    private UnityTransport m_UnityTransportComp;
    
    private void Awake()
    {
        m_UnityTransportComp = FindObjectOfType<UnityTransport>();
        //m_UnityTransportComp.Protocol = UnityTransport.ProtocolType.RelayUnityTransport;
        if(m_UnityTransportComp.Protocol != UnityTransport.ProtocolType.RelayUnityTransport)
            Debug.LogWarning("UnityTransportProtocol is not set to use Relay. Change this in the scene", m_UnityTransportComp);
    }

    public async Task<string> SetupRelay(int numPlayers = 8)
    {
        try
        {
            m_RelayAllocation = await RelayService.Instance.CreateAllocationAsync(numPlayers);
            RelayServerData relayData = new RelayServerData(m_RelayAllocation, "dtls");
            m_UnityTransportComp.SetRelayServerData(relayData);
            return await RelayService.Instance.GetJoinCodeAsync(m_RelayAllocation.AllocationId);
        } 
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }

        return string.Empty;
    }

    public void Update()
    {
        if (m_RelayAllocation != null)
        {
           // m_RelayAllocation. ping?
        }
    }

    public async Task JoinRelay(string relayCode)
    {
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            m_UnityTransportComp.SetRelayServerData(relayServerData);
        } 
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
        }
    }
}
