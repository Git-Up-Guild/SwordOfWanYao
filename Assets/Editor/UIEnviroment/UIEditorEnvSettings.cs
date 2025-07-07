using XGame.Utils;

using UnityEditor;
using UnityEditor.SceneManagement;
using XGameEditor.Config;

namespace XGameEditor.UIEditorEnv
{
    public class UIEditorEnvSettings : ScriptableConfig
    {
        public ResourceRef guiEditorScene_Overlay;
        public ResourceRef guiEditorScene_Camera;

        [MenuItem("XGame/GUI编辑环境/创建配置", false, 1001)]
        public static void CreateSettings()
        {
            var settings = SingletonScriptableObjectAsset<UIEditorEnvSettings>.Instance(true);
            XGameEditorUtility.PingObject(settings);
        }

        [MenuItem("XGame/GUI编辑环境/显示配置", false, 1002)]
        public static void ShowSettings()
        {
            var settings = SingletonScriptableObjectAsset<UIEditorEnvSettings>.Instance(true);
            XGameEditorUtility.PingObject(settings);
        }

        [MenuItem("XGame/GUI编辑环境/打开编辑(Overlay)", false, 1003)]
        public static void OpenUGUIEditorSceneOverlay()
        {
            var settings = SingletonScriptableObjectAsset<UIEditorEnvSettings>.Instance(true);
            if (settings == null)
            {
                EditorUtility.DisplayDialog("提示", "请先创建一个配置！", "知道了");
                return;
            }    

            EditorSceneManager.OpenScene("Assets/" + settings.guiEditorScene_Overlay.path);
        }

        [MenuItem("XGame/GUI编辑环境/打开编辑(Camera)", false, 1004)]
        public static void OpenUGUIEditorSceneCamera()
        {
            var settings = SingletonScriptableObjectAsset<UIEditorEnvSettings>.Instance(true);
            if (settings == null)
            {
                EditorUtility.DisplayDialog("提示", "请先创建一个配置！", "知道了");
                return;
            }

            EditorSceneManager.OpenScene("Assets/" + settings.guiEditorScene_Camera.path);
        }
    }
}