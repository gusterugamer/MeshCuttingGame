using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour, IDragHandler
{
    [SerializeField] private Camera _mainCamera;

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPostition = Vector3.zero;

    public SpriteShapeController victim;
    public Material capMat;

    private bool _isInCollider;

    public void OnDrag(PointerEventData eventData)
    {       
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        var position = Vector3.zero;
        if (Physics.Raycast(ray, out var hit))
        {
            position = hit.point;
        }
        else return;

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
