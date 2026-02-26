using UnityEngine;

public class StrikeBallJudge : MonoBehaviour
{
    [SerializeField] private StrikeZone _strikeZone;
    [SerializeField] private StrikeZoneUI _strikeZoneUI;

    

    private void OnReleased(PitchBallMove ball)
    {
        if (_strikeZone == null)
        {
            _strikeZone = FindObjectOfType<StrikeZone>();
        }

        if (!BallTrajectoryPredictor.TryGetCrossPointAtZ(
                ball.Trajectory, _strikeZone.CenterZ, out var point))
        {
            Debug.LogWarning("[StrikeBallJudge] ストライクゾーンでの交差点の予測に失敗しました！");
            return;
        }

        bool isStrike = _strikeZone.IsInZone(point);
    }
}
