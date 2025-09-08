using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MinimapController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public List<Transform> mapCenters = new();
    public Camera minimapCamera;

    [Header("Behavior")]
    public float includeDistance = 50f;
    public float padding = 5f;
    public float minSize = 10f;
    public float maxSize = 100f;
    public float height = 50f;
    public bool rotateWithPlayer = false;
    public float smoothSpeed = 8f;

    Vector3 velocity = Vector3.zero;
    float sizeVelocity = 0f;

    void Reset()
    {
        minimapCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player == null || minimapCamera == null) return;

        List<Vector2> pts = new List<Vector2>();
        Vector3 p = player.position;
        pts.Add(new Vector2(p.x, p.z));

        foreach (var t in mapCenters)
        {
            if (t == null) continue;
            Vector2 tc = new Vector2(t.position.x, t.position.z);
            float dist = Vector2.Distance(new Vector2(p.x, p.z), tc);
            if (dist <= includeDistance)
                pts.Add(tc);
        }

        if (pts.Count == 1)
        {
            pts.Add(pts[0] + new Vector2(0.1f, 0.1f));
        }

        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
        foreach (var v in pts)
        {
            if (v.x < minX) minX = v.x;
            if (v.x > maxX) maxX = v.x;
            if (v.y < minZ) minZ = v.y;
            if (v.y > maxZ) maxZ = v.y;
        }

        Vector2 center2D = new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
        float width = maxX - minX;
        float heightXZ = maxZ - minZ;

        float halfHeight = heightXZ * 0.5f;
        float halfWidth = width * 0.5f;
        float aspect = minimapCamera.aspect > 0 ? minimapCamera.aspect : (16f / 9f);
        float requiredSize = Mathf.Max(halfHeight, halfWidth / aspect) + padding;
        requiredSize = Mathf.Clamp(requiredSize, minSize, maxSize);

        minimapCamera.orthographicSize = Mathf.SmoothDamp(minimapCamera.orthographicSize, requiredSize, ref sizeVelocity, 1f / Mathf.Max(0.0001f, smoothSpeed));

        Vector3 targetPos = new Vector3(center2D.x, height, center2D.y);

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, 1f / Mathf.Max(0.0001f, smoothSpeed));

        if (rotateWithPlayer)
        {
            float yaw = player.eulerAngles.y;
            transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
