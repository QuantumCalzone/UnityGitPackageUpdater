// Copyright 2021 Guney Ozsan
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UnityEditor;

public static class UnityRefresher
{
    /// <summary>
    /// Imitates 'Ctrl/Cmd+R' by forcing a refresh-and-recompile by updating
    /// `Player Settings/Other Settings/Scripting Define Symbols` with a dummy define. However,
    /// unlike 'Ctrl/Cmd+R' it does not force Package Manager to resolve packages. See discussion on
    /// http://answers.unity.com/answers/1210416/view.html.
    /// </summary>
    public static void RefreshExceptPackages()
    {
        BuildTargetGroup selectedBuildTargetGroup =
            EditorUserBuildSettings.selectedBuildTargetGroup;
        string updatedDefines = GetUpdatedDefines(selectedBuildTargetGroup);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(selectedBuildTargetGroup,
            updatedDefines);

        string GetUpdatedDefines(BuildTargetGroup buildTargetGroup)
        {
            const string refreshDefine = "TEMP_UNITY_REFRESH_ENFORCER";
            string defines =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Contains(refreshDefine)
                ? GetDefinesWithRefreshDefineRemoved()
                : GetDefinesWithRefreshDefineAppended();

            string GetDefinesWithRefreshDefineRemoved()
            {
                int refreshDefineIndex =
                    defines.IndexOf(refreshDefine, StringComparison.Ordinal);
                int carriageIndex = refreshDefineIndex;
                bool endsWithSemicolon = false;

                while (true)
                {
                    carriageIndex++;

                    if (carriageIndex == defines.Length)
                    {
                        break;
                    }

                    if (defines[carriageIndex] == ';')
                    {
                        endsWithSemicolon = true;
                        break;
                    }
                }

                int refreshDefineLength = carriageIndex - refreshDefineIndex +
                                          (endsWithSemicolon ? 1 : 0);
                return defines.Remove(refreshDefineIndex, refreshDefineLength);
            }

            string GetDefinesWithRefreshDefineAppended()
            {
                return defines.Length == 0
                    ? refreshDefine
                    : defines + ";" + refreshDefine; 
            }
        }
    }
}
