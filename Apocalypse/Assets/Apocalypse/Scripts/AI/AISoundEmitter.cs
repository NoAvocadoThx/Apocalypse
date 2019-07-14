using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundEmitter : MonoBehaviour
{

    //inspector
    [SerializeField] private float _decayRate = 0;


    //private
    private SphereCollider _collider = null;
    private float _srcRadius = 0.0f;
    private float _targetRadius = 0.0f;
    private float _interpolator = 0.0f;
    private float _interPolateSpeed = 0.0f;


    /*********************************************************/
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<SphereCollider>();
        if (!_collider) return;

        //set radius val
        _srcRadius = _targetRadius = _collider.radius;
        //setup interpolator
        _interpolator = 0.0f;
        if (_decayRate > 0.02f)
        {
            _interPolateSpeed = 1.0f / _decayRate;
        }
        else
        {
            _interPolateSpeed = 0.0f;
        }
    }

    /*********************************************************/
    private void FixedUpdate()
    {
        if (!_collider) return;
        _interpolator = Mathf.Clamp01(_interpolator + Time.deltaTime * _interPolateSpeed);
        _collider.radius = Mathf.Lerp(_srcRadius, _targetRadius, _interpolator);

        //smaller than 0
        if (_collider.radius < Mathf.Epsilon) _collider.enabled = false;
        else _collider.enabled = true;
    }


    /*********************************************************/
    public void SetRadius(float radius,bool instantResize=false)
    {
        if (!_collider||_targetRadius==radius) return;
       
        
        //set the radius of collider
        _srcRadius = (instantResize||radius>_collider.radius)?radius: _collider.radius;
        _targetRadius = radius;
        _interpolator = 0.0f;
    }

    /*********************************************************/
    private void Update()
    {
        //walking radius
        SetRadius(2.0f);
        if (Input.GetKeyDown(KeyCode.R)) SetRadius(15.0f);
    }



}
