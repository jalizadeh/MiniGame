using UnityEngine;

/*
 * Interface for damageable objects
 */
public interface IDamageable
{
    void TakeHit(float damage, Vector3 hitPosition, Vector3 hitDirection);

    void TakeDamage(float damage);
}
