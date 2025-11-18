// Enhanced PlayerController.cs - Simple version with aiming trail
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;
    public float rotationSpeed = 50f;
    public GameObject dartPrefab;
    public Transform dartSpawnPoint;
    public float dartForce = 20f;

    [Header("Input Keys")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode shootKey = KeyCode.W;

    public KeyCode left2Key = KeyCode.J;
    public KeyCode right2Key = KeyCode.L;
    public KeyCode shoot2Key = KeyCode.I;

    [Header("Aiming Trail")]
    public LineRenderer aimLine;
    public float aimRange = 25f;

    private float currentRotation = 0f;
    public ObjectPool dartPool;

    void Start()
    {
        SetupAimingLine();
    }

    void SetupAimingLine()
    {
        if (aimLine == null)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
        }

        // Configure line renderer
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startWidth = 0.1f;
        aimLine.endWidth = 0.05f;
        aimLine.positionCount = 2;
        aimLine.useWorldSpace = true;

        // Set color based on player number
        if (playerNumber == 1)
            aimLine.endColor = Color.blue;
        else
            aimLine.endColor = Color.red;
    }

    void Update()
    {
        HandleInput();
        UpdateAimingLine();

    }

    void HandleInput()
    {
        // Rotation
        if (Input.GetKey(leftKey))
        {
            currentRotation -= rotationSpeed * Time.deltaTime;
            currentRotation = Mathf.Clamp(currentRotation, -55f, 55f);
        }
        if (Input.GetKey(rightKey))
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            currentRotation = Mathf.Clamp(currentRotation, -55f, 55f);
        }

        // Apply rotation
        if (playerNumber == 1)
            transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        else
            transform.rotation = Quaternion.Euler(0, currentRotation + 180, 0);

        // Shooting
        if (Input.GetKeyDown(shootKey))
        {
            ShootDart();
        }
    }

    void UpdateAimingLine()
    {
        if (aimLine != null && dartSpawnPoint != null)
        {
            // Start position is where dart spawns
            Vector3 startPos = dartSpawnPoint.position;

            // End position is forward from dart spawn point
            Vector3 endPos = startPos + dartSpawnPoint.right * aimRange;

            // Set line positions
            aimLine.SetPosition(0, startPos);
            aimLine.SetPosition(1, endPos);
        }
    }

    void ShootDart()
    {
        if (dartPrefab != null && dartSpawnPoint != null)
        {
            GameObject dart = dartPool.GetObject();
            dart.transform.position = dartSpawnPoint.position;
            dart.transform.rotation = dartSpawnPoint.rotation;
            Rigidbody rb = dart.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (playerNumber == 1)
                    rb.AddForce(dartSpawnPoint.up * dartForce, ForceMode.Impulse);
                else
                    rb.AddForce(-dartSpawnPoint.up * dartForce, ForceMode.Impulse);
            }

            // Add trail to dart
            AddTrailToDart(dart);

            // Destroy dart after 3 seconds
            StartCoroutine(DeactivateDart(dart));
        }
    }

    IEnumerator DeactivateDart(GameObject dart)
    {
        yield return new WaitForSeconds(3f);
        if (dart.activeSelf)
            dartPool.ReturnObject(dart);
    }

    void AddTrailToDart(GameObject dart)
    {
        TrailRenderer trail = dart.GetComponent<TrailRenderer>();

        if (trail == null)
        {
            trail = dart.AddComponent<TrailRenderer>();
        }

        // Configure trail
        //trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startWidth = 0.08f;
        trail.endWidth = 0.02f;
        trail.time = 0.8f;

        // Set trail color based on player
        if (playerNumber == 1)
            trail.endColor = Color.cyan;
        else
            trail.endColor = Color.yellow;
    }
}