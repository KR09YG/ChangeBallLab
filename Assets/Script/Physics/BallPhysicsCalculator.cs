// BallPhysicsCalculator.cs
using System.Collections.Generic;
using UnityEngine;

public struct PitchRequest
{
    public BallData BallData;
    public Vector3 ReleasePoint;
    public Vector3 PassPoint;
    public Vector3 ThrowDirection;
    public float StopZ;
    public TrajectorySettings Settings;
    public BounceSettings BounceSettings;
}

public static class BallPhysicsCalculator
{
    private const float KMH_TO_MS = 1f / 3.6f;
    private const float SCALE = 0.6123f;

    public struct SimulationConfig
    {
        public float DeltaTime;
        public float MaxSimulationTime;
        public float? StopAtZ;
        public BounceSettings BounceSettings;
        public string GroundLayer;
    }

    public static List<Vector3> CalculateTrajectory(PitchRequest request)
    {
        Debug.Log("========== 軌道計算開始 ==========");

        float speedMs = request.BallData.Speed * KMH_TO_MS * SCALE;
        float dt = request.Settings?.DeltaTime ?? 0.01f;

        AerodynamicState aero = AerodynamicState.FromBallData(
            request.BallData, speedMs, request.ReleasePoint.y);

        Vector3 initialVelocity = PitchVelocitySolver.FindOptimalVelocity(
            request.ReleasePoint,
            request.PassPoint,
            aero,
            speedMs,
            dt,
            SCALE
        );

        var config = new SimulationConfig
        {
            DeltaTime = dt,
            MaxSimulationTime = request.Settings?.MaxSimulationTime ?? 5f,
            StopAtZ = request.StopZ,
            BounceSettings = request.BounceSettings
        };
        Debug.Log($"[Aero] SpinAxis={aero.SpinAxis}");
        Debug.Log($"[Aero] EffectiveAW={aero.EffectiveAngularVelocity}");
        Debug.Log($"[Aero] Cl={aero.Cl}");
        Debug.Log($"[Initial] velocity={initialVelocity}");
        List<Vector3> trajectory = SimulateTrajectory(
            request.ReleasePoint,
            initialVelocity,
            aero.SpinAxis,
            aero.EffectiveAngularVelocity,
            aero.Cl,
            config,
            SCALE
        );

        if (trajectory.Count > 0)
        {
            int step = Mathf.Max(1, trajectory.Count / 10);
            Debug.Log("=== 軌道確認 ===");
            for (int i = 0; i < trajectory.Count; i += step)
                Debug.Log($"  [{i:D3}] Y={trajectory[i].y:F4}");
            Debug.Log($"  リリースY={request.ReleasePoint.y:F4} 終点Y={trajectory[trajectory.Count - 1].y:F4} 差={trajectory[trajectory.Count - 1].y - request.ReleasePoint.y:F4}m");
        }

        Debug.Log($"  リリース X={request.ReleasePoint.x:F4} Y={request.ReleasePoint.y:F4}");
        Debug.Log($"  終点 X={trajectory[trajectory.Count - 1].x:F4} Y={trajectory[trajectory.Count - 1].y:F4}");
        Debug.Log($"  横変化(X): {trajectory[trajectory.Count - 1].x - request.ReleasePoint.x:F4}m");

        Debug.Log($"[完了] 初速: {initialVelocity} ({speedMs * 3.6f:F1}km/h)");
        Debug.Log("========== 軌道計算完了 ==========");
        return trajectory;
    }

    /// <summary>
    /// SpinTilt/SpinEfficiencyからSpinAxisを計算
    /// Tilt=0°  X+ → マグヌス力上 (ストレート)
    /// Tilt=90° Y+ → マグヌス力左 (シュート方向)
    /// Tilt=180°X- → マグヌス力下 (カーブ)
    /// Tilt=270°Y- → マグヌス力右 (スライダー方向)
    /// </summary>
    public static Vector3 ToSpinAxis(float spinTilt, float spinEfficiency)
    {
        float rad = spinTilt * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * spinEfficiency;
        float y = Mathf.Sin(rad) * spinEfficiency;
        float z = 1.0f - spinEfficiency;
        Vector3 axis = new Vector3(x, y, z);
        return axis.magnitude > 1e-6f ? axis.normalized : Vector3.forward;
    }

    /// <summary>有効回転数ベースでClを動的計算</summary>
    public static float CalcCl(float speedMs, float effectiveRpm)
    {
        float omega = effectiveRpm * BallPhysicsConstants.RPM_TO_RAD_PER_SEC;
        float spinParam = speedMs > 0f
            ? (BallPhysicsConstants.BALL_RADIUS * omega) / speedMs
            : 0f;
        return Mathf.Clamp(
            BallPhysicsConstants.CL_FACTOR_A * spinParam /
            (1f + BallPhysicsConstants.CL_FACTOR_B * spinParam),
            0f, BallPhysicsConstants.CL_MAX);
    }

    /// <summary>Z+方向飛行時の縦方向マグヌス加速度（正=上向き）</summary>
    public static float CalcMagnusVerticalAccel(
        float speedMs, float rpm, float spinTilt, float spinEfficiency)
    {
        float rad = spinTilt * Mathf.Deg2Rad;
        float xComp = Mathf.Cos(rad) * spinEfficiency;
        float cl = CalcCl(speedMs, rpm * spinEfficiency);
        float fMag = 0.5f * BallPhysicsConstants.AIR_DENSITY
            * speedMs * speedMs * BallPhysicsConstants.CROSS_SECTION * cl;
        return fMag * xComp / BallPhysicsConstants.BALL_MASS;
    }

    public static List<Vector3> SimulateTrajectory(
        Vector3 startPosition,
        Vector3 initialVelocity,
        Vector3 spinAxisNormalized,
        float angularVelocity,
        float liftCoefficient,
        SimulationConfig config,
        float scale)
    {
        return BallTrajectorySimulator.SimulateTrajectory(
            startPosition, initialVelocity, spinAxisNormalized,
            angularVelocity, liftCoefficient, config, scale);
    }

    public static Vector3 FindPointAtZ(List<Vector3> trajectory, float targetZ)
    {
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            Vector3 p1 = trajectory[i];
            Vector3 p2 = trajectory[i + 1];
            if ((p1.z <= targetZ && p2.z >= targetZ) ||
                (p1.z >= targetZ && p2.z <= targetZ))
            {
                float t = Mathf.InverseLerp(p1.z, p2.z, targetZ);
                return Vector3.Lerp(p1, p2, t);
            }
        }
        return trajectory.Count > 0 ? trajectory[trajectory.Count - 1] : Vector3.zero;
    }

    public static (List<Vector3> trajectory, string firstGroundLayer) SimulateTrajectoryWithGroundInfo(
        Vector3 startPosition,
        Vector3 initialVelocity,
        Vector3 spinAxisNormalized,
        float angularVelocity,
        float liftCoefficient,
        SimulationConfig config)
    {
        var result = BallTrajectorySimulator.SimulateTrajectoryWithMetadata(
            startPosition, initialVelocity, spinAxisNormalized,
            angularVelocity, liftCoefficient, config, SCALE);
        return (result.Points, result.FirstGroundLayer);
    }
}