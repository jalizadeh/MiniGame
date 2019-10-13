using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    public LayerMask targetMask;
    public SpriteRenderer dot;
    public Color dotHighlightColor;
    Color originialDotColor;
    public float rotationSpeed = 40f;

    // Start is called before the first frame update
    void Start()
    {
        originialDotColor = dot.color;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    public void DetectTarget(Ray ray) {
        if(Physics.Raycast(ray, 100, targetMask))
        {
            dot.color = dotHighlightColor;
        } else
        {
            dot.color = originialDotColor;
        }
    }
}
