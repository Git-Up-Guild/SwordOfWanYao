////编辑器用的测试脚本，因为Unity Assets里没有合适位置便于上传
//#if UNITY_EDITOR

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using rkt;
//using XClient.Common;
//using XClient.UI;
//using UnityEngine;
//using UnityEngine.Events;

//namespace UI.TestUIWindow
//{
//    public class UIWindowTester : MonoBehaviour
//    {
//        public class WindowModelEvent : UnityEvent<WindowModel> { }

//        public UIWindowsModelConfig configs;
//        public List<WindowModel> _windowModels;
//        public WindowModel curMainWindowModel;

//        public string windowModelStr;
//        //public WindowModel CurSubWindowModel;
//        public float intervalTime = 1.5f;
//        public int curIndex;

//        public WindowModelEvent onWindowShow = new WindowModelEvent();
//        public WindowModelEvent onWindowHide = new WindowModelEvent();
//        public UnityEvent onUIWindowTesterStart = new UnityEvent();
//        public UnityEvent onUIWindowTesterFinish = new UnityEvent();

//        private bool wait = true;
//        private float lastWindowCreateTime;
//        public float maxWaitTimeForWindowCreation =3f;

//        private static UIWindowTester _instance;
//        private string logFMT = "##UIWindowTester: Window:{0},Stage:{1},Exception:{2}";

//        private Coroutine _cor;

//        [ContextMenu("Set CurModel By String")]
//        public void SetCurModelByString()
//        {
//            try
//            {
//                curMainWindowModel =
//                    (XClient.Common.WindowModel) System.Enum.Parse(typeof(XClient.Common.WindowModel), windowModelStr);
//            }
//            catch (System.Exception e)
//            {
//                curMainWindowModel = WindowModel.None;
//            }
//        }

//        [ContextMenu("StartTestAll")]
//        public void StartTest()
//        {
//            SetOpInterval();
//            _cor=StartCoroutine(TestWindows());
//        }

//        private void OnTestStart()
//        {
//            if (onUIWindowTesterStart != null)
//            {
//                onUIWindowTesterStart.Invoke();
//            }
//        }

//        [ContextMenu("StartTestCur")]
//        public void StartTestCur()
//        {
//            _windowModels = new List<WindowModel>() { curMainWindowModel };
//            StartTest();
//        }

//        private void SetOpInterval()
//        {
//            waitForPerOp = new WaitForSeconds(intervalTime);
//        }

//        [ContextMenu("StopTest")]
//        public void StopTest()
//        {
//            if (_cor != null)
//            {
//                StopCoroutine(_cor);
//                _cor = null;
//                OnTestFinish();
//            }
//        }

//        private void OnTestFinish()
//        {
//            if (onUIWindowTesterFinish != null)
//            {
//                onUIWindowTesterFinish.Invoke();
//            }
//        }

//        public static UIWindowTester Instance
//        {
//            get
//            {
//                if (!Application.isPlaying)
//                {
//                    Debug.LogError("UIWindowTester 需要游戏运行时使用");
//                    return null;
//                }
//                if (_instance ==null)
//                {
//                    var GO = new GameObject("UIWindowTester");
//                    _instance = GO.AddComponent<UIWindowTester>();
//                }

//                return _instance;
//            }
//        }

//        private WaitForSeconds waitForPerOp;

//        //public void OnGUI()
//        //{
//        //    GUILayout.Label(CurWindowModel.ToString(), "box",GUILayout.Height(Screen.height/7f),GUILayout.Width(Screen.width/6f));
//        //}

//        private IEnumerator TestWindows()
//        {
//            OnTestStart();
//            //if (configs==null)
//            //    yield break;
//            //foreach (var config in configs.WindowsModelConfig)
//            for (int i = 0; i < _windowModels.Count; i++)
//            {
//                var rootWindow = _windowModels[i];
//                curIndex = i;
//                //var rootWindow = config.winModel;
//                curMainWindowModel = rootWindow;
//                if (onWindowShow != null)
//                {
//                    onWindowShow.Invoke(rootWindow);
//                }

//                wait = true;
//                lastWindowCreateTime = Time.realtimeSinceStartup;
//                UIManager.Instance.CreateAsyncWindow(rootWindow, WindowLoadCall);
//                while (wait)
//                {
//                    yield return null;
//                    // 避免等待卡死
//                    if (Time.realtimeSinceStartup - lastWindowCreateTime >= maxWaitTimeForWindowCreation)
//                    {
//                        Debug.LogError(string.Format(logFMT, rootWindow, "CreateAsyncWindow", "Over Max Wait Time！！"));
//                        break;
//                    }
//                }
//                UIWindow window= UIManager.Instance.GetWindow<UIWindow>(rootWindow);
//                if (window == null)
//                {
//                    Debug.LogError(string.Format(logFMT, rootWindow, "GetWindow<UIWindow>", "None"));
//                    continue;
//                }
//                yield return waitForPerOp;

