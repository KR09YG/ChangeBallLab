// PitchVelocitySolver.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

internal static class PitchVelocitySolver
{
    internal static Vector3 FindOptimalVelocity(
        Vector3 startPoint,
        Vector3 passPoint,
        AerodynamicState aero,
        float desiredSpeed,
        float dt,
        float scale)
    {
        Debug.Log("[初速計算] 開始");

        float ratio = aero.Ratio;
        float wApex, wPass;
        if (ratio > 1.0f)
        {
            wApex = BallPhysicsConstants.COST_W_APEX_RISING;
            wPass = BallPhysicsConstants.COST_W_PASS_RISING;
        }
        else if (ratio < -0.2f)
        {
            wApex = BallPhysicsConstants.COST_W_APEX_ARC;
            wPass = BallPhysicsConstants.COST_W_PASS_DEFAULT;
        }
        else
        {
            wApex = BallPhysicsConstants.COST_W_APEX_FLAT;
            wPass = BallPhysicsConstants.COST_W_PASS_DEFAULT;
        }

        // === グリッドサーチ ===
        var candidates = new List<(float cost, float pitch, float yaw, float speed)>();

        int speedSteps = BallPhysicsConstants.GRID_SPEED_STEPS;
        int pitchSteps = BallPhysicsConstants.GRID_PITCH_STEPS;
        int yawSteps = BallPhysicsConstants.GRID_YAW_STEPS;

        for (int si = 0; si < speedSteps; si++)
        {
            float speed = desiredSpeed + Mathf.Lerp(
                -BallPhysicsConstants.GRID_SPEED_RANGE,
                 BallPhysicsConstants.GRID_SPEED_RANGE,
                speedSteps > 1 ? (float)si / (speedSteps - 1) : 0.5f);

            for (int pi = 0; pi < pitchSteps; pi++)
            {
                float pitch = aero.PitchCenterDeg + Mathf.Lerp(
                    -BallPhysicsConstants.GRID_PITCH_RANGE,
                     BallPhysicsConstants.GRID_PITCH_RANGE,
                    (float)pi / (pitchSteps - 1));

                for (int yi = 0; yi < yawSteps; yi++)
                {
                    float yaw = Mathf.Lerp(
                        -BallPhysicsConstants.GRID_YAW_RANGE,
                         BallPhysicsConstants.GRID_YAW_RANGE,
                        (float)yi / (yawSteps - 1));

                    Vector3 vel = VelocityFromAngles(speed, pitch, yaw);
                    Vector3 posAtZ = SimulateToZ(startPoint, vel, aero, passPoint.z, dt, scale);
                    Vector3 error = passPoint - posAtZ;
                    float errorXY = new Vector2(error.x, error.y).magnitude;

                    float maxY = CalcMaxY(startPoint, vel, aero, passPoint.z, dt);
                    float apexDiff = maxY - aero.DesiredApexY;
                    float cost = wPass * errorXY * errorXY + wApex * apexDiff * apexDiff;

                    candidates.Add((cost, pitch, yaw, speed));
                }
            }
        }

        candidates.Sort((a, b) => a.cost.CompareTo(b.cost));

        // === 上位候補を収束させてapex条件で選ぶ ===
        Vector3 bestVel = VelocityFromAngles(desiredSpeed, aero.PitchCenterDeg, 0f);
        float bestApex = aero.DesiredApexY > startPoint.y ? -float.MaxValue : float.MaxValue;
        float bestErr = float.MaxValue;

        int topN = Mathf.Min(BallPhysicsConstants.GRID_TOP_CANDIDATES, candidates.Count);
        for (int i = 0; i < topN; i++)
        {
            var (_, pInit, yInit, s) = candidates[i];
            ConvToPassPoint(startPoint, passPoint, aero, s, ref pInit, ref yInit, dt,
                out float err, scale);

            if (err > BallPhysicsConstants.CONVERGE_MAX_ERROR) continue;

            Vector3 vel = VelocityFromAngles(s, pInit, yInit);
            float maxY = CalcMaxY(startPoint, vel, aero, passPoint.z, dt);

            if (aero.DesiredApexY > startPoint.y)
            {
                if (maxY > bestApex)
                {
                    bestApex = maxY; bestErr = err; bestVel = vel;
                }
            }
            else
            {
                if (err < bestErr)
                {
                    bestErr = err; bestApex = maxY; bestVel = vel;
                }
            }
        }

        Debug.Log($"[初速計算] 完了: vel={bestVel} 誤差:{bestErr * 100f:F2}cm");
        return bestVel;
    }

