using System;
using UnityEngine;

[Serializable]
public struct BallData
{
    public string Name;
    public float Speed;
    public float RotateSpeed;
    public float Control;
    public int CostPoint;
    public ChangeDirection ChangeDirection;
    public float SpinTilt;
    [Range(0, 1)]
    public float SpinEfficiency;
}

public enum ChangeDirection
{
    Left,
    Right,
    Up,
    Down,
    RightDown,
    LeftDown
}
