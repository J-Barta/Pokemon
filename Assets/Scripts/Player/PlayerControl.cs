using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed;
    public float runSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask GrassLayer;

    private bool isMoving;
    private bool isRunning;
    private Vector2 input;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        isRunning = Input.GetKey("space");

        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            
            //Remove diagonal movement
            if (input.x != 0) input.y = 0;

            if(input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if(IsWalkable(targetPos))
                {
                    if (!isRunning)
                    {
                        StartCoroutine(Move(targetPos, moveSpeed));
                    }
                    else
                    {
                        StartCoroutine(Move(targetPos, runSpeed));
                    }
                }
            }
        }

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isRunning);
    }

    IEnumerator Move(Vector3 targetPos, float Speed)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.3f, solidObjectsLayer) != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.3f, GrassLayer) != null)
        {
            if (Random.Range(1, 101) <= 10)
            {
                Debug.Log("Encountered a wild pokemon");
            }
        }
    }
}   
