using UnityEngine;

public abstract class PointCostCalculator : MonoBehaviour
{
    protected virtual int PointCalculate(float defaultParam, float currentParam, int delta)
    {
        float beforeDistance = Mathf.Abs(currentParam - defaultParam);
        float afterDistance = Mathf.Abs((currentParam + delta) - defaultParam);

        if (afterDistance > beforeDistance)
            return -1;

        if (afterDistance < beforeDistance)
            return 1;

        return 0;
    }
}
