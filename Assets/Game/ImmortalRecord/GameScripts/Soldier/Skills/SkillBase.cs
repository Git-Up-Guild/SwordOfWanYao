using UnityEngine;

// 抽象技能基类，封装运行时所需的额外信息和释放逻辑
public abstract class SkillBase : IAnimationEventReceiver
{
    protected SoldierModel Model;
    public SoldierSkillDataBase Data { get; }

    protected SkillBase(SoldierModel model, SoldierSkillDataBase data)
    {

        Model = model;
        Data = data;

    }

    // 技能释放方法
    public abstract void Cast(Transform target = null, Vector3 position = default);
    public abstract void OnAnimationEvent(string eventName);
    
    public virtual void InitRelay(Animator animator)
    {

        var animatorGO = animator.gameObject;
        var relay = animatorGO.GetComponent<SkillEventRelay>() ?? animatorGO.AddComponent<SkillEventRelay>();
        relay.Init(this);

    }

}

