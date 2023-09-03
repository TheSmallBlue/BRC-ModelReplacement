using System.IO;
using BepInEx;
using UnityEngine;
using Reptile;
using UnityEngine.TextCore.Text;

namespace ModelReplacement
{
    public static class Compatibility
    {
        static BepInEx.Logging.ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(nameof(Compatibility));
        public static void CheckForOldModVersions(){

            string[] configFiles = Directory.GetFiles(Paths.ConfigPath, "ModelReplacement.cfg");

            if (configFiles.Length == 0) return;

            log.LogWarning("You're using a model made with an old version of this mod! We will now be re-formatting it to the new version. Make sure to consult the github repo if you have any questions!");
            
            foreach (string file in configFiles)
            {
                string configCharaString = File.ReadAllLines(file)[8];
                Characters chara = (Characters)int.Parse(configCharaString[configCharaString.Length - 1].ToString());

                string newCharacterFolder = Path.Combine(Paths.PluginPath, "ModelReplacement", chara.ToString());
                string characterAsset = Path.Combine(Paths.PluginPath, "ModelReplacement", "characterasset.asset");

                Directory.CreateDirectory(newCharacterFolder);
                
                File.Move(file, Path.Combine(newCharacterFolder, "ModelReplacement.cfg"));
                File.Move(characterAsset, Path.Combine(newCharacterFolder, "characterasset.asset"));
            }
            
        }
    }


}