using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{

    public float startingHealth;
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        TakeDamage(damage);
    }


    [ContextMenu("Self Destruct")]
    void Die() {
        dead = true;

        if (OnDeath != null)
            OnDeath();

        GameObject.Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }
}
