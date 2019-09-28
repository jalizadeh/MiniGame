using UnityEngine;

/*
 * Interface for damageable objects
 */
public interface IDamageable
{
    void TakeHit(float damage, RaycastHit hit);

    void TakeDamage(float damage);
}
