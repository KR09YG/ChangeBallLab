using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PitcherParameter", menuName = "ScriptableObjects/PitcherParameter", order = 1)]
public class PitcherParameter : ScriptableObject
{
    public int Point;
    public float Speed;
    public float Control;
    public float Rotation;
    public PitcherParameterStruct GetParameter()
    {
        return new PitcherParameterStruct(Point, Speed, Control, Rotation);
    }
}

public struct PitcherParameterStruct
{
    public int Point;
    public float Speed;
    public float Control;
    public float Rotation;
    public PitcherParameterStruct(int point, float speed, float control, float rotation)
    {
        Point = point;
        Speed = speed;
        Control = control;
        Rotation = rotation;
    }
}
