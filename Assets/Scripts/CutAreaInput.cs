using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class CutAreaInput : MonoBehaviour, IDragHandler
{
    private Camera _mainCamera;

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPostition = Vector3.zero;

    [SerializeField] private MeshCut mc;

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
            Debug.Log($"Pointer hit collider {position}");
        }
        else
        {
            if(!_isInCollider)
                _startPosition = position;
            else
            {
                _endPostition = position;

                mc.StartCutting(_startPosition, _endPostition);

                _isInCollider = false;
                _startPosition = Vector3.zero;
                _endPostition = Vector3.zero;
            }
        }
    }
}
