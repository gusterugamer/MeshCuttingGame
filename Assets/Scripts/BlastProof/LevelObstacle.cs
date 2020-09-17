using UnityEngine;
 
[RequireComponent(typeof(Rigidbody2D),typeof(BoxCollider2D))]
public class  LevelObstacle : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D bc;

    private Vector3 _startPos;

    public Vector3 StartPos { get => _startPos;}

    private void Awake()
    {
        Init();
    }

    private void Start()
    {        
    }   

    private void Init()
    {
        float randomX = Random.Range(-10f, 10f);
        float randomY = Random.Range(-10f, 10f);
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

    public void SetStartPosition(Vector3 pos)
    {
        _startPos = pos;
    }

    public void Reset()
    {
        transform.position = _startPos;
        Init();
    }
}
