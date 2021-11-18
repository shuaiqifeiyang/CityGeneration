using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment
{
    public int timestamp;
    public Vector3 start;
    public Vector3 end;
    public int width;
    public SegmentType type;
    public float[] density = new float[3];
    // 这两个变量用来street生成street的时候用
    public float verticalLen;
    public bool expanded=true;

    public Segment(int ttimestamp)
    {
        timestamp = ttimestamp;
    }
    public Segment(Vector3 tstart)
    {
        start = tstart;
    }
    public Segment(Vector3 tstart, Vector3 tend, int twidth)
    {
        start = tstart;
        end = tend;
        width = twidth;
    }
    public Segment(int ttimestamp, Vector3 tstart, Vector3 tend, SegmentType ttype)
    {
        timestamp = ttimestamp;
        start = tstart;
        end = tend;
        type = ttype;
        if (ttype == SegmentType.HIGHWAY)
        {
            width = 20;
        }else if(ttype == SegmentType.STREET)
        {
            width = 10;
        }
    }
    public static Vector3 GetVerticalDir(Vector3 dir)
    {
        if (dir.z == 0)
        {
            return new Vector3(0, 0, -1);
        }
        else
        {
            return new Vector3(-dir.z / dir.x, 0, 1).normalized;
        }
    }
    // 检测两个segment的相交情况
    // s1 是已经确认的segment, s2是待检测的segment
    public static Vector3 is2SegmentIntersect(Segment s1, Segment s2)
    {
        float x1 = s1.start.x, y1 = s1.start.z;
        float x2 = s1.end.x, y2 = s1.end.z;
        float x3 = s2.start.x, y3 = s2.start.z;
        float x4 = s2.end.x, y4 = s2.end.z;
        //if (
        //    MyUtil.VectorEqual(s1.start, s2.end) ||
        //    MyUtil.VectorEqual(s1.start, s2.start) ||
        //    MyUtil.VectorEqual(s1.end, s2.start) ||
        //    MyUtil.VectorEqual(s1.end, s2.end) ||

        //    (
        //        PointOnSegment(s2.start, s1) ||
        //        PointOnSegment(s2.end, s1) ||
        //        PointOnSegment(s1.start, s2) ||
        //        PointOnSegment(s1.end, s2)
        //    )
        //) {

        //}


        // 不平行
        if (System.Math.Abs((y4 - y3) * (x2 - x1) - (y2 - y1) * (x4 - x3))>0.00001f
            )
        {
            if(
                MyUtil.VectorEqual(s1.start, s2.end) ||
                MyUtil.VectorEqual(s1.start, s2.start) ||
                MyUtil.VectorEqual(s1.end, s2.start) ||
                MyUtil.VectorEqual(s1.end, s2.end) ||
                PointOnSegment(s2.start, s1) ||
                PointOnSegment(s2.end, s1) ||
                PointOnSegment(s1.start, s2) ||
                PointOnSegment(s1.end, s2))
            {
                return new Vector3((float)IntersectionType.UNSAME_LINE_ADJACENT, 0, 0);
            }
            float t1 = (x3 * (y4 - y3) + y1 * (x4 - x3) - y3 * (x4 - x3) - x1 * (y4 - y3)) / ((x2 - x1) * (y4 - y3) - (x4 - x3) * (y2 - y1));
            float t2 = (x1 * (y2 - y1) + y3 * (x2 - x1) - y1 * (x2 - x1) - x3 * (y2 - y1)) / ((x4 - x3) * (y2 - y1) - (x2 - x1) * (y4 - y3));
            if (t1 >= 0.0 && t1 <= 1.0 && t2 >= 0.0 && t2 <= 1.0)
            {
                return new Vector3(x1 + t1 * (x2 - x1), 0, y1 + t1 * (y2 - y1));
            }
        }
        else // 平行
        {
            if (
                MyUtil.VectorEqual(s1.start, s2.end) ||
                MyUtil.VectorEqual(s1.start, s2.start) ||
                MyUtil.VectorEqual(s1.end, s2.start) ||
                MyUtil.VectorEqual(s1.end, s2.end)
                )
            {
                return new Vector3((float)IntersectionType.SAME_LINE_ADJACENT, 0, 0);
            }
            //
            if (PointOnSegment(s2.start, s1) || PointOnSegment(s2.end, s1) ||
               PointOnSegment(s1.start, s2) || PointOnSegment(s1.end, s2))
            {
                return new Vector3((float)IntersectionType.SAME_LINE_OVERLAP, 0, 0);
            }
            return new Vector3((float)IntersectionType.SAME_LINE_NO_INTERSECTION, 0, 0);
        }
        return new Vector3((float)IntersectionType.NO_INTERSECTION, 0, 0);
    }

    public static bool PointOnSegment(Vector3 p, Segment seg)
    {
        float x1 = seg.start.x, y1 = seg.start.z;
        float x2 = seg.end.x, y2 = seg.end.z;
        float x3 = p.x, y3 = p.z;
        return System.Math.Abs((y2 - y3) * (x1 - x3) - (y1 - y3) * (x2 - x3))<0.00000001f;
    }

}

public enum SegmentType
{
    HIGHWAY,
    STREET
}
public enum IntersectionType
{
    ADJACENT = 9999,
    NO_INTERSECTION = 10000,
    UNSAME_LINE_ADJACENT = 10001,
    SAME_LINE_ADJACENT = 10002,
    SAME_LINE_NO_INTERSECTION = 10003,
    SAME_LINE_OVERLAP = 10004
}
