using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
  
    public Transform[] muzzle;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    float nextShotTime;

    public Transform shell;
    public Transform shellEjection;

    MuzzleFlash muzzleFlash;

    
    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;
    public int burstCount;
    int shotsRemainingBurst;
    bool triggerReleasedSinceLastShot;
    

    private void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingBurst = burstCount;
    }

    void Shoot() {
        if(Time.time > nextShotTime)
        {
            if(fireMode == FireMode.Burst)
            {
                if (shotsRemainingBurst == 0) {
                    return;
                }

                shotsRemainingBurst--;
            }

            if (fireMode == FireMode.Single) {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }

            for (int i = 0; i < muzzle.Length; i++)
            {
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, muzzle[i].position, muzzle[i].rotation) as Projectile;
                newProjectile.setSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerReleased() {
        triggerReleasedSinceLastShot = true;

        //reload the gun
        shotsRemainingBurst = burstCount;
    }

}
