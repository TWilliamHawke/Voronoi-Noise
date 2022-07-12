using UnityEngine;

namespace Voronoi
{
    public static  class VectorExtension
    {
        public static Vector2 MirrorX(this Vector2 vc, float xAxis)
        {
            float x = xAxis * 2 - vc.x;

            return new Vector2(x, vc.y);
        }
        
        public static Vector2 MirrorY(this Vector2 vc, float yAxis)
        {
            float y = yAxis * 2 - vc.y;

            return new Vector2(vc.x, y);
        }

        public static Vector3 AddZ(this Vector2 vc, float z = 0f)
        {
            return new Vector3(vc.x, vc.y, z);
        }

        public static Vector2 RemoveZ(this Vector3 vc)
        {
            return new Vector2(vc.x, vc.y);
        }
        public static Vector2 RemoveY(this Vector3 vc)
        {
            return new Vector2(vc.x, vc.z);
        }
    }

}

