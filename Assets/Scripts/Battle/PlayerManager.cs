using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Constatns
{
    public const int MAX_RACKET_NUM = 5;
}
public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private JoystickPanel joystickPanel;
    private GameObject[] rackets = new GameObject[Constatns.MAX_RACKET_NUM];

    private Racket CachedSelectedRacket;

    // Start is called before the first frame update
    void Start()
    {
        // TODO
        // Add more rackets
        rackets[0] = transform.Find("Racket").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject clickedObject = GetClickedObject();
            if (clickedObject == null)
            {
                return;
            }

            CachedSelectedRacket = clickedObject.GetComponent<Racket>();

            joystickPanel.SetJoystickVisible(true);
            joystickPanel.SetJoystickPosition(
                Camera.main.WorldToScreenPoint(CachedSelectedRacket.transform.position)
            );
        }
        else if (Input.GetMouseButtonUp(0))
        {
            joystickPanel.SetJoystickVisible(false);

            if (CachedSelectedRacket == null)
            {
                return;
            }

            CachedSelectedRacket.Shooting(joystickPanel.GetInputVector());
            CachedSelectedRacket = null;
        }
    }

    private GameObject GetClickedObject()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

        if (hit.collider == null)
        {
            return null;
        }

        GameObject clickedObject = hit.transform.gameObject;
        foreach (var iter in rackets)
        {
            if (iter == clickedObject)
            {
                return clickedObject;
            }
        }

        return null;
    }
}
