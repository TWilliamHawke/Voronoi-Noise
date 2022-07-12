using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public struct Diagonoid
    {
        public Edge edge;
        public Vector2 v1;
        public Vector2 v2;

        public Vector2 edgeStart => edge.start;
        public Vector2 edgeEnd => edge.end;

        public Diagonoid(Edge edge, Vector2 v1)
        {
            this.edge = edge;
            this.v1 = v1;
            v2 = Vector2.zero;
        }

        public Diagonoid(Edge edge, Vector2 v1, Vector2 v2)
        {
            this.edge = edge;
            this.v1 = v1;
            this.v2 = v2;
            if (v1 == v2)
            {
                this.v2 = Vector2.zero;
            }
        }

        public bool IsComlete()
        {
            return v2 != Vector2.zero;
        }

        public bool MainEdgeIsCorrect()
        {
            var t1 = new Triangle(edge.start, edge.end, v1);
            var t1Center = t1.center;

            var t2 = new Triangle(edge.start, edge.end, v2);
            var t2Center = t2.center;

            return (v1 - t1Center).sqrMagnitude <= (v2 - t1Center).sqrMagnitude &&
                (v2 - t2Center).sqrMagnitude <= (v1 - t2Center).sqrMagnitude;
        }

        public Diagonoid CreateFullVersion(Vector2 point)
        {
            bool sameSide = edge.PointsFromSameSide(point, v1);

            if (!sameSide || v2 == Vector2.zero && !sameSide) return new Diagonoid(edge, point, v1);

            return new Diagonoid(edge, point, v2);
        }

        public Diagonoid CreateAltVersion(Vector2 point)
        {
            if (edge.PointsFromSameSide(point, v1)) return new Diagonoid(edge, point, v2);
            return new Diagonoid(edge, point, v1);
        }

        public bool VerticesFromSameSide()
        {
            return edge.PointsFromSameSide(v1, v2);
        }

        public override string ToString()
        {
            return $"{edge}, v:{v1}, {v2}";
        }



    }
}

