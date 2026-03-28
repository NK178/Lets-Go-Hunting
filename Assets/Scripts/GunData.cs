using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Scriptable Objects/GunData")]
public class GunData : ScriptableObject
{
    public int magazineAmmo;
    public float fireRate;
    public float damage; 

    
}
