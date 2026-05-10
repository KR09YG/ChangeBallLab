using System.Collections.Generic;
using UnityEngine;

public class PitchBallMove : BallMoveTrajectory
{
    public List<Vector3> Trajectory => _trajectory;

    private MeshRenderer _renderer;
    private Vector3 _spinAxis;
    private float _spinRate;

    public void Setup(
        List<Vector3> trajectory,
        float deltaTime,
        Vector3 spinAxis,
        float spinRate)
    {
        Debug.Log($"{trajectory.Count}点の軌道でボール移動を初期化します");

        _elapsedTime = 0f;
        _trajectoryProgress = 0f;
        _isMoving = false;
        _trajectory = trajectory;
        _trajectoryDeltaTime = deltaTime;
        _spinAxis = spinAxis;
        _spinRate = spinRate;

        transform.position = trajectory[0];

        if (_renderer == null)
            _renderer = GetComponent<MeshRenderer>();

        _renderer.enabled = true;

        StartMoving();
    }

    public void StartMoving()
    {
        _isMoving = true;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void ApplySpin()
    {
        float deg = _spinRate * 360f / 60f * _spinSpeedMultiplier;
        transform.Rotate(_spinAxis, deg * Time.deltaTime);
    }

    protected override void OnReachedEnd()
    {
        Debug.Log("PitchBallMove: ボールがターゲットに到達しました");
        _isMoving = false;
        _trajectory = null;
        _elapsedTime = 0f;
    }
}