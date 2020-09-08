using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private Camera _mainCamera;

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPostition = Vector3.zero;

    [SerializeField] private CustomBoundryBox cbm;

    public SpriteShapeController victim;
    public Material capMat;

    private bool _isInCollider;

    public void OnDrag(PointerEventData eventData)
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        var position = Vector3.zero;
        Physics.Raycast(ray, out var hit);
        position = hit.point;


        position.z = 0f;

        if (Physics2D.Raycast(position, Vector2.zero, 0f))
        {
            _isInCollider = true;
            KeyValuePair<int, int> pair;
            var y = BlastProof.Mathematics.ClosestDistanceToPolygon(cbm.ToArray(), position, ref pair);
            //Debug.Log(y);
            if (y < 2.0f)
            {
                var x = BlastProof.Mathematics.ProjectPointOnLineSegment(cbm.m_CustomBox[pair.Key].m_pos, cbm.m_CustomBox[pair.Value].m_pos, position);
                _endPostition = x;
                Debug.Log(x);
                if (MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition))
                {
                    _isInCollider = false;
                }
            }
        }

        if (!_isInCollider)
        {
            _startPosition = position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
    }
}
