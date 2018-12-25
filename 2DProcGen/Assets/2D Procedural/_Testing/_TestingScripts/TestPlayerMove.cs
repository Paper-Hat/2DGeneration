using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerMove : MonoBehaviour
{
    private Rigidbody2D playerRB;
    [SerializeField] private float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MakeMovement();
        playerRB.velocity = new Vector2(0, 0);
    }

    private void MakeMovement()
    {
        Vector2 axes = DetermineProperMovement();
        if (!axes.Equals(Vector2.zero))
        {
            Vector2 movForce = axes * speed;
            playerRB.AddForce(movForce);
        }
    }
    private Vector2 DetermineProperMovement()
    {
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) >= 1 && Mathf.Abs(Input.GetAxisRaw("Vertical")) >= 1)
            return new Vector2(Input.GetAxisRaw("Horizontal") / 1.4144f, Input.GetAxisRaw("Vertical") / 1.4144f);
        else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
}
