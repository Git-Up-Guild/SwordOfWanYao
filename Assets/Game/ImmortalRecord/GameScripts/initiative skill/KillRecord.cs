using UnityEngine;
public class KillRecord : MonoBehaviour
{
    public static KillRecord Instance { get; private set; }

    private int killCount = 0;
    public void AddKill(int count = 1)
    {
        killCount += count;
    }

    public int GetKillCount()
    {
        return killCount;
    }
}
