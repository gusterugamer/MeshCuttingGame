using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleUseof_MeshCut : MonoBehaviour
{

    public Material capMaterial;
    public List<IntersectionPoint> list;

    private Vector3 startPos = Vector3.zero;
    private Vector3 endPos = Vector3.zero;
    private Camera mainCam;
    public GameObject victim;

    // Use this for initialization
    void Start()
    {
        mainCam = Camera.main;
        startPos = new Vector2(1.0f, 1.0f);
        endPos = Vector2.zero;
        victim.GetComponent<CustomBoundryBox>().CreateCustomBoundary();
    }

    void Update()
    {
        GetMousePosition();
        Debug.DrawLine(startPos, endPos, Color.black);
    }

    void CutStuff(Vector3 startPos, Vector3 endPos)
    {
        GameObject[] pieces = MeshCut.Cut(victim, capMaterial, startPos, endPos);
        list = MeshCut.intersectionPoint;

        //TEST ONLY
        if (list != null)
        {
            foreach (var point in list)
            {
                Debug.Log("Point: " + point._pos);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                GameObject instanta = Instantiate(cube, Vector3.zero, Quaternion.identity, victim.transform);
                instanta.transform.localPosition = new Vector3(point._pos.x, point._pos.y, -0.5f);
                Destroy(instanta.GetComponent<BoxCollider>());
            }
        }
        //TEST ONLY
        if (pieces != null)
        {
            if (!pieces[1].GetComponent<DrawBounds>())
                pieces[1].AddComponent<DrawBounds>();
        }
    }

    void GetMousePosition()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;       
            Ray startRay = mainCam.ScreenPointToRay(startPos);           
            RaycastHit hit;
            Physics.Raycast(startRay, out hit, Mathf.Infinity);          
            startPos = hit.point;
        }
        if (Input.GetMouseButtonUp(0))
        {
            endPos = Input.mousePosition;
            Ray endRay = mainCam.ScreenPointToRay(endPos);
            RaycastHit hit;
            Physics.Raycast(endRay, out hit, Mathf.Infinity);
            endPos = hit.point;            

            mousePosToWorld();
        }
    }

    void mousePosToWorld()
    {
        CutStuff(startPos, endPos);
        Debug.Log("MousePos: " + "start: " + startPos + "end: " + endPos);
    }

    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5.0f);
        Gizmos.DrawLine(transform.position + transform.up * 0.5f, transform.position + transform.up * 0.5f + transform.forward * 5.0f);
        Gizmos.DrawLine(transform.position + -transform.up * 0.5f, transform.position + -transform.up * 0.5f + transform.forward * 5.0f);

        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + -transform.up * 0.5f);

    }

}
