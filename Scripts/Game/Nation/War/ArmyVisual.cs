using UnityEngine;

public class ArmyVisual : MonoBehaviour
{
    public void SetPosition(Vector2 pos)
    {
        transform.localPosition = pos;
    }

    public void LookAtTarget(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}