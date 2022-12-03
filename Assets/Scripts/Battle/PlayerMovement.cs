using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool wasJustCLicked;
    private bool canMove;
    private Vector2 playerSize;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        playerSize = GetComponent<SpriteRenderer>().bounds.extents;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (wasJustCLicked)
            {
                wasJustCLicked = false;

                // when you capture it
                if ((mousePos.x >= transform.position.x && mousePos.x < transform.position.x + playerSize.x ||
                    mousePos.x <= transform.position.x && mousePos.x > transform.position.x -playerSize.x) &&
                    (mousePos.y >= transform.position.y && mousePos.y < transform.position.y + playerSize.y ||
                    mousePos.y <= transform.position.y && mousePos.y > transform.position.y - playerSize.y))
                {
                    canMove = true;
                }
                else
                {
                    canMove = false;
                }
            }

            if (canMove)
            {
                rb.MovePosition(mousePos);
            }
        }
        else
        {
            wasJustCLicked = true;
        }
    }
}
