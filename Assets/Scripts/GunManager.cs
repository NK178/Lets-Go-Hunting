using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private float gunPositionLerpFactor;
    [SerializeField] private float gunRotationLerpFactor;

    [SerializeField] private float shootRaycastDistance = 80f;
    [SerializeField] private float autoReloadTime = 0.3f;

    [SerializeField] private List<GunTypeContainer> containerList;


    [SerializeField] private bool DEBUG_disableGunModel; 

    public static Action<int, int> onAmmoCountChanged; 

    private GunTypeContainer currentGunContainer;
    private int currentGunAmmo;

    private Transform gunPlayerTransform; 
    //private Vector3 gunTargetPosition; 
    private Vector3 gunFacingDirection;

    //I think i shld just create the 3d mouse pos here instead of from the camera controller 
    private Vector3 worldMousePos;
    private Vector2 screenMousePos;

    private Transform currentFirePoint;

    private bool firedGun;
    private bool isReloading;
    private bool isInShootingMode = false;
    private bool lerpGunPosition = true; 
    private IEnumerator fireRateCoroutine = null;

    

    private float fireRateTimer = 0;

    private SHIPMODE referenceShipMode; 

    void Awake()
    {

        gunPlayerTransform = GameObject.FindGameObjectWithTag("GunPoint").transform;
        ChangeGun(startingType);

        firedGun = false;
        isReloading = false;


        onAmmoCountChanged?.Invoke(currentGunAmmo, currentGunContainer.gunData.magazineAmmo);


    }

    private void OnEnable()
    {
        PlayerInputManager.onLeftMouseHold += ShootWeapon;
        CameraController.onFirstPersonCameraRotate += ReadCameraTransform;
        //PlayerMovement.onGunPlaceholderMove += HandleGunTargetPosition;

        Ship.onShipChangedMode += HandleWeaponShipMode;

        //bad way but will work for now 
        CameraController.onMouseMoved += SetScreenMousePos; 
        CameraController.onMouseMoved3DPos += Set3DMousePos;
    }

    private void OnDisable()
    {
        PlayerInputManager.onLeftMouseHold -= ShootWeapon;
        CameraController.onFirstPersonCameraRotate -= ReadCameraTransform;
        Ship.onShipChangedMode -= HandleWeaponShipMode;

        CameraController.onMouseMoved -= SetScreenMousePos;
        CameraController.onMouseMoved3DPos -= Set3DMousePos;


        //PlayerMovement.onGunPlaceholderMove -= HandleGunTargetPosition;


    }

    // Update is called once per frame
    void Update()   
    {
        //currentGunContainer.model.transform.position = gunTargetPosition;
        //Vector3 lerpVector = Vector3.Lerp(currentGunContainer.model.transform.forward, gunFacingDirection, Time.deltaTime * gunRotationLerpFactor);
        //currentGunContainer.model.transform.rotation = Quaternion.LookRotation(lerpVector);


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

    //private void LateUpdate()
    //{
    //    if (gunPlayerTransform != null)
    //    {
    //        //currentGunContainer.model.transform.position = gunPlayerTransform.transform.position;

    //        //Vector3 positionLerp = Vector3.Lerp(currentGunContainer.model.transform.position, 
    //        //                                    gunPlayerTransform.transform.position, 
    //        //                                    Time.deltaTime * gunPositionLerpFactor);


    //        Vector3 positionLerp = gunPlayerTransform.transform.position;
    //        if (lerpGunPosition)
    //        {
    //            positionLerp = Vector3.Lerp(currentGunContainer.model.transform.position,
    //                        gunPlayerTransform.transform.position,
    //                        Time.deltaTime * gunPositionLerpFactor);
    //        }

    //        Vector3 lerpVector = Vector3.Lerp(currentGunContainer.model.transform.forward, 
    //                                          gunFacingDirection, 
    //                                          Time.deltaTime * gunRotationLerpFactor);

    //        currentGunContainer.model.transform.position = positionLerp;
    //        currentGunContainer.model.transform.rotation = Quaternion.LookRotation(lerpVector);
    //    }
    //}


    private void LateUpdate()
    {
        if (gunPlayerTransform != null)
        {

            Vector3 positionLerp = Vector3.Lerp(currentGunContainer.model.transform.position,
                                        gunPlayerTransform.transform.position,
                                        Time.deltaTime * gunPositionLerpFactor);
            Vector3 rotationLerp = Vector3.Lerp(currentGunContainer.model.transform.forward,
                                              gunFacingDirection,
                                              Time.deltaTime * gunRotationLerpFactor);

            if (referenceShipMode == SHIPMODE.COMBAT)
            {
                positionLerp = gunPlayerTransform.transform.position;
                //Debug.Log("WORLD MOUSE: " + worldMousePos + " GUNTIP: " + currentFirePoint);
                //Vector3 lookVector = -(worldMousePos - currentGunContainer.model.transform.position).normalized;


                Vector3 lookVector = (worldMousePos - currentFirePoint.position).normalized;
                rotationLerp = Vector3.RotateTowards(
                                //-currentGunContainer.model.transform.forward,
                                currentFirePoint.forward,
                                lookVector,
                                gunRotationLerpFactor * Time.deltaTime,
                                0.0f
                            );


                //rotationLerp = Vector3.Lerp(currentGunContainer.model.transform.forward,
                //                            lookVector,
                //                            Time.deltaTime * gunRotationLerpFactor);
            }

            currentGunContainer.model.transform.position = positionLerp;
            currentGunContainer.model.transform.rotation = Quaternion.LookRotation(rotationLerp); 

        }
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
        //kinda a bad method should change 
        if (currentGunContainer == null || firedGun || isReloading || !isInShootingMode)
            return;

        fireRateTimer = 0f;
        firedGun = true;

        currentGunAmmo -= 1;
        onAmmoCountChanged?.Invoke(currentGunAmmo, currentGunContainer.gunData.magazineAmmo);

        if (currentGunAmmo <= 0)
            StartCoroutine(ReloadCoroutine());

        GameObject instance = Instantiate(DEBUG_GunMuzzleFlash, currentFirePoint.transform);
        HandleMouseRaycastShoot();
    }

    private void HandleMouseRaycastShoot()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, shootRaycastDistance, enemyLayerMask))
        {
            Debug.Log("Hit: " + hit.collider.name);

            Enemy enemyRef = hit.collider.gameObject.GetComponent<Enemy>();
            if (enemyRef != null)
            {
                enemyRef.TakeDamage(currentGunContainer.gunData.damage);
            }
        }
    }

    //For intial testing not gona be used 
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
                if (!DEBUG_disableGunModel)
                    container.model.SetActive(true);
                currentGunContainer = container;
                currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
                currentFirePoint = container.model.transform.Find("FirePoint");
            }
        }

    }

    private void HandleWeaponShipMode(SHIPMODE mode)
    {
        referenceShipMode = mode;

        switch (referenceShipMode)
        {
            case SHIPMODE.IDLE:
                isInShootingMode = false;
                lerpGunPosition = true;
                currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
                onAmmoCountChanged?.Invoke(currentGunAmmo, currentGunContainer.gunData.magazineAmmo);
                break;
            case SHIPMODE.MANUAL_DRIVE:
                lerpGunPosition = true;
                isInShootingMode = false;
                currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
                onAmmoCountChanged?.Invoke(currentGunAmmo, currentGunContainer.gunData.magazineAmmo);
                break;
            case SHIPMODE.COMBAT:
                lerpGunPosition = false;
                isInShootingMode = true; 
                break;
        }

    }


    private void SetScreenMousePos(Vector2 mousePos)
    {
        screenMousePos = mousePos;  
    }

    private void Set3DMousePos(Vector3 mousePos3D)
    {
        worldMousePos = mousePos3D;
    }
}
