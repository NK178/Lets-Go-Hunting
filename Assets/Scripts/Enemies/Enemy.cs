using System.Collections;
using System.Linq;
using UnityEngine;



public enum ENEMYTYPE
{
    SWARM, 
    BOSS,   
    NUM_TYPE
}


public class Enemy : MonoBehaviour
{

    [Header("References")]
    [SerializeField] protected EnemyData enemyData;
    [SerializeField] protected Animator animator = null;


    [Header("Damage Effects")]
    [SerializeField] private Color damageColour; 

    protected float currentHealth; 
    protected bool isActive; 
    protected bool isAlive;


    //for damage flash and for those who have multiple materials
    private Material[] modelMaterials;

    protected virtual void Start()
    {
        currentHealth = enemyData.maxHealth;
        isAlive = true;
        isActive = true;

        Transform model = transform.Find("Model");

        //TODO: I needa write a shader for this 
        //if (model != null)
        //{
        //    Renderer modelRenderer = model.GetComponentInChildren<Renderer>();
        //    if (modelRenderer != null)
        //        modelMaterials = modelRenderer.materials;

        //}
    }


    // Update is called once per frame
    void Update()
    {
    }


    //reaction when being shot at 
    protected void TriggerHitReaction()
    {
       
        
    }

    protected void TriggerHitDamageFlash()
    {
        if (modelMaterials == null || modelMaterials.Length == 0)
            return; 

        foreach (Material mat in modelMaterials)
        {
            StartCoroutine(DamageEffectCoroutine(mat));
        }
    }

    private IEnumerator DamageEffectCoroutine(Material mat)
    {
        float damageEffectDuration = 0.7f;

        Color originalColour = mat.color;
        mat.color = damageColour;

        float elapsedTime = 0f;
        while (elapsedTime < damageEffectDuration)
        {
            mat.color = Color.Lerp(damageColour,
            originalColour, elapsedTime / damageEffectDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        mat.color = originalColour;

    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isAlive = false;
        }
        else
        {
            //TriggerHitDamageFlash();
        }
    }


    public bool IsEntityAlive()
    {
        return isAlive; 
    }
}
