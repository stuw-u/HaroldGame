using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

struct SkidVertData
{
    public float3 position;
    public float3 normal;
    public float3 uv;

    public SkidVertData (float3 position, float3 normal, float3 uv)
    {
        this.position = position;
        this.normal = normal;
        this.uv = uv;
    }
}

public class SkidmarkRenderer : MonoBehaviour
{
    [Header("Rendering")]
    public Material material;
    public float Width = 0.2f;
    public float Tiling = 1f;
    public int RenderLayer = 0;
    public float SurfaceOffset = 0.1f;

    [Header("Point placing")]
    public Transform inhertUpVector;
    public bool AutoCheckPoints = true;
    public float MinUpAngleChange = 1f;
    public float MinChangeScalar = 0.01f;
    public float MaxRaycastDistance = 0.3f;
    public bool DoUseRaycastLayer = true;
    public int RaycastLayer = 7;

    // Mesh data
    private Mesh mesh;
    private NativeList<SkidVertData> vertData;
    private NativeList<uint> indexData;
    private VertexAttributeDescriptor[] layout;

    // Point placing data
    private int pointCount = 0;
    private float totalDistance = 0f;
    private float3 lastPoint;
    private float3 lastForward;
    private float3 lastUp;
    private float lastIntensity;

    // Auto point placing
    private bool doPlacePoints = true;
    private float recentIntensity = 1f;


    private void Awake ()
    {
        layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3),
        };

        vertData = new(4096, Allocator.Persistent);
        indexData = new(4096, Allocator.Persistent);
        mesh = new Mesh();
    }


    private void OnDestroy ()
    {
        vertData.Dispose();
        indexData.Dispose();
    }


    public void LateUpdate ()
    {
        if (AutoCheckPoints)
        {
            if (doPlacePoints)
            {
                Ray skidPlacerRay = new Ray(transform.position, -inhertUpVector.up);
                int layer = DoUseRaycastLayer ? 1 << RaycastLayer : ~0;
                if (Physics.Raycast(skidPlacerRay, out RaycastHit hitInfo, MaxRaycastDistance, layer))
                {
                    UpdateMark(hitInfo.point, hitInfo.normal, lastIntensity);
                }
                else
                {
                    CutMarks();
                }
            }
            else
            {
                CutMarks();
            }
        }

        // Pausing the game will prevent this from running and make the mark invisible
        Graphics.DrawMesh(
            mesh: mesh,
            matrix: Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one),
            material: material,
            layer: RenderLayer,
            camera: null,
            submeshIndex: 0,
            properties: null,
            castShadows: false,
            receiveShadows: true);
    }


    // Updates somes values for "AutoCheckPoints" to work properly
    public void UpdateState (bool doPlacePoints, float intensity)
    {
        // Clamp intensity and set doPlacePoints to false if it's 0
        lastIntensity = Mathf.Clamp01(intensity);
        this.doPlacePoints = intensity == 0f ? false : doPlacePoints;
    }


    // Cuts the mark entierly, UpdateMark will then create a new mark not linked to the previous
    public void CutMarks ()
    {
        if (pointCount == 0)
        {
            return;
        }

        // Reset our data
        pointCount = 0;
        totalDistance = 0f;

        // Fade the skidmark cut
        SkidVertData vert0 = vertData[^2];
        vert0.uv.z = 0;
        vertData[^2] = vert0;
        SkidVertData vert1 = vertData[^1];
        vert1.uv.z = 0;
        vertData[^1] = vert1;
    }


    // Manually checks if a new segment can be generated at a given point 
    public void UpdateMark (float3 point, float3 normal, float intensity)
    {
        intensity = math.saturate(intensity);

        // Calculate the vectors and scalar we need
        float3 forward = math.normalizesafe(point - lastPoint);
        float3 right = math.normalizesafe(math.cross(normal, forward));
        float distanceToLast = (pointCount == 0) ? 0f : math.distance(point, lastPoint);
        float angleToLast = (pointCount == 0) ? 0f : Vector3.Angle(lastForward, forward) / 360f;
        float upAngleChange = Vector3.Angle(normal, lastUp);
        lastUp = normal;

        // Figure out where our two next point should be if we were to place one
        point += normal * SurfaceOffset;
        float3 point0 = point - right * Width;
        float3 point1 = point + right * Width;

        // The third UV coord is used for intensity
        float3 uv0 = new float3(0f, (totalDistance + distanceToLast) * Tiling, intensity);
        float3 uv1 = new float3(1f, (totalDistance + distanceToLast) * Tiling, intensity);

        SkidVertData vert0 = new SkidVertData(point0, normal, uv0);
        SkidVertData vert1 = new SkidVertData(point1, normal, uv1);

        // Calculates a "change" factor that will determine if the path we're in has changed enough that a new point is required
        float changeScalar = angleToLast * distanceToLast;

        // If change is not required (and if possible) simply move the two most recent points and rebuild mesh
        if (changeScalar < MinChangeScalar && upAngleChange < MinUpAngleChange && pointCount > 1)
        {
            vertData[^2] = vert0;
            vertData[^1] = vert1;
            RebuildMesh();
            return;
        }

        // Two now points are required
        vertData.Add(vert0);
        vertData.Add(vert1);

        // Generate a new quad (if possible)
        if (pointCount >= 1)
        {
            uint vertIndex = (uint)vertData.Length - 4;
            indexData.Add(vertIndex + 3);
            indexData.Add(vertIndex + 1);
            indexData.Add(vertIndex + 0);
            indexData.Add(vertIndex + 2);
            indexData.Add(vertIndex + 3);
            indexData.Add(vertIndex + 0);
        }
        RebuildMesh();

        // Update all our data
        pointCount++;
        totalDistance += distanceToLast;
        lastPoint = point;
        lastIntensity = intensity;
        lastForward = forward;
    }


    // Updates the mesh using the advenced API
    private void RebuildMesh ()
    {
        mesh.SetVertexBufferParams(vertData.Length, layout);
        mesh.SetVertexBufferData(vertData.AsArray(), 0, 0, vertData.Length);
        mesh.SetIndexBufferParams(indexData.Length, IndexFormat.UInt32);
        mesh.SetIndexBufferData(indexData.AsArray(), 0, 0, indexData.Length, MeshUpdateFlags.Default); // Change options if needed
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, indexData.Length), MeshUpdateFlags.DontValidateIndices); // Change options if needed
        mesh.RecalculateBounds();
    }

    public void Clear ()
    {
        indexData.Clear();
        vertData.Clear();
        RebuildMesh();
        pointCount = 0;
        totalDistance = 0;
    }
}
