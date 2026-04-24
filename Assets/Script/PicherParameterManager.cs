using TMPro;
using UnityEngine;

public class PicherParameter : PointCostCalculator
{
    [Header("Reference")]
    [SerializeField] private TextMeshProUGUI _pointText;
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _rotationText;
    [SerializeField] private TextMeshProUGUI _controlText;
    [SerializeField] private PitcherBuild _pitcherBuild;

    private void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        _pointText.text = $"Point: {_pitcherBuild.CurrentPoint}";
        _speedText.text = $"Speed: {_pitcherBuild.CurrentSpeed}";
        _rotationText.text = $"Rotation: {_pitcherBuild.CurrentRotation}";
        _controlText.text = $"Control: {_pitcherBuild.CurrentControl}";
    }

    public void SpeedSet(int speed)
    {
        _pitcherBuild.SetPoint(
            PointCalculate(_pitcherBuild.DefaultSpeed, _pitcherBuild.CurrentSpeed, speed));
        _pitcherBuild.SetSpeed(speed);
        UpdateText();
    }

    public void ControlSet(int control)
    {
        _pitcherBuild.SetControl(control);
        _pitcherBuild.SetPoint(
            PointCalculate(_pitcherBuild.DefaultControl, _pitcherBuild.CurrentControl, control));
        UpdateText();
    }

    public void RotationSet(int rotation)
    {
        _pitcherBuild.SetPoint(
            PointCalculate(_pitcherBuild.DefaultRotation, _pitcherBuild.CurrentRotation, rotation));
        _pitcherBuild.SetRotation(rotation);
        UpdateText();
    }

    protected override int PointCalculate(float defaultParam, float currentParam, int delta)
    {
        return base.PointCalculate(defaultParam, currentParam, delta);
    }
}
