using UnityEngine;

public class DashMonster : MonoBehaviour
{
    public float hp = 100f;

    public void Update()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
