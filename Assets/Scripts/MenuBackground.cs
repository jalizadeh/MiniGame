using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackground : MonoBehaviour
{
    Camera camera;
    float percent;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        percent += Time.deltaTime * 0.01f;
        camera.backgroundColor = new Color(percent, percent, percent, 1f);
    }
}
