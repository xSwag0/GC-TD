using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    public string towerName;
    
    public float range;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float projectileSize;
    public float damage;

    public int cost;
    public Sprite sprite;
    [Header("Tower Build limit")]
    public int buildLimit = 0;

    public GameObject prefab;
}