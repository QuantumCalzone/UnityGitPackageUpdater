using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace QuantumCalzone
{
    public class PackageUpdaterEditorWindow : EditorWindow
    {
        private struct Package
        {
            public Package(string manifestLine, int packageIndex)
            {
                ManifestLine = manifestLine;
                PackageIndex = packageIndex;
            }

            public string ManifestLine;
            public int PackageIndex;
        }

        #region Fields
        private List<Package> packages = new List<Package>();
        private Vector2 scrollPosition = Vector2.zero;
        private bool verbose = false;
        #endregion

        #region Properties
        private static string ManifestPath {
            get {
                var projectPath = Directory.GetParent(Application.dataPath).FullName;
                var manifestPath = Path.Combine(projectPath, "Packages", "manifest.json");
                return manifestPath;
            }
        }

        private static string PackagesLockPath {
            get {
                var projectPath = Directory.GetParent(Application.dataPath).FullName;
                var packagesLockPath = Path.Combine(projectPath, "Packages", "packages-lock.json");
                return packagesLockPath;
            }
        }
        #endregion

        #region Editor Messages
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Refresh"))
            {
                RefreshPackages();
            }

            if (packages.Count > 0)
            {
                if (GUILayout.Button("Update All"))
                {
                    for (var i = 0; i < packages.Count; i++)
                    {
                        /*
                        var progressBarTitle = "Updating";
                        var progressBarInfo = "???";
                        var progressBarProgress = (float)i / (float)packages.Count;
                        Debug.Log(string.Format("i: {0} / packages.Count: {1} = {2}", i, packages.Count, progressBarProgress));
                        */

                        /*
                        if (EditorUtility.DisplayCancelableProgressBar(
                            title: progressBarTitle,
                            info: progressBarInfo,
                            progress: progressBarProgress))
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                        */

                        /*
                        EditorUtility.DisplayProgressBar(
                            title: progressBarTitle,
                            info: progressBarInfo,
                            progress: progressBarProgress
                        );
                        */

                        ReinstallPackage(packages[i].PackageIndex);
                    }

                    //EditorUtility.ClearProgressBar();
                }

                EditorGUILayout.HelpBox("Or select a package below to update", MessageType.Info);
            }

            scrollPosition = GUILayout.BeginScrollView(
                scrollPosition: scrollPosition,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false,
                horizontalScrollbar: GUIStyle.none,
                verticalScrollbar: GUI.skin.verticalScrollbar,
                background: "Box"
            );

            scrollPosition.x = 0;

            for (var i = 0; i < packages.Count; i++)
            {
                if (GUILayout.Button(packages[i].ManifestLine))
                {
                    ReinstallPackage(packages[i].PackageIndex);
                }
            }

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Methods
        [MenuItem("Window/Package Updater")]
        private static void OpenWindow()
        {
            var packageReinstallerWindow = (PackageUpdaterEditorWindow)GetWindow(
                t: typeof(PackageUpdaterEditorWindow),
                utility: false,
                title: "Package Updater"
            );
            packageReinstallerWindow.Show();
            packageReinstallerWindow.RefreshPackages();
        }

        public void RefreshPackages()
        {
            if (verbose) Debug.Log("RefreshPackages");

            packages.Clear();

            var manifestLines = File.ReadAllText(ManifestPath).Split('\n');

            for (var i = 0; i < manifestLines.Length; i++)
            {
                var manifestLine = manifestLines[i];
                if (manifestLine.Contains("com."))
                {
                    manifestLine = manifestLine.Split(':')[0];
                    manifestLine = manifestLine.Replace("    \"", string.Empty);
                    manifestLine = manifestLine.Replace("\"", string.Empty);
                    manifestLine = manifestLine.Replace("com.", string.Empty);
                    packages.Add(new Package(manifestLine, i));
                }
            }
        }

        private void ReinstallPackage(int atLine)
        {
            if (verbose) Debug.Log(string.Format("ReinstallPackage ( atLine: {0} )", atLine));

            var manifestLines = File.ReadAllText(ManifestPath).Split('\n');
            var oldManifest = string.Join("\n", manifestLines);
            ArrayUtility.RemoveAt(ref manifestLines, atLine);
            var newManifest = string.Join("\n", manifestLines);
            File.WriteAllText(ManifestPath, newManifest);
            AssetDatabase.Refresh();
            File.WriteAllText(ManifestPath, oldManifest);
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
