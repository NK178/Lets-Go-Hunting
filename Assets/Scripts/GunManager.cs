using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GUNTYPE
{
    M1911,
    AK47,
    NUM_TYPE
}

[System.Serializable]
public class GunTypeContainer
{
    public GunData gunData;
    public GameObject container;
}

public class GunManager : MonoBehaviour
{

    [SerializeField] private GameObject DEBUG_GunMuzzleFlash;

    [Header("References")]
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private TrailRenderer bulletTrailPrefab;
    [SerializeField] private GUNTYPE startingType;

    [Header("Gun Settings")]
    [SerializeField] private float trailSpeed = 15;
    [SerializeField] private float shootRaycastDistance = 80f;
    [SerializeField] private float autoReloadTime = 0.3f;

    [Header("Gun Model Settings")]
    [SerializeField] private float gunPositionLerpFactor;
    [SerializeField] private float gunRotationLerpFactor;
    [SerializeField] private float gunRecoilStrength;

    //I just put here for testing, idk if should be here or not 
    [Header("Impluse")]
    [SerializeField] private CinemachineImpulseSource impluseSource;
    


    [Header("Guns")]
    [SerializeField] private List<GunTypeContainer> containerList;


    [SerializeField] private bool DEBUG_disableGunModel;

    public static Action<int, int> onAmmoCountChanged;

    private GunTypeContainer currentGunContainer;
    private int currentGunAmmo;

    private Transform gunPlayerTransform;

    private Vector2 screenMousePos;

    private Transform currentFirePoint;
    private Transform currentGunModel; 
    private Transform cameraTransformRef;

    private bool firedGun;
    private bool isReloading;
    private bool isInShootingMode = false;
    private IEnumerator fireRateCoroutine = null;


