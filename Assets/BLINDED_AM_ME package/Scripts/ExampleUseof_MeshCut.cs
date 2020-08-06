using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleUseof_MeshCut : MonoBehaviour {

	public Material capMaterial;
	public List<IntersectionPoint> list;

	private Vector3 startPos = Vector3.zero;
	private Vector3 endPos = Vector3.zero;
	private Camera mainCam;
	public GameObject victim;

	// Use this for initialization
	void Start () {
		mainCam = Camera.main;
		startPos = new Vector2(1.0f, 1.0f);
		endPos = Vector2.zero;
		victim.GetComponent<CustomBoundryBox>().CreateCustomBoundary();
	}
	
	void Update(){
		GetMousePosition();
		Debug.DrawLine(startPos, endPos, Color.black);
	}

	void CutStuff(Vector3 startPos, Vector3 endPos)
    {
		GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(victim, transform.position, transform.right, capMaterial, startPos, endPos);
			list = BLINDED_AM_ME.MeshCut.intersectionPoint;	

			foreach (var point in list)
            {
				Debug.Log("Point: " + point._pos);
				GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
				cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
				GameObject instanta = Instantiate(cube, Vector3.zero, Quaternion.identity, victim.transform);
				instanta.transform.localPosition= new Vector3(point._pos.x, point._pos.y, -0.5f);				
			}

			if (!pieces[1].GetComponent<DrawBounds>())
				pieces[1].AddComponent<DrawBounds>();					
	}	

	void GetMousePosition()
    {	
		if (Input.GetMouseButtonDown(0))
		{
			startPos = Input.mousePosition;
			startPos.z = Mathf.Abs(victim.transform.position.z - mainCam.transform.position.z);
			startPos = mainCam.ScreenToWorldPoint(startPos);
		}
		if (Input.GetMouseButtonUp(0))
		{
			endPos = Input.mousePosition;
			endPos.z = Mathf.Abs(victim.transform.position.z - mainCam.transform.position.z);
			endPos = mainCam.ScreenToWorldPoint(endPos);
			mousePosToWorld();
		}	
	}

	void mousePosToWorld()
    {	
		CutStuff(startPos, endPos);
		Debug.Log("MousePos: " + "start: " + startPos + "end: " + endPos);
    }
	
	void OnDrawGizmosSelected() {

		Gizmos.color = Color.green;

		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5.0f);
		Gizmos.DrawLine(transform.position + transform.up * 0.5f, transform.position + transform.up * 0.5f + transform.forward * 5.0f);
		Gizmos.DrawLine(transform.position + -transform.up * 0.5f, transform.position + -transform.up * 0.5f + transform.forward * 5.0f);

		Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
		Gizmos.DrawLine(transform.position,  transform.position + -transform.up * 0.5f);

	}

}
