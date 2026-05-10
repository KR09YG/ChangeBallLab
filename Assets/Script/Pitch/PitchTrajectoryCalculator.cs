using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// “Š‹…‹O“¹‚ÌŒvZ‚ğ’S“–‚·‚éÃ“IƒNƒ‰ƒX
/// </summary>
public static class PitchTrajectoryCalculator
{
    /// <summary>
    /// “Š‹…‹O“¹‚ğŒvZ
    /// </summary>
    public static List<Vector3> PitchCalculate(
        BallData ballData,
        Vector3 releasePoint,
        Vector3 passPoint,
        float stopZ,
        bool enableDebugLogs = false,
        TrajectoryDebugger debugger = null)
    {
        if (enableDebugLogs)
        {
            Debug.Log("========== ‹O“¹ŒvZŠJn ==========");
            Debug.Log($"Release: {releasePoint}");
            Debug.Log($"PassPoint: {passPoint}");
            Debug.Log($"StopZ: {stopZ}");
            Debug.Log($"‹…í: {ballData.Name}");
        }

        var request = new PitchRequest
        {
            BallData = ballData,
            ReleasePoint = releasePoint,
            PassPoint = passPoint,
            StopZ = stopZ
        };

        List<Vector3> trajectory = BallPhysicsCalculator.CalculateTrajectory(request);

        if (enableDebugLogs)
        {
            LogTrajectoryResults(trajectory);
        }

        if (debugger != null)
        {
            debugger.SetTrajectory(trajectory);
        }

        if (enableDebugLogs)
        {
            Debug.Log("========== ‹O“¹ŒvZŠ®—¹ ==========");
        }

        return trajectory;
    }

    /// <summary>
    /// ‹O“¹‚Ì‘•Ï‰»—Ê‚ğŒvZ
    /// </summary>
    public static float CalculateTotalCurve(List<Vector3> trajectory)
    {
        if (trajectory == null || trajectory.Count < 3)
        {
            return 0f;
        }

        Vector3 start = trajectory[0];
        Vector3 end = trajectory[trajectory.Count - 1];
        Vector3 straightLine = end - start;
        float maxDeviation = 0f;

        for (int i = 1; i < trajectory.Count - 1; i++)
        {
            Vector3 point = trajectory[i];
            float deviation = Vector3.Cross(straightLine, point - start).magnitude / straightLine.magnitude;
            if (deviation > maxDeviation)
            {
                maxDeviation = deviation;
            }
        }

        return maxDeviation;
    }

    /// <summary>
    /// ‹O“¹ŒvZŒ‹‰Ê‚ğƒƒOo—Í
    /// </summary>
    private static void LogTrajectoryResults(List<Vector3> trajectory)
    {
        Debug.Log($"‹O“¹ƒ|ƒCƒ“ƒg”: {trajectory.Count}");
        if (trajectory.Count > 0)
        {
            Debug.Log($"‹O“¹ŠJn“_: {trajectory[0]}");
            Debug.Log($"‹O“¹I“_: {trajectory[trajectory.Count - 1]}");
            float curveAmount = CalculateTotalCurve(trajectory);
            Debug.Log($"•Ï‰»—Ê: {curveAmount * 100f:F2}cm");
        }
    }
}