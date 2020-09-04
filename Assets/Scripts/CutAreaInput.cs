using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour, IDragHandler
{
    private Camera _mainCamera;

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPostition = Vector3.zero;

    public SpriteShapeController victim;
    public Material capMat;

    private bool _isInCollider;
    private void Awake()
    {
        _mainCamera = Camera.main;
    }   

    public void OnDrag(PointerEventData eventData)
    {
        var position = _mainCamera.ScreenToWorldPoint(eventData.position);
        position.z = 0f;      

        if (Physics2D.Raycast(position, Vector2.zero, 0f))
        {
            _isInCollider = true;
            //_startPosition = position;       
        }
        else
        {
            if (!_isInCollider)
            {
                _startPosition = position;
                _endPostition = position;
            }
            else
            {
                _endPostition = position;

                MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition);

                _isInCollider = false;
                _startPosition = Vector3.zero;
                _endPostition = Vector3.zero;
            }
        }
    }
}
