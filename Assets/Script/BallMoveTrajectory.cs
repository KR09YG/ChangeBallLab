using System.Collections.Generic;
using UnityEngine;

public abstract class BallMoveTrajectory : MonoBehaviour
{
    [Header("共通")]
    [SerializeField] protected float _visualSpeedMultiplier = 1.0f;
    [SerializeField] protected bool _enableSpin = true;
    [SerializeField] protected float _spinSpeedMultiplier = 1.0f;

    [Header("軌道再生設定")]
    [SerializeField] protected float _trajectoryDeltaTime = 0.01f;

    protected float _elapsedTime;

    protected List<Vector3> _trajectory;
    protected float _trajectoryProgress;

    protected bool _isMoving;

    protected virtual void Update()
    {
        if (!_isMoving || _trajectory == null) return;
        MoveAlongTrajectory();
        if (_enableSpin) ApplySpin();
    }

    protected void MoveAlongTrajectory()
    {
        // 実時間での経過時間を加算
        _elapsedTime += Time.deltaTime * _visualSpeedMultiplier;

        // 軌道データのインデックスを計算（0.01秒刻み）
        int index = Mathf.FloorToInt(_elapsedTime / _trajectoryDeltaTime);

        if (index >= _trajectory.Count - 1)
        {
            transform.position = _trajectory[^1];
            OnReachedEnd();
            return;
        }

        float remainder = _elapsedTime - (index * _trajectoryDeltaTime);
        float t = Mathf.Clamp01(remainder / _trajectoryDeltaTime);

        transform.position = Vector3.Lerp(_trajectory[index], _trajectory[index + 1], t);
    }

    protected abstract void ApplySpin();
    protected abstract void OnReachedEnd();
}
