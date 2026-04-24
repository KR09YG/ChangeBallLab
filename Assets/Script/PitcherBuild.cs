using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PitcherBuild : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] PitcherBuildInitialize _pitcherInit;
    [SerializeField] PitcherParameter _pitcherParameter;
    [SerializeField] TextMeshProUGUI _pointText;
    private PitcherParameterStruct _defaultPitcherParameterStruct;
    private PitcherParameterStruct _currentPitcherParameterStruct;
    private List<BallData> _hasBalls = new List<BallData>();
    private int _currentBallCostPoint = 0;

    public int CurrentPoint => _currentPitcherParameterStruct.Point;
    public int DefaultPoint => _defaultPitcherParameterStruct.Point;
    public float CurrentSpeed => _currentPitcherParameterStruct.Speed;
    public float DefaultSpeed => _defaultPitcherParameterStruct.Speed; 
    public float CurrentControl => _currentPitcherParameterStruct.Control;
    public float DefaultControl => _defaultPitcherParameterStruct.Control;
    public float CurrentRotation => _currentPitcherParameterStruct.Rotation;
    public float DefaultRotation => _defaultPitcherParameterStruct.Rotation;

    private void Awake()
    {
        _defaultPitcherParameterStruct = _pitcherParameter.GetParameter();
        _currentPitcherParameterStruct = _defaultPitcherParameterStruct;
    }

    private void PointTextSet()
    {
        _pointText.text = $"Point: {_currentPitcherParameterStruct.Point}";
    }

    public void SetPoint(int point)
    {
        Debug.Log(point);
        _currentPitcherParameterStruct.Point += point;
        PointTextSet();
    }

    public void PitcherBuilding()
    {
        _pitcherInit.RaisedEvent(_hasBalls);
    }

    public void SetSpeed(float speed)
    {
        _currentPitcherParameterStruct.Speed += speed;
    }

    public void SetControl(float control)
    {
        _currentPitcherParameterStruct.Control += control;
    }

    public void SetRotation(float rotation)
    {
        _currentPitcherParameterStruct.Rotation += rotation;
    }

    public bool CanAddBall(BallData ballData)
    {
        int costPoint = ballData.CostPoint;
        return _currentPitcherParameterStruct.Point >= costPoint;
    }

    public void AddBall(BallData ballData)
    {
        _hasBalls.Add(ballData);
        int costPoint = BallCostCalculate.CalculateCost(_hasBalls);
        SetPoint(-costPoint + _currentBallCostPoint);
        _currentBallCostPoint = costPoint;
    }

    public void RemoveBall(BallData ballData)
    {
        _hasBalls.Remove(ballData);
        int costPoint = BallCostCalculate.CalculateCost(_hasBalls);
        SetPoint(ballData.CostPoint);
        _currentBallCostPoint = costPoint;
    }
} 
