using UnityEngine;
using UnityEditor;

namespace Nightfox.Tools.Eol.Editor
{
    /// <summary>
    /// Editor menu class for normalizing
    /// line endings.
    /// </summary>
    internal sealed class AutofixLineEndingsMenu
    {
        #region constants

        internal const string root = "Assets";

        internal const string autoFixKeyName = "Toolfox-AutofixLineEndings";
        internal const string targetKeyName = "Toolfox-TargetLineEnding";

        internal const string assetsMenuPath = "Assets/";
        internal const string toolsMenuPath = "Tools/Consistent Line Endings/";

        internal const string normalizeAssetMenuPath = assetsMenuPath + "Normalize All Line Endings";
        internal const string normalizeToolsMenuPath = toolsMenuPath + "Normalize All Line Endings";
        internal const string autoFixMenuPath = toolsMenuPath + "Autofix Mode %l";

        internal const string autoDetectMenuPath = toolsMenuPath + "Auto-Detect";
        internal const string crMenuPath = toolsMenuPath + "CR (Mac)";
        internal const string lfMenuPath = toolsMenuPath + "LF (Unix or Mac)";
        internal const string crlfMenuPath = toolsMenuPath + "CR+LF (Windows)";

        internal const string autoDetectAssetsMenuPath = assetsMenuPath + "Target Line Ending/Auto-Detect";
        internal const string crAssetsMenuPath = assetsMenuPath + "Target Line Ending/CR (Mac)";
        internal const string lfAssetsMenuPath = assetsMenuPath + "Target Line Ending/LF (Unix or Mac)";
        internal const string crlfAssetsMenuPath = assetsMenuPath + "Target Line Ending/CR+LF (Windows)";

        internal const string clearKeysMenuPath = toolsMenuPath + "Clear Asset Editor Prefs";
        internal const string versionMenuPath = toolsMenuPath + "About Asset...";

        private static readonly string[] extensions = { ".cs", ".js", ".boo", ".shader", ".compute", ".txt", ".xml", ".json", ".yaml", ".csv", ".html", ".htm", ".rtf" };

        #endregion


        #region menus

        [MenuItem(autoFixMenuPath, false, 1)]
        private static void ToggleAutofix()
        {
            bool autoFixing = false;
            
            if (!InitPrefs())
                return;
            
            //toggle
            autoFixing = !EditorPrefs.GetBool(autoFixKeyName);
            Debug.Log("Line ending autofix: " + (autoFixing ? "On" : "Off"));
            EditorPrefs.SetBool(autoFixKeyName, autoFixing);

            if (autoFixing)
                AutofixEndings();
        }

        [MenuItem(normalizeToolsMenuPath, false, 2)]
        [MenuItem(normalizeAssetMenuPath, false, 2000)]
        private static void NormalizeAll()
        {
            if (!InitPrefs())
                return;

            ConvertLineEndings("Assets");
        }

        [MenuItem(autoDetectMenuPath, false, 14)]
        //[MenuItem(autoDetectAssetsMenuPath, false, 1996)]
        private static void ToggleAutoDetect()
        {
            EditorPrefs.SetInt(targetKeyName, (int)EolStyle.Default);
        }

        [MenuItem(crMenuPath, false, 15)]
        //[MenuItem(crAssetsMenuPath, false, 1997)] 
        private static void ToggleCR()
        {
            EditorPrefs.SetInt(targetKeyName, (int)EolStyle.CR);
        }

        [MenuItem(lfMenuPath, false, 16)]
        //[MenuItem(lfAssetsMenuPath, false, 1998)] 
        private static void ToggleLF()
        {
            EditorPrefs.SetInt(targetKeyName, (int)EolStyle.LF);
        }

        [MenuItem(crlfMenuPath, false, 17)]
        //[MenuItem(crlfAssetsMenuPath, false, 1999)]
        private static void ToggleCRLF()
        {
            EditorPrefs.SetInt(targetKeyName, (int)EolStyle.CRLF);
        }

        [MenuItem(clearKeysMenuPath, false, 30)]
        private static void ClearEolEditorPrefs()
        {
            if (EditorPrefs.HasKey(autoFixKeyName))
                EditorPrefs.DeleteKey(autoFixKeyName);
            if (EditorPrefs.HasKey(targetKeyName))
                EditorPrefs.DeleteKey(targetKeyName);
        }

