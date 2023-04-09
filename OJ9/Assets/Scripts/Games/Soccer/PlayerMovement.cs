using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    public delegate void AimDoneDelegate(Vector2 _vector2, int _playerId);
    public AimDoneDelegate aimDoneDelegate;

    bool onGoingAim;
    private Rigidbody2D rb;

    [SerializeField] private JoystickPanel joystickPanel;
    [SerializeField] public int paddleId;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetEnableJoystick(bool _enabled)
    {
        joystickPanel.enabled = _enabled;
    }

    private bool CanAim()
    {
        return rb.velocity.magnitude == 0.0f && joystickPanel != null;
    }

    private void OnMouseDown()
    {
        if (!CanAim())
        {
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
        aimDoneDelegate(oppositeDir, paddleId);
    }
}
