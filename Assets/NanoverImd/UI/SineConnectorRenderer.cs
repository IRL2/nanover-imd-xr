using System;
using UnityEngine;

[ExecuteAlways]
public class SineConnectorRenderer : MonoBehaviour
{
    [SerializeField]
    private Vector3 startPoint;

    [SerializeField]
    private Vector3 endPoint;

#pragma warning disable 0109
    [SerializeField]
    private new LineRenderer renderer;
#pragma warning restore 0109

    [SerializeField]
    private float segmentsPerMeter;

    [SerializeField]
    private float frequency;

    [SerializeField]
    private float speed;

    [SerializeField]
    private float amplitude;

    [SerializeField]
    private float width = 0.02f;

    [SerializeField]
    private float scaling = 0.1f;

    public Vector3 StartPosition
    {
        get => startPoint;
        set => startPoint = value;
    }
    
    public Vector3 EndPosition
    {
        get => endPoint;
        set => endPoint = value;
    }
    
    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        var dist = Vector3.Distance(startPoint, endPoint);
        var dir = endPoint - startPoint;

        var scaling = 1f + this.scaling / dist;

        var frequency = this.frequency * scaling;
        var amplitude = this.amplitude / scaling;
        var width = this.width / scaling;
        var segmentsPerMeter = Mathf.Clamp(this.segmentsPerMeter * scaling, 1, 10000f);

        int segments = (int) Mathf.Clamp(segmentsPerMeter * dist, 2, 10000f);

        var cameraDir = Camera.main.transform.InverseTransformDirection(dir);
        cameraDir.z = 0;
        var x = cameraDir.x;
        cameraDir.x = -cameraDir.y;
        cameraDir.y = x;
        var up = Camera.main.transform.TransformDirection(cameraDir);
        up = (up - Vector3.Project(up, dir)).normalized;

        renderer.positionCount = segments;

        renderer.startWidth = width;
        renderer.endWidth = width;

        for (var i = 0; i < segments; i++)
        {
            float t = (float) i / (segments - 1f);
            var p = Vector3.Lerp(startPoint, endPoint, t);
            p += up * Mathf.Sin(t * dist * frequency - speed * Time.time) * Mathf.Sin(t * Mathf.PI) * amplitude;
            renderer.SetPosition(i, p);
        }
    }
}