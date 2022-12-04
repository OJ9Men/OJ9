using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickPanel : MonoBehaviour, IDragHandler
{
    private Joystick joystick;

    public Vector3 GetInputVector()
    {
        return joystick.GetInputVector();
    }

    public void SetJoystickVisible(bool isVisible)
    {
        joystick.SetVisible(isVisible);
    }

    public bool GetJoystickVisible()
    {
        return joystick.gameObject.activeSelf;
    }
    public void SetJoystickPosition(Vector3 inPosition)
    {
        joystick.SetPosition(inPosition);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.NotImplementedException();

        if (joystick != null)
            joystick.OnDrag(eventData);
    }

    void Awake()
    {
        joystick = transform.GetChild(0).transform.GetComponent<Joystick>();
    }

}
