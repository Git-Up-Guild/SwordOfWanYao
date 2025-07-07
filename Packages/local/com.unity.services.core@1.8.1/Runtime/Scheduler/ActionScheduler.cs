using System;
using System.Collections.Generic;
using UnityEngine.LowLevel;
using Unity.Services.Core.Internal;
using NotNull = JetBrains.Annotations.NotNullAttribute;

namespace Unity.Services.Core.Scheduler.Internal
{
    class ActionScheduler : IActionScheduler
    {
        const long k_MinimumIdValue = 1;

        internal readonly PlayerLoopSystem SchedulerLoopSystem;

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

        readonly List<ScheduledInvocation> m_ExpiredActions = new List<ScheduledInvocation>();

        long m_NextId = k_MinimumIdValue;

        static Stack<ScheduledInvocation> m_stackPool = new Stack<ScheduledInvocation>();

        public ActionScheduler()
            : this(new UtcTimeProvider()) {}

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

                //var scheduledInvocation = new ScheduledInvocation
                //{
                //    Action = action,
                //    InvocationTime = m_TimeProvider.Now.AddSeconds(delaySeconds),
                //    ActionId = m_NextId++
                //};

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

        internal void ExecuteExpiredActions()
        {
            lock (m_Lock)
            {
                m_ExpiredActions.Clear();

                while (m_ScheduledActions.Count > 0
                       && m_ScheduledActions.Min?.InvocationTime <= m_TimeProvider.Now)
                {
                    var expiredAction = m_ScheduledActions.ExtractMin();
                    m_ExpiredActions.Add(expiredAction);
                    m_ScheduledActions.Remove(expiredAction);
                    m_IdScheduledInvocationMap.Remove(expiredAction.ActionId);
                }

                foreach (var expiredAction in m_ExpiredActions)
                {
                    try
                    {
                        expiredAction.Action();
                        RecycleScheduledInvocation(expiredAction);
                    }
                    catch (Exception e)
                    {
                        CoreLogger.LogException(e);
                    }
                }

                m_ExpiredActions.Clear();
            }
        }

        internal static void UpdateCurrentPlayerLoopWith(
            List<PlayerLoopSystem> subSystemList, PlayerLoopSystem currentPlayerLoop)
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
                UpdateCurrentPlayerLoopWith(currentSubSystems, currentPlayerLoop);
            }
        }

        public void QuitPlayerLoopSystem()
        {
            var currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var currentSubSystems = new List<PlayerLoopSystem>(currentPlayerLoop.subSystemList);
            if (currentSubSystems.Remove(SchedulerLoopSystem))
            {
                UpdateCurrentPlayerLoopWith(currentSubSystems, currentPlayerLoop);
            }
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
