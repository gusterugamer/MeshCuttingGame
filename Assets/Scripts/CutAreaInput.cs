using BlastProof;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private SpriteShapeController victim;
    [SerializeField] private Material capMat;
    [SerializeField] private LevelManager LM;

    private Cutter cutter;

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPostition = Vector3.zero;

    private bool _isInCollider = false;
    private bool _collidedWithObject = false;

    public delegate void CutDelegate();
    public delegate void ObjectCutDelegate();

    public event CutDelegate OnCutDone;
    public event ObjectCutDelegate OnObjectCut;

    private void Start()
    {
        cutter = new Cutter();
        _startPosition = Vector3.zero;
        _endPostition = Vector3.zero;
    }

    public void Update()
    {
        GetCutPoints();
    }

    private void GetCutPoints()
    {
        if (Input.GetMouseButton(0))
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
            }
            else
            {
                if (!_isInCollider)
                {
                    _startPosition = position;
                    _collidedWithObject = false;
                }
                else
                {
                    _endPostition = position;
                    if (_startPosition != cbm.PolygonCenter && _endPostition != cbm.PolygonCenter)
                    {
                        if (Physics.Raycast(_startPosition, (_endPostition - _startPosition).normalized, out RaycastHit hit1, Vector3.Distance(_startPosition, _endPostition)))
                        {
                            if (hit1.collider.tag == "Obstacle" && !_collidedWithObject)
                            {
                                OnObjectCut?.Invoke();
                                _collidedWithObject = true;                               
                            }
                        }
                    }

                    Plane plane = Mathematics.SlicePlane(_startPosition, _endPostition, _mainCamera.transform.forward);

                    if (LM.IsObjectsOnSameSide(plane) && !_collidedWithObject)
                    {
                        if (cutter.Cut(victim, capMat, _startPosition, _endPostition))
                        {
                            OnCutDone?.Invoke();
                        }
                        _isInCollider = false;
                        _startPosition = cbm.PolygonCenter;
                        _endPostition = cbm.PolygonCenter;
                    }
                }
            }
        }
    }
}
