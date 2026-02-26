using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class BattingBallMove : BallMoveTrajectory
{    
    [Tooltip("ファールになった時の表示時間(ms)"),SerializeField] private int _foulBallDisplayTime = 6000;

    private bool _hasLanded = false;
    private BattingBallResult _result;

    public event Action<BattingBallMove> OnBallLanded;

    public bool IsMoving => _isMoving;
    public bool HasLanded => _hasLanded;
    public BattingBallResult Result => _result;

    protected override void Update()
    {
        base.Update();

        if (!_isMoving) return;

        if (_result.LandingPosition == transform.position)
        {
            if (!_hasLanded)
            {
                _hasLanded = true;
                _isMoving = false;
                OnBallLanded?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// 打撃結果受信
    /// </summary>
    private void OnResultReceived(BattingBallResult result)
    {
        _result = result;
        _trajectory = result.TrajectoryPoints;

        if (result.BallType == BattingBallType.Miss)
            return;

        if (_trajectory == null || _trajectory.Count < 2)
        {
            Debug.LogError("[BattingBall] 軌道未設定");
            return;
        }

        if (_result.BallType == BattingBallType.Foul)
        {
            _ = WaitFoulBallAsync();
        }

        _elapsedTime = 0f;
        _isMoving = false;
        _hasLanded = false;

        transform.position = _trajectory[0];

        StartMoving();
    }

    private async UniTaskVoid WaitFoulBallAsync()
    {
        await UniTask.Delay(_foulBallDisplayTime);
        _isMoving = false;
    }

    private void StartMoving()
    {
        _elapsedTime = 0f;
        _isMoving = true;
        _hasLanded = false;
    }    

    /// <summary>
    /// 見た目速度変更
    /// </summary>
    public void SetVisualSpeed(float multiplier)
    {
        _visualSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    protected override void ApplySpin()
    {
        _result.SpinAxis.Normalize();
        float deg = _result.SpinRate * 360f / 60f * _spinSpeedMultiplier;
    }

    /// <summary>
    /// 軌道終了到達
    /// </summary>
    protected override void OnReachedEnd()
    {

    }
}
