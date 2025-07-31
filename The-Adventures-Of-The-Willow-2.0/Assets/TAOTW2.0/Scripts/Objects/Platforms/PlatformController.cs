using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointBehaviorType
{
    Loop,     // Caminho circular
    PingPong  // Vai e volta
}

[System.Serializable]
public class RuntimeWaypointData
{
    public Vector3 Position;
    public float TimeNode;
    public float StopTime;
}

public class PlatformController : MonoBehaviour
{
    public List<RuntimeWaypointData> waypointsData = new List<RuntimeWaypointData>();
    private int currentWaypointIndex = 0;
    private int direction = 1;

    [SerializeField] private Rigidbody2D rb;
    public bool editing = false;
    public WaypointBehaviorType behaviorType = WaypointBehaviorType.Loop;

    public bool rightStart;
    public bool initialStart;
    public string platformMoveid;
    public string thisPlatformNameSaveEditor;

    [HideInInspector] public LineRenderer lineRenderer;
    [SerializeField] private GameObject lineRenderPrefab;

    private Coroutine movementCoroutine;

    private void Start()
    {
        direction = rightStart ? 1 : -1;

        if (editing)
        {
            GameObject lineRenderObj = Instantiate(lineRenderPrefab, transform.position, Quaternion.identity, PlatformNodeEditor.instance.nodesLineRendererContainer);
            lineRenderer = lineRenderObj.GetComponent<LineRenderer>();
            RenderLine();
        }
        else if (initialStart && waypointsData.Count > 1)
        {
            movementCoroutine = StartCoroutine(MoveThroughWaypoints());
        }
    }

    private IEnumerator MoveThroughWaypoints()
    {
        while (true)
        {
            RuntimeWaypointData currentNode = waypointsData[currentWaypointIndex];
            RuntimeWaypointData nextNode = GetNextWaypoint();

            if (currentNode.StopTime > 0f)
                yield return new WaitForSeconds(currentNode.StopTime);

            yield return StartCoroutine(MoveToPosition(nextNode.Position, nextNode.TimeNode));

            UpdateWaypointIndex();
            yield return null;
        }
    }

    private RuntimeWaypointData GetNextWaypoint()
    {
        int nextIndex = currentWaypointIndex + direction;

        // Se ultrapassar os limites
        if (nextIndex >= waypointsData.Count || nextIndex < 0)
        {
            if (behaviorType == WaypointBehaviorType.PingPong)
            {
                direction *= -1;
                nextIndex = currentWaypointIndex + direction;
            }
            else if (behaviorType == WaypointBehaviorType.Loop)
            {
                nextIndex = Mod(nextIndex, waypointsData.Count);
            }
        }

        return waypointsData[nextIndex];
    }

    private void UpdateWaypointIndex()
    {
        currentWaypointIndex += direction;

        if (currentWaypointIndex >= waypointsData.Count || currentWaypointIndex < 0)
        {
            if (behaviorType == WaypointBehaviorType.PingPong)
            {
                direction *= -1;
                currentWaypointIndex += direction * 2;
            }
            else if (behaviorType == WaypointBehaviorType.Loop)
            {
                currentWaypointIndex = Mod(currentWaypointIndex, waypointsData.Count);
            }
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            rb.position = targetPos;
            yield break;
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
            rb.MovePosition(newPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPos);
    }

    private int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public void SetWaypointsFromEditor(List<PlatformNodeEditor.EditorWaypointData> editorWaypoints)
    {
        waypointsData.Clear();
        foreach (var wp in editorWaypoints)
        {
            waypointsData.Add(new RuntimeWaypointData
            {
                Position = wp.position,
                TimeNode = wp.TimeNode,
                StopTime = wp.StopTime
            });
        }

        RenderLine();
    }

    public void RenderLine()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = waypointsData.Count;
        for (int i = 0; i < waypointsData.Count; i++)
        {
            lineRenderer.SetPosition(i, waypointsData[i].Position);
        }
    }
}
