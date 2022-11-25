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
    private Joystick joystick;
    private GameObject[] rackets = new GameObject[Constatns.MAX_RACKET_NUM];
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
            joystick.SetVisible(true);
            
            // Gunny BUG
            // When change this position of joystick then event of joystick does not handled
            joystick.gameObject.transform.position = Camera.main.WorldToScreenPoint(clickedObject.transform.position);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            joystick.SetVisible(false);
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