    /// <summary>pitch/yawを数値勾配降下でpassPointに収束させる</summary>
    private static void ConvToPassPoint(
        Vector3 start, Vector3 passPoint, AerodynamicState aero,
        float speed, ref float pitch, ref float yaw, float dt, out float finalErr, float scale, int iterations = 40)
    {
        float lr = BallPhysicsConstants.CONVERGE_LR_INITIAL;
        float d = BallPhysicsConstants.CONVERGE_GRAD_DELTA;
        finalErr = 999f;

        for (int i = 0; i < BallPhysicsConstants.CONVERGE_ITERATIONS; i++)
        {
            Vector3 vel = VelocityFromAngles(speed, pitch, yaw);
            Vector3 posAtZ = SimulateToZ(start, vel, aero, passPoint.z, dt, scale);
            Vector3 error = passPoint - posAtZ;
            finalErr = new Vector2(error.x, error.y).magnitude;
            if (finalErr < BallPhysicsConstants.CONVERGE_TOLERANCE) break;

            Vector3 posDp = SimulateToZ(start, VelocityFromAngles(speed, pitch + d, yaw), aero, passPoint.z, dt, scale);
            Vector3 posDy = SimulateToZ(start, VelocityFromAngles(speed, pitch, yaw + d), aero, passPoint.z, dt, scale);

            Vector3 gradP = (posDp - posAtZ) / d;
            Vector3 gradY = (posDy - posAtZ) / d;

            float dp = Vector3.Dot(error, gradP) / (Vector3.Dot(gradP, gradP) + 1e-6f) * lr;
            float dy = Vector3.Dot(error, gradY) / (Vector3.Dot(gradY, gradY) + 1e-6f) * lr;

            pitch += Mathf.Clamp(dp, -BallPhysicsConstants.CONVERGE_MAX_STEP, BallPhysicsConstants.CONVERGE_MAX_STEP);
            yaw += Mathf.Clamp(dy, -BallPhysicsConstants.CONVERGE_MAX_STEP, BallPhysicsConstants.CONVERGE_MAX_STEP);
            lr *= BallPhysicsConstants.CONVERGE_LR_DECAY;
        }
    }

    /// <summary>passPoint.zまでシミュレーションしてXY位置を返す</summary>
    private static Vector3 SimulateToZ(
        Vector3 start, Vector3 velocity, AerodynamicState aero,
        float targetZ, float dt, float scale)
    {
        Vector3 pos = start;
        Vector3 vel = velocity;
        Vector3 prev = pos;

        for (int i = 0; i < BallPhysicsConstants.MAX_TRAJECTORY_POINTS; i++)
        {
            prev = pos;
            Vector3 drag = BallAerodynamics.CalculateDragForce(vel);
            Vector3 magnus = BallAerodynamics.CalculateMagnusForce(
                vel, aero.SpinAxis, aero.EffectiveAngularVelocity, aero.Cl);
            Vector3 acc = Physics.gravity * scale + (drag + magnus) / BallPhysicsConstants.BALL_MASS; vel += acc * dt;
            pos += vel * dt;

            if ((prev.z - targetZ) * (pos.z - targetZ) <= 0f)
            {
                float t = (targetZ - prev.z) / (pos.z - prev.z);
                return Vector3.Lerp(prev, pos, t);
            }
        }
        return pos;
    }

    /// <summary>passPoint.zまでの最高Y座標を返す</summary>
    private static float CalcMaxY(
        Vector3 start, Vector3 velocity, AerodynamicState aero,
        float targetZ, float dt)
    {
        Vector3 pos = start;
        Vector3 vel = velocity;
        Vector3 prev = pos;
        float maxY = start.y;

        for (int i = 0; i < BallPhysicsConstants.MAX_TRAJECTORY_POINTS; i++)
        {
            prev = pos;
            Vector3 drag = BallAerodynamics.CalculateDragForce(vel);
            Vector3 magnus = BallAerodynamics.CalculateMagnusForce(
                vel, aero.SpinAxis, aero.EffectiveAngularVelocity, aero.Cl);
            Vector3 acc = Physics.gravity + (drag + magnus) / BallPhysicsConstants.BALL_MASS;
            vel += acc * dt;
            pos += vel * dt;

            if (pos.y > maxY) maxY = pos.y;
            if ((prev.z - targetZ) * (pos.z - targetZ) <= 0f) break;
        }
        return maxY;
    }

    internal static void ConvergeFromInitial(
    Vector3 startPoint,
    Vector3 passPoint,
    AerodynamicState aero,
    float desiredSpeed,
    ref float pitch,
    ref float yaw,
    int iterations, float scale)
    {
        // 既存のConvToPassPointと同じロジック、反復数だけ外から指定
        ConvToPassPoint(startPoint, passPoint, aero, desiredSpeed, ref pitch, ref yaw, 0.01f, out _, scale, iterations);
    }

    /// <summary>pitch/yaw角から速度ベクトルを計算</summary>
    private static Vector3 VelocityFromAngles(float speed, float pitchDeg, float yawDeg)
    {
        float p = pitchDeg * Mathf.Deg2Rad;
        float y = yawDeg * Mathf.Deg2Rad;
        return new Vector3(
            speed * Mathf.Cos(p) * Mathf.Sin(y),
            speed * Mathf.Sin(p),
            speed * Mathf.Cos(p) * Mathf.Cos(y)
        );
    }
}