using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterMovement : MonoBehaviour
{
    private FormationManager manager;
    private Rigidbody rb;
    private bool isLeader;
    private bool hasReachedTarget;

    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float rotationSpeed = 5f;
    public float slowingRadius = 2f;
    public float waypointThreshold = 0.5f;

    [Header("Obstacle Avoidance Settings")]
    public float rayDistance = 2.0f;
    public float obstacleAvoidanceStrength = 1.0f;

    private List<Vector3> path;
    private int currentPathIndex = 0;

    private Quaternion formationRotation;

    // Initialize character with the formation manager and specify if it's the leader
    public void Initialize(FormationManager manager, bool isLeader)
    {
        this.manager = manager;
        this.isLeader = isLeader;

        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY;

        rb.useGravity = false;

        transform.rotation = Quaternion.LookRotation(Vector3.forward);
    }

    // Set the path for the leader
    public void SetPath(List<Vector3> newPath)
    {
        if (isLeader)
        {
            path = newPath;
            currentPathIndex = 0;
            hasReachedTarget = false;
        }
    }

    private void FixedUpdate()
    {
        GameObject[] characters = manager.GetCharacters();
        int index = Array.IndexOf(characters, gameObject);

        if (index < 0 || index >= characters.Length)
        {
            Debug.LogError("Character not found in the formation manager.");
            return;
        }

        formationRotation = manager.GetFormationTargetRotation(index);

        if (isLeader)
        {
            HandleLeaderMovement();
        }
        else
        {
            HandleFollowerMovement(index);
        }

        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            RotateTowardsMovementDirection();
        }
        else if (hasReachedTarget)
        {
            RotateTowardsFormation();
        }
    }

    #region Movement Handlers
    private void HandleLeaderMovement()
    {
        if (path != null && path.Count > 0)
        {
            MoveAlongPath();
        }
        else
        {
            StopMovement();
        }
    }

    private void HandleFollowerMovement(int index)
    {
        Vector3 formationTarget = manager.GetFormationTargetPosition(index);
        MoveTowardsTarget(formationTarget);
    }
    #endregion

    #region Path Following
    private void MoveAlongPath()
    {
        if (currentPathIndex >= path.Count)
        {
            StopMovement();
            return;
        }

        Vector3 target = path[currentPathIndex];
        MoveTowardsTarget(target);

        if (Vector3.Distance(transform.position, target) < waypointThreshold)
        {
            currentPathIndex++;
        }
    }

    private void StopMovement()
    {
        rb.velocity = Vector3.zero;
        hasReachedTarget = true;
    }
    #endregion

    #region Target Movement
    private void MoveTowardsTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0;

        float distance = direction.magnitude;

        if (distance < waypointThreshold)
        {
            StopMovement();
            return;
        }

        hasReachedTarget = false;

        float targetSpeed = (distance > slowingRadius) ? maxSpeed : maxSpeed * (distance / slowingRadius);

        Vector3 desiredVelocity = direction.normalized * targetSpeed;

        Vector3 avoidance = CalculateObstacleAvoidance();

        Vector3 steering = desiredVelocity + avoidance;

        rb.velocity = Vector3.ClampMagnitude(steering, maxSpeed);
    }

    private Vector3 CalculateObstacleAvoidance()
    {
        Vector3 avoidance = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                Vector3 avoidDir = Vector3.Reflect(transform.forward, hit.normal);
                avoidance = avoidDir.normalized * obstacleAvoidanceStrength;

                float proximityFactor = (rayDistance - hit.distance) / rayDistance;
                avoidance *= proximityFactor;
            }
        }

        return avoidance;
    }
    #endregion

    #region Rotation
    private void RotateTowardsMovementDirection()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = 0;

        if (velocity.sqrMagnitude > 0.0001f)
        {
            Quaternion movementRotation = Quaternion.LookRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, movementRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void RotateTowardsFormation()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, formationRotation, rotationSpeed * Time.fixedDeltaTime);
    }
    #endregion
}
