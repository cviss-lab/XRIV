using System;
using UnityEngine;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

[System.Serializable]
public class QuadCoords
{
    public float x;
    public float y;
}

[System.Serializable]
public class TagData
{
    public string id;
    public QuadCoords[] coords;
    public int[] triangles;
}

[System.Serializable]
public class metaData
{
    public float height;
    public float width;
    public string format;
}
[System.Serializable]
public class AnalysedObject
{
    public TagData[] objects;
    public metaData metadata;
    public string requestId;
}

[System.Serializable]
public class Vector3D
{
    public float x;
    public float y;
    public float z;

    public Vector3D(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[System.Serializable]
public class Curve
{
    public Vector3D[] curve;
    public Curve(Vector3D[] curve)
    {
        this.curve = curve;
    }
}

[System.Serializable]
public class CurveList: IDisposable
{
    public Curve[] curvelist;
    public CurveList(int i)
    {
        this.curvelist = new Curve[i];
    }
    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // free managed resources
        }
        // free native resources if there are any.
    }
}

[System.Serializable]
public class Area
{
    public float area;
    public float x;
    public float y;
    public float z;
    public float nx;
    public float ny;
    public float nz;
}

[System.Serializable]
public class Region
{
    public Area area;
    public List<MeshObject> meshList;
    public Region()
    {
        this.area = new Area();
        this.meshList = new List<MeshObject>();
    }
}

[System.Serializable]
public class SessionData
{
    public List<Region> Regions;
    public string anchorID;

    public SessionData()
    {
        this.Regions = new List<Region>();
        this.anchorID = "";
    }
}

[System.Serializable]
public class SessionMarker
{
    public Vector3D[] points;
    public Vector3D[] dirN;
    public Vector3D[] dirP;
    public string anchorID;

    public SessionMarker(int i)
    {
        this.points = new Vector3D[i];
        this.dirN = new Vector3D[i];
        this.dirP = new Vector3D[i];
        this.anchorID = "";
    }
}

[System.Serializable]
public class MeshObject
{
    public Vector3D[] vertices;
    public int[] triangles;
}

[System.Serializable]
public class MeshObjectList
{
    public MeshObject[] meshList;
    public string anchorID;
}

class MyGCCollectClass
{
    public void cleanUp()
    {
        // Collect all generations of memory.
        GC.Collect();
        //Debug.Log(string.Format("Memory used after full collection:   {0:N0}",
        //                  GC.GetTotalMemory(true)));
    }
}

namespace JsonConversion
{
    public static class JsonConversionExtensions
    {
        public static Vector3D VectorToJson(UnityEngine.Vector3 v)
        {
            return new Vector3D(v.x, v.y, v.z);
        }
        public static UnityEngine.Vector3 JsonToVector(Vector3D v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }
        public static Vector3D[] VectorListToJson(UnityEngine.Vector3[] vl)
        {
            Vector3D[] vl3d = new Vector3D[vl.Length];
            for (int i = 0; i < vl.Length; ++i)
            {
                vl3d[i] = VectorToJson(vl[i]);
            }
            return vl3d;
                
        }
        public static UnityEngine.Vector3[] JsonToVectorList(Vector3D[] vl3d)
        {
            UnityEngine.Vector3[] vl = new UnityEngine.Vector3[vl3d.Length];
            for (int i = 0; i < vl3d.Length; ++i)
            {
                vl[i] = JsonToVector(vl3d[i]);
            }
            return vl;

        }

        public static Vector3D[] VectorListToJson2(UnityEngine.Vector3[] vl, UnityEngine.Transform T1, UnityEngine.Transform T2)
        {
            Vector3D[] vl3d = new Vector3D[vl.Length];
            for (int i = 0; i < vl.Length; ++i)
            {
                //vl3d[i] = VectorToJson(T.MultiplyPoint3x4(vl[i]));
                vl3d[i] = VectorToJson(T2.InverseTransformPoint(T1.TransformPoint(vl[i])));
            }
            return vl3d;
        }

    }
}