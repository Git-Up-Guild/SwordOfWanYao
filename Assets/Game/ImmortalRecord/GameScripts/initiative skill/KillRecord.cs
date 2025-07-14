using UnityEngine;
public interface IKillRecordable
{
    // 增加击杀数
    public void AddKill(int count = 1);

    // 获取当前击杀数
    public int GetKillCount();
}


// 示例：每次击杀敌人后记录击杀数+1
public class KillRecord : MonoBehaviour, IKillRecordable
{
    public int killCount = 0;

    public void AddKill(int count = 1)
    {
        killCount += count;
    }

    public int GetKillCount()
    {
        return killCount;
    }
}
