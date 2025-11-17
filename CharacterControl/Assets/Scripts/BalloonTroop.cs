// BalloonTroop.cs - Troops that collect bananas and bring them back to base
using UnityEngine;

public class BalloonTroop : MonoBehaviour
{
    [Header("Troop Settings")]
    public int ownerPlayerNumber = 1;
    public TroopType troopType = TroopType.Regular;
    public int health = 1;
    public float moveSpeed = 3f;
    
    [Header("Collection Settings")]
    public int bananasToCollect = 20;
    private int currentBananas = 0;
    
    [Header("Visual")]
    public TrailRenderer trail;
    
    private Transform[] pathToTree;
    private Transform[] pathToBase;
    private int currentWaypointIndex = 0;
    private bool returningToBase = false;
    private bool hasCollected = false;
    
    private Transform targetBase;
    private BananaTree targetTree;
    
    public enum TroopType
    {
        Regular,    // Balanced
        Speed,      // Fast but weak
        Tank        // Slow but tanky
    }

    void Start()
    {
        SetupTroopStats();
        SetupVisuals();
        FindTargets();
        SetupPath();
    }
    
    void SetupTroopStats()
    {
        switch (troopType)
        {
            case TroopType.Regular:
                moveSpeed = 3f;
                health = 1;
                break;
            case TroopType.Speed:
                moveSpeed = 5f;
                health = 1;
                break;
            case TroopType.Tank:
                moveSpeed = 2f;
                health = 3;
                break;
        }
    }
    
    void SetupVisuals()
    {
        // Set color based on owner and type
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color baseColor = ownerPlayerNumber == 1 ? 
                new Color(0.5f, 0.5f, 1f) : new Color(1f, 0.5f, 0.5f);
            
            // Modify color based on type
            switch (troopType)
            {
                case TroopType.Speed:
                    baseColor = Color.Lerp(baseColor, Color.green, 0.3f);
                    break;
                case TroopType.Tank:
                    baseColor = Color.Lerp(baseColor, Color.gray, 0.3f);
                    break;
            }
            
            renderer.material.color = baseColor;
        }
        
        // Setup trail
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }
        
        Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
        trail.material = trailMaterial;
        trail.startWidth = 0.15f;
        trail.endWidth = 0.03f;
        trail.time = 0.5f;
        
        Color troopColor = ownerPlayerNumber == 1 ? 
            new Color(0.6f, 0.6f, 1f, 0.8f) : new Color(1f, 0.6f, 0.6f, 0.8f);
        trail.startColor = troopColor;
        trail.endColor = troopColor;
    }
    
    void FindTargets()
    {
        // Find banana tree
        targetTree = FindFirstObjectByType<BananaTree>();
        
        // Find own base
        BaseHealth[] bases = FindObjectsByType<BaseHealth>(FindObjectsSortMode.None);
        foreach (BaseHealth baseHealth in bases)
        {
            if (baseHealth.playerNumber == ownerPlayerNumber)
            {
                targetBase = baseHealth.transform;
                break;
            }
        }
    }
    
    void SetupPath()
    {
        // Find path to tree
        GameObject pathToTreeObj = GameObject.Find("PathToTree_P" + ownerPlayerNumber);
        if (pathToTreeObj != null)
        {
            pathToTree = new Transform[pathToTreeObj.transform.childCount];
            for (int i = 0; i < pathToTreeObj.transform.childCount; i++)
            {
                pathToTree[i] = pathToTreeObj.transform.GetChild(i);
            }
        }
        
        // Find path back to base
        GameObject pathToBaseObj = GameObject.Find("PathToBase_P" + ownerPlayerNumber);
        if (pathToBaseObj != null)
        {
            pathToBase = new Transform[pathToBaseObj.transform.childCount];
            for (int i = 0; i < pathToBaseObj.transform.childCount; i++)
            {
                pathToBase[i] = pathToBaseObj.transform.GetChild(i);
            }
        }
    }
    
    void Update()
    {
        if (!returningToBase)
        {
            MoveAlongPath(pathToTree);
        }
        else
        {
            MoveAlongPath(pathToBase);
        }
    }
    
    void MoveAlongPath(Transform[] path)
    {
        if (path == null || path.Length == 0) return;
        
        // Get current waypoint
        if (currentWaypointIndex >= path.Length)
        {
            OnReachedDestination();
            return;
        }
        
        Transform targetWaypoint = path[currentWaypointIndex];
        
        // Move toward waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.5f)
        {
            currentWaypointIndex++;
        }
    }
    
    void OnReachedDestination()
    {
        if (!returningToBase && !hasCollected)
        {
            // Reached banana tree - collect bananas
            CollectBananas();
            returningToBase = true;
            currentWaypointIndex = 0;
        }
        else if (returningToBase)
        {
            // Reached base - deliver bananas
            DeliverBananas();
            Destroy(gameObject);
        }
    }
    
    void CollectBananas()
    {
        if (targetTree != null)
        {
            currentBananas = targetTree.HarvestBananas(bananasToCollect);
            hasCollected = true;
            Debug.Log("Balloon troop collected " + currentBananas + " bananas!");
        }
    }
    
    void DeliverBananas()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && currentBananas > 0)
        {
            gm.AddCurrency(ownerPlayerNumber, currentBananas);
            Debug.Log("Player " + ownerPlayerNumber + " delivered " + currentBananas + " bananas!");
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        
        if (health <= 0)
        {
            Debug.Log("Balloon troop destroyed!");
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Dart"))
        {
            TakeDamage(1);
        }
    }
}