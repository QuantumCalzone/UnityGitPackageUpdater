using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

namespace GitPackageUpdater
{
    public class GitPackageUpdaterEditorWindow : EditorWindow
    {
        private static List<string> packages = new List<string>();
        private Vector2 scrollPosition = Vector2.zero;
        private static bool verbose = false;

        private static string ManifestPath {
            get {
                var projectPath = Directory.GetParent(Application.dataPath).FullName;
                var manifestPath = Path.Combine(projectPath, "Packages", "manifest.json");
                return manifestPath;
            }
        }

        private static JSONNode Manifest
        {
            get
            {
                var manifestSource = File.ReadAllText(ManifestPath);
                var manifest = JSON.Parse(manifestSource);
                return manifest;
            }
        }

        private static string PackagesLockPath {
            get {
                var projectPath = Directory.GetParent(Application.dataPath).FullName;
                var packagesLockPath = Path.Combine(projectPath, "Packages", "packages-lock.json");
                return packagesLockPath;
            }
        }

        private static JSONNode PackagesLock
        {
            get
            {
                var packagesLockSource = File.ReadAllText(PackagesLockPath);
                var packagesLock = JSON.Parse(packagesLockSource);
                return packagesLock;
            }
        }

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
                    ReinstallAllGitPackages();
                    RefreshUnity();
                }

                if (GUILayout.Button("Update All (Including Non-Git)"))
                {
                    File.Delete(PackagesLockPath);
                    RefreshUnity();
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
                if (GUILayout.Button(packages[i]))
                {
                    ReinstallPackage(packages[i]);
                    RefreshUnity();
                }
            }

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        [MenuItem("Window/Git Package Updater")]
        private static void OpenWindow()
        {
            var gitPackageReinstallerWindow = (GitPackageUpdaterEditorWindow)GetWindow(
                t: typeof(GitPackageUpdaterEditorWindow),
                utility: false,
                title: "Git Package Updater"
            );
            gitPackageReinstallerWindow.Show();
            gitPackageReinstallerWindow.RefreshPackages();
        }

        private static void RefreshUnity()
        {
            AssetDatabase.Refresh();
#if UNITY_2020_1_OR_NEWER
            Client.Resolve();
#else
            Debug.LogWarning("Press 'Ctrl/Cmd+R' to force Unity Package Manager to resolve packages.");
#endif
        }

        public void RefreshPackages()
        {
            if (verbose) Debug.Log("RefreshPackages");

            packages.Clear();

            var manifest = Manifest;
            var dependencies = manifest["dependencies"];
            foreach (var dependency in dependencies)
            {
                if (dependency.Value.Value.Contains(".git"))
                {
                    packages.Add(dependency.Key);
                }
            }
        }

        private void ReinstallAllGitPackages()
        {
            if (verbose) Debug.Log("ReinstallAllGitPackages");

            for (var i = 0; i < packages.Count; i++)
            {
                var package = packages[i];
                ReinstallPackage(package);
            }
        }

        private void ReinstallPackage(string package)
        {
            if (verbose) Debug.Log(string.Format("ReinstallPackage ( package: {0} )", package));

            var packagesLock = PackagesLock;
            var dependencies = packagesLock["dependencies"];
            dependencies[package]["hash"] = string.Empty;

            using (var streamWriter = new StreamWriter(PackagesLockPath))
            {
                streamWriter.Write(dependencies.ToString(1));
            }
        }
    }
}
