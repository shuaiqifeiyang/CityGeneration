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

    private float minHighway; // 高速公路长度的下限
    private float minStreet; // 街道长度的下限
    private float spawnStreetLower;
    private float spawnStreetHigher;

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
    public class SegDensityComp : IComparer<Segment>
    {
        public int Compare(Segment s1, Segment s2)
        {
            if(s1.density[2] - s1.density[1] > s2.density[2] - s2.density[1])
            {
                return 1;
            }else if(s1.density[2] - s1.density[1] == s2.density[2] - s2.density[1])
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
    private PriorityQueue<Segment, minHeapComp> minSegmentHeap;

    public GenerateSegment(Texture2D tpeopleDensity)
    {
        minSegmentHeap = new PriorityQueue<Segment, minHeapComp>();
        resSegments = new List<Segment>();
        peopleDensity = tpeopleDensity;
        pixels = peopleDensity.GetPixels();
        mapScale = 10;
        width = peopleDensity.width;
        height = peopleDensity.height;
        swidth = peopleDensity.width*mapScale;
        sheight = peopleDensity.height*mapScale;
        minHighway = swidth / 30;
        minStreet = swidth / 100;
        spawnStreetLower = swidth / 70;
        spawnStreetHigher = swidth / 50;

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
        while (!minSegmentHeap.empty() && segmentCount<10000)
        {
            segmentCount++;
            Segment cur = minSegmentHeap.top();
            minSegmentHeap.pop();
            bool isSuccess = LocalConstraints(ref cur);
            if (isSuccess)
            {
                cur.density[0] = getPeopleDensity(cur.start);
                cur.density[1] = getPeopleDensity((cur.start + cur.end) / 2);
                cur.density[2] = getPeopleDensity(cur.end);
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
        Debug.Log("生成的总的线段数");
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
        if ((cur.end - cur.start).magnitude < minHighway)
        {
            return false;
        }
        int intersectionCount = 0;
        foreach(Segment seg in resSegments)
        {
            Vector3 intersection = Segment.is2SegmentIntersect(seg, cur);
            Vector3 tintersection = new Vector3(0, 0, 0);
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
                //if(!MyUtil.VectorEqual(intersection, cur.start)) {
                    intersectionCount++;
                    tintersection = intersection;
                //}
            }
            if (intersectionCount > 1) return false;
            if (intersectionCount == 1) cur.end = tintersection;
        }
        if ((cur.end - cur.start).magnitude < minHighway)
        {
            return false;
        }
        return true;
    }
    private bool LocalConstraintsStreet(ref Segment cur)
    {
        if ((cur.end - cur.start).magnitude < minStreet)
        {
            return false;
        }
        int intersectionCount = 0;
        foreach (Segment seg in resSegments)
        {
            Vector3 intersection = Segment.is2SegmentIntersect(seg, cur);
            Vector3 tintersection;
            if (intersection.x == (float)IntersectionType.SAME_LINE_OVERLAP)
            {
                return true;
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
                intersectionCount++;
                tintersection = intersection;
            }
            if (intersectionCount > 1) return false;
            if (intersectionCount == 1)
            {
                cur.end = tintersection;
                cur.expanded = false;
            }
            if ((cur.end - cur.start).magnitude < minStreet)
            {
                return false;
            }
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
        if (cur.expanded == false) return res;

        Vector3 dir = (cur.end - cur.start).normalized;
        float len = (cur.end - cur.start).magnitude;
        float[] angles = { -90.0f, 0.0f, 90.0f };
        for(int i = 0; i < 3; i++)
        {
            float tlen;
            Vector3 d = Quaternion.Euler(0, angles[i], 0) * dir;
            if (angles[i] == 0)
            {
                tlen = len;
            }
            else
            {
                tlen = cur.verticalLen-1;
            }
            Segment next = new Segment(cur.timestamp + 1, cur.end + d, cur.end + (d * tlen), SegmentType.STREET);
            if(angles[i] != 0)
            {
                next.expanded = false;
            }
            else
            {
                next.expanded = true;
                next.verticalLen = cur.verticalLen;   
            }
            if (isValidPoint(cur.end + (d * tlen)))
            {
                bool isValid = true;
                foreach (Segment seg in resSegments)
                {
                    if (seg == cur) continue;
                    if (Segment.is2SegmentIntersect(seg, next).x == (float)IntersectionType.SAME_LINE_OVERLAP)
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
        return res;
    }
    private List<Segment> GlobalGoalsHighway(Segment cur)
    {
        List<Segment> res = new List<Segment>();
        Vector3 dir = (cur.end - cur.start).normalized;
        float[] angles = new float[6];
        Vector3[] newEnds = new Vector3[6];
        angles[0] = Random.Range(-135, -90);
        angles[1] = Random.Range(-90, -45);
        angles[2] = Random.Range(-45, 0);
        angles[3] = Random.Range(0, 45);
        angles[4] = Random.Range(45, 90);
        angles[5] = Random.Range(90, 135);

        List<Segment> candidate = new List<Segment>();
        for (int i = 0; i < 6; i++)
        {

            float length = Random.Range(800, 1200);
            Vector3 newDir1 = (Quaternion.Euler(0, angles[i], 0) * dir) * length;
            newEnds[i] = cur.end + newDir1;
            Segment next = new Segment(1, cur.end, newEnds[i], SegmentType.HIGHWAY);
            next.density[0] = getPeopleDensity(next.start);
            next.density[1] = getPeopleDensity((next.start + next.end) / 2);
            next.density[2] = getPeopleDensity(next.end);
            candidate.Add(next);
            
        }
        SegDensityComp c = new SegDensityComp();
        candidate.Sort(0, candidate.Count, c);
        for(int i=0;i<candidate.Count && i<2; i++)
        {
            float average = (candidate[i].density[0] + candidate[i].density[1] + candidate[i].density[2]) / 3;
            float diff = candidate[i].density[2] - candidate[i].density[1];
            if (candidate[i].density[2] > 0 && average > 0.22)
            {
                res.Add(candidate[i]);
            }
            //res.Add(candidate[i]);
        }

        return res;
    }
    List<Segment> SpawnStreetFromHighway(Segment s)
    {
        float len = (s.end - s.start).magnitude;
        Vector3 dir = (s.end - s.start).normalized;
        List<Segment> res = new List<Segment>();
        if (len < 600) return res;
        float streetLen = Random.Range(spawnStreetLower, spawnStreetHigher);
        int randInterval = UnityEngine.Random.Range(6, 9);
        for(int i = 1; i < randInterval; i++)
        {
            Vector3 newStart = s.start + dir * (len / randInterval) * i;
            Vector3 vdir = Segment.GetVerticalDir(dir);
            Vector3 newEnd = newStart + vdir * streetLen;
            Vector3 rnewEnd = newStart - vdir * streetLen;
            Segment str1 = new Segment(100, newStart + vdir*5, newEnd, SegmentType.STREET);
            str1.verticalLen = len / randInterval;
            Segment str2 = new Segment(100, newStart - vdir * 5, rnewEnd, SegmentType.STREET);
            str2.verticalLen = len / randInterval;
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
        return p.x > -swidth / 2 && p.x < swidth / 2 && p.z > -sheight / 2 && p.z < sheight/2 && getPeopleDensity(p)>0; 
    }
}

