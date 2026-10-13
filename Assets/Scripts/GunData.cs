using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Scriptable Objects/GunData")]
public class GunData : ScriptableObject
{
    public GUNTYPE type;
    public int magazineAmmo;
    public float fireRate;
    public float damage; 

    
}
