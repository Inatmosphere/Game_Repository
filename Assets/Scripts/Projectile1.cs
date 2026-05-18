using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Projectile1 : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject bullet;
    public float shootForce;
    public float upwardForce;

    // weapon stat area
    [Header("Weapon Stats")]
    public float timeBetweenShooting;
    public float spread;
    public float reloadTime;
    public float timeBetweenShots;
    public int magazineSize;
    public int bulletsPerTap;
    public bool allowButtonHold;

    int bulletsLeft;
    int bulletsShot;

    // state
    bool shooting;
    bool readyToShoot;
    bool reloading;

    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;

    bool allowInvoke = true;

    // Special effect area
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    void Update()
    {
        MyInput();

        if (ammunitionDisplay != null)
        {
            // Ensure bulletsPerTap is not 0 to avoid division by zero
            int magDisplay = (bulletsPerTap > 0) ? bulletsLeft / bulletsPerTap : bulletsLeft;
            int magSizeDisplay = (bulletsPerTap > 0) ? magazineSize / bulletsPerTap : magazineSize;

            ammunitionDisplay.SetText(magDisplay + " / " + magSizeDisplay);
        }
    }

    void MyInput()
    {
        // reload-----------
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
            Reload();

        // typo fix: bulletsLeft (not bulletLeft)
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0)
            Reload();

        // handle shooting input
        if (allowButtonHold)
            shooting = Input.GetKey(KeyCode.Mouse0);
        else
            shooting = Input.GetKeyDown(KeyCode.Mouse0);

        // shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    void Shoot()
    {
        readyToShoot = false;

        // spawn bullet
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot++;

        // ray from center of screen
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75f);

        // direction calculations
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0f);

        // point bullet toward target
        currentBullet.transform.forward = directionWithSpread.normalized;

        Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // main impulse
            rb.AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
            // upward force (typo fix: transform, not tranform)
            rb.AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);
        }

        // muzzle flash
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        // reset shot
        if (allowInvoke)
        {
            Invoke(nameof(ResetShot), timeBetweenShooting);
            allowInvoke = false;
        }

        // multi‑shot per tap
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }

    void ResetShot()
    {
        // typo fix: allowInvoke, not allowToShoot
        allowInvoke = true;
        readyToShoot = true;
    }

    void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}