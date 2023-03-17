using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public int FPS = 60;

    [HideInInspector]
    public static float fps;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = FPS;
    }

    // Update is called once per frame
    void Update()
    {
        fps = 1.0f / Time.deltaTime;
    }
}
