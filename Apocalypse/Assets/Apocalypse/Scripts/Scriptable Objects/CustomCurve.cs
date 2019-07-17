using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Custom Animation Curve")]
public class CustomCurve : ScriptableObject
{
    [SerializeField] AnimationCurve _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

    public float Evaluate(float v)
    {
        return _curve.Evaluate(v);
    }
}
