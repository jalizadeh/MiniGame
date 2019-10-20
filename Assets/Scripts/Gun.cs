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
    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;
    public int burstCount;
    int shotsRemainingBurst;
    bool triggerReleasedSinceLastShot;


    [Header("Effects")]
    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    MuzzleFlash muzzleFlash;



    [Header("Recoil")]
    //for gun shooting animation
    public Vector2 recoilKickMinMax = new Vector2(0.05f, 0.2f);
    public Vector2 recoilAngleMinMax = new Vector2(3, 5);
    public float recoilMoveSettleTime = 0.1f;
    public float recoilRotationSettleTime = 0.1f;
    Vector3 recoidSmoothDampVelocity;
    float recoilRotationSmoothDampVelocity;
    float recoilAngle;


    [Header("Gun Reload")]
    public int projectilesPerMag = 10;
    int remainingProjectiles;
    public float reloadTime = 0.3f;
    bool isReloading;
    public float maxReloadAngle = 10f;


    private void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingBurst = burstCount;

        remainingProjectiles = projectilesPerMag;
    }

    //used "LateUpdate" to make sure all changes happen after "Aim()"
    private void LateUpdate()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoidSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        //auto-reload
        if (!isReloading && remainingProjectiles == 0)
        {
            Reload();
        }
    }

    void Shoot() {
        if(!isReloading && Time.time > nextShotTime && remainingProjectiles > 0)
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
                if(remainingProjectiles == 0)
                {
                    break;
                }
                remainingProjectiles--;
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, muzzle[i].position, muzzle[i].rotation) as Projectile;
                newProjectile.setSpeed(muzzleVelocity);
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();

            //on each shoot, the gun kick back itself
            transform.localPosition += Vector3.forward * Random.Range(recoilKickMinMax.x, recoilKickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootSound, transform.position);
        }
    }

    // if I press "G", it shoots for ever
    public void OnAutoShoot()
    {
        Shoot();
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


    public void Aim(Vector3 aimPoint) {
        transform.LookAt(aimPoint);
    }

    //if not reloading and the remaining is less
    public void Reload() {
            if (!isReloading && remainingProjectiles != projectilesPerMag)
            {
                StartCoroutine(ReloadAnimation());
                AudioManager.instance.PlaySound(reloadSound, transform.position);
            }
    }

    IEnumerator ReloadAnimation() {
        isReloading = true;

        float speed = 1 / reloadTime;
        float percent = 0f;
        Vector3 initialRotation = transform.localEulerAngles;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRotation + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        remainingProjectiles = projectilesPerMag;
    }
}
