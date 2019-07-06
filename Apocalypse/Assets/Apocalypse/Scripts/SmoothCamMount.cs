﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamMount : MonoBehaviour
{
    public Transform Mount = null;
    public float speed = 5.0f;
    private int _attackHash = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, Mount.position, Time.deltaTime * speed);
        transform.rotation = Quaternion.Slerp(transform.rotation, Mount.rotation, Time.deltaTime * speed);
    }
}
