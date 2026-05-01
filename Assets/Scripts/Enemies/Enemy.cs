using UnityEngine;



public enum ENEMYTYPE
{
    SWARM, 

    NUM_TYPE
}


public class Enemy : MonoBehaviour
{

    [SerializeField] protected EnemyData enemyData;

    [SerializeField] protected Animator animator = null; 

    protected float currentHealth; 
    protected bool isActive; 
    protected bool isAlive; 

    void Start()
    {
        currentHealth = enemyData.maxHealth;
        isAlive = true; 
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;    
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isAlive = false;
        }
    }

    public bool IsEntityAlive()
    {
        return isAlive; 
    }
}
