using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class IsometricCameraController : MonoBehaviour
{
    public float yaw = 45f, pitch = 30f, distance = 30f;
    public float defaultSize = 7f, minSize = 3.5f, maxSize = 10f, padding = 2.5f;
    public float smoothTime = 0.35f;

    Camera cam;
    Vector3 focusPoint, currentFocus, overviewCenter, velocity, shakeOffset;
    float targetSize, sizeVel;
    Transform follow;

    public Camera Camera => cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        targetSize = defaultSize;
        cam.orthographicSize = defaultSize;
    }

    public void SetOverview(Vector3 center) { overviewCenter = center; focusPoint = center; targetSize = defaultSize; }

    public void Snap()
    {
        currentFocus = focusPoint;
        cam.orthographicSize = targetSize;
        transform.position = currentFocus - transform.forward * distance;
    }

    public void ShowOverview() { follow = null; focusPoint = overviewCenter; targetSize = defaultSize; }
    public void Follow(Transform t) { follow = t; targetSize = Mathf.Max(minSize, defaultSize * 0.7f); }

    public void FocusOn(Vector3 p, float size = -1f)
    {
        follow = null;
        focusPoint = p;
        targetSize = size > 0f ? size : Mathf.Max(minSize, defaultSize * 0.65f);
    }

    public void FocusOnPoints(IList<Vector3> pts)
    {
        follow = null;
        if (pts == null || pts.Count == 0) return;
        Vector3 min = pts[0], max = pts[0];
        for (int i = 1; i < pts.Count; i++) { min = Vector3.Min(min, pts[i]); max = Vector3.Max(max, pts[i]); }
        focusPoint = (min + max) * 0.5f;
        float ext = Mathf.Max(max.x - min.x, max.z - min.z) * 0.5f;
        targetSize = Mathf.Clamp(ext * 0.8f + padding, minSize, maxSize);
    }

    public void Shake(float duration, float magnitude) { StartCoroutine(ShakeRoutine(duration, magnitude)); }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            shakeOffset = Random.insideUnitSphere * magnitude * (1f - t / duration);
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }

    void LateUpdate()
    {
        if (follow) focusPoint = follow.position;
        currentFocus = Vector3.SmoothDamp(currentFocus, focusPoint, ref velocity, smoothTime);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref sizeVel, smoothTime);
        transform.position = currentFocus - transform.forward * distance + shakeOffset;
    }
}
