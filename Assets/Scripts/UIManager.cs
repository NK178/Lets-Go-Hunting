using UnityEngine;


public class UIManager : MonoBehaviour
{


    [Header("Player Crosshair")]
    [SerializeField] private GameObject crosshair;


    [Header("Ship Health")]
    [SerializeField] private GameObject healthPanel;

    private GameObject healthBar;
    private Vector2 originalHealthSizeDelta;
    private Vector2 originalCrosshairPosition; 
    
    void Awake()
    {

        healthBar = healthPanel.transform.Find("Health").gameObject;
        originalHealthSizeDelta = healthBar.GetComponent<RectTransform>().sizeDelta;
        originalCrosshairPosition = crosshair.GetComponent<RectTransform>().position; 
    }


    private void OnEnable()
    {
        Ship.onShipHealthChanged += UpdateShipHealthUI;
        Ship.onShipIsCombatMode += ToggleUICombatMode; 
        CameraController.onMouseMoved += UpdateCrosshairPosition;
    }

    private void OnDisable()
    {
        Ship.onShipHealthChanged -= UpdateShipHealthUI;
        Ship.onShipIsCombatMode -= ToggleUICombatMode;
        CameraController.onMouseMoved -= UpdateCrosshairPosition;
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

    private void ToggleUICombatMode(bool condition)
    {
        if (!condition)
        {
            crosshair.GetComponent<RectTransform>().position = originalCrosshairPosition;
        }
    }

    private void UpdateCrosshairPosition(Vector2 position)
    {
        crosshair.GetComponent<RectTransform>().position = position;
    }


    // Update is called once per frame
    void Update()
    {
        
    }



}
