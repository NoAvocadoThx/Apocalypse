﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private Animator _animator = null;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical") * 5.66f;

        _animator.SetFloat("Horizontal", xAxis);
        _animator.SetFloat("Vertical", yAxis);
    }
}
