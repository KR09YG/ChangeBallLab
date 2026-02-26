using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PitchBallMove : BallMoveTrajectory
{
    private PitchPreset _preset;

    public List<Vector3> Trajectory => _trajectory;
    private MeshRenderer _renderer;

    private void OnAtBatReset()
    {
        if (_renderer == null)
        {
            var renderer = GetComponent<Renderer>();
        }
        else
        {
            _renderer.enabled = false;
        }
    }

    public void Initialize(List<Vector3> trajectory, PitchPreset preset)
    {
        Debug.Log($"{trajectory.Count}点の軌道でボール移動を初期化します");
        _elapsedTime = 0f;
        _trajectoryProgress = 0f; 
        _isMoving = false;
        _trajectory = trajectory;
        _preset = preset;
        transform.position = trajectory[0];
        _isMoving = true;
        if (_renderer == null)
        {
            _renderer = GetComponent<MeshRenderer>();
        }
        _renderer.enabled = true;
    }

    protected override void Update()
    {
        base.Update();
    }

    private void OnBattingInput()
    {
        _isMoving = false;
    }

    private void OnBattingResultEvent(BattingBallResult result)
    {
        if (result.BallType == BattingBallType.Miss)
        {
            _isMoving = true;
        }
    }

    protected override void ApplySpin()
    {
        float deg = _preset.SpinRate * 360f / 60f * _spinSpeedMultiplier;
        transform.Rotate(_preset.NormalizedSpinAxis, deg * Time.deltaTime);
    }

    protected override void OnReachedEnd()
    {
        Debug.Log("PitchBallMove: ボールがターゲットに到達しました");
        _isMoving = false;
        
        _trajectory = null;
        _elapsedTime = 0f;
    }
}
