using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public struct Edge
    {
        public Vector2 start;
        public Vector2 end;

        public float length => Vector2.Distance(start, end);
        public Vector2 center => (start + end) / 2;
        public Vector2 direction => end - start;

        public Edge(Vector2 v1, Vector2 v2)
        {
            start = v1.x < v2.x  ? v1 : v2;

            if(v1.x == v2.x)
            {
                start = v1.y < v2.y ? v1 : v2;
            }

            end = start == v1 ? v2 : v1;
        }

        public bool PointsFromSameSide(Vector2 p1, Vector2 p2)
        {
            var topoint1 = p1 - start;
            var topoint2 = p2 - start;

            var cross1 = topoint1.x * direction.y - topoint1.y * direction.x;
            var cross2 = topoint2.x * direction.y - topoint2.y * direction.x;
            return cross1 * cross2 > 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge)) return false;
            return Equals((Edge)obj);
        }

        public static bool operator ==(Edge a, Edge b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Edge a, Edge b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            int hashCode = -1406327342;
            hashCode = hashCode * -1521134295 + start.x.GetHashCode() + end.x.GetHashCode();
            hashCode = hashCode * -1521134295 + start.y.GetHashCode() + end.y.GetHashCode();
            return hashCode;
        }

        bool Equals(Edge other)
        {
            return (other.start == this.start) && (other.end == this.end);
        }

        public override string ToString()
        {
            return $"From {start} to {end}";
        }
    }
}

