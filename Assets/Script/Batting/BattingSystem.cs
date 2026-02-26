using UnityEngine;

public class BattingSystem : MonoBehaviour
{
    private bool _canSwing = true;
    private bool _isSwinging = false;

    private void Awake()
    {
        _canSwing = false;
        _isSwinging = false;
    }

    private void Update()
    {
        if (_canSwing && Input.GetMouseButtonDown(0) && !_isSwinging)
        {
            _isSwinging = true;
            _canSwing = false;
        }
    }

    public void StartBattingCalculate()
    {
    }

    /// <summary>
    /// ボールがリリースされたときの処理
    /// </summary>
    private void ReleasedBall(PitchBallMove ball)
    {
        Debug.Log("[BattingSystem] Ball Released - Swing is now allowed.");
        _canSwing = true;
        _isSwinging = false;
    }
}
