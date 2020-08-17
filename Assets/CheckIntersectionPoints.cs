using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIntersectionPoints : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        foreach(var col in collision.contacts)
        {
            var go = new GameObject();
            go.transform.position = col.point;
        }
    }
}
