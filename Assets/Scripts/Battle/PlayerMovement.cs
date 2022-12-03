using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private bool wasJustCLicked;
    private bool canMove;
    private Vector2 playerSize;
    private Rigidbody2D rb;

    public Transform boundaryHolder;
    Boundary playerBoundary;

    struct Boundary
    {
        public float Up, Down, Left, Right;

        public Boundary(float up, float down, float left, float right)
        {
            Up = up;
            Down = down;
            Left = left;
            Right = right;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        playerSize = GetComponent<SpriteRenderer>().bounds.extents;
        rb = GetComponent<Rigidbody2D>();

        playerBoundary = new Boundary(
            boundaryHolder.GetChild(0).position.y,
            boundaryHolder.GetChild(1).position.y,
            boundaryHolder.GetChild(2).position.x,
            boundaryHolder.GetChild(3).position.x
        );
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
                    mousePos.x <= transform.position.x && mousePos.x > transform.position.x - playerSize.x) &&
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
                Vector2 clampedMousePos = new Vector2(
                    Mathf.Clamp(mousePos.x, playerBoundary.Left, playerBoundary.Right),
                    Mathf.Clamp(mousePos.y, playerBoundary.Down, playerBoundary.Up)
                );

                rb.MovePosition(clampedMousePos);
            }
        }
        else
        {
            wasJustCLicked = true;
        }
    }
}
