using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    /// <summary>
    /// Wrapper class for managing layout rebuilding of CanvasElement.
    /// </summary>
    public class LayoutRebuilder : ICanvasElement
    {
        private RectTransform m_ToRebuild;
        //There are a few of reasons we need to cache the Hash fromt he transform:
        //  - This is a ValueType (struct) and .Net calculates Hash from the Value Type fields.
        //  - The key of a Dictionary should have a constant Hash value.
        //  - It's possible for the Transform to get nulled from the Native side.
        // We use this struct with the IndexedSet container, which uses a dictionary as part of it's implementation
        // So this struct gets used as a key to a dictionary, so we need to guarantee a constant Hash value.
        private int m_CachedHashFromTransform;

        static ObjectPool<LayoutRebuilder> s_Rebuilders = new ObjectPool<LayoutRebuilder>(() => new LayoutRebuilder(), null, x => x.Clear());



        //代理函数
        static UnityAction<Component> s_PerformLayoutCalculation_Horizontal_Action = PerformLayoutCalculation_Horizontal_Action;
        static UnityAction<Component> s_PerformLayoutControl_Horizontal_Action = PerformLayoutControl_Horizontal_Action;
        static UnityAction<Component> s_PerformLayoutCalculation_Vertical_Action = PerformLayoutCalculation_Vertical_Action;
        static UnityAction<Component> s_PerformLayoutControl_Vertical_Action = PerformLayoutControl_Vertical_Action;

        //当前帧的计算列表
        static HashSet<RectTransform> s_HashHoriRectTransform = new HashSet<RectTransform>();
        static HashSet<RectTransform> s_HashHoriCtrlRectTransform = new HashSet<RectTransform>();
        static HashSet<RectTransform> s_HashVercticalRectTransform = new HashSet<RectTransform>();
        static HashSet<RectTransform> s_HashVercticalCtrlRectTransform = new HashSet<RectTransform>();

        private void Initialize(RectTransform controller)
        {
            m_ToRebuild = controller;
            m_CachedHashFromTransform = controller.GetHashCode();
        }

        private void Clear()
        {
            m_ToRebuild = null;
            m_CachedHashFromTransform = 0;
        }

        static LayoutRebuilder()
        {
            RectTransform.reapplyDrivenProperties += ReapplyDrivenProperties;
        }

        static void ReapplyDrivenProperties(RectTransform driven)
        {
            Graphic gp = driven.GetComponent<Graphic>();
            if(null!= gp&&gp.HadLayout()==false)
            {
                //Debug.LogError("跳过:ReapplyDrivenProperties driven="+ driven.name);
                return;
            }

            MarkLayoutForRebuild(driven, gp);
        }

        public Transform transform { get { return m_ToRebuild; }}

        /// <summary>
        /// Has the native representation of this LayoutRebuilder been destroyed?
        /// </summary>
        public bool IsDestroyed()
        {
            return m_ToRebuild == null;
        }

        static void StripDisabledBehavioursFromList(List<ILayoutController> components)
        {
            components.RemoveAll(e => e is Behaviour && !((Behaviour)e).isActiveAndEnabled);
        }

        static void StripDisabledBehavioursFromList(List<ILayoutElement> components)
        {
            components.RemoveAll(e => e is Behaviour && !((Behaviour)e).isActiveAndEnabled);
        }

        /// <summary>
        /// Forces an immediate rebuild of the layout element and child layout elements affected by the calculations.
        /// </summary>
        /// <param name="layoutRoot">The layout element to perform the layout rebuild on.</param>
        /// <remarks>
        /// Normal use of the layout system should not use this method. Instead MarkLayoutForRebuild should be used instead, which triggers a delayed layout rebuild during the next layout pass. The delayed rebuild automatically handles objects in the entire layout hierarchy in the correct order, and prevents multiple recalculations for the same layout elements.
        /// However, for special layout calculation needs, ::ref::ForceRebuildLayoutImmediate can be used to get the layout of a sub-tree resolved immediately. This can even be done from inside layout calculation methods such as ILayoutController.SetLayoutHorizontal orILayoutController.SetLayoutVertical. Usage should be restricted to cases where multiple layout passes are unavaoidable despite the extra cost in performance.
        /// </remarks>
        public static void ForceRebuildLayoutImmediate(RectTransform layoutRoot)
        {
            var rebuilder = s_Rebuilders.Get();
            rebuilder.Initialize(layoutRoot);
            rebuilder.Rebuild(CanvasUpdate.Layout);
            s_Rebuilders.Release(rebuilder);
        }

        public static void ClearRebuildItems()
        {
            s_HashHoriRectTransform.Clear();
            s_HashVercticalRectTransform.Clear();
            s_HashHoriCtrlRectTransform.Clear();
            s_HashVercticalCtrlRectTransform.Clear();
        }
             

        public void Rebuild(CanvasUpdate executing)
        {
            switch (executing)
            {
                case CanvasUpdate.Layout:
                    // It's unfortunate that we'll perform the same GetComponents querys for the tree 2 times,
                    // but each tree have to be fully iterated before going to the next action,
                    // so reusing the results would entail storing results in a Dictionary or similar,
                    // which is probably a bigger overhead than performing GetComponents multiple times.
                    PerformLayoutCalculation(m_ToRebuild, s_PerformLayoutCalculation_Horizontal_Action,s_HashHoriRectTransform);
                    PerformLayoutControl(m_ToRebuild, s_PerformLayoutControl_Horizontal_Action, s_HashHoriCtrlRectTransform);
                    PerformLayoutCalculation(m_ToRebuild, s_PerformLayoutCalculation_Vertical_Action, s_HashVercticalRectTransform);
                    PerformLayoutControl(m_ToRebuild, s_PerformLayoutControl_Vertical_Action, s_HashVercticalCtrlRectTransform);
                    break;
            }
        }

        static private void PerformLayoutCalculation_Horizontal_Action(Component e)
        {
            (e as ILayoutElement).CalculateLayoutInputHorizontal();
        }

        static private void PerformLayoutCalculation_Vertical_Action(Component e)
        {
            (e as ILayoutElement).CalculateLayoutInputVertical();
        }


        static private void PerformLayoutControl_Horizontal_Action(Component e)
        {
            (e as ILayoutController).SetLayoutHorizontal();
        }

        static private void PerformLayoutControl_Vertical_Action(Component e)
        {
            (e as ILayoutController).SetLayoutVertical();
        }


        private void PerformLayoutControl(RectTransform rect, UnityAction<Component> action, HashSet<RectTransform> hashRTs)
        {
            if (rect == null || hashRTs.Contains(rect))
                return;

            hashRTs.Add(rect);

            var components = ListPool<ILayoutController>.Get();
            rect.GetComponents<ILayoutController>(components);
           

            // If there are no controllers on this rect we can skip this entire sub-tree
            // We don't need to consider controllers on children deeper in the sub-tree either,
            // since they will be their own roots.
            if (components.Count > 0)
            {
                StripDisabledBehavioursFromList(components);

                // Layout control needs to executed top down with parents being done before their children,
                // because the children rely on the sizes of the parents.

                // First call layout controllers that may change their own RectTransform
                for (int i = 0; i < components.Count; i++)
                    if (components[i] is ILayoutSelfController)
                        action(components[i] as Component);

                // Then call the remaining, such as layout groups that change their children, taking their own RectTransform size into account.
                for (int i = 0; i < components.Count; i++)
                    if (!(components[i] is ILayoutSelfController))
                    {
                        var scrollRect = components[i] as Component;

                        if (scrollRect && scrollRect is UnityEngine.UI.ScrollRect)
                        {
                            if (((UnityEngine.UI.ScrollRect)scrollRect).content != rect)
                                action(scrollRect);
                        }
                        else
                        {
                            action(scrollRect);
                        }
                    }

                for (int i = 0; i < rect.childCount; i++)
                    PerformLayoutControl(rect.GetChild(i) as RectTransform, action, hashRTs);
            }

            ListPool<ILayoutController>.Release(components);
        }

        private void PerformLayoutCalculation(RectTransform rect, UnityAction<Component> action, HashSet<RectTransform> hashRTs)
        {
            if (rect == null|| hashRTs.Contains(rect))
                return;

            hashRTs.Add(rect);

            var components = ListPool<ILayoutElement>.Get();
            rect.GetComponents<ILayoutElement>(components); //typeof(ILayoutElement)
            

            // If there are no controllers on this rect we can skip this entire sub-tree
            // We don't need to consider controllers on children deeper in the sub-tree either,
            // since they will be their own roots.
            if (components.Count > 0  || rect.GetComponent<ILayoutGroup>()!=null)
            {
                StripDisabledBehavioursFromList(components);

                // Layout calculations needs to executed bottom up with children being done before their parents,
                // because the parent calculated sizes rely on the sizes of the children.

                for (int i = 0; i < rect.childCount; i++)
                    PerformLayoutCalculation(rect.GetChild(i) as RectTransform, action, hashRTs);

                for (int i = 0; i < components.Count; i++)
                    action(components[i] as Component);
            }

            ListPool<ILayoutElement>.Release(components);
        }

        /// <summary>
        /// Mark the given RectTransform as needing it's layout to be recalculated during the next layout pass.
        /// </summary>
        /// <param name="rect">Rect to rebuild.</param>
        public static bool MarkLayoutForRebuild(RectTransform rect, Graphic grp = null)
        {
            if (rect == null || rect.gameObject == null)
                return false;


            CanvasUpdateRegistry.AddLayoutRectTransform(rect, grp);
            return true;
        }

        public static bool RealMarkLayoutForRebuild(RectTransform rect)
        {
            var comps = ListPool<Component>.Get();
            bool validLayoutGroup = true;
            RectTransform layoutRoot = rect;
            var parent = layoutRoot.parent as RectTransform;
            while (validLayoutGroup && !(parent == null || parent.gameObject == null))
            {
                validLayoutGroup = false;
                parent.GetComponents(typeof(ILayoutGroup), comps);

                for (int i = 0; i < comps.Count; ++i)
                {
                    var cur = comps[i];
                    if (cur != null && cur is Behaviour && ((Behaviour)cur).isActiveAndEnabled)
                    {
                        validLayoutGroup = true;
                        layoutRoot = parent;
                        break;
                    }
                }

                parent = parent.parent as RectTransform;
            }

            // We know the layout root is valid if it's not the same as the rect,
            // since we checked that above. But if they're the same we still need to check.
            if (layoutRoot == rect && !ValidController(layoutRoot, comps))
            {
                ListPool<Component>.Release(comps);
                return false;
            }

            MarkLayoutRootForRebuild(layoutRoot);
            ListPool<Component>.Release(comps);
            return true;
        }

        private static bool ValidController(RectTransform layoutRoot, List<Component> comps)
        {
            if (layoutRoot == null || layoutRoot.gameObject == null)
                return false;

            layoutRoot.GetComponents(typeof(ILayoutController), comps);
            for (int i = 0; i < comps.Count; ++i)
            {
                var cur = comps[i];
                if (cur != null && cur is Behaviour && ((Behaviour)cur).isActiveAndEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static void MarkLayoutRootForRebuild(RectTransform controller)
        {
            if (controller == null)
                return;

            var rebuilder = s_Rebuilders.Get();
            rebuilder.Initialize(controller);
            if (!CanvasUpdateRegistry.TryRegisterCanvasElementForLayoutRebuild(rebuilder))
                s_Rebuilders.Release(rebuilder);
        }

        public void LayoutComplete()
        {
            s_Rebuilders.Release(this);
        }

        public void GraphicUpdateComplete()
        {}

        public override int GetHashCode()
        {
            return m_CachedHashFromTransform;
        }

        /// <summary>
        /// Does the passed rebuilder point to the same CanvasElement.
        /// </summary>
        /// <param name="obj">The other object to compare</param>
        /// <returns>Are they equal</returns>
        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }

        public override string ToString()
        {
            return "(Layout Rebuilder for) " + m_ToRebuild;
        }
    }
}
