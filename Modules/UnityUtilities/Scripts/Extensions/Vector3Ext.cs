namespace UnityEngine
{
    public static class Vector3Ext
    {
        private const double EPSILON = 0.0005f;
        private const double EPSILON_SQUARED = EPSILON * EPSILON;
        private const double EPSILON_VIEWING_VECTOR = 0.000000000000001f;
        public static bool FuzzyEquals0(this float v, double epsilon = EPSILON) => FuzzyEquals(v, 0, epsilon);
        public static bool FuzzyEquals(this float v, float b, double epsilon = EPSILON) => Mathf.Abs(v - b) < epsilon;
        public static bool FuzzyEquals(this Vector2 a, Vector2 b, double epsilon = EPSILON_SQUARED) => FuzzyEquals0(Vector2.SqrMagnitude(a - b), epsilon);
        public static bool FuzzyEquals0(this Vector2 a, double epsilon = EPSILON_SQUARED) => FuzzyEquals0(Vector2.SqrMagnitude(a), epsilon);
        public static bool FuzzyEquals(this Vector3 a, Vector3 b, double epsilon = EPSILON_SQUARED) => FuzzyEquals0(Vector3.SqrMagnitude(a - b), epsilon);
        public static bool FuzzyEquals0(this Vector3 a, double epsilon = EPSILON_SQUARED) => FuzzyEquals0(Vector3.SqrMagnitude(a), epsilon);
        /// <summary>
        /// Use to filter out Vector3's that wouldn't cause an issue when used with <see cref="Quaternion.LookRotation(Vector3)"/>
        /// </summary>
        public static bool IsValidViewingVector(this Vector3 a) => !FuzzyEquals0(Vector3.SqrMagnitude(a), EPSILON_VIEWING_VECTOR);
        public static Vector3 LerpAngle(Vector3 start, Vector3 end, float ratio)
        {
            Vector3 lerped;
            lerped.x = Mathf.LerpAngle(start.x, end.x, ratio);
            lerped.y = Mathf.LerpAngle(start.y, end.y, ratio);
            lerped.z = Mathf.LerpAngle(start.z, end.z, ratio);
            return lerped;
        }

        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            value.x = Mathf.Clamp(value.x, min.x, max.x);
            value.y = Mathf.Clamp(value.y, min.y, max.y);
            value.z = Mathf.Clamp(value.z, min.z, max.z);
            return value;
        }

        public static float[] ToFloatArray(this Vector3 vector)
        {
            return new float[3] { vector.x, vector.y, vector.z };
        }

        public static Vector3 ToVector3(this float[] array)
        {
            return FromFloatArray(array);
        }

        public static Vector3 FromFloatArray(float[] array)
        {
            if (array.Length != 3)
                throw new System.ArgumentException("array must be length of 3");

            return new Vector3(array[0], array[1], array[2]);
        }

        #region Multilerp
        // This is not very optimized. There are at least n + 1 and at most 2n Vector3.Distance
        // calls (where n is the number of waypoints). 
        public static Vector3 MultiLerp(Vector3[] waypoints, float ratio)
        {
            Vector3 position = Vector3.zero;
            float totalDistance = waypoints.MultiDistance();
            float distanceTravelled = totalDistance * ratio;

            int indexLow = GetVectorIndexFromDistanceTravelled(waypoints, distanceTravelled);
            int indexHigh = indexLow + 1;

            // we're done
            if (indexHigh > waypoints.Length - 1)
                return waypoints[waypoints.Length - 1];


            // calculate the distance along this waypoint to the next
            Vector3[] completedWaypoints = new Vector3[indexLow + 1];

            for (int i = 0; i < indexLow + 1; i++)
            {
                completedWaypoints[i] = waypoints[i];
            }

            float distanceCoveredByPreviousWaypoints = completedWaypoints.MultiDistance();
            float distanceTravelledThisSegment =
                distanceTravelled - distanceCoveredByPreviousWaypoints;
            float distanceThisSegment = Vector3.Distance(waypoints[indexLow], waypoints[indexHigh]);

            float currentRatio = distanceTravelledThisSegment / distanceThisSegment;
            position = Vector3.Lerp(waypoints[indexLow], waypoints[indexHigh], currentRatio);

            return position;
        }

        public static float MultiDistance(this Vector3[] waypoints)
        {
            float distance = 0f;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (i + 1 > waypoints.Length - 1)
                    break;

                distance += Vector3.Distance(waypoints[i], waypoints[i + 1]);
            }

            return distance;
        }

        public static int GetVectorIndexFromDistanceTravelled(
                Vector3[] waypoints, float distanceTravelled)
        {
            float distance = 0f;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (i + 1 > waypoints.Length - 1)
                    return waypoints.Length - 1;

                float segmentDistance = Vector3.Distance(waypoints[i], waypoints[i + 1]);

                if (segmentDistance + distance > distanceTravelled)
                {
                    return i;
                }

                distance += segmentDistance;
            }

            return waypoints.Length - 1;
        }
        #endregion

        public static bool Approximately(Vector3 me, Vector3 other, float allowedDifference = 0.005f)
        {
            var dx = me.x - other.x;
            if (Mathf.Abs(dx) > allowedDifference)
                return false;

            var dy = me.y - other.y;
            if (Mathf.Abs(dy) > allowedDifference)
                return false;

            var dz = me.z - other.z;

            return Mathf.Abs(dz) >= allowedDifference;
        }

        //With percentage i.e. between 0 and 1
        public static bool ApproximatelyByPercent(Vector3 me, Vector3 other, float percentage)
        {
            var dx = me.x - other.x;
            if (Mathf.Abs(dx) > me.x * percentage)
                return false;

            var dy = me.y - other.y;
            if (Mathf.Abs(dy) > me.y * percentage)
                return false;

            var dz = me.z - other.z;

            return Mathf.Abs(dz) >= me.z * percentage;
        }

        public static string ToStringExt(this Vector3 vector3, string numFormat = "N5")
        {
            return string.Format(
                "({0}, {1}, {2})",
                vector3.x.ToString(numFormat),
                vector3.y.ToString(numFormat),
                vector3.z.ToString(numFormat));
        }


        /// <summary>
        /// Useful for quickly finding the magnitude of a vector with only one component that has a value
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static float Sum(this Vector3 v1)
        {
            return v1.x + v1.y + v1.z;
        }
    }
}