using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCSpawnAndMove : MonoBehaviour
{
    public enum NPCType { Friendly, Neutral, Enemy }

    [Header("Pooling")]
    public ObjectPool npcPool;

    [System.Serializable]
    public class TypeSettings
    {
        [Header("Path & Spawning")]
        public Transform pathParent;

        public int maxActive = 6;
        public string tagOnSpawn = "";

        [Header("Appearance")]
        [Tooltip("If set, all spawned instances for this type will use this Material.")]
        public Material materialForType;

        [Header("Movement")]
        public float moveSpeed = 2.5f;
        public float turnSpeed = 360f;

        [Tooltip("Desired hover height above the path surface (meters).")]
        public float hoverHeight = 0.5f;

        [Tooltip("Vertical follow speed in m/s. Higher = snappier height following.")]
        public float heightFollowSpeed = 5f;

        [Tooltip("Ray starts this far above the NPC and casts downward.")]
        public float raycastStartHeight = 2f;

        [Tooltip("Layers considered as the path ground (EXCLUDE the NPC layer!).")]
        public LayerMask groundMask = ~0;

        [HideInInspector] public List<Vector3> waypoints = new List<Vector3>();
        [HideInInspector] public float _spawnTimer;
        [HideInInspector] public int _activeCount;
    }

    [Header("Per-Type Settings")]
    public TypeSettings friendly;
    public TypeSettings neutral;
    public TypeSettings enemy;

    private class Agent
    {
        public GameObject go;
        public Transform tr;
        public NPCType type;
        public TypeSettings cfg;
        public int wpIndex; // current waypoint index to move toward
    }

    private readonly List<Agent> _agents = new List<Agent>();
    private readonly RaycastHit[] _hits = new RaycastHit[8];
    private void Awake()
    {
        BuildWaypoints(friendly);
        BuildWaypoints(neutral);
        BuildWaypoints(enemy);
    }

    private void Update()
    {
        // Spawn logic per type
        TrySpawnType(NPCType.Friendly, friendly);
        TrySpawnType(NPCType.Neutral, neutral);
        TrySpawnType(NPCType.Enemy, enemy);

        // Movement update for all active agents
        float dt = Time.deltaTime;
        for (int i = _agents.Count - 1; i >= 0; i--)
        {
            var a = _agents[i];
            MoveAgent(a, dt);
        }
    }

    private void TrySpawnType(NPCType type, TypeSettings cfg)
    {
        if (!npcPool || cfg.pathParent == null || cfg.waypoints.Count < 2 || cfg._activeCount >= cfg.maxActive)
            return;

        cfg._spawnTimer += Time.deltaTime;
        if (cfg._spawnTimer < Random.Range(1.75f, 6f)) return;
        cfg._spawnTimer = 0f;

        GameObject go = npcPool.GetObject();
        var tr = go.transform;

        if (go.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Place at first waypoint and face the second
        Vector3 start = cfg.waypoints[0];
        Vector3 next = cfg.waypoints[1];
        tr.SetPositionAndRotation(start, LookToward(start, next));

        // Snap initial height to ground directly under spawn
        SnapHeightToGround(tr, cfg);

        if (!string.IsNullOrEmpty(cfg.tagOnSpawn)) go.tag = cfg.tagOnSpawn;

        ApplyTypeMaterial(go, cfg);

        var agent = new Agent { go = go, tr = tr, type = type, cfg = cfg, wpIndex = 1 };
        _agents.Add(agent);
        cfg._activeCount++;
    }

    private static void ApplyTypeMaterial(GameObject go, TypeSettings cfg)
    {
        if (!go || cfg == null || cfg.materialForType == null) return;

        // Update all child renderers (skip trail/line renderers).
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r || r is TrailRenderer || r is LineRenderer) continue;

            // Work with sharedMaterials to avoid per-instance material clones
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;
            mats[0] = cfg.materialForType;

            r.sharedMaterials = mats; // apply back
        }
    }

    private void MoveAgent(Agent a, float dt)
    {
        var cfg = a.cfg;
        var wps = cfg.waypoints;

        Vector3 pos = a.tr.position;
        Vector3 target = wps[Mathf.Clamp(a.wpIndex, 0, wps.Count - 1)];

        // Horizontal movement
        Vector3 toTarget = target - pos;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float distXZ = toTargetXZ.magnitude;

        if (distXZ <= Mathf.Epsilon)
        {
            pos.x = target.x; pos.z = target.z;
            a.wpIndex++;

            if (a.wpIndex >= wps.Count)
            {
                DespawnAgent(a);
                return; 
            }

            target = wps[a.wpIndex];
            toTarget = target - pos;
            toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
            distXZ = toTargetXZ.magnitude;
            if (distXZ <= Mathf.Epsilon) return;
        }

        Vector3 dirXZ = toTargetXZ / distXZ;
        float step = cfg.moveSpeed * dt;
        if (step >= distXZ)
        {
            pos.x = target.x;
            pos.z = target.z;
        }
        else
        {
            pos.x += dirXZ.x * step;
            pos.z += dirXZ.z * step;
        }

        // Stable height
        float desiredY = pos.y;
        if (TryGetGroundY(new Vector3(pos.x, pos.y, pos.z), cfg, a.tr, out float groundY))
            desiredY = groundY + cfg.hoverHeight;

        // Use MoveTowards to avoid overshoot/oscillation
        pos.y = Mathf.MoveTowards(pos.y, desiredY, cfg.heightFollowSpeed * dt);

        a.tr.position = pos;

        // Rotate to face motion
        if (dirXZ.sqrMagnitude > 0.0001f)
        {
            Quaternion face = Quaternion.LookRotation(dirXZ, Vector3.up);
            a.tr.rotation = Quaternion.RotateTowards(a.tr.rotation, face, cfg.turnSpeed * dt);
        }
    }

    private bool TryGetGroundY(Vector3 pos, TypeSettings cfg, Transform ignoreRoot, out float groundY)
    {
        float rayLen = cfg.raycastStartHeight * 2f;
        Vector3 origin = new Vector3(pos.x, pos.y + cfg.raycastStartHeight, pos.z);

        int count = Physics.RaycastNonAlloc(
            origin, Vector3.down, _hits, rayLen,
            cfg.groundMask, QueryTriggerInteraction.Ignore);

        float closest = float.MaxValue;
        float y = 0f;

        for (int i = 0; i < count; i++)
        {
            var h = _hits[i];
            if (!h.collider) continue;

            // Skip any collider that belongs to this NPC (prevents self-hit float-up)
            if (ignoreRoot && h.collider.transform.IsChildOf(ignoreRoot))
                continue;

            if (h.distance < closest)
            {
                closest = h.distance;
                y = h.point.y;
            }
        }

        if (closest < float.MaxValue)
        {
            groundY = y;
            return true;
        }

        groundY = default;
        return false;
    }

    private void SnapHeightToGround(Transform tr, TypeSettings cfg)
    {
        if (TryGetGroundY(tr.position, cfg, tr, out float gy))
        {
            var p = tr.position;
            p.y = gy + cfg.hoverHeight;
            tr.position = p;
        }
    }

    private void DespawnAgent(Agent a)
    {
        a.cfg._activeCount = Mathf.Max(0, a.cfg._activeCount - 1);
        npcPool.ReturnObject(a.go);
        _agents.Remove(a);
    }

    private static Quaternion LookToward(Vector3 from, Vector3 to)
    {
        Vector3 fwd = to - from;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        return Quaternion.LookRotation(fwd, Vector3.up);
    }

    private void BuildWaypoints(TypeSettings cfg)
    {
        cfg.waypoints.Clear();
        if (cfg.pathParent == null) return;

        for (int i = 0; i < cfg.pathParent.childCount; i++)
        {
            Transform c = cfg.pathParent.GetChild(i);

            float topY = c.position.y;
            if (c.TryGetComponent<Renderer>(out var r))
                topY = r.bounds.max.y;
            else if (c.TryGetComponent<Collider>(out var col))
                topY = col.bounds.max.y;

            Vector3 wp = c.position;
            wp.y = topY + cfg.hoverHeight; // starting height; runtime height uses raycast
            cfg.waypoints.Add(wp);
        }
    }
}
