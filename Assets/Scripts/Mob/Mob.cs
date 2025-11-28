using System;
using UnityEngine;

public class Mob : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    public static event Action<EnemyData> OnBreach;
    
    private Path _currentPath;
    
    private Vector3 _targetPosition;
    private int _currentWaypoint = 0;

    private void Awake()
    {
        _currentPath = GameObject.Find("Path_Map1").GetComponent<Path>();    
    }
    
    private void OnEnable()
    {
        _currentWaypoint = 0;
        _targetPosition = _currentPath.GetWaypointPosition(_currentWaypoint);
    }
    
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, enemyData.moveSpeed * Time.deltaTime);
        
        float distanceToWaypoint = (transform.position - _targetPosition).magnitude;
        if (distanceToWaypoint <= 0.1f)
        {
            if (_currentWaypoint < _currentPath.waypoints.Length - 1)
            {
                _currentWaypoint++;
                _targetPosition = _currentPath.GetWaypointPosition(_currentWaypoint);
            }
            else
            {
                OnBreach?.Invoke(enemyData);
                gameObject.SetActive(false);
            }
            
        }
    }
}
