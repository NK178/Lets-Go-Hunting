using TMPro;
using UnityEngine;


public class UIManager : MonoBehaviour
{


    [Header("Player Crosshair")]
    [SerializeField] private GameObject crosshair;


    [Header("Ship Health")]
    [SerializeField] private GameObject healthPanel;
    private GameObject healthBar;

    [SerializeField] private GameObject ammoPanel;
    private TMP_Text ammoText;


    [Header("DEBUG")]
    [SerializeField] private bool enableDebugText = false;
    [SerializeField] private TMP_Text shipPropellerSpeed; 
    [SerializeField] private TMP_Text shipRudderForce; 
    [SerializeField] private TMP_Text targetCheckpointDistance; 
    
    private Vector2 originalHealthSizeDelta;
    private Vector2 originalCrosshairPosition; 
    
    void Awake()
    {

        healthBar = healthPanel.transform.Find("Health").gameObject;
        ammoText = ammoPanel.transform.Find("Ammo").gameObject.GetComponent<TMP_Text>();    
        originalHealthSizeDelta = healthBar.GetComponent<RectTransform>().sizeDelta;
        originalCrosshairPosition = crosshair.GetComponent<RectTransform>().position;


        shipRef = GameObject.FindAnyObjectByType<Ship>(); 
    }


    private void OnEnable()
    {
        Ship.onShipHealthChanged += UpdateShipHealthUI;
        //Ship.onShipIsCombatMode += ToggleUICombatMode; 
        CameraController.onMouseMoved += UpdateCrosshairPosition;
        GunManager.onAmmoCountChanged += UpdateAmmoCount; 
        Ship.onShipChangedMode += HandleUIShipMode; 

    }

    private void OnDisable()
    {
        Ship.onShipHealthChanged -= UpdateShipHealthUI;
        //Ship.onShipIsCombatMode -= ToggleUICombatMode;
        CameraController.onMouseMoved -= UpdateCrosshairPosition;
        GunManager.onAmmoCountChanged -= UpdateAmmoCount;

        Ship.onShipChangedMode -= HandleUIShipMode;
    }


    private void HandleUIShipMode(SHIPMODE shipMode)
    {
        switch (shipMode)
        {

            case SHIPMODE.IDLE:
                crosshair.GetComponent<RectTransform>().position = originalCrosshairPosition;
                crosshair.gameObject.SetActive(true);
                break;
            case SHIPMODE.MANUAL_DRIVE:
                crosshair.GetComponent<RectTransform>().position = originalCrosshairPosition;
                crosshair.gameObject.SetActive(false);
                break;
            case SHIPMODE.COMBAT:
                crosshair.gameObject.SetActive(true);
                break;
        }
    }


    private void UpdateShipHealthUI(float health, float healthPercentage)
    {
        RectTransform rect = healthBar.GetComponent<RectTransform>();
        if (rect != null)
        {
            
            rect.sizeDelta = new Vector2(originalHealthSizeDelta.x * healthPercentage,
                                         originalHealthSizeDelta.y);

            Debug.Log("RECT: " + rect.sizeDelta);
        }

    }

    //private void ToggleUICombatMode(bool condition)
    //{
    //    if (!condition)
    //    {
    //        crosshair.GetComponent<RectTransform>().position = originalCrosshairPosition;
    //    }
    //}


    private void UpdateAmmoCount(int currentAmmo, int maxAmmo)
    {

        if (ammoText == null)
            return; 

        string newText = currentAmmo.ToString() + "/" + maxAmmo.ToString();
        ammoText.text = newText; 
    }

    private void UpdateCrosshairPosition(Vector2 position)
    {
        crosshair.GetComponent<RectTransform>().position = position;
    }


    //FOR DEBUG

    private Ship shipRef;
    

    // Update is called once per frame
    void Update()
    {
        
        if (enableDebugText)
        {
            if (shipRef != null)
            {
                shipPropellerSpeed.text = "Propeller Sp: " + shipRef.GetPropellerSpeed();
                shipRudderForce.text = "Rudder Sp: " + shipRef.GetRudderSpeed();
                targetCheckpointDistance.text = "Dist To CheckPt: " + shipRef.DEBUG_GetDistanceToCurrentCheckpoint();
            }
        }
    }



}
