using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{

    public Texture2D peopleDensity;
    private int width, height;
    private GenerateSegment generateSegment;
    private List<Segment> segments;

    public PathManager pathManager;

    // Start is called before the first frame update
    void Start()
    {
        generateSegment = new GenerateSegment(peopleDensity);
        //949, 595
        width = peopleDensity.width;
        height = peopleDensity.height;
        segments = generateSegment.Running();
        StartCoroutine("InstantiateRoad");
        
    }
    IEnumerator InstantiateRoad()
    {
        for(int i = 0; i < segments.Count; i++)
        {
            pathManager.DrawSegment(segments[i]);
            //yield return new WaitForSeconds(.1f);
            yield return null;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
