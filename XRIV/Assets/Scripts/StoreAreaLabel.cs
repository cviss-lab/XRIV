using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreAreaLabel : MonoBehaviour
{
    public List<GameObject> MeshObjects = new List<GameObject>();
    public Area areaLabel;
    public static StoreAreaLabel instance;
    public bool reloaded = false;
    private void Awake()
    {
        instance = this;
    }

}
