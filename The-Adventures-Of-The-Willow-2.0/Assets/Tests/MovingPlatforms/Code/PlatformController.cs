using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  public enum WaypointPathType
    {
        Closed,
        Open
    }

    public enum WaypointBehaviorType
    {
        Loop,
        PingPong
    }

public class PlatformController : MonoBehaviour
{
    public List<Vector3> waypoints = new List<Vector3>();
    public string thisPlatformNameSaveEditor;

    [Header("Platform Waypoint Settings")]
    [SerializeField] private Rigidbody2D rb;
    public bool editing = false;

    public WaypointPathType pathType = WaypointPathType.Closed;
    public WaypointBehaviorType behaviorType = WaypointBehaviorType.Loop;

    public float moveSpeed = 5f; // Speed of movement
    public float stopDistance = 0.1f; // Distance to consider reaching a waypoint

    private int lastWaypointIndex = -1;
    private int currentWaypointIndex = 0;
    private int direction = 1; // 1 for forward, -1 for reverse
    public bool rightStart;
    public string platformMoveid;
    public bool initialStart;

    [HideInInspector] public LineRenderer lineRenderer;
    [SerializeField] private GameObject lineRenderPrefab;

    private void Start()
    {
        if (editing)
        {
            GameObject lineRenderObj = Instantiate(lineRenderPrefab, transform.position, Quaternion.identity, PlatformNodeEditor.instance.nodesLineRendererContainer);
            lineRenderer = lineRenderObj.GetComponent<LineRenderer>();
            RenderLine();
        }
    }
    private void Update()
    {
        if (waypoints.Count == 0)
            return;
        if (!editing)
        {
            if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex]) <= stopDistance)
            {
                if (pathType == WaypointPathType.Closed)
                {
                    switch (behaviorType)
                    {
                        case WaypointBehaviorType.Loop:
                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                        case WaypointBehaviorType.PingPong:
                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || (lastWaypointIndex == waypoints.Count - 1 && currentWaypointIndex == 0 && direction > 0))
                            {
                                direction *= -1;
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                    }
                }
                else if (pathType == WaypointPathType.Open)
                {
                    switch (behaviorType)
                    {
                        case WaypointBehaviorType.Loop:
                            int nextWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);

                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || (lastWaypointIndex == waypoints.Count - 2 && currentWaypointIndex == waypoints.Count - 1 && direction > 0))
                            {
                                transform.position = waypoints[nextWaypointIndex];
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                        case WaypointBehaviorType.PingPong:
                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || (lastWaypointIndex == waypoints.Count - 2 && currentWaypointIndex == waypoints.Count - 1 && direction > 0))
                            {
                                direction *= -1;
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!editing)
        {
            MoveToWaypoint(waypoints[currentWaypointIndex]);
        }
    }
    private void MoveToWaypoint(Vector3 waypoint)
    {
        Vector2 direction = (waypoint - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }
    public void RenderLine()
    {
        lineRenderer.positionCount = waypoints.Count;

        for (int i = 0; i < waypoints.Count; i++)
        {
            lineRenderer.SetPosition(i, waypoints[i]);
        }
    }
    //public void UpdatePlatformWaypoints(List<Vector3> newWaypoints)
    //{
    //    waypoints = newWaypoints;
    //}

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}

