using UnityEngine;
using System.Collections.Generic;
using HitTrax.CoreUtilities;
using static HitTrax.CoreUtilities.SafeFunctions;
using HitTrax.GlobalMessagingService;
using static HitTrax.UnityUtilities.PhysicsUtilities;
using Cysharp.Threading.Tasks;


namespace HitTrax.UnityUtilities
{
    public struct MsgPhysicsSimUpdate : IMessageKey<SimulationTransformData> { }

    public static class PhysicsUtilities
    {
        static public Safe<GameObject> SetLinearVelocity(this Safe<GameObject> obj, Safe<Vector3> velocity)
            => obj.IfSome(o => o.SetLinearVelocity(velocity));

        static public GameObject SetLinearVelocity(this GameObject obj, Safe<Vector3> velocity)
        {
            if (obj == null)
            {
                return obj;
            }

            velocity
                .IfSome(vel =>
                {
                    obj.MaybeComponent<Rigidbody>()
                       .IfSome(rb => rb.linearVelocity = vel);
                });

            return obj;
        }

        static public GameObject DeactivateGravity(this GameObject obj)
        {
            obj.Safe().DeactivateGravity();
            return obj;
        }

        static public Safe<GameObject> DeactivateGravity(this Safe<GameObject> obj)
        {
            obj
                .MaybeComponent<Rigidbody>()
                .IfSome(rb =>
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                });

            return obj;
        }

        static public Safe<GameObject> ActivateGravity(this Safe<GameObject> obj)
        {
            obj
                .MaybeComponent<Rigidbody>()
                .IfSome(rb =>
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                });

