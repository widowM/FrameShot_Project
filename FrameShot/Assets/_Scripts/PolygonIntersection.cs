using UnityEngine;
using System.Collections.Generic;

public class PolygonIntersection
{
    public static Vector2[] GetPolygonIntersection(Vector2[] subjectPolygon, Vector2[] clipPolygon)
    {
        // Create a list to store the output vertices
        List<Vector2> outputList = new List<Vector2>(subjectPolygon);
        
        // For each edge in the clip polygon
        for (int i = 0; i < clipPolygon.Length; i++)
        {
            // Get the current clip edge
            Vector2 clipEdgeStart = clipPolygon[i];
            Vector2 clipEdgeEnd = clipPolygon[(i + 1) % clipPolygon.Length];
            
            // Create a new list for the input vertices
            List<Vector2> inputList = new List<Vector2>(outputList);
            outputList.Clear();
            
            // If there are no vertices left, exit early
            if (inputList.Count == 0) break;
            
            // Get the last point of input polygon
            Vector2 s = inputList[inputList.Count - 1];
            
            foreach (Vector2 e in inputList)
            {
                // If the current point is inside the clip edge
                if (IsInside(clipEdgeStart, clipEdgeEnd, e))
                {
                    // If the previous point wasn't inside
                    if (!IsInside(clipEdgeStart, clipEdgeEnd, s))
                    {
                        // Add the intersection point
                        Vector2 intersection = GetIntersection(s, e, clipEdgeStart, clipEdgeEnd);
                        outputList.Add(intersection);
                    }
                    outputList.Add(e);
                }
                // If the current point is not inside but previous was
                else if (IsInside(clipEdgeStart, clipEdgeEnd, s))
                {
                    // Add the intersection point
                    Vector2 intersection = GetIntersection(s, e, clipEdgeStart, clipEdgeEnd);
                    outputList.Add(intersection);
                }
                s = e;
            }
        }
        
        return outputList.ToArray();
    }
    
    // Helper method to check if a point is inside an edge
    private static bool IsInside(Vector2 edgeStart, Vector2 edgeEnd, Vector2 point)
    {
        return (edgeEnd.x - edgeStart.x) * (point.y - edgeStart.y) -
               (edgeEnd.y - edgeStart.y) * (point.x - edgeStart.x) <= 0;
    }
    
    // Helper method to get the intersection point of two lines
    private static Vector2 GetIntersection(Vector2 line1Start, Vector2 line1End, 
                                         Vector2 line2Start, Vector2 line2End)
    {
        float x1 = line1Start.x;
        float y1 = line1Start.y;
        float x2 = line1End.x;
        float y2 = line1End.y;
        float x3 = line2Start.x;
        float y3 = line2Start.y;
        float x4 = line2End.x;
        float y4 = line2End.y;
        
        float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (denominator == 0) return Vector2.zero; // Lines are parallel
        
        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
        
        return new Vector2(
            x1 + t * (x2 - x1),
            y1 + t * (y2 - y1)
        );
    }
}