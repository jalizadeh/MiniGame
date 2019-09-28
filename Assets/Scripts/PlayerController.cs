using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody rigidbody;
    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Move with arrow keys
    public void Move(Vector3 _velocity) {
        velocity = _velocity;
    }

    // Rotate based on mouse position
    public void LookAt(Vector3 lookPoint) {
        // the lookPoint is located on the ground -> y=0
        // to prevent rotation around y axis, we have 2 ways

        // #1: change the lookPoint.y, to match the object's y
        //Vector3 correctedHeight = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);

        // #2: lookPoint.y = 0, so add the object's y, with that
        transform.LookAt(lookPoint + Vector3.up * transform.position.y);
    }

    // Same is Update(), but because of rigid body it is better to use this
    private void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);    
    }

}
