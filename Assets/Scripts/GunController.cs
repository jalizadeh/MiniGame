using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun startingGun;
    Gun equippedGun;


    private void Start()
    {
        if (startingGun != null)
            EquipGun(startingGun);
    }


    public void EquipGun(Gun gunToEquip) {
        if (equippedGun != null)
            Destroy(equippedGun.gameObject);

        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
    }


    public void OnTriggerHold() {
        if (equippedGun != null)
            equippedGun.OnTriggerHold();
    }

    public void OnTriggerReleased()
    {
        if (equippedGun != null)
            equippedGun.OnTriggerReleased();
    }


    //Get the Y-position of the gun
    public float GunHeight
    {
        get
        {
            return weaponHold.position.y;
        }
    }


    //More precise look-at mechanism
    public void Aim(Vector3 aimPoint) {
        if (equippedGun != null)
            equippedGun.Aim(aimPoint);
    }


    //Reload the gun
    public void Reload()
    {
        if (equippedGun != null)
            equippedGun.Reload();
    }
}
