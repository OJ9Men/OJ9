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

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
        if (isVisible)
        {
            ResetGuage();
        }
    }
    public void SetPosition(Vector3 newPos)
    {
        transform.position = newPos;
        lever.transform.position = newPos;
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

        var inputDir = eventData.position - rectTransform.anchoredPosition;
        var clampedDir = inputDir.magnitude < leverRange ? inputDir : inputDir.normalized * leverRange;
        lever.anchoredPosition = clampedDir;
        inputVector = clampedDir / leverRange;

        DrawGuage();
    }

    private void DrawGuage()
    {
        Vector3 diffVector = Input.mousePosition - transform.position;
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
    private void ResetGuage()
    {
        for (int i = 0; i < (int)leverRange; ++i)
        {
            powerDots[i].gameObject.SetActive(false);
            aimDots[i].gameObject.SetActive(false);
        }
    }
}
