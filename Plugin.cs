using BepInEx;
using System.Collections;
using Reptile;
using UnityEngine;
using System.IO;
using HarmonyLib;
using System.CodeDom;
using UnityEngine.TextCore.Text;

namespace ModelReplacement
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {

        private void Awake()
        {
            SavedVariables.charaPrefab = SavedVariables.GetBundle().LoadAsset<GameObject>("Chara");
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony("io.smallblue.ModelReplacement");
            harmony.PatchAll();
        }
    }

    static class SavedVariables{

        public static Characters characterToChange = Characters.ringdude;

        public static bool overwriteDefaultTextures = false;

        public static GameObject charaPrefab;

        static AssetBundle LoadedBundle = null;

        public static AssetBundle GetBundle(){
            if(LoadedBundle == null){
                LoadedBundle = AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "ModelReplacement", "characterasset.asset"));
            }

            return LoadedBundle;
        }
    }

    [HarmonyPatch(typeof(Reptile.CharacterConstructor))]
    [HarmonyPatch(nameof(Reptile.CharacterConstructor.CreateCharacterFbx))]
    class FbxCreationPatcher
    {
        static GameObject Postfix(GameObject returnValue, Characters character, CharacterConstructor __instance)
        {
            var log = BepInEx.Logging.Logger.CreateLogSource(nameof(FbxCreationPatcher));

            if (character == SavedVariables.characterToChange)
            {
                log.LogInfo("Character replaced!");
                return CreateCompatibleGameObject(returnValue);
            } else {
                return returnValue;
            }
        }

        static GameObject CreateCompatibleGameObject(GameObject intendedReturnValue)
        {
            var log = BepInEx.Logging.Logger.CreateLogSource(nameof(FbxCreationPatcher));
            // TODO: Rename bones based on human bones, for now we name the bones manually
            // TODO: Rename object with SkinnedMeshRenderer to "mesh"

            // Get the Chara prefab from our assetbundle
            Transform baseTransform = Object.Instantiate(SavedVariables.charaPrefab).transform; //SavedVariables.charaPrefab.transform;

            // Get the transform from the character the game actually wants to show
            Transform intendedGameObjectTransform = intendedReturnValue.transform;

            // Compare the gameobjects, if theres an object we dont have in our prefab, we add it for compatibility
            foreach (Transform child in intendedGameObjectTransform.GetAllChildren())
            {
                if(baseTransform.Find(child.name) == null){
                    child.parent = baseTransform;
                }
            }

            // Set the animator to the intended character's
            baseTransform.GetComponent<Animator>().runtimeAnimatorController = intendedReturnValue.GetComponent<Animator>().runtimeAnimatorController;

            return baseTransform.gameObject;
        }
    }

    [HarmonyPatch(typeof(Reptile.CharacterConstructor))]
    [HarmonyPatch(nameof(Reptile.CharacterConstructor.CreateCharacterMaterial))]
    class MaterialCreatorPatcher
    {
        static Material Postfix(Material returnValue, Characters character, int outfit, CharacterConstructor __instance)
        {

            if (character == SavedVariables.characterToChange)
            {
                Material targetMat = Object.Instantiate(SavedVariables.charaPrefab).GetComponentInChildren<SkinnedMeshRenderer>().material;
                
                if(!SavedVariables.overwriteDefaultTextures){
                    targetMat.shader = returnValue.shader;
                }
                
                return targetMat;
            }
            else
            {
                return returnValue;
            }
        }
    }
}