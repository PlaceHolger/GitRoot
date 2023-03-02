using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EventOnInput : MonoBehaviour
{
    [SerializeField]
    private UnityEvent AnyKeyPressed;
    
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.anyKey.IsPressed())
        {
            AnyKeyPressed.Invoke();
        }
    }
}
