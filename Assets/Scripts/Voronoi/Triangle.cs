using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public struct Triangle
    {
        public Vector2 v1, v2, v3, center;

        public Triangle(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            center = FindCenter(v1, v2, v3);
        }

        public static bool operator ==(Triangle a, Triangle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Triangle a, Triangle b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Triangle)) return false;
            return Equals((Triangle)obj);
        }

        bool PointIsVertex(Vector2 point)
        {
            return point == v1 || point == v2 || point == v3;
        }

        bool Equals(Triangle other)
        {
            return PointIsVertex(other.v1)
                && PointIsVertex(other.v2)
                && PointIsVertex(other.v3);
        }

        public override int GetHashCode()
        {
            int hashCode = -557448262;
            hashCode = hashCode * -1521134295 + v1.x.GetHashCode() + v2.x.GetHashCode() + v3.x.GetHashCode();
            hashCode = hashCode * -1521134295 + v1.y.GetHashCode() + v2.y.GetHashCode() + v3.y.GetHashCode();
            return hashCode;
        }

        //Math magic
        static Vector2 FindCenter(Vector2 a, Vector2 b, Vector2 c)
        {
            float ad = a.x * a.x + a.y * a.y;
            float bd = b.x * b.x + b.y * b.y;
            float cd = c.x * c.x + c.y * c.y;
            float D = 2 * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));

            float x = 1 / D * (ad * (b.y - c.y) + bd * (c.y - a.y) + cd * (a.y - b.y));
            float y = 1 / D * (ad * (c.x - b.x) + bd * (a.x - c.x) + cd * (b.x - a.x));

            return new Vector2(x, y);
        }

        public override string ToString()
        {
            return $"{v1}, {v2}, {v3}";
        }

    }
}

