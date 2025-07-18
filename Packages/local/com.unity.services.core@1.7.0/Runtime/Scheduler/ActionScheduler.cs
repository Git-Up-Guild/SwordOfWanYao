using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using Unity.Services.Core.Internal;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Scheduler.Internal
{
    class ActionScheduler : IActionScheduler
    {
        readonly ITimeProvider m_TimeProvider;

        /// <remarks>
        /// Members requiring thread safety:
        /// * <see cref="m_NextId"/>.
        /// * <see cref="m_ScheduledActions"/>.
        /// * <see cref="m_IdScheduledInvocationMap"/>.
        /// </remarks>
        readonly object m_Lock = new object();

        readonly MinimumBinaryHeap<ScheduledInvocation> m_ScheduledActions
            = new MinimumBinaryHeap<ScheduledInvocation>(new ScheduledInvocationComparer());

        readonly Dictionary<long, ScheduledInvocation> m_IdScheduledInvocationMap
            = new Dictionary<long, ScheduledInvocation>();

        const long k_MinimumIdValue = 1;

        internal readonly PlayerLoopSystem SchedulerLoopSystem;

        long m_NextId = k_MinimumIdValue;


        static Stack<ScheduledInvocation> m_stackPool = new Stack<ScheduledInvocation>();

        public ActionScheduler()
            : this(new UtcTimeProvider()) { }

        public ActionScheduler(ITimeProvider timeProvider)
        {
            m_TimeProvider = timeProvider;
            SchedulerLoopSystem = new PlayerLoopSystem
            {
                type = typeof(ActionScheduler),
                updateDelegate = ExecuteExpiredActions
            };
        }

        public int ScheduledActionsCount => m_ScheduledActions.Count;

        public long ScheduleAction([NotNull] Action action, double delaySeconds = 0)
        {
            if (delaySeconds < 0)
            {
                throw new ArgumentException("delaySeconds can not be negative");
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (m_Lock)
            {
                var scheduledInvocation = CreateScheduledInvocation();
                scheduledInvocation.Action = action;
                scheduledInvocation.InvocationTime = m_TimeProvider.Now.AddSeconds(delaySeconds);
                scheduledInvocation.ActionId = m_NextId++;

                /*
                new ScheduledInvocation
                {
                    Action = action,
                    InvocationTime = m_TimeProvider.Now.AddSeconds(delaySeconds),
                    ActionId = m_NextId++
                };
                */

                if (m_NextId < k_MinimumIdValue)
                {
                    m_NextId = k_MinimumIdValue;
                }

                m_ScheduledActions.Insert(scheduledInvocation);
                m_IdScheduledInvocationMap.Add(scheduledInvocation.ActionId, scheduledInvocation);

                return scheduledInvocation.ActionId;
            }
        }

        public void CancelAction(long actionId)
        {
            lock (m_Lock)
            {
                if (!m_IdScheduledInvocationMap.TryGetValue(actionId, out var scheduledInvocation))
                {
                    return;
                }

                m_ScheduledActions.Remove(scheduledInvocation);
                m_IdScheduledInvocationMap.Remove(scheduledInvocation.ActionId);
            }
        }

        List<ScheduledInvocation> scheduledInvocationList = new List<ScheduledInvocation>();
        internal void ExecuteExpiredActions()
        {

            lock (m_Lock)
            {
                scheduledInvocationList.Clear();
                while (m_ScheduledActions.Count > 0
                       && m_ScheduledActions.Min?.InvocationTime <= m_TimeProvider.Now)
                {
                    var scheduledInvocation = m_ScheduledActions.ExtractMin();
                    scheduledInvocationList.Add(scheduledInvocation);
                    m_ScheduledActions.Remove(scheduledInvocation);
                    m_IdScheduledInvocationMap.Remove(scheduledInvocation.ActionId);
                }
            }

            foreach (var scheduledInv in scheduledInvocationList)
            {
                try
                {
                    scheduledInv.Action();
                    RecycleScheduledInvocation(scheduledInv);
                }
                catch (Exception e)
                {
                    CoreLogger.LogException(e);
                }
            }

            //scheduledInvocationList.Clear();
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearActionSchedulerFromPlayerLoop()
        {
            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var currentSubSystems = new List<PlayerLoopSystem>(currentPlayerLoop.subSystemList);
            for (var i = currentSubSystems.Count - 1; i >= 0; i--)
            {
                if (currentSubSystems[i].type == typeof(ActionScheduler))
                {
                    currentSubSystems.RemoveAt(i);
                }
            }

            UpdateSubSystemList(currentSubSystems, currentPlayerLoop);
        }

#endif

        static void UpdateSubSystemList(List<PlayerLoopSystem> subSystemList, PlayerLoopSystem currentPlayerLoop)
        {
            currentPlayerLoop.subSystemList = subSystemList.ToArray();
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
        }

        public void JoinPlayerLoopSystem()
        {
            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var currentSubSystems = new List<PlayerLoopSystem>(currentPlayerLoop.subSystemList);
            if (!currentSubSystems.Contains(SchedulerLoopSystem))
            {
                currentSubSystems.Add(SchedulerLoopSystem);
                UpdateSubSystemList(currentSubSystems, currentPlayerLoop);
            }
        }

        public void QuitPlayerLoopSystem()
        {
            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var currentSubSystems = new List<PlayerLoopSystem>(currentPlayerLoop.subSystemList);
            currentSubSystems.Remove(SchedulerLoopSystem);
            UpdateSubSystemList(currentSubSystems, currentPlayerLoop);
        }

        static public ScheduledInvocation CreateScheduledInvocation()
        {
            ScheduledInvocation si = null;
            if (m_stackPool.Count > 0)
            {
                si = m_stackPool.Pop();
            }
            else
            {
                si = new ScheduledInvocation();
            }

            return si;
        }

        static public void RecycleScheduledInvocation(ScheduledInvocation si)
        {
            if (si != null && m_stackPool.Count < 10)
                m_stackPool.Push(si);
        }
    }
}
