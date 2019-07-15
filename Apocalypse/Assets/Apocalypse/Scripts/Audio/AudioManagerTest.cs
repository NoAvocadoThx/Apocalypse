using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerTest : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        if (AudioManager.instance)
        {
            AudioManager.instance.SetTrackVolume("Zombies", 10, 5);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
