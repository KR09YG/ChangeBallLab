// BallPhysicsConstants.cs
using UnityEngine;

internal static class BallPhysicsConstants
{
    // === 物理定数（実寸） ===
    internal const float AIR_DENSITY = 1.225f;
    internal const float BALL_MASS = 0.145f; 
    internal const float BALL_RADIUS = 0.02241f;
    internal const float CROSS_SECTION = Mathf.PI * BALL_RADIUS * BALL_RADIUS;
    internal const float DRAG_COEFFICIENT = 0.3f;
    internal const float GRAVITY_HALF = 0.5f;
    internal const float MAGNUS_FORCE_HALF = 0.5f;
    internal const float DRAG_FORCE_HALF = 0.5f;

    // === 単位変換 ===
    internal const float RPM_TO_RAD_PER_SEC = 2f * Mathf.PI / 60f;

    // === Cl動的計算 ===
    internal const float CL_FACTOR_A = 2.5f;
    internal const float CL_FACTOR_B = 2.0f;
    internal const float CL_MAX = 0.6f;

    // === グリッドサーチ ===
    internal const int GRID_SPEED_STEPS = 3;
    internal const int GRID_PITCH_STEPS = 15;
    internal const int GRID_YAW_STEPS = 9;
    internal const float GRID_SPEED_RANGE = 1.5f;
    internal const float GRID_PITCH_RANGE = 10.0f;
    internal const float GRID_YAW_RANGE = 5.0f;
    internal const int GRID_TOP_CANDIDATES = 8;

    // === 局所収束 ===
    internal const int CONVERGE_ITERATIONS = 40;
    internal const float CONVERGE_GRAD_DELTA = 0.1f;
    internal const float CONVERGE_LR_INITIAL = 0.3f;
    internal const float CONVERGE_LR_DECAY = 0.93f;
    internal const float CONVERGE_MAX_STEP = 1.0f;
    internal const float CONVERGE_TOLERANCE = 0.003f;
    internal const float CONVERGE_MAX_ERROR = 2.0f;

    // === コスト重み ===
    internal const float COST_W_PASS_DEFAULT = 10.0f;
    internal const float COST_W_APEX_RISING = 15.0f;
    internal const float COST_W_APEX_ARC = 10.0f;
    internal const float COST_W_APEX_FLAT = 3.0f;
    internal const float COST_W_PASS_RISING = 8.0f;

    // === apex計算 ===
    internal const float APEX_RATIO_RISING_SCALE = 0.6f;
    internal const float APEX_RATIO_RISING_MAX = 0.8f;
    internal const float APEX_ARC_OFFSET = 0.15f;
    internal const float PITCH_ARC_SCALE = 8.0f;
    internal const float PITCH_ARC_MAX = 10.0f;
    internal const float PITCH_FLAT_SCALE = 2.0f;

    // === シミュレーション ===
    internal const float GROUND_LEVEL = -0.5f;
    internal const int MAX_TRAJECTORY_POINTS = 10000;
    internal const float MIN_VELOCITY_SQUARED = 0.01f;
    internal const float MIN_MAGNUS_DIRECTION_SQUARED = 0.0001f;
    internal const float MIN_DRAG_VELOCITY = 0.001f;
    internal const float NET_EPSILON = 0.001f;
    internal const float DRAG_FACTOR_BASE = 1.0f;
    internal const float DRAG_MASS_FACTOR = 2f;
}