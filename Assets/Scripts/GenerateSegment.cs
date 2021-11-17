using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateSegment
{
    private Texture2D peopleDensity;
    private Color[] pixels;
    // width和height是people density map上的坐标
    private int width;
    private int height;
    private int mapScale;
    // swidth和sheight是实际Unity里面plane的坐标
    private int sheight;
    private int swidth;

    public List<Segment> resSegments;

    // 定义PriorityQueue里面的比较接口
    public class minHeapComp: IComparer<Segment>
    {
        public int Compare(Segment s1, Segment s2)
        {
            if (s1.timestamp < s2.timestamp)
            {
                return 1;
            }else if (s1.timestamp == s2.timestamp)
            {
                return 0;
            }
            return -1;
        }
    }
    private PriorityQueue<Segment, minHeapComp> minSegmentHeap;

    public GenerateSegment(Texture2D tpeopleDensity)
    {
        minSegmentHeap = new PriorityQueue<Segment, minHeapComp>();
        peopleDensity = tpeopleDensity;
        pixels = peopleDensity.GetPixels();
        mapScale = 10;
        width = peopleDensity.width;
        height = peopleDensity.height;
        swidth = peopleDensity.width*mapScale;
        sheight = peopleDensity.height*mapScale;
        resSegments = new List<Segment>();
    }
    private List<Segment> GenerateInitialSegments()
    {
        List<Segment> res = new List<Segment>();
        Vector3 prev = new Vector3(-swidth / 2, 0, sheight / 2);
        // 纵向初始化道路
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Vector3 p = new Vector3(
                    Random.Range(-swidth / 2 + i * swidth / 16, -swidth / 2 + (i + 1) * swidth / 16),
                    0,
                    Random.Range(-sheight / 2 + j * sheight / 8, -sheight / 2 + (j + 1) * sheight / 8)
                );
                //Vector3 p = new Vector3(i, 0, j);
                Segment t = new Segment(1, prev, p, SegmentType.HIGHWAY);

                if (j != 0 && getPeopleDensity(prev) > 0.5 && getPeopleDensity(p) > 0.5)
                {
                    res.Add(t);
                }
                prev = p;
            }
        }
        // 横向初始化道路
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < 16; i++)
            {
                Vector3 p = new Vector3(
                    Random.Range(-swidth / 2 + i * swidth / 16, -swidth / 2 + (i + 1) * swidth / 16),
                    0,
                    Random.Range(-sheight / 2 + j * sheight / 8, -sheight / 2 + (j + 1) * sheight / 8)
                );
                //Vector3 p = new Vector3(i, 0, j);
                Segment t = new Segment(1, prev, p, SegmentType.HIGHWAY);

                if (i != 0 && getPeopleDensity(prev) > 0.5 && getPeopleDensity(p) > 0.5)
                {
                    res.Add(t);
                }
                prev = p;
            }
        }
        return res;
    }
    // 从图片里面把相应坐标的人口密度取出来
    private float getPeopleDensity(Vector3 pos)
    {
        int x = (int)pos.x/mapScale + width / 2;
        int z = (int)pos.z/mapScale + height / 2;
        int index = z * width + x;
        if(x<0 || x>=width || z<0 || z > height)
        {
            return 0;
        }
        if(index<0 || index >= width * height)
        {
            return 0;
        }
        return (pixels[index].r+ pixels[index].g+ pixels[index].b)/3;
    }

    public List<Segment> Running()
    {
        while (minSegmentHeap.size() == 0)
        {
            List<Segment> initialSegments = GenerateInitialSegments();
            foreach (Segment initialSegment in initialSegments)
            {
                minSegmentHeap.push(initialSegment);
            }
        }
        // 分两个阶段，highways, street
        int segmentCount = 0;
        while (!minSegmentHeap.empty() && segmentCount<50000)
        {
            segmentCount++;
            Segment cur = minSegmentHeap.top();
            if ((cur.end - cur.start).magnitude < 90) continue;
            minSegmentHeap.pop();
            //Debug.Log(minSegmentHeap.size());
            Debug.Log(cur.start);
            bool isSuccess = LocalConstraints(ref cur);
            Debug.Log(cur.start);
            if ((cur.end - cur.start).magnitude < 90) continue;
            //Debug.Log("Hello2");
            if (isSuccess)
            {
                resSegments.Add(cur);
                if (cur.type == SegmentType.HIGHWAY)
                {

                    List<Segment> streets = SpawnStreetFromHighway(cur);
                    for(int i = 0; i < streets.Count; i++)
                    {
                        minSegmentHeap.push(streets[i]);
                    }
                }
                List<Segment> nextSeg=GlobalGoals(cur);
                for(int i = 0; i < nextSeg.Count; i++)
                {
                    minSegmentHeap.push(nextSeg[i]);
                }
            }

        }
        Debug.Log(resSegments.Count);
        return resSegments;
    }
    private bool LocalConstraints(ref Segment cur)
    {
        if (cur.timestamp < 0) return false;
        if (cur.type == SegmentType.HIGHWAY)
        {
            return LocalConstraintsHighway(ref cur);
        }
        else if(cur.type == SegmentType.STREET)
        {
            return LocalConstraintsStreet(ref cur);
        }
        return false;
    }
    private bool LocalConstraintsHighway(ref Segment cur)
    {
        int intersectionCount = 0;
        foreach(Segment seg in resSegments)
        {
            Vector3 intersection = Segment.is2SegmentIntersect(seg, cur);
            Vector3 tintersection;
            if (intersection.x == (float)IntersectionType.SAME_LINE_ADJACENT ||
                intersection.x == (float)IntersectionType.SAME_LINE_OVERLAP ||
                intersection.x == (float)IntersectionType.SAME_LINE_NO_INTERSECTION ||
                intersection.x == (float)IntersectionType.NO_INTERSECTION ||
                intersection.x == (float)IntersectionType.UNSAME_LINE_ADJACENT)
            {
                continue;
            }
            else
            {
                Debug.Log(intersection);
                intersectionCount++;
                tintersection = intersection;
            }
            if (intersectionCount > 1) return false;
            if (intersectionCount == 1) cur.end = tintersection;
        }
        return true;
    }
    private bool LocalConstraintsStreet(ref Segment cur)
    {
        int intersectionCount = 0;
        foreach (Segment seg in resSegments)
        {
            Vector3 intersection = Segment.is2SegmentIntersect(seg, cur);
            Vector3 tintersection;
            if (intersection.x == (float)IntersectionType.SAME_LINE_OVERLAP)
            {
                return false;
            }
            if (intersection.x == (float)IntersectionType.SAME_LINE_ADJACENT ||
                intersection.x == (float)IntersectionType.SAME_LINE_NO_INTERSECTION ||
                intersection.x == (float)IntersectionType.NO_INTERSECTION ||
                intersection.x == (float)IntersectionType.UNSAME_LINE_ADJACENT)
            {
                continue;
            }
            else
            {
                Debug.Log(intersection);
                intersectionCount++;
                tintersection = intersection;
            }
            if (intersectionCount > 1) return false;
            if (intersectionCount == 1) cur.end = tintersection;
        }
        return true;
    }
    public List<Segment> GlobalGoals(Segment cur)
    {
        List<Segment> res = new List<Segment>();
        if (cur.type == SegmentType.HIGHWAY)
        {
            return GlobalGoalsHighway(cur);

        } else if(cur.type == SegmentType.STREET)
        {
            return GlobalGoalsStreet(cur);
        }
        return res;
    }
    private List<Segment> GlobalGoalsStreet(Segment cur)
    {
        List<Segment> res = new List<Segment>();
        Vector3 dir = (cur.end - cur.start).normalized;
        float len = (cur.end - cur.start).magnitude;
        float[] angles = { -90.0f, 0.0f, 90.0f };
        int t = 0;
        for(int i = 0; i < 3; i++)
        {
            // Segment next = new Segment(200, cur.end, newEnds[i], SegmentType.STREET);
            if (true)
            {
                float tlen = len;
                if (angles[i] != 0)
                {
                    tlen = len * UnityEngine.Random.Range(0.8f, 1.0f);
                    t = 1000;
                }
                else
                {
                    t = 0;
                }
                Vector3 d = Quaternion.Euler(0, angles[i], 0) * dir;

                if (isValidPoint(cur.end + (d * tlen)))
                {
                    Segment next = new Segment(cur.timestamp+t, cur.end, cur.end + (d * tlen), SegmentType.STREET);
                    bool isValid = true;
                    foreach (Segment seg in resSegments)
                    {
                        if (Segment.is2SegmentIntersect(seg, next).x == (float)IntersectionType.SAME_LINE_OVERLAP ||
                            Segment.is2SegmentIntersect(seg, next).x == (float)IntersectionType.SAME_LINE_ADJACENT)
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid)
                    {
                        res.Add(next);
                    }
                    
                } 

            }
        }
        return res;
    }
    private List<Segment> GlobalGoalsHighway(Segment cur)
    {
        List<Segment> res = new List<Segment>();
        Vector3 dir = (cur.end - cur.start).normalized;
        float[] angles = new float[6];
        Vector3[] newEnds = new Vector3[6];
        angles[0] = Random.Range(-70, -15);
        angles[1] = Random.Range(-70, -15);
        angles[2] = Random.Range(-15, 15);
        angles[3] = Random.Range(-15, 15);
        angles[4] = Random.Range(15, 70);
        angles[5] = Random.Range(15, 70);
        int count = 0;
        for (int i = 0; i < 6 && count<2; i++)
        {
            float length = Random.Range(800, 1200);
            Vector3 newDir1 = (Quaternion.Euler(0, angles[i], 0) * dir) * length;
            newEnds[i] = cur.end + newDir1;
            if (!isValidPoint(newEnds[i]))
            {
                continue;
            }
            Segment next = new Segment(1, cur.end, newEnds[i], SegmentType.HIGHWAY);

            float curDensity = getDensity(next);
            float curDiffDensity = getDiffDensity(next);
            if (curDensity > 0.9)
            {
                count++;
                next.timestamp = 1;
                res.Add(next);
            }
            else if(curDensity > 0.7)
            {
                count++;
                next.timestamp = 2;
                res.Add(next);
            }
            else if (curDensity > 0.3)
            {
                count++;
                next.timestamp = 5;
                res.Add(next);
            }
            else if (curDiffDensity > 0.5)
            {
                count++;
                next.timestamp = 3;
                res.Add(next);
            }
            else if (curDiffDensity > 0.2)
            {
                count++;
                next.timestamp = 4;
                res.Add(next);
            }

            //res.Add(next);
        }


        return res;
    }
    List<Segment> SpawnStreetFromHighway(Segment s)
    {
        float len = (s.end - s.start).magnitude;
        Vector3 dir = (s.end - s.start).normalized;
        List<Segment> res = new List<Segment>();
        if (len < 500) return res;
        float streetLen = Random.Range(120, 160);
        for(int i = 1; i < 7; i++)
        {
            Vector3 newStart = s.start + dir * (len / 7) * i;
            Vector3 vdir = Segment.GetVerticalDir(dir);
            Vector3 newEnd = newStart + vdir * streetLen;
            Vector3 rnewEnd = newStart - vdir * streetLen;
            Segment str1 = new Segment(100, newStart, newEnd, SegmentType.STREET);
            Segment str2 = new Segment(100, newStart, rnewEnd, SegmentType.STREET);
            res.Add(str1);
            res.Add(str2);
        }
        return res;
    }

    private bool isaPointClosetoAnother(Vector3 src, Vector3 target, float radius)
    {
        return (target - src).magnitude < radius;
    }

    private float getDiffDensity(Segment s)
    {
        Vector3 midPoint = (s.end + s.start) * 0.5f;
        float farPointDensity = getPeopleDensity(s.end);
        float midPointDensity = getPeopleDensity(midPoint);
        return farPointDensity - midPointDensity;
    }

    private float getDensity(Segment s)
    {
        Vector3 midPoint = (s.end + s.start) * 0.5f;
        float farPointDensity = getPeopleDensity(s.end);
        float midPointDensity = getPeopleDensity(midPoint);
        return (farPointDensity + midPointDensity) / 2;
    }

    private bool isValidPoint(Vector3 p)
    {
        return p.x > -swidth / 2 && p.x < swidth / 2 && p.z > -sheight / 2 && p.z < sheight/2; 
    }
}

