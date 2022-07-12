using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voronoi
{
    public class GridSorting : ISortingAlgorithm
    {
        int _gridWidth;
        int _gridHeight;

        public GridSorting(int gridWidth, int gridHeight)
        {
            _gridWidth = gridWidth;
            _gridHeight = gridHeight;
        }

        public void SortPoints(List<Vector2> points)
        {
            for (int i = 0; i < _gridWidth; i++)
            {
                int start = i * _gridHeight;
                int end = start + _gridHeight;
                SortPoints(start, end, points);
            }

        }

        private void SortPoints(int from, int to, List<Vector2> points)
        {
            int smallestIdx = 0;

            for (int i = from; i < to; i++)
            {
                smallestIdx = i;
                for (int j = i; j < to; j++)
                {
                    if (points[j].x >= points[smallestIdx].x) continue;
                    smallestIdx = j;
                }

                SwapPoints(i, smallestIdx, ref points);
            }

        }

        private void SwapPoints(int idx1, int idx2, ref List<Vector2> points)
        {
            if (idx1 == idx2) return;
            var temp = points[idx1];
            points[idx1] = points[idx2];
            points[idx2] = temp;
        }
    }
}

