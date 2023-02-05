using UnityEngine;
using UnityEngine.Events;

public class EventOnActivate : MonoBehaviour
{
    [SerializeField]
    private UnityEvent EventOnEnable;
    [SerializeField]
    private UnityEvent EventOnStart;
    
    private void OnEnable()
    {
        EventOnEnable.Invoke();
    }

    void Start()
    {
        EventOnStart.Invoke();
    }
}
