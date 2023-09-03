using System.IO;
using BepInEx;
using UnityEngine;
using Reptile;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ModelReplacement
{
    public static class MRConfigLoader
    {
        static BepInEx.Logging.ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(nameof(MRConfigLoader));
        
        public struct charaToReplace
        {
            public Characters replacedChara;
            public Vector3[] leftSkateVectors;
            public Vector3[] rightSkateVectors;

            public bool overwriteShaders;

            public GameObject prefab;
        }

        public static List<charaToReplace> replacableCharas = new List<charaToReplace>();
        

        public static void LoadConfigValues(){
            List<string> subfolders = Directory.GetDirectories(Path.Combine(Paths.PluginPath, "ModelReplacement")).ToList();

            foreach (string folder in subfolders)
            {
                if(folder.Contains("Template")) return;
                
                string[] configFiles = Directory.GetFiles(folder,"*.cfg");

                string[] assetFiles = Directory.GetFiles(folder, "*.asset");

                if (configFiles.Length == 0 || assetFiles.Length == 0)
                {
                    log.LogError("There is no config / asset file on " + folder + "!");
                    return;
                }

                if(configFiles.Length > 1 || assetFiles.Length > 1)
                {
                    log.LogError("There are more than one config / asset files on " + folder + "!");
                    return;
                }

                charaToReplace newReplacableChara = new charaToReplace();

                foreach (string line in File.ReadAllLines(configFiles[0]))
                {
                    if(line.Split()[0] == "charaToReplace"){
                        newReplacableChara.replacedChara = (Characters)int.Parse(line[line.Length - 1].ToString());
                    }

                    newReplacableChara.leftSkateVectors = new Vector3[3];
                    newReplacableChara.rightSkateVectors = new Vector3[3];

                    if (line.Split()[0] == "inlineSkatesDir")
                    {
                        string lastLetterOfIdentifier = Utils.GetLastLetterOfString(line.Split()[0]);

                        if (lastLetterOfIdentifier != "L" && lastLetterOfIdentifier != "R"){
                            newReplacableChara.leftSkateVectors[0] = GetVectorFromConfigString(line);
                            newReplacableChara.rightSkateVectors[0] = GetVectorFromConfigString(line);
                        } else {
                            if (lastLetterOfIdentifier == "L")
                            {
                                newReplacableChara.leftSkateVectors[0] = GetVectorFromConfigString(line);
                            }
                            else
                            {
                                newReplacableChara.rightSkateVectors[0] = GetVectorFromConfigString(line);
                            }
                        }
                    }
                    if (line.Split()[0] == "inlineSkatesPos")
                    {
                        if (Utils.GetLastLetterOfString(line.Split()[0]) == "L")
                        {
                            newReplacableChara.leftSkateVectors[1] = GetVectorFromConfigString(line);
                        }
                        else
                        {
                            newReplacableChara.rightSkateVectors[1] = GetVectorFromConfigString(line);
                        }
                    }
                    if (line.Split()[0] == "inlineSkatesScale")
                    {  
                        if (Utils.GetLastLetterOfString(line.Split()[0]) == "L")
                        {
                            newReplacableChara.leftSkateVectors[2] = GetVectorFromConfigString(line);
                        }
                        else
                        {
                            newReplacableChara.rightSkateVectors[2] = GetVectorFromConfigString(line);
                        }
                    }

                    if(line.Split()[0] == "shaderOverwritten"){
                        if(line.Contains("true")){
                            newReplacableChara.overwriteShaders = true;
                        } else {
                            newReplacableChara.overwriteShaders = false;
                        }
                    }
                }

                newReplacableChara.prefab = AssetBundle.LoadFromFile(assetFiles[0]).LoadAsset<GameObject>("Chara");
                AssetBundle.UnloadAllAssetBundles(false);

                replacableCharas.Add(newReplacableChara);
            }
        }

        static Vector3 GetVectorFromConfigString(string source)
        {
            int from = source.IndexOf("{");
            int to = source.IndexOf("}") + 1;
            string json = source.Substring(from, to - from);

            return JsonUtility.FromJson<Vector3>(json);
        }

    }


}