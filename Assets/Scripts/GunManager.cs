using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum GUNTYPE {
    M1911,
    AK47,
    NUM_TYPE
}

[System.Serializable]
public class GunTypeContainer
{
    public GunData gunData;
    public GameObject model;
}

public class GunManager : MonoBehaviour
{

    [SerializeField] private GameObject DEBUG_GunMuzzleFlash;


    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private TrailRenderer bulletTrailPrefab;
    [SerializeField] private GUNTYPE startingType; 


    [SerializeField] private float trailSpeed = 15;
    [SerializeField] private float gunVerticalLerpFactor;

    [SerializeField] private float autoReloadTime = 0.3f;

    [SerializeField] private List<GunTypeContainer> containerList;

    private GunTypeContainer currentGunContainer;
    private int currentGunAmmo; 
    private Vector3 gunTargetPosition; 
    private Vector3 gunFacingDirection;

    private Transform currentFirePoint;

    private bool firedGun;
    private bool isReloading; 
    private IEnumerator fireRateCoroutine = null;

    private float fireRateTimer = 0; 

    void Awake()
    {
        ChangeGun(startingType);

        firedGun = false;
        isReloading = false;
    }

    private void OnEnable()
    {
        PlayerInputManager.onLeftMouseHold += ShootWeapon;
        CameraController.onFirstPersonCameraRotate += ReadCameraTransform;
        PlayerMovement.onGunPlaceholderMove += HandleGunTargetPosition;
    }

    private void OnDisable()
    {
        PlayerInputManager.onLeftMouseHold -= ShootWeapon;
        CameraController.onFirstPersonCameraRotate -= ReadCameraTransform;
        PlayerMovement.onGunPlaceholderMove -= HandleGunTargetPosition;
    }

    // Update is called once per frame
    void Update()   
    {
        currentGunContainer.model.transform.position = gunTargetPosition;
        Vector3 lerpVector = Vector3.Lerp(currentGunContainer.model.transform.forward, gunFacingDirection, Time.deltaTime * gunVerticalLerpFactor);
        currentGunContainer.model.transform.rotation = Quaternion.LookRotation(lerpVector);


        if (firedGun)
        {
            fireRateTimer += Time.deltaTime; 
            if (fireRateTimer > currentGunContainer.gunData.fireRate)
            {
                fireRateTimer = 0f;
                firedGun = false; 
            }
        }
    }


    private void HandleGunTargetPosition(Vector3 targetPos)
    {
        gunTargetPosition = targetPos;
    }


    private void ReadCameraTransform(Vector3 forward, Vector3 right)
    {
        if (currentGunContainer == null)
            return;

        gunFacingDirection = -forward; 
        //currentGunContainer.model.transform.rotation = Quaternion.LookRotation(-forward);
    }

    private IEnumerator ShootFireRateCoroutine()
    {
        if (currentGunContainer == null)
            yield return null;
        yield return new WaitForSeconds(currentGunContainer.gunData.fireRate);
        Debug.Log("FIRED GUN FALSE");
        firedGun = false;
        fireRateCoroutine = null;
    }

    private void ShootWeapon()
    {

        if (currentGunContainer == null || firedGun || isReloading)
            return;


        fireRateTimer = 0f;
        firedGun = true;

        currentGunAmmo -= 1;
        if (currentGunAmmo <= 0)
            StartCoroutine(ReloadCoroutine());

        //if (fireRateCoroutine == null)
        //{
        //    fireRateCoroutine = ShootFireRateCoroutine();
        //    StartCoroutine(fireRateCoroutine);
        //}
        Debug.Log("CURRENT AMMO: " + currentGunAmmo);
        GameObject instance = Instantiate(DEBUG_GunMuzzleFlash, currentFirePoint.transform);
        HandleHitScanShoot();
    }


    private void HandleHitScanShoot()
    {
        int distance = 100;
        RaycastHit hitInfo;
        Physics.Raycast(currentFirePoint.position, currentFirePoint.forward, out hitInfo, enemyLayerMask, distance);

        if (hitInfo.collider != null)
        {
            Debug.Log("Hit something");

            if (hitInfo.collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.TakeDamage(currentGunContainer.gunData.damage);
            }
        }
        HandleBulletTrail();
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(autoReloadTime);
        currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
        isReloading = false;
    }

    private void HandleBulletTrail()
    {
        TrailRenderer trail = Instantiate(bulletTrailPrefab, currentFirePoint.position, currentFirePoint.rotation);
        StartCoroutine(BulletTrailCoroutine(trail, currentFirePoint.forward)); 
    }


    private IEnumerator BulletTrailCoroutine(TrailRenderer trail, Vector3 direction)
    {
        float timer = 0;
        while (timer < 1)
        {
            trail.transform.position += direction * trailSpeed * timer;
            timer += Time.deltaTime / trail.time;
            yield return null;
        }

        Destroy(trail.gameObject, trail.time);
    }

    private void ChangeGun(GUNTYPE gunType)
    {
        foreach (GunTypeContainer container in containerList)
        {
            container.model.SetActive(false);
            if (container.gunData.type == gunType)
            {
                container.model.SetActive(true);
                currentGunContainer = container;
                currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
                currentFirePoint = container.model.transform.Find("FirePoint");
            }
        }

    }
}
