using Nanover.Frame;
using Nanover.Visualisation;
using Nanover.Visualisation.Utility;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class LineToSplineTest : MonoBehaviour
{
    private readonly IndirectMeshDrawCommand atomDrawCommand = new IndirectMeshDrawCommand();
    private readonly IndirectMeshDrawCommand bondDrawCommand = new IndirectMeshDrawCommand();

    [Header("Config")]
    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private float Scale;

    [SerializeField]
    private Color Color;

    [Header("Rendering")]
    [SerializeField]
    private Material atomMaterial;

    [SerializeField]
    private Mesh atomMesh;

    [SerializeField]
    private Material bondMaterial;

    [SerializeField]
    private Mesh bondMesh;

    private Vector3[] positions = new Vector3[0];
    private Color[] colors = new Color[0];
    private float[] scales = new float[0];
    private BondPair[] bonds = new BondPair[0];

    private void Update()
    {
        UpdateData();
        UpdateRendering();
        UpdateParticles();
        Render();
    }

    private void OnDestroy()
    {
        atomDrawCommand.Dispose();
        bondDrawCommand.Dispose();
    }

    private void UpdateData()
    {
        Resize();

        for (int i = 0; i < positions.Length; ++i)
        {
            positions[i] = lineRenderer.GetPosition(i);
            //colors[i] = Color.white;
            scales[i] = Scale;
        }

        for (int i = 0; i < positions.Length - 1; ++i)
        {
            bonds[i].A = i;
            bonds[i].B = i + 1;
        }

        Scale = lineRenderer.widthMultiplier;
        Color = lineRenderer.startColor;
    }

    private void Resize()
    {
        var particleCount = lineRenderer.positionCount;

        if (particleCount == positions.Length)
            return;

        positions = new Vector3[particleCount];
        colors = new Color[particleCount];
        scales = new float[particleCount];
        bonds = new BondPair[particleCount - 1];
    }

    private void UpdateRendering()
    {
        atomDrawCommand.SetMesh(atomMesh);
        atomDrawCommand.SetMaterial(atomMaterial);

        bondDrawCommand.SetMesh(bondMesh);
        bondDrawCommand.SetMaterial(bondMaterial);
    }

    private void UpdateParticles()
    {
        atomDrawCommand.SetColor("_Color", Color);
        atomDrawCommand.SetInstanceCount(positions.Length);
        InstancingUtility.SetPositions(atomDrawCommand, positions);
        //InstancingUtility.SetColors(atomDrawCommand, colors);
        InstancingUtility.SetScales(atomDrawCommand, scales);
        InstancingUtility.SetTransform(atomDrawCommand, transform);

        if (bonds.Length > 0)
        {
            bondDrawCommand.SetColor("_Color", Color);
            bondDrawCommand.SetFloat("_GradientWidth", 1.0f);
            bondDrawCommand.SetFloat("_EdgeScale", Scale);
            bondDrawCommand.SetInstanceCount(bonds.Length);
            InstancingUtility.SetPositions(bondDrawCommand, positions);
            //InstancingUtility.SetColors(bondDrawCommand, colors);
            InstancingUtility.SetEdges(bondDrawCommand, bonds);
            InstancingUtility.SetTransform(bondDrawCommand, transform);
        }
    }

    private void Render()
    {
        atomDrawCommand.MarkForRenderingThisFrame();
        bondDrawCommand.MarkForRenderingThisFrame();
    }
}
