using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickPanel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Joystick joystick;

    public void SetJoystickVisible(bool isVisible)
    {
        joystick.gameObject.SetActive(isVisible);
    }
    public void SetJoystickPosition(Vector3 inPosition)
    {
        joystick.transform.position = inPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData == null)
            throw new System.NotImplementedException();
        joystick.OnBeginDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.NotImplementedException();

        joystick.OnEndDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.NotImplementedException();

        if (joystick != null)
            joystick.OnDrag(eventData);
    }

    // Start is called before the first frame update
    void Start()
    {
        joystick = transform.GetChild(0).transform.GetComponent<Joystick>();
    }
}
