using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinManager : MonoBehaviour
{
    public InputActionReference JoinP1Action;
    public GameObject player1;
    public InputActionReference JoinP2Action;
    public GameObject player2;
    public InputActionReference JoinP3Action;
    public GameObject player3;
    public InputActionReference JoinP4Action;
    public GameObject player4;

    // Start is called before the first frame update
    void Start()
    {
        var ailogic = player1.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }
        ailogic = player2.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }
        ailogic = player3.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }
        ailogic = player4.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }

        player1.SetActive(false);
        player2.SetActive(false);
        player3.SetActive(false);
        player4.SetActive(false);
    }

    private void OnEnable()
    {
        JoinP1Action.asset.Enable();
        JoinP1Action.action.performed += P1Joined;

        JoinP2Action.asset.Enable();
        JoinP2Action.action.performed += P2Joined;

        JoinP3Action.asset.Enable();
        JoinP3Action.action.performed += P3Joined;

        JoinP4Action.asset.Enable();
        JoinP4Action.action.performed += P4Joined;
    }

    private void OnDisable()
    {
        JoinP1Action.action.performed -= P1Joined;
        JoinP2Action.action.performed -= P2Joined;
        JoinP3Action.action.performed -= P3Joined;
        JoinP4Action.action.performed -= P4Joined;
    }

    private void P1Joined(InputAction.CallbackContext obj)
    {
        player1.SetActive(true);
        var ailogic = player1.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }
        JoinP1Action.action.performed -= P1Joined;
    }

    private void P2Joined(InputAction.CallbackContext obj)
    {
        player2.SetActive(true);
        var ailogic = player2.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }
        JoinP2Action.action.performed -= P2Joined;
    }

    private void P3Joined(InputAction.CallbackContext obj)
    {
        player3.SetActive(true);
        var ailogic = player3.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }
        JoinP3Action.action.performed -= P3Joined;
    }

    private void P4Joined(InputAction.CallbackContext obj)
    {
        player4.SetActive(true);
        var ailogic = player4.transform.Find("AILogic");
        if (ailogic)
        {
            ailogic.gameObject.SetActive(false);
        }
        JoinP4Action.action.performed -= P4Joined;
    }
}
