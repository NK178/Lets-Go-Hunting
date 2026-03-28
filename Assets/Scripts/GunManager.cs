using System.Collections.Generic;
using UnityEngine;


public enum GUNTYPE {
    M1911,
    AK47,
    NUM_TYPE
}


[System.Serializable]
public class GunTypeContainer
{
    public GUNTYPE type;
    public GunData gunData;
    public GameObject model;
    public GameObject firePoint; 
}

public class GunManager : MonoBehaviour
{

    [SerializeField] private GameObject DEBUG_GunMuzzleFlash; 


    [SerializeField] private GUNTYPE startingType; 
    [SerializeField] private List<GunTypeContainer> containerList;


    [SerializeField] private float gunVerticalLerpFactor; 
    

    private GunTypeContainer currentGunContainer;
    private Vector3 gunTargetPosition; 
    private Vector3 gunFacingDirection; 

    void Awake()
    {
        ChangeGun(startingType);
    }

    private void OnEnable()
    {
        PlayerInputManager.onLeftClick += ShootWeapon;
        CameraController.onFirstPersonCameraRotate += ReadCameraTransform;
        PlayerMovement.onGunPlaceholderMove += HandleGunTargetPosition;
    }

    private void OnDisable()
    {
        PlayerInputManager.onLeftClick -= ShootWeapon;
        CameraController.onFirstPersonCameraRotate -= ReadCameraTransform;
        PlayerMovement.onGunPlaceholderMove -= HandleGunTargetPosition;
    }

    // Update is called once per frame
    void Update()   
    {


        currentGunContainer.model.transform.position = gunTargetPosition; 


        float lerpFactor = 2f;
        Vector3 lerpVector = Vector3.Lerp(currentGunContainer.model.transform.forward, gunFacingDirection, Time.deltaTime * gunVerticalLerpFactor);
        currentGunContainer.model.transform.rotation = Quaternion.LookRotation(lerpVector);
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


    private void ShootWeapon()
    {

        if (currentGunContainer == null)
            return; 


        Debug.Log("FIRING WEAPON CHECK AMMO: " + currentGunContainer.gunData.magazineAmmo);

        GameObject firePoint = currentGunContainer.firePoint;

        GameObject instance = Instantiate(DEBUG_GunMuzzleFlash, firePoint.transform);
    }

    private void ChangeGun(GUNTYPE gunType)
    {
        foreach (GunTypeContainer container in containerList)
        {
            
            container.model.SetActive(false);
            if (container.type == gunType)
            {
                container.model.SetActive(true);
                currentGunContainer = container;
            }
        }

    }
}
