using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public GameObject pathPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void DrawSegment(Segment segment)
    {
        Vector3 startPoint = segment.start, endPoint = segment.end;
        Vector3 pos = new Vector3((startPoint.x+endPoint.x)/2, (startPoint.y + endPoint.y) / 2, (startPoint.z + endPoint.z) / 2);
        float length = (startPoint - endPoint).magnitude;
        GameObject segmentInstance=Instantiate(pathPrefab, pos, Quaternion.identity, transform);

        segmentInstance.transform.localScale += new Vector3(length, 0, segment.width);

        Vector3 dir = endPoint - startPoint;
        if (dir.z < 0)
        {
            dir.z = -dir.z;
            dir.x = -dir.x;
        }
        float angle = Vector3.Angle(dir, new Vector3(1, 0, 0));
        //Debug.Log(angle);
        segmentInstance.transform.Rotate(new Vector3(0, -angle, 0));
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
