using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleUseof_MeshCut : MonoBehaviour {

	public Material capMaterial;
	public List<CustomBoundryBox.BoundaryPoint> list;

	private Vector3 startPos;
	private Vector3 endPos;
	private Camera mainCam;

	// Use this for initialization
	void Start () {
		mainCam = Camera.main;		
	}
	
	void Update(){
		GetMousePosition();
		Debug.DrawLine(startPos, endPos, Color.black);
	}

	void CutStuff(Vector3 startPos, Vector3 endPos)
    {
		RaycastHit hit;

		if (Physics.Raycast(transform.position, transform.forward, out hit))
		{

			GameObject victim = hit.collider.gameObject;

			GameObject[] pieces = BLINDED_AM_ME.MeshCut.Cut(victim, transform.position, transform.right, capMaterial, startPos, endPos);
			list = BLINDED_AM_ME.MeshCut.intersectionPoint;

			foreach (var point in list)
            {
				Debug.Log("Point: " + point.m_pos);
            }

			if (!pieces[1].GetComponent<DrawBounds>())
				pieces[1].AddComponent<DrawBounds>();

			//Destroy(pieces[1], 1);
		}
	}

	void GetMousePosition()
    {
		if (Input.GetMouseButtonDown(0))
		{
			startPos = Input.mousePosition;
			startPos.z = 10.0f;
		}
		if (Input.GetMouseButtonUp(0))
		{
			endPos = Input.mousePosition;
			endPos.z = 10.0f;
			mousePosToWorld();
		}
	}

	void mousePosToWorld()
    {
		startPos = mainCam.ScreenToWorldPoint(startPos);
		endPos = mainCam.ScreenToWorldPoint(endPos);
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
