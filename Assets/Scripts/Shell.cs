using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public Rigidbody rigidbody;
    public float minForce;
    public float maxForce;
    float force;

    float waitTime = 4f;
    float fadeTime = 2f;

    // Start is called before the first frame update
    void Start()
    {
        force = Random.Range(minForce, maxForce);
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(Vector3.right * force);
        rigidbody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade() {
        yield return new WaitForSeconds(waitTime);

        float percent = 0;
        Material mat = GetComponent<Renderer>().material;
        Color initialColor = mat.color;

        while (percent < 1) {
            percent += Time.deltaTime * 1 / fadeTime;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }

        //after fad out, destroy the object
        Destroy(gameObject);
    }
}
