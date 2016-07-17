using System;
using Fusee.Math.Core;

namespace Fusee.Tutorial.Core.Assets
{
    static class Constants
    {
        public const float GRAVITY = -0.1f;
        public const float BUNKER_SCALE = (1.0f / 500.0f) * MapGenerator.tileSize;
        public const float PROJECTILE_SCALE = BUNKER_SCALE;
        public static int projectile_Count = 0;

        public static Random random = new Random();

        public static float degreeToRadian(double degree)
        {
            return (float)(System.Math.PI * degree / 180.0);
        }

        public static double radianToDegree(double radian)
        {
            return (float)(radian * (180.0 / System.Math.PI));
        }

        public static float2 angleToVector(float _angle)
        {
            float2 angleVector = new float2((float)System.Math.Cos(_angle), -(float)System.Math.Sin(_angle));
            return angleVector;
        }

        public static float vectorToAngle(float2 _vector)
        {
            float angleFromVector = (float)System.Math.Atan2(_vector.x, -_vector.y);
            return angleFromVector;
        }
    }
}