            return obj;
        }

        static public GameObject ActivateGravity(this GameObject obj)
        {
            obj.Safe().ActivateGravity();
            return obj;
        }


        // For a static target
        public static Safe<Vector3> CalculateLaunchVelocity(Safe<Vector3> start, Safe<Vector3> target, float throwSpeed, float yGravity)
            => start.Select(p1 => target.Select(p2 => CalculateLaunchVelocity(p1, p2, throwSpeed, yGravity)));


        // For a static target
        public static Safe<Vector3> CalculateLaunchVelocity(Vector3 start, Vector3 target, float throwSpeed, float yGravity)
        {
            yGravity = -yGravity;
            $"Vel Start Pos {start} End Pos {target} Speed {throwSpeed} Gravity {yGravity}".Log("ThrowDecision");
            Vector3 toTarget = target - start;
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);
            float y = toTarget.y;
            float xz = toTargetXZ.magnitude;

            float speedSquared = throwSpeed * throwSpeed;
            float underRoot = speedSquared * speedSquared - yGravity * (yGravity * xz * xz + 2 * y * speedSquared);

            if (underRoot < 0)
            {
                // No valid trajectory
                return None;
            }

            float root = Mathf.Sqrt(underRoot);
            float angle1 = Mathf.Atan((speedSquared + root) / (yGravity * xz));
            float angle2 = Mathf.Atan((speedSquared - root) / (yGravity * xz));

            float chosenAngle = Mathf.Min(angle1, angle2); // Pick the lower trajectory
            Vector3 dir = toTargetXZ.normalized;

            return dir * throwSpeed * Mathf.Cos(chosenAngle) + Vector3.up * throwSpeed * Mathf.Sin(chosenAngle);
        }

        /// <summary>
        /// Returns the flight time (seconds) for the same solution used by CalculateLaunchVelocity.
        /// Uses the lower-arc solution. Mirrors your gravity sign flip.
        /// Returns None if no valid trajectory exists.
        /// </summary>
        public static Safe<float> CalculateFlightTime(Vector3 start, Vector3 target, float throwSpeed, float yGravity)
        {
            yGravity = -yGravity; // match your function's convention (negative down)


            Vector3 toTarget = target - start;
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
            float y = toTarget.y;
            float xz = toTargetXZ.magnitude;


            float speedSquared = throwSpeed * throwSpeed;
            float underRoot = speedSquared * speedSquared - yGravity * (yGravity * xz * xz + 2f * y * speedSquared);


            if (underRoot < 0f)
            {
                return None; // no valid ballistic arc
            }

            float root = Mathf.Sqrt(underRoot);
            float angle1 = Mathf.Atan((speedSquared + root) / (yGravity * xz));
            float angle2 = Mathf.Atan((speedSquared - root) / (yGravity * xz));
            float chosenAngle = Mathf.Min(angle1, angle2); // lower trajectory, same as your launch calc

            if (xz <= 1e-6f)
            {
                // Purely vertical shot to hit an offset in Y: solve 0.5*g*t^2 + v_y*t - y = 0
                float vy = throwSpeed * Mathf.Sin(chosenAngle);
                float a = 0.5f * yGravity;
                float b = vy;
                float c = -y;
                float disc = b * b - 4f * a * c;

                if (disc < 0f)
                {
                    return None;
                }

                float sqrtDisc = Mathf.Sqrt(disc);
                float t1 = (-b + sqrtDisc) / (2f * a);
                float t2 = (-b - sqrtDisc) / (2f * a);


                float t = t1;
                if (t < 0f || (t2 > 0f && t2 < t))
                {
                    t = t2;
                }


                if (t < 0f)
                {
                    return None;
                }


                return t;
            }


            // For non-vertical shots, horizontal kinematics gives the time directly.
            float time = xz / (throwSpeed * Mathf.Cos(chosenAngle));
            return time;
        }

        public struct CalcLaunchInfo
        {
            public Vector3 velocity;
            public float timeToReach;
            public Vector3 interceptPoint;

            public override string ToString()
            {
                return $"Velo {velocity} Time {timeToReach} Intercept {interceptPoint}";
            }
        }

        // For a moving target
        public static Safe<CalcLaunchInfo> CalculateLaunchVelocityForMovingTarget(
            Vector3 throwPos,
            Vector3 targetPosition,
            float maxThrowSpeed,
            Vector3 recieversVelocity,
            float maxTime = 5f,
            float timeStep = 0.02f)
        {
            for (float t = timeStep; t <= maxTime; t += timeStep)
            {
                // Receiver's future position
                Vector3 targetPos = targetPosition + recieversVelocity * t;

                // Displacement
                Vector3 displacement = targetPos - throwPos;

                // Horizontal (XZ) components
                Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);
                float distanceXZ = displacementXZ.magnitude;

                // Required horizontal speed
                Vector3 dirXZ = displacementXZ.normalized;
                float vxz = distanceXZ / t;

                // Vertical component (with gravity)
                float vy = (displacement.y - 0.5f * Physics.gravity.y * t * t) / t;

                // Build velocity vector
                Vector3 velocity = dirXZ * vxz;
                velocity.y = vy;

                // Check if within max throw speed
                if (velocity.magnitude <= maxThrowSpeed)
                {
                    // earliest valid solution
                    return new CalcLaunchInfo
                    {
                        velocity = velocity,
                        timeToReach = t,
                        interceptPoint = targetPos
                    };
                }
            }

            return None; // no valid solution found
        }

        // A simple struct to return multiple values
        public struct TrajectoryData
        {
            public Vector3 initialVelocity;
            public Vector3 apexPosition;
            public float timeToApex;
        }

        // TODO: Rename

        public static Safe<TrajectoryData> GetTrajectoryData(Rigidbody projectileRigidbody, float arcHeight, Transform target)
            => GetTrajectoryData(projectileRigidbody, arcHeight, target.Pos());

        public static Safe<TrajectoryData> GetTrajectoryData(Rigidbody projectileRigidbody, float arcHeight, Vector3 target)
        {
            Vector3 startPosition = projectileRigidbody.position;
            Vector3 endPosition = target;
            float gravity = Physics.gravity.y;

            // Calculate the horizontal direction and distance
            Vector3 horizontalDirection = new Vector3(endPosition.x, 0, endPosition.z) - new Vector3(startPosition.x, 0, startPosition.z);
            float horizontalDistance = horizontalDirection.magnitude;

            // Calculate the initial vertical velocity required to reach the specified arc height
            // This is done relative to the HIGHER of the start/end heights to ensure the apex is always above.
            float maxElevation = Mathf.Max(startPosition.y, endPosition.y) + arcHeight;
            float initialVerticalVelocity = Mathf.Sqrt(-2 * gravity * (maxElevation - startPosition.y));

            // Time to reach the apex
            float timeToApex = -initialVerticalVelocity / gravity;

            // Time to fall from the apex to the target
            float verticalFallDistance = maxElevation - endPosition.y;
            float timeToFall = Mathf.Sqrt(-2 * verticalFallDistance / gravity);

            float totalTime = timeToApex + timeToFall;

            if (totalTime <= 0)
            {
                Debug.LogError("Cannot calculate trajectory. Total time is zero or negative.");
                return None;
            }

            // Calculate the required horizontal velocity
            float horizontalVelocity = horizontalDistance / totalTime;

            // Combine into the final velocity vector
            Vector3 finalVelocity = (horizontalDirection.normalized * horizontalVelocity) + (Vector3.up * initialVerticalVelocity);

            // --- Calculate the Apex Position ---
            // Apex XZ position is based on horizontal velocity and time to apex
            Vector3 apexXZ = new Vector3(startPosition.x, 0, startPosition.z) + (horizontalDirection.normalized * horizontalVelocity * timeToApex);

            // Apex Y position is the maximum elevation
            Vector3 apexPosition = new Vector3(apexXZ.x, maxElevation, apexXZ.z);

            if (
                 float.IsNaN(finalVelocity.x) ||
                 float.IsNaN(finalVelocity.y) ||
                 float.IsNaN(finalVelocity.z)
            )
            {
                $"NaN found: Proj {projectileRigidbody.Pos()} H {arcHeight} Tar {target}".Log();
                return None;
            }

            return new TrajectoryData
            {
                initialVelocity = finalVelocity,
                apexPosition = apexPosition,
                timeToApex = timeToApex
            };
        }

        public struct SimulationTransformData
        {
            public Vector3 pos;
            public float time;
            public float deltaTime;
            public Vector3 velocity;
        }

        private static bool _isSimulatingPhysics = false;

        //(Priyal) TODO - if we DEFINITELy want to do this in one frame, we should just get rid of the numFrames and async stuff altogether. For now we'll support it until somebody can do more experiments
        public static async UniTask<List<SimulationTransformData>> SimulatePhysicsAsync(GameObject trackedObject, float totalTime, int numFrames = 1)
        {
            Physics.simulationMode = SimulationMode.Script;

            float step = Time.fixedDeltaTime; // usually 0.02f
            int steps = Mathf.CeilToInt(totalTime / step);
            float timeStamp = 0;
            List<SimulationTransformData> points = new List<SimulationTransformData>();
            var rb = trackedObject.GetComponent<Rigidbody>();
            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            _isSimulatingPhysics = true;
            var frame = Time.frameCount;
            var time = Time.time;
            for (int i = 0; i < steps; i++)
            {
                if (!_isSimulatingPhysics)//canceled from elsewhere
                {
                    break;
                }

                Physics.Simulate(step);
                timeStamp += step;
                var point = new SimulationTransformData
                {
                    pos = trackedObject.Pos(),
                    time = timeStamp,
                    velocity = rb.linearVelocity,
                    deltaTime = step
                };
                points.Add(point);
                MessageServices.V1.Raise<MsgPhysicsSimUpdate, SimulationTransformData>(point);
                if (numFrames > 0 && i % Mathf.CeilToInt(steps / (float)numFrames) == 0 && i + 1 < steps)
                {
                    await UniTask.Yield();
                }
            }

            _isSimulatingPhysics = false;
            stopwatch.Stop();
            Physics.simulationMode = SimulationMode.FixedUpdate;
            return points;
        }

        public static void CancelSimulation()
        {
            _isSimulatingPhysics = false;
        }

        static int _lineIndex = 0;

        public static List<SimulationTransformData> SimulatePhysics(GameObject trackedObject, float maxTime, float heightThreshold = 0)
        {
            Physics.simulationMode = SimulationMode.Script;
            var rb = trackedObject.GetComponent<Rigidbody>();

            float step = Time.fixedDeltaTime; // usually 0.02f
            int maxSteps = Mathf.CeilToInt(maxTime / step);

            float timeStamp = 0;
            List<SimulationTransformData> points = new List<SimulationTransformData>();

            for (int i = 0; i < maxSteps; i++)
            {
                Physics.Simulate(step);
                timeStamp += step;
                points.Add(new SimulationTransformData
                {
                    pos = trackedObject.Pos(),
                    time = timeStamp,
                    velocity = rb?.linearVelocity ?? Vector3.zero,
                    deltaTime = step
                });
            }

            _lineIndex++;

            Physics.simulationMode = SimulationMode.FixedUpdate;
            return points;
        }

    }
}
