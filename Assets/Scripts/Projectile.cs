using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float speed = 10f;
    float damage = 1f;
    public LayerMask collisionLayer;

    //after how many seconds the object destroy
    float lifeTime = 2f;
    float skinWidth = 0.1f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, collisionLayer);
        if (initialCollisions.Length > 0) {
            OnHitObject(initialCollisions[0], transform.position);
        }
    }


    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = speed * Time.deltaTime;
        CheckCollision(distance);

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }


    void CheckCollision(float distance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, distance + skinWidth, collisionLayer, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }


    void OnHitObject(Collider c, Vector3 hitPosition)
    {
        //print(hit.transform.gameObject.name);
        IDamageable damageableObject = c.GetComponent<IDamageable>();
        if (damageableObject != null) //make sure this item can be damaged, like enemy
        {
            damageableObject.TakeHit(damage, hitPosition, transform.forward);
        }

        //if the projectile hit an object, it is destroyed
        GameObject.Destroy(gameObject);
    }
}
