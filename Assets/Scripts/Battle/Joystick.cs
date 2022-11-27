using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour
{
    [SerializeField]
    private GameObject powerDotPrefab;
    [SerializeField]
    private GameObject aimDotPrefab;

    [SerializeField]
    private RectTransform lever;
    private RectTransform rectTransform;

    [SerializeField, Range(10f, 150f)]
    private float leverRange;

    private Vector2 inputVector;
    private GameObject[] powerDots;
    private GameObject[] aimDots;

    public Vector2 GetInputVector()
    {
        return inputVector;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        powerDots = new GameObject[(int)leverRange];
        aimDots = new GameObject[(int)leverRange];

        for (int i = 0; i < (int)leverRange; ++i)
        {
            powerDots[i] = Instantiate(powerDotPrefab, transform, false);
            aimDots[i] = Instantiate(aimDotPrefab, transform, false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
    }

    public void ControlJoystickLever(PointerEventData eventData)
    {
        if (!rectTransform)
        {
            // before Awake
            return;
        }

        if (!gameObject.active)
        {
            return;
        }

        var inputDir = eventData.position - rectTransform.anchoredPosition;
        var clampedDir = inputDir.magnitude < leverRange ? inputDir : inputDir.normalized * leverRange;
        lever.anchoredPosition = clampedDir;
        inputVector = clampedDir / leverRange;

        DrawGuage();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        lever.anchoredPosition = Vector2.zero;
        HideGuage();
    }

    private void DrawGuage()
    {
        Vector3 diffVector = Input.mousePosition - transform.position;

        float force = Mathf.Clamp(diffVector.magnitude / leverRange, 0.0f, 1.0f);

        for (int i = 0; i < (int)leverRange; ++i)
        {
            if (diffVector.magnitude < i)
            {
                powerDots[i].gameObject.SetActive(false);
                aimDots[i].gameObject.SetActive(false);
            }
            else
            {
                powerDots[i].gameObject.SetActive(true);
                powerDots[i].GetComponent<RectTransform>().anchoredPosition = diffVector * (i / diffVector.magnitude);

                aimDots[i].gameObject.SetActive(true);
                aimDots[i].GetComponent<RectTransform>().anchoredPosition = diffVector * (-1.0f) * (i / diffVector.magnitude);
            }
        }
    }
    private void HideGuage()
    {
        for (int i = 0; i < (int)leverRange; ++i)
        {
            powerDots[i].gameObject.SetActive(false);
            aimDots[i].gameObject.SetActive(false);
        }
    }
}
