using UnityEngine;

public class SkillEventRelay : MonoBehaviour
{
    private IAnimationEventReceiver receiver;

    public void Init(IAnimationEventReceiver skill)
    {
        receiver = skill;
    }

    public void TriggerEvent(string eventName)
    {
        if (receiver == null) 
            CustomLogger.LogWarning("Can't find receiver");

        receiver.OnAnimationEvent(eventName);
    }
}
