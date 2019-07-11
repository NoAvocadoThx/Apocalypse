using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class CameraScreenEffect : MonoBehaviour
{
    [SerializeField] private Shader _shader = null;
    [SerializeField] private float _bloodAmount = 0.0f;
    [SerializeField] private float _minBloodAmount = 0.0f;
    [SerializeField] private Texture2D _bloodTex = null;
    [SerializeField] private Texture2D _bloodNormal = null;
    [SerializeField] private float _distortion = 1.0f;
    [SerializeField] private bool _autoFade = true;
    [SerializeField] private float _fadeSpeed = 0.05f;

    private Material _material = null;


    //getter and setter
    public float bloodAmount { get { return _bloodAmount; } set { _bloodAmount = value; } }
    public float minBloodAmount { get { return _minBloodAmount; } set { _minBloodAmount = value; } }
    public float fadeSpeed { get { return _fadeSpeed; } set { _fadeSpeed = value; } }
    public bool autoFade { get { return _autoFade; } set { _autoFade = value; } }

    /*********************************************************/
    void Update()
    {
        if (_autoFade)
        {
            _bloodAmount -= _fadeSpeed * Time.deltaTime;
            _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
        }
    }

    /*********************************************************/
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!_shader) return;
        if (!_material)
        {
            _material = new Material(_shader);
        }
        if (!_material) return;

        //send date to shaders
        if (_bloodTex)
        {
            _material.SetTexture("_BloodTex", _bloodTex);
        }
        if (_bloodNormal)
        {
            _material.SetTexture("_BloodBump", _bloodNormal);
        }
        _material.SetFloat("_Distortion", _distortion);
        _material.SetFloat("_BloodAmount", _bloodAmount);

        Graphics.Blit(source, destination, _material);
    }
}
