﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController : MonoBehaviour
{
    public float speed = 0.0f;

    private Animator _controller = null;
    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _controller.SetFloat("Speed", speed);
    }
}
