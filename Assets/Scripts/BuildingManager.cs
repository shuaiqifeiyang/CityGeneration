using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class BuildingManager : MonoBehaviour
{
    public GameObject buildingPrefab;
    
    // use to control the num of building generate by the roads
    private List<int> randomList = new List<int> {3, 4, 5};

    public void DrawBuilding(Segment segment)
    {
        if (segment.type == SegmentType.HIGHWAY)
        {
            return;
        }
        
        if(segment.type == SegmentType.STREET)
        {
            int buildingSize = randomList[UnityEngine.Random.Range(0, randomList.Count)];
            
            Vector3 slopeSegment = segment.end - segment.start;
            Vector3 slopeVertical = new Vector3(-slopeSegment[2], slopeSegment[1], slopeSegment[0]).normalized;
            Vector3 curPosition = segment.start;
            slopeSegment /= (buildingSize + 1);
            float offset = slopeSegment.magnitude / 1.5f;
            float desity = segment.density[0] + segment.density[1] + segment.density[2];

            for (int i = 1; i <= buildingSize; i++)
            {
                curPosition += i * slopeSegment;
                
                GameObject buildObj1 = Instantiate(buildingPrefab, curPosition + offset * slopeVertical, 
                    Quaternion.identity, transform);
                GameObject buildObj2 = Instantiate(buildingPrefab, curPosition - offset * slopeVertical, 
                    Quaternion.identity, transform);
                buildObj1.transform.localScale = new Vector3(offset, offset * (1.0f + 2 * desity * desity + 1 * desity), offset);
                buildObj2.transform.localScale = new Vector3(offset, offset * (1.0f + 2 * desity * desity + 1 * desity), offset);
                
                if (slopeSegment.z < 0)
                {
                    slopeSegment.z = -slopeSegment.z;
                    slopeSegment.x = -slopeSegment.x;
                }
                float angle = Vector3.Angle(slopeSegment, new Vector3(1, 0, 0));
                //Debug.Log(angle);
                buildObj1.transform.Rotate(new Vector3(0, -angle, 0));
                buildObj2.transform.Rotate(new Vector3(0, -angle, 0));
            }
            
            
            
        }
        
    }
}
