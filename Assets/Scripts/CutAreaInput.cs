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

    private Vector3 _startPosition;
    private Vector3 _endPostition;
    private Vector2[] polygon;

    private bool _isInCollider = false;
    private bool _collidedWithObject = false;

    public delegate void CutDelegate();
    public delegate void ObjectCutDelegate();

    public event CutDelegate OnCutDone;
    public event ObjectCutDelegate OnObjectCut;

    private void Start()
    {
        polygon = cbm.ToArray();
        cutter = new Cutter();
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
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

            if (Mathematics.IsPointInPolygon(position, polygon))
            {
                _isInCollider = true;
            }
            else
            {
                if (!_isInCollider)
                {
                    _startPosition = position;
                }
                else
                {
                    _endPostition = position;
                    //if (_startPosition != cbm.PolygonCenter && _endPostition != cbm.PolygonCenter)
                    //{
                    //    RaycastHit2D hit1;
                    //    Debug.DrawRay(_startPosition, (_endPostition - _startPosition).normalized);
                    //    if (hit1 = Physics2D.Raycast(_startPosition, (_endPostition - _startPosition).normalized, Vector2.Distance(_startPosition, _endPostition), LayerMask.NameToLayer("Obstacles")))
                    //    {
                    //        OnObjectCut?.Invoke();
                    //        _collidedWithObject = true;
                    //    }
                    //}        


                    if (!_collidedWithObject)
                    {
                        Time.timeScale = 0.0f;
                        if (cutter.Cut(victim, capMat, _startPosition, _endPostition, LM.Obstacles))
                        {
                            OnCutDone?.Invoke();
                        }
                        _isInCollider = false;
                        _startPosition = cbm.PolygonCenter;
                        _endPostition = cbm.PolygonCenter;
                        Time.timeScale = 1.0f;
                    }
                }
            }
        }
    }
}