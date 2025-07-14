using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InitiativeSkillManager : MonoBehaviour
{
    public static InitiativeSkillManager Instance { get; private set; }

    [Header("能量值")]
    public int energy = 0;
    public int maxEnergy = 100;
    public Slider energySlider;
    public Button skillButton;

    [Header("选择点界面")]
    public GameObject selectPanel;
    public Button backButton;

    [Header("光柱动画Prefab")]
    public GameObject lightBeamPrefab; // 用Aseprite导出的Prefab

    private Vector2? leftPoint = null;
    private Vector2? rightPoint = null;
    private bool isSelecting = false;

    void Awake()
    {
        Instance = this;
        skillButton.onClick.AddListener(OnSkillButtonClick);
        if (backButton != null)
            backButton.onClick.AddListener(ExitSelectMode);
        selectPanel.SetActive(false);
        UpdateUI();
    }

    public void AddEnergy(int value)
    {
        energy = Mathf.Min(energy + value, maxEnergy);
        UpdateUI();
    }

    public void OnEnemyKilled()
    {
        AddEnergy(1);
    }

    private void UpdateUI()
    {
        if (energySlider != null)
            energySlider.value = (float)energy / maxEnergy;
        skillButton.interactable = (energy >= maxEnergy);
    }

    private void OnSkillButtonClick()
    {
        if (energy < maxEnergy) return;
        isSelecting = true;
        selectPanel.SetActive(true);
        leftPoint = null;
        rightPoint = null;
    }

    public void OnSelectPoint(Vector2 pos)
    {
        if (!leftPoint.HasValue)
        {
            leftPoint = pos;
        }
        else if (!rightPoint.HasValue)
        {
            rightPoint = pos;
            TryCastSkill();
        }
    }

    private void TryCastSkill()
    {
        if (leftPoint.HasValue && rightPoint.HasValue)
        {
            CastLightBeam(leftPoint.Value, rightPoint.Value);
            energy -= maxEnergy;
            UpdateUI();
            ExitSelectMode();
        }
    }

    public void ExitSelectMode()
    {
        isSelecting = false;
        selectPanel.SetActive(false);
        leftPoint = null;
        rightPoint = null;
    }

    private void CastLightBeam(Vector2 left, Vector2 right)
    {
        // 实例化光柱动画
        var go = Instantiate(lightBeamPrefab);
        go.transform.position = right;
        var dir = (left - right).normalized;
        float length = Vector2.Distance(left, right);
        go.transform.right = dir;
        go.transform.localScale = new Vector3(length, 1, 1);

        // 检测碰撞并转化
        StartCoroutine(BeamCoroutine(go, right, left, 0.5f)); 
    }

    private System.Collections.IEnumerator BeamCoroutine(GameObject beam, Vector2 from, Vector2 to, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            CheckBeamHit(from, to);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(beam);
    }

    private void CheckBeamHit(Vector2 from, Vector2 to)
    {
        var all = GameObject.FindObjectsOfType<SoldierModel>();
        foreach (var enemy in all)
        {
            if (!IsEnemy(enemy)) continue;
            Vector2 pos = enemy.transform.position;
            if (IsPointOnBeam(pos, from, to, 1.0f)) // 1.0f为光柱宽度
            {
                TryConvertEnemy(enemy);
            }
        }
    }

    private bool IsPointOnBeam(Vector2 pos, Vector2 from, Vector2 to, float width)
    {
        float dist = Vector2.Distance(from, to);
        float proj = Vector2.Dot((pos - from), (to - from).normalized);
        if (proj < 0 || proj > dist) return false;
        float perp = Vector2.Distance(pos, from + (to - from).normalized * proj);
        return perp <= width;
    }

    private bool IsEnemy(SoldierModel model)
    {
        // 你可以根据Camp或Type判断
        return model.Camp == SoldierCamp.Enemy;
    }

    private void TryConvertEnemy(SoldierModel enemy)
    {
        SoldierType newType = GetConvertType(enemy.Type);
        if (newType == enemy.Type) return;
        Vector3 pos = enemy.transform.position;
        Destroy(enemy.gameObject);
        SpawnManager.Instance.AddToSpawnList(newType);
        // 立即生成新兵种
        var spawner = GameObject.FindObjectOfType<AllySpawner>();
        if (spawner != null)
            spawner.Spawn(newType).transform.position = pos;
    }

    private SoldierType GetConvertType(SoldierType type)
    {
        switch (type)
        {
            case SoldierType.Swiftbeak: return SoldierType.Blade;
            case SoldierType.DarkArcher: return SoldierType.Archer;
            case SoldierType.Exploder: return SoldierType.FireMage;
            case SoldierType.PlagueHealer: return SoldierType.WindPriest;
            case SoldierType.IronWarlord: return SoldierType.LightMonk;
            default: return type;
        }
    }

    // 供选择界面调用
    public bool IsSelecting() => isSelecting;
}