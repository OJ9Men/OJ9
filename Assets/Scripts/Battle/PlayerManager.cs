using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Constatns
{
    public const int MAX_RACKET_NUM = 5;
}
public class PlayerManager : MonoBehaviour
{
    public AimJoystick aimJoystick;
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
            if (!IsClickedRacket())
            {
                return;
            }

            aimJoystick.gameObject.SetActive(true);
            aimJoystick.gameObject.transform.position = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            aimJoystick.gameObject.SetActive(false);
        }
    }

    private bool IsClickedRacket()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

        if (hit.collider == null)
        {
            return false;
        }

        GameObject clickedObject = hit.transform.gameObject;
        foreach (var iter in rackets)
        {
            if (iter == clickedObject)
            {
                return true;
            }
        }

        return false;
    }
}
