using UnityEngine;


public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject healthPanel;
    private GameObject healthBar;
    private Vector2 originalHealthSizeDelta;
    
    void Awake()
    {

        healthBar = healthPanel.transform.Find("Health").gameObject;
        originalHealthSizeDelta = healthBar.GetComponent<RectTransform>().sizeDelta;
    }


    private void OnEnable()
    {
        Ship.onShipHealthChanged += UpdateShipHealthUI;
    }

    private void OnDisable()
    {
        Ship.onShipHealthChanged -= UpdateShipHealthUI;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
