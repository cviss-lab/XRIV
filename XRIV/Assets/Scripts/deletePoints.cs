using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deletePoints : MonoBehaviour
{
    // Start is called before the first frame update
    public void deleteLastPoint()
    {
        for (int i = DrawLines.instance.allList.Count - 1; i >= 0; --i)
        {
            if (DrawLines.instance.allList[i] == null)
            {
                continue;
            }
            Destroy(DrawLines.instance.allList[i].gameObject);
            break;
        }

    }

    public void deleteAllPoints()
    {
        for (int i = DrawLines.instance.allList.Count - 1; i >= 0; --i)
        {
            if (DrawLines.instance.allList[i] == null)
            {
                continue;
            }
            Destroy(DrawLines.instance.allList[i].gameObject);
        }
    }

    public void resetMesh()
    {
        GameObject[] meshList = GameObject.FindGameObjectsWithTag("Mesh");
        foreach (GameObject m in meshList)
        {
            Destroy(m);
        }

        GameObject[] labelList = GameObject.FindGameObjectsWithTag("label");
        foreach (GameObject l in labelList)
        {
            Destroy(l);
        }
    }
}