//                #region Check for UISubCommonWindow Tabs
//                {
//                    var uiSubCommonWindow = window as UISubCommonWindow;
//                    if (uiSubCommonWindow != null)
//                    {
//                        FieldInfo fi = typeof(UISubCommonWindow).GetField("commonWin", BindingFlags.NonPublic | BindingFlags.Instance);
//                        if (fi != null)
//                        {
//                            SubCommonWindow subCommonWindow = (SubCommonWindow)fi.GetValue(uiSubCommonWindow);
//                            if (subCommonWindow != null)
//                            {
//                                var func2BtnInfo = subCommonWindow.m_tabToggles.Function2BtnInfo;
//                                //foreach中可能窗口制作人会修改迭代内容func2BtnInfo
//                                List<EnFunctionBtn> funcs = func2BtnInfo.Keys.ToList();
//                                foreach (var func in funcs)
//                                {
//                                    try
//                                    {
//                                        uiSubCommonWindow.ShowTabIndex(func);
//                                    }
//                                    catch (Exception e)
//                                    {
//                                        Debug.LogError(string.Format(logFMT, rootWindow, "uiSubCommonWindow.ShowTabIndex", e));
//                                        window.Hide(true);
//                                        continue;
//                                    }
//                                    yield return waitForPerOp;
//                                }
//                            }
//                        }
//                    }
//                } 
//                #endregion

//                #region Check for UICommonWindow Tabs

//                {
//                    var uiCommonWindow = window as UICommonWindow;
//                    if (uiCommonWindow != null)
//                    {
//                        FieldInfo fi = typeof(UICommonWindow).GetField("commonWin", BindingFlags.NonPublic | BindingFlags.Instance);
//                        if (fi != null)
//                        {
//                            CommonWindow subCommonWindow = (CommonWindow)fi.GetValue(uiCommonWindow);
//                            if (subCommonWindow != null)
//                            {
//                                var func2BtnInfo = subCommonWindow.m_tabToggles.Function2BtnInfo;
//                                List<EnFunctionBtn> funcs = func2BtnInfo.Keys.ToList();
//                                foreach (var func in funcs)
//                                {
//                                    try
//                                    {
//                                        uiCommonWindow.ShowTabIndex(func);
//                                    }
//                                    catch (Exception e)
//                                    {
//                                        Debug.LogError(string.Format(logFMT, rootWindow, "uiCommonWindow.ShowTabIndex", e));
//                                        window.Hide(true);
//                                        continue;
//                                    }
//                                    yield return waitForPerOp;
//                                }
//                            }
//                        }
//                    } 
//                }
//                #endregion

//                try
//                {
//                    window.Hide(true);
//                }
//                catch (Exception e)
//                {
//                    Debug.LogError(string.Format(logFMT, rootWindow, "Hide", e));
//                    continue;
//                }
//                yield return waitForPerOp;

//                if (onWindowHide != null)
//                {
//                    onWindowHide.Invoke(rootWindow);
//                }
//            }
//            _cor = null;
//            OnTestFinish();
//        }

//        void WindowLoadCall(object parentObj, object currentObj)
//        {
//            UIWindow window = (parentObj as GameObject).GetComponent<UIWindow>();
//            if (window == null)
//            {
//                Debug.LogError("WindowLoadCall::GetComponent<UIWindow>()==null,parentObj: " + parentObj);
//                return;
//            }
//            window.Show();
//            wait = false;
//        }


//        ////没法很好通用
//        //IEnumerator ShowTabs<T>(UIWindow window) where T : UIWindow
//        //{
//        //    var uiSubCommonWindow = window as T;
//        //    if (uiSubCommonWindow == null)
//        //    {
//        //        yield break;
//        //    }

//        //    FieldInfo fi = typeof(T).GetField("commonWin", BindingFlags.NonPublic | BindingFlags.Instance);
//        //    if (fi == null)
//        //    {
//        //        Debug.LogError(string.Format(logFMT, uiSubCommonWindow.GetType(), "GetField(commonWin)", null));
//        //        yield break;
//        //    }

//        //    var methodInfo = typeof(T).GetMethod("ShowTabIndex");
//        //    if (methodInfo == null)
//        //    {
//        //        Debug.LogError(string.Format(logFMT, uiSubCommonWindow.GetType(), "GetMethod(ShowTabIndex)", null));
//        //        yield break;
//        //    }

//        //    SubCommonWindow subCommonWindow = (SubCommonWindow) fi.GetValue(uiSubCommonWindow);
//        //    if (subCommonWindow == null)
//        //    {
//        //        Debug.LogError(string.Format(logFMT, uiSubCommonWindow.GetType(), "GetMethod(ShowTabIndex)", null));
//        //        yield break;
//        //    }

//        //    var func2BtnInfo = subCommonWindow.m_tabToggles.Function2BtnInfo;
//        //    List<EnFunctionBtn> funcs = func2BtnInfo.Keys.ToList();
//        //    foreach (var func in funcs)
//        //    {
//        //        try
//        //        {
//        //            methodInfo.Invoke(uiSubCommonWindow, new object[] {func});
//        //        }
//        //        catch (Exception e)
//        //        {
//        //            Debug.LogError(string.Format(logFMT, uiSubCommonWindow.GetType(), "ShowTabIndex", e));
//        //            window.Hide(true);
//        //            continue;
//        //        }

//        //        yield return waitrForPerOp;
//        //    }
//        //}
//    }
//}
//#endif