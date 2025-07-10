using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    private static EventManager m_instance;
    private Dictionary<string, Action<object>> m_eventDictionary;
    private static bool applicationIsQuitting = false;
    
    //委托映射表 泛型 => 非泛型，同时带source过滤
    private Dictionary<(string eventName, object source, Delegate listener), Action<object>> m_sourceListenerMap
        = new Dictionary<(string, object, Delegate), Action<object>>();
    public static EventManager Instance
    {

        get
        {

            if (applicationIsQuitting)
            {

                return null;

            }

            if (m_instance == null)
            {

                GameObject go = new GameObject("EventManager");
                m_instance = go.AddComponent<EventManager>();
                DontDestroyOnLoad(go);

            }

            return m_instance;
        }

    }

    //单例初始化
    private void Awake() {

        if (m_instance != null && m_instance != this) {

            Destroy(gameObject);
            return;

        }

        m_instance = this;
        DontDestroyOnLoad(gameObject);
        m_eventDictionary = new Dictionary<string, Action<object>>();

    }

    //在unity完全退出前触发
    private void OnApplicationQuit() {

        applicationIsQuitting = true;
        m_instance = null;
        Destroy(gameObject);

    }

    //订阅事件
    public void Subscribe(string eventName, Action<object> listener) {

        if (m_eventDictionary.TryGetValue(eventName, out Action<object> existingEvent)) {

            existingEvent += listener;
            m_eventDictionary[eventName] = existingEvent;

        }

        else {

            m_eventDictionary.Add(eventName, listener);

        }

    }

    public void Subscribe<T>(string eventName, object source, Action<T> listener) where T : IEventData
    {
        Action<object> wrapper = e =>
        {
            if (e is T t && t.Source == source )
                listener?.Invoke(t);
        };

        // 注册 wrapper
        Subscribe(eventName, wrapper);

        // 保存映射
        m_sourceListenerMap[(eventName, source, listener)] = wrapper;

    }

    //取消订阅
    public void Unsubscribe(string eventName, Action<object> listener)
    {

        if (m_eventDictionary.TryGetValue(eventName, out Action<object> existingEvent))
        {

            existingEvent -= listener;

            if (existingEvent == null)
            {

                m_eventDictionary.Remove(eventName);

            }

            else
            {

                m_eventDictionary[eventName] = existingEvent;

            }

        }

    }
    
    public void Unsubscribe<T>(string eventName, object source, Action<T> listener) where T : IEventData
    {

        var key = (eventName, source, (Delegate)listener);

        if (m_sourceListenerMap.TryGetValue(key, out var wrapper))
        {
            Unsubscribe(eventName, wrapper); // 调用原始的非泛型版本
            m_sourceListenerMap.Remove(key);
        }

    }
    

    //事件触发器
    public void TriggerEvent(string eventName, object parameter = null)
    {

        if (m_eventDictionary.TryGetValue(eventName, out Action<object> thisEvent))
        {

            thisEvent?.Invoke(parameter);

        }

    }

}