        [MenuItem(versionMenuPath, false, 42)]
        private static void CheckAssetVersion()
        {
            EditorUtility.DisplayDialog("About Consistent Line Endings", "Consistent Line Endings\nVersion 1.0\n\n(c) Nightfox", "Ok.");
        }

        #endregion

        #region menu validation

        [MenuItem(autoFixMenuPath, true)]
        private static bool ToggleAutofixValidation()
        {
            Menu.SetChecked(autoFixMenuPath, EditorPrefs.GetBool(autoFixKeyName));
            return true;
        }

        [MenuItem(normalizeToolsMenuPath, true)]
        [MenuItem(normalizeAssetMenuPath, true)]
        private static bool NormalizeAllValidation()
        {
            return true;
        }

        [MenuItem(autoDetectMenuPath, true)]
        //[MenuItem(autoDetectAssetsMenuPath, true)]
        private static bool ToggleAutoDetectValidation()
        {
            Menu.SetChecked(autoDetectMenuPath, EditorPrefs.GetInt(targetKeyName) == 0);
            //Menu.SetChecked(autoDetectAssetsMenuPath, EditorPrefs.GetInt(targetKeyName) == 0);
            return true;
        }

        [MenuItem(crMenuPath, true)]
        //[MenuItem(crAssetsMenuPath, true)]
        private static bool ToggleCRValidation()
        {
            Menu.SetChecked(crMenuPath, EditorPrefs.GetInt(targetKeyName) == 1);
            //Menu.SetChecked(crAssetsMenuPath, EditorPrefs.GetInt(targetKeyName) == 1);
            return true;
        }

        [MenuItem(lfMenuPath, true)]
        //[MenuItem(lfAssetsMenuPath, true)]
        private static bool ToggleLFValidation()
        {
            Menu.SetChecked(lfMenuPath, EditorPrefs.GetInt(targetKeyName) == 2);
            //Menu.SetChecked(lfAssetsMenuPath, EditorPrefs.GetInt(targetKeyName) == 2);
            return true; 
        }

        [MenuItem(crlfMenuPath, true)]
        //[MenuItem(crlfAssetsMenuPath, true)]
        private static bool ToggleCRLFValidation()
        {
            Menu.SetChecked(crlfMenuPath, EditorPrefs.GetInt(targetKeyName) == 3);
            //Menu.SetChecked(crlfAssetsMenuPath, EditorPrefs.GetInt(targetKeyName) == 3);
            return true; 
        }

        [MenuItem(clearKeysMenuPath, true)]
        private static bool ClearEolEditorPrefsValidation()
        {
            return EditorPrefs.HasKey(autoFixKeyName) || EditorPrefs.HasKey(targetKeyName);
        }

        [MenuItem(versionMenuPath, true)]
        private static bool CheckAssetVersionValidation()
        {
            return EditorPrefs.HasKey(autoFixKeyName) || EditorPrefs.HasKey(targetKeyName);
        }

        #endregion

        #region void methods

        private static bool InitPrefs()
        {
            if (!EditorPrefs.HasKey(autoFixKeyName))
            {
                int option = EditorUtility.DisplayDialogComplex("Consistent Line Endings Note",
                                                                "Please make a backup of your project before using this asset.",
                                                                "Okay, hold on!", "I already did.", "I'm not sure yet.");
                if (option == 0 || option == 2)
                    return false;
                else
                    EditorPrefs.SetBool(autoFixKeyName, false);
            }

            if (!EditorPrefs.HasKey(targetKeyName))
                EditorPrefs.SetInt(targetKeyName, (int)EolStyle.Default);

            return true;
        }

        private static void ConvertLineEndings(string path)
        {
            EolStyle targetEnding = GetEolStyle();
            LineEndings.ConvertAll(path, extensions, targetEnding);
        }

        #endregion

        #region return methods

        private static EolStyle GetEolStyle()
        {
            int value = EditorPrefs.GetInt(targetKeyName);

            if ((EolStyle)value == EolStyle.Default)
                return LineEndings.GetDefaultOSLineEnding();
            else
                return (EolStyle)value;
        }

        #endregion

        #region callbacks

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AutofixEndings()
        {
            if (EditorPrefs.HasKey(autoFixKeyName) && EditorPrefs.GetBool(autoFixKeyName))
            {
                ConvertLineEndings(root);
            }
        }

        #endregion
    }
}
