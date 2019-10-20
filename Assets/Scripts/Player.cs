using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5f;
    PlayerController controller;
    GunController gunController;

    Camera viewCamera;

    public Crosshairs crosshair;


    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start(); // run Start() in `LivingEntity` first, then this Start()
    }


    void OnNewWave(int waveNumber) {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 velocity = direction.normalized * moveSpeed;
        controller.Move(velocity);


        // Look at mouse position
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.forward * gunController.GunHeight);
        float rayDistance;

        if(groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);

            controller.LookAt(point);

            crosshair.transform.position = point;
            crosshair.DetectTarget(ray);

            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gunController.Aim(point);
            }
            
        }


        // Weapon shoots
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        // Stop shooting
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerReleased();
        }


        //Reload the gun
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }

        // GOD mode, auto shoot, no need for holding mouse button
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(AutoShoot());
        }

        if(transform.position.y < -5)
        {
            TakeDamage(health);
        }
    }

    IEnumerator AutoShoot() {
        while (true)
        {
            gunController.OnAutoShoot();
            yield return null;
        }
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
