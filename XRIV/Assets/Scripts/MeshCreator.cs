using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCreator : MonoBehaviour
{
    public static MeshCreator instance;
    public Material mat;

    [HideInInspector]
    public bool reloaded = false;
    [HideInInspector]
    public float area;
    [HideInInspector]
    public Vector3[] vertices2;
    [HideInInspector]
    public int[] triangles2;

    private void Awake()
    {
        instance = this;
    }

    public void createMesh(Vector3[] vertices, int[] triangles)
    {
        // vertices must be defined clockwise 
        gameObject.name = string.Format("Mesh{0}", ClickGlobal.tapsCount);
        gameObject.tag = "Mesh";
        vertices2 = vertices;
        triangles2 = triangles;

        //if (SpectatorViewController.SpecatorViewOn)
        //{
        //    Vector3 C = vertices.Average();
        //    float dmax = 0f;
        //    for (int i = 0; i < vertices.Length; i++)
        //    {
        //        float d = (vertices[i] - C).magnitude;
        //        if (d > dmax)
        //        {
        //            dmax = d;
        //        }
        //    }

        //    float t = Math.Min(0.02f,dmax/5);

        //    int loopCount = (int)Math.Round(dmax / t);

        //    for (int j = 1; j < loopCount; j++)
        //    {
        //        GameObject obj = new GameObject("lineMesh");
        //        LineRenderer line = obj.AddComponent<LineRenderer>();
        //        line.positionCount = vertices.Length;
        //        line.startWidth = t * 2;
        //        line.endWidth = t * 2;
        //        line.material = mat;
        //        line.loop = true;
        //        for (int i = 0; i < vertices.Length; i += 1)
        //        {
        //            Vector3 dx = vertices[i] - C;
        //            Vector3 slope = dx.normalized;
        //            Vector3 x1 = vertices[i] - slope * j * t * (dx.magnitude/dmax);
        //            line.SetPosition(i, x1);
        //        }
        //        obj.transform.SetParent(gameObject.transform, true);
        //    }
        //}
        //else
        //{

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        //}
    }

    public void createMesh2(Vector3[] vertices, int[] triangles)
    {
        // vertices must be defined clockwise 

        //GameObject meshObject = Instantiate(meshPrefab, parentObject.transform);

        gameObject.name = string.Format("Mesh{0}", ClickGlobal.tapsCount);
        gameObject.tag = "Mesh";

        //for (int i = 0; i < triangles.Length; i += 3)
        //{
        //    Vector3 x1 = vertices[triangles[i]];
        //    Vector3 x2 = vertices[triangles[i + 1]];
        //    Vector3 x3 = vertices[triangles[i + 2]];
        //    Vector3 x12 = (x1 + x2) / 2;

        //    float d1 = 0;
        //    float d2 = (x2 - x1).magnitude;

        //    GameObject obj = new GameObject("lineMesh");
        //    LineRenderer line = obj.AddComponent<LineRenderer>();
        //    line.positionCount = 2;
        //    line.SetPosition(0, x3);
        //    line.SetPosition(1, x12);
        //    line.startWidth = d1;
        //    line.endWidth = d2;
        //    obj.transform.SetParent(gameObject.transform, false);

        //}

        ////////////////
        //LineRenderer line = gameObject.AddComponent<LineRenderer>();
        //line.positionCount = vertices.Length;
        //line.startWidth = 0.015f;
        //line.endWidth = 0.015f;
        //line.material = mat;
        //line.loop = true;
        //for (int i = 0; i < vertices.Length; i += 1)
        //{
        //    Vector3 x1 = vertices[i];
        //    line.SetPosition(i, x1);
        //}


        float t = 0.015f;
        Vector3 C = vertices.Average();
        float dmax = 0f;
        for (int i = 0; i< vertices.Length; i++) 
        {
            float d = (vertices[i] - C).magnitude;
            if (d > dmax)
            {
                dmax = d;
            }
        }

        int loopCount = (int)Math.Round(dmax / t);

        for (int j=0; j < loopCount; j++)
        {
            GameObject obj = new GameObject("lineMesh");
            LineRenderer line = obj.AddComponent<LineRenderer>();
            line.positionCount = vertices.Length;
            line.startWidth = t*2;
            line.endWidth = t*2;
            line.material = mat;
            line.loop = true;
            for (int i = 0; i < vertices.Length; i += 1)
            {
                Vector3 slope = (vertices[i] - C).normalized;
                Vector3 x1 = vertices[i] - slope*j*t;
                line.SetPosition(i, x1);
            }
            obj.transform.SetParent(gameObject.transform,true);
        }




    }

    public float CalculateSurfaceArea(Mesh mesh)
    {
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        double sum = 0.0;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 corner = vertices[triangles[i]];
            Vector3 a = vertices[triangles[i + 1]] - corner;
            Vector3 b = vertices[triangles[i + 2]] - corner;

            sum += Vector3.Cross(a, b).magnitude;
        }

        return (float)(sum / 2.0);
    }

    public float surfaceAreaList(GameObject[] meshList)
    {
        float sum = 0;
        foreach (GameObject g in meshList)
        {
            if (g.name == string.Format("Mesh{0}", ClickGlobal.tapsCount))
            {
                Mesh m = g.GetComponent<MeshFilter>().mesh;
                sum += CalculateSurfaceArea(m);
            }
        }
        return sum;
    }

    public void createMesh1(Vector3[] verticesList)
    {
        foreach (Vector3 p in verticesList)
        {
            DrawLines.instance.drawLine(p);
        }
    }
    //public void createMesh2(Vector3[] verticesList)
    //{
    //    // vertices must be defined clockwise 

    //    GameObject meshObject = new GameObject("Mesh");
    //    meshObject.tag = "Mesh";
    //    Mesh mesh = new Mesh();
    //    Vector3[] vertices = new Vector3[verticesList.Length+1];
    //    int[] triangles = new int[verticesList.Length * 3];

    //    Vector3 cen = verticesList.Average();
    //    vertices[0] = cen;
    //    verticesList.CopyTo(vertices, 1);

    //    for (int i = 0; i < vertices.Length - 2; i++)
    //    {
    //        triangles[i * 3] = i + 2;
    //        triangles[i * 3 + 1] = 0;
    //        triangles[i * 3 + 2] = i + 1;
    //    }

    //    triangles[(vertices.Length-2) * 3] = 1;
    //    triangles[(vertices.Length-2) * 3 + 1] = 0;
    //    triangles[(vertices.Length-2) * 3 + 2] = vertices.Length-1;

    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;

    //    MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
    //    meshFilter.mesh = mesh;

    //    MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
    //    meshRenderer.material = mat;

    //}
}