    private Vector3 currentGunPosition;

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
        CameraController.onCameraTransformChanged += ReadCameraTransform;
        Ship.onShipChangedMode += HandleWeaponShipMode;
        CameraController.onMouseMoved += SetScreenMousePos;
    }

    private void OnDisable()
    {
        PlayerInputManager.onLeftMouseHold -= ShootWeapon;
        CameraController.onCameraTransformChanged -= ReadCameraTransform;
        Ship.onShipChangedMode -= HandleWeaponShipMode;
        CameraController.onMouseMoved -= SetScreenMousePos;
    }

    // Update is called once per frame
    void Update()
    {
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


    [SerializeField] private float recoilRecoverySpeed = 10f;

    ////newest version 
    //private void LateUpdate()
    //{

    //    if (gunPlayerTransform == null)
    //        return;

    //    Vector3 newPosition = Vector3.zero;
    //    Vector3 newRotation = Vector3.zero;

    //    if (isInShootingMode)
    //    {
    //        newPosition = gunPlayerTransform.transform.position;

    //        Vector3 cameraForward = cameraTransformRef.forward;
    //        Vector3 camPosition = cameraTransformRef.position;
    //        Vector3 planePos = camPosition + cameraForward * 20;

    //        Plane plane = new Plane(cameraForward, planePos);
    //        Ray ray = Camera.main.ScreenPointToRay(screenMousePos);
    //        if (plane.Raycast(ray, out float distance))
    //        {
    //            Vector3 worldMousePos = ray.GetPoint(distance);
    //            newRotation = (worldMousePos - currentFirePoint.position).normalized;
    //        }
    //        currentGunModel.transform.rotation = Quaternion.LookRotation(newRotation);
    //        currentGunModel.localPosition = Vector3.Lerp(currentGunModel.localPosition, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
    //    }
    //    else
    //    {
    //        newPosition = Vector3.Lerp(currentGunContainer.container.transform.position,
    //                                    gunPlayerTransform.transform.position,
    //                                    Time.deltaTime * gunPositionLerpFactor);
    //        newRotation = Vector3.Lerp(currentGunContainer.container.transform.forward,
    //                                          cameraTransformRef.forward,
    //                                          Time.deltaTime * gunRotationLerpFactor);

    //        currentGunContainer.container.transform.rotation = Quaternion.LookRotation(newRotation);
    //    }


    //    currentGunContainer.container.transform.position = newPosition;
    //    currentGunModel.localPosition = Vector3.Lerp(currentGunModel.localPosition, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
    //}

    private void LateUpdate()
    {

        if (gunPlayerTransform == null)
            return;

        Vector3 newPosition = Vector3.zero;
        Vector3 newRotation = Vector3.zero;

        if (referenceShipMode == SHIPMODE.COMBAT)
        {
            newPosition = gunPlayerTransform.transform.position;

            Vector3 cameraForward = cameraTransformRef.forward;
            Vector3 camPosition = cameraTransformRef.position;
            Vector3 planePos = camPosition + cameraForward * 20;

            Plane plane = new Plane(cameraForward, planePos);
            Ray ray = Camera.main.ScreenPointToRay(screenMousePos);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldMousePos = ray.GetPoint(distance);
                newRotation = (worldMousePos - currentFirePoint.position).normalized;
            }
            currentGunModel.transform.rotation = Quaternion.LookRotation(newRotation);
            currentGunModel.localPosition = Vector3.Lerp(currentGunModel.localPosition, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
        }
        else if (referenceShipMode == SHIPMODE.MANUAL_DRIVE)
        {
            newPosition = gunPlayerTransform.transform.position;
            //newRotation = cameraTransformRef.forward;

            //float panAngle = Mathf.Atan2(cameraTransformRef.forward.x, cameraTransformRef.forward.z) * Mathf.Rad2Deg;
            //Quaternion currentRotation = currentGunContainer.container.transform.rotation;
            //newRotation = new Vector3(currentRotation.eulerAngles.x, panAngle, currentRotation.eulerAngles.z);

            //currentGunContainer.container.transform.position = newPosition;
            //currentGunContainer.container.transform.rotation = Quaternion.LookRotation(newRotation);

        }
        else if (referenceShipMode == SHIPMODE.IDLE)
        {
            newPosition = Vector3.Lerp(currentGunContainer.container.transform.position,
                                        gunPlayerTransform.transform.position,
                                        Time.deltaTime * gunPositionLerpFactor);

            Quaternion slerpRotation = Quaternion.Slerp(
                                    currentGunContainer.container.transform.rotation,
                                    cameraTransformRef.rotation, // Match the exact camera rotation matrix
                                    Time.deltaTime * gunRotationLerpFactor
                                );

            currentGunContainer.container.transform.rotation = slerpRotation;
            currentGunModel.localRotation = Quaternion.identity;
        }


        currentGunContainer.container.transform.position = newPosition;


        //Debug.Log("MODEL ROT: " + currentGunModel.transform.rotation);

        //currentGunModel.localPosition = Vector3.Lerp(currentGunModel.localPosition, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
    }

    private void ReadCameraTransform(Transform cameraTransform)
    {
        cameraTransformRef = cameraTransform;
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
        
        //This is the way
        Vector3 worldBackward = -currentGunModel.forward;
        Vector3 parentLocalBackward = currentGunContainer.container.transform.InverseTransformDirection(worldBackward);
        Vector3 kickbackPosition = currentGunModel.localPosition + parentLocalBackward * gunRecoilStrength;
        currentGunModel.localPosition = kickbackPosition;

        if (currentGunAmmo <= 0)
            StartCoroutine(ReloadCoroutine());

        HandleBulletTrail();
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
            //Debug.Log("Hit: " + hit.collider.name);

            Enemy enemyRef = hit.collider.gameObject.GetComponent<Enemy>();
            if (enemyRef != null)
            {
                enemyRef.TakeDamage(currentGunContainer.gunData.damage);
                impluseSource.GenerateImpulse(); 
            }
        }
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
        TrailRenderer trail = Instantiate(bulletTrailPrefab, currentFirePoint.transform);
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
            container.container.SetActive(false);
            if (container.gunData.type == gunType)
            {
                if (!DEBUG_disableGunModel)
                    container.container.SetActive(true);
                currentGunContainer = container;
                currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
                currentGunModel = currentGunContainer.container.transform.Find("Model");
                currentFirePoint = currentGunModel.Find("FirePoint");

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
                currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
                onAmmoCountChanged?.Invoke(currentGunAmmo, currentGunContainer.gunData.magazineAmmo);

                currentGunContainer.container.transform.rotation = Quaternion.LookRotation(cameraTransformRef.forward);
                currentGunModel.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                //currentGunModel.transform.position = new Vector3(0, 0, 0);

                Debug.Log("CURRENT GUN MODEL ROT: " + currentGunModel.transform.rotation);


                break;
            case SHIPMODE.MANUAL_DRIVE:
                isInShootingMode = false;
                currentGunAmmo = currentGunContainer.gunData.magazineAmmo;
                onAmmoCountChanged?.Invoke(currentGunAmmo, currentGunContainer.gunData.magazineAmmo);
                break;
            case SHIPMODE.COMBAT:
                currentGunPosition = gunPlayerTransform.transform.position;
                isInShootingMode = true;
                break;
        }

    }


    private void SetScreenMousePos(Vector2 mousePos)
    {
        screenMousePos = mousePos;

    }

}



