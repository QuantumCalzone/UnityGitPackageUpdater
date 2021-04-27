using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace QuantumCalzone
{
    public class GitPackageUpdaterEditorWindow : EditorWindow
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
        private static Regex afterLastOccurrenceOfHashtag = new Regex("([^#]+$)");
        private static Regex afterLastOccurrenceOfQuotes = new Regex("([^\"]+$)");
        private static Regex betweenQuotes = new Regex("(?<=\")(.*?)(?=\")");
        private static bool verbose = false;
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
                    ReinstallAllGitPackages();
                }

                if (GUILayout.Button("Update All (Including Non-Git)"))
                {
                    File.Delete(PackagesLockPath);

                    AssetDatabase.Refresh();
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
                    ReinstallPackage(packages[i].ManifestLine);
                }
            }

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Methods
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

        public void RefreshPackages()
        {
            if (verbose) Debug.Log("RefreshPackages");

            packages.Clear();

            var manifestLines = File.ReadAllText(ManifestPath).Split('\n');

            for (var i = 0; i < manifestLines.Length; i++)
            {
                var manifestLine = manifestLines[i];
                if (manifestLine.Contains("com.") && manifestLine.Contains(".git"))
                {
                    manifestLine = manifestLine.Split(':')[0];
                    manifestLine = manifestLine.Replace("    \"", string.Empty);
                    manifestLine = manifestLine.Replace("\"", string.Empty);
                    manifestLine = manifestLine.Replace("com.", string.Empty);
                    packages.Add(new Package(manifestLine, i));
                }
            }
        }

        private void ReinstallAllGitPackages()
        {
            if (verbose) Debug.Log("ReinstallAllGitPackages");

            for (var i = 0; i < packages.Count; i++)
            {
                var package = packages[i];
                ReinstallPackage(package.ManifestLine);
            }
        }

        private void ReinstallPackage(string packageName)
        {
            if (verbose) Debug.Log(string.Format("ReinstallPackage ( packageName: {0} )", packageName));

            var packagesLockLines = File.ReadAllLines(PackagesLockPath);
            var foundPackage = false;
            var changed = false;
            for (var i = 0; i < packagesLockLines.Length; i++)
            {
                var packagesLockLine = packagesLockLines[i];
                if (packagesLockLine.Contains(packageName))
                {
                    foundPackage = true;
                }

                //if (foundPackage && packagesLockLine.Contains("version"))
                //{
                //    var afterLastOccurrenceOfHashtagResult = afterLastOccurrenceOfHashtag.Match(packagesLockLine);
                //    if (afterLastOccurrenceOfHashtagResult.Success)
                //    {
                //        var afterLastOccurrenceOfHashtagResultValue = '#' + afterLastOccurrenceOfHashtagResult.Groups[0].Value;
                //        packagesLockLine = packagesLockLine.Replace(afterLastOccurrenceOfHashtagResultValue, string.Empty);
                //        var afterLastOccurrenceOfQuotesResult = afterLastOccurrenceOfQuotes.Match(packagesLockLine);
                //        if (afterLastOccurrenceOfQuotesResult.Success)
                //        {
                //            packagesLockLine = afterLastOccurrenceOfQuotesResult.Groups[0].Value;
                //            break;
                //        }
                //    }
                //}

                if (foundPackage && packagesLockLine.Contains("hash"))
                {
                    packagesLockLine = packagesLockLine.Replace("\"hash\": ", string.Empty);
                    var betweenQuotesResult = betweenQuotes.Match(packagesLockLine);
                    if (betweenQuotesResult.Success)
                    {
                        var hash = betweenQuotesResult.Groups[0].Value;
                        packagesLockLines[i] = packagesLockLines[i].Replace(hash, string.Empty);
                        changed = true;
                    }
                    foundPackage = false;
                }
            }

            if (changed)
            {
                var newpackagesLock = string.Empty;
                for (var i = 0; i < packagesLockLines.Length; i++)
                {
                    newpackagesLock += packagesLockLines[i];
                    newpackagesLock += '\n';
                }

                File.WriteAllText(PackagesLockPath, newpackagesLock);
            }
        }
        #endregion
    }
}
