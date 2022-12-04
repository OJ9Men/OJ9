using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    bool onGoingAim;
    private Rigidbody2D rb;

    [SerializeField]
    private JoystickPanel joystickPanel;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private bool CanAim()
    {
        return rb.velocity.magnitude == 0.0f;
    }

    private void OnMouseDown()
    {
        if (!CanAim())
        {
            Debug.LogWarning("Should be stopped");
            return;
        }

        onGoingAim = true;
        joystickPanel.SetJoystickVisible(true);
        joystickPanel.SetJoystickPosition(Camera.main.WorldToScreenPoint(transform.position));

    }
    private void OnMouseUp()
    {
        if (!onGoingAim)
        {
            return;
        }

        joystickPanel.SetJoystickVisible(false);
        Vector2 oppositeDir = -joystickPanel.GetInputVector();
        rb.AddForce(Constants.FORCE_MAGNITUDE * oppositeDir);

        onGoingAim = false;
    }
}
