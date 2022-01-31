using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using JsonConversion;
using System.Text;
using TMPro;


public class ResultsLabel : MonoBehaviour
{
    public static ResultsLabel instance;
    public GameObject textPrefab;
    public float maxDistance;
    public LayerMask m_raycastLayer;
    public GameObject markerCube;
    public GameObject meshPrefab;
    public bool newMarker;
    public static List<GameObject> MeshObjectsTemp = new List<GameObject>();
    public static List<GameObject> markerObjects = new List<GameObject>();
    private Camera cam;
    private float pixHeight;
    private float pixWidth;
    private float xscale;
    private float yscale;
    private GameObject analyzeLabel;
    private TextMeshPro analyzeLabelText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
    }

    public void CreateLabel()
    {
        if (analyzeLabel != null)
        {
            Destroy(analyzeLabel);
        }

        analyzeLabel = Instantiate(textPrefab);
        analyzeLabel.name = "LabelAnalyzing";
        analyzeLabelText = analyzeLabel.GetComponent<TextMeshPro>();
        analyzeLabelText.rectTransform.localScale = new Vector3(0.0125f, 0.0125f, 0.0125f);
        analyzeLabelText.rectTransform.position = transform.position;
        analyzeLabelText.rectTransform.rotation = transform.rotation;
        //analyzeLabelText.text = "Analysing...";
    }

    public GameObject CreateObjectLabel(Vector3 labelPoint, Quaternion labelRot, string t, float scale = 0.01f)
    {
        GameObject LabelPlaced = Instantiate(textPrefab);
        textPrefab.name = string.Format("Area{0}",ClickGlobal.tapsCount);
        LabelPlaced.tag = "label";
        TextMeshPro LabelPlacedText = LabelPlaced.GetComponent<TextMeshPro>();
        LabelPlacedText.rectTransform.localScale = new Vector3(scale, scale, scale);
        LabelPlacedText.text = t;

        LabelPlacedText.rectTransform.position = labelPoint;
        LabelPlacedText.rectTransform.rotation = labelRot;

        return LabelPlaced;
    }

    public void SetObjectLabels(AnalysedObject analysedObject)
    {
        if (analyzeLabel != null)
        {
            Destroy(analyzeLabel);
        }
        pixWidth = analysedObject.metadata.width;
        pixHeight = analysedObject.metadata.height;
        yscale = cam.pixelHeight / pixHeight;
        xscale = cam.pixelWidth / pixWidth;

        if (analysedObject.objects.Length > 0)
        {
            using (CurveList curveList = new CurveList(analysedObject.objects.Length))
            {
                for (int i = 0; i < analysedObject.objects.Length; ++i)
                {
                    TagData tag = analysedObject.objects[i];
                    curveList.curvelist[i] = PlaceMesh(tag);
                }

                StartCoroutine(NetworkManager.instance.CalculateArea(curveList));
            }
        }
        else
        {
            ClickGlobal.instance.finishedCapturing();
        }             
    }

    public void SetMarkerCubes(AnalysedObject analysedObject)
    {
        //analyzeLabelText.text = "";
        if (ClickGlobal.captureVideo & (analysedObject.objects.Length > 0))
        {
            DestroyGameObjects(markerObjects);
        } else if ((!ClickGlobal.captureVideo) & (!newMarker))
        {
            DestroyGameObjects(markerObjects);
        }

        if (analyzeLabel != null)
        {
            Destroy(analyzeLabel);
        }
        

        pixWidth = analysedObject.metadata.width;
        pixHeight = analysedObject.metadata.height;
        yscale = cam.pixelHeight / pixHeight;
        xscale = cam.pixelWidth / pixWidth;

        if (analysedObject.objects.Length > 0)
        {
            using (CurveList curveList = new CurveList(analysedObject.objects.Length))
            {
                for (int i = 0; i < analysedObject.objects.Length; ++i)
                {
                    TagData tag = analysedObject.objects[i];
                    PlaceCube(tag);
                }
            }
        }
        else
        {
            ClickGlobal.instance.finishedCapturing();
        }
    }

    public Curve PlaceMesh(TagData tag)
    {        
        Vector3[] ptList = new Vector3[tag.coords.Length];
        int i = 0;
        foreach (QuadCoords coords in tag.coords)
        {
            float x = coords.x * xscale;
            float y = (pixHeight - coords.y) * yscale;

            Ray ray = CameraProjection.instance.ScreenPointToRay(new Vector3(x, y));
            RaycastHit centerHit;
            if (Physics.Raycast(ray, out centerHit,
                maxDistance, m_raycastLayer))
            {
                // distance from which the frames are drawn
                ptList[i] = centerHit.point;
            }
            else
            {
                // genetrate hit point at max distance
                ptList[i] = ray.GetPoint(maxDistance);
            }


            i++;
        }

        //MeshCreator.instance.createMesh(ptList, tag.triangles, Relocalization.instance.azureAnchor);
        GameObject meshObject = Instantiate(meshPrefab);
        meshObject.GetComponent<MeshCreator>().createMesh(ptList, tag.triangles);

        MeshObjectsTemp.Add(meshObject);

        Curve curve = new Curve(JsonConversionExtensions.VectorListToJson(ptList));

        return curve;
    }

    public void PlaceCube(TagData tag)
    {

        float x1 = (tag.coords[0].x) * xscale;
        float y1 = (pixHeight - (tag.coords[0].y)) * yscale;

        float x2 = (tag.coords[1].x) * xscale;
        float y2 = (pixHeight - (tag.coords[1].y)) * yscale;

        float x3 = (tag.coords[2].x) * xscale;
        float y3 = (pixHeight - (tag.coords[2].y)) * yscale;

        float x4 = (tag.coords[3].x) * xscale;
        float y4 = (pixHeight - (tag.coords[3].y)) * yscale;

        //CameraProjection.instance.ApplyTransform();
        Ray ray = CameraProjection.instance.ScreenPointToRay(new Vector3((x1 + x2 + x3 + x4) / 4, (y1 + y2 + y3 + y4) / 4));
        RaycastHit centerHit;
        Vector3 labelPoint;
        Vector3 meshNorm;
        Vector3 meshParallel;

        if (Physics.Raycast(ray, out centerHit,
            maxDistance, m_raycastLayer))
        {
            // distance from which the frames are drawn
            labelPoint = centerHit.point;
            meshNorm = centerHit.normal;
        }
        else
        {
            // genetrate hit point at max distance
            labelPoint = ray.GetPoint(maxDistance);
            meshNorm = -1 * ray.direction;
        }

        //CameraProjection.instance.ApplyTransform();
        Ray rayP = CameraProjection.instance.ScreenPointToRay(new Vector3((x1 + x2) / 2, (y1 + y2) / 2));
        RaycastHit edgeHit;
        if (Physics.Raycast(rayP, out edgeHit,
            maxDistance, m_raycastLayer))
        {
            // distance from which the frames are drawn
            meshParallel = edgeHit.point - labelPoint;
        }
        else
        {
            // genetrate hit point at max distance
            meshParallel = rayP.GetPoint(maxDistance) - labelPoint;
        }

        Quaternion cubeRot = Quaternion.LookRotation(meshNorm, meshParallel);

        GameObject Cube = Instantiate(markerCube, labelPoint + meshNorm * markerCube.transform.localScale.x / 2, cubeRot);

        markerObjects.Add(Cube);
    }


    public void placeAreaLabel(Area areaLabel, bool reloaded = false)
    {
        float offset = 0.03f;
        Quaternion R = Quaternion.LookRotation(new Vector3(areaLabel.nx, areaLabel.ny, areaLabel.nz), Vector3.up);
        Vector3 P = new Vector3(areaLabel.x, areaLabel.y, areaLabel.z) - R * Vector3.forward * offset;
        GameObject areaObject = CreateObjectLabel(P, R, string.Format(@"A = {0:0.000} m2 ", areaLabel.area),
                         (float)Math.Max(0.060f * Math.Sqrt(areaLabel.area), 0.0175));
        StoreAreaLabel s = areaObject.AddComponent<StoreAreaLabel>();
        if (reloaded)
        {
            s.reloaded = true;
        }
        s.areaLabel = areaLabel;
        s.MeshObjects = MeshObjectsTemp;
        MeshObjectsTemp = new List<GameObject>();
    }


    public void DestroyGameObjects(List<GameObject> GameList) 
    {
        foreach (GameObject cube in GameList)
        {
            Destroy(cube);
        }
    }

}