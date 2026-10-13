using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{


    public ENEMYTYPE type;
    public float maxHealth;
    public float damage;
    public int points; 
}
