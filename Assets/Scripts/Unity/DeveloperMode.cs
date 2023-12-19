#if UNITY_EDITOR
using UnityEditor;

public class DeveloperMode : EditorWindow
{
    [MenuItem("Window/Custom/Enable Developer Mode")]
    static void EnableDeveloperMode()
    {
        EditorPrefs.SetBool("DeveloperMode", true);
    }

    [MenuItem("Window/Custom/Disable Developer Mode")]
    static void DisableDeveloperMode()
    {
        EditorPrefs.SetBool("DeveloperMode", false);
    }
}
#endif