using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
public class  LevelObstacle : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D bc;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {        
    }

    private void Move(Vector2 direction)
    {
        transform.Translate(direction);
    }

    private void Init()
    {
        float randomX = Random.Range(-10, 10);
        float randomY = Random.Range(-10, 10);
        rb = GetComponent<Rigidbody2D>();
        rb.angularDrag = 0.0f;
        rb.drag = 0.0f;
        rb.gravityScale = 0.0f;
        rb.velocity = new Vector2(randomX, randomY);
        gameObject.layer = LayerMask.NameToLayer("Obstacles");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log(collision.transform.name);
    }
}
