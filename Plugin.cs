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
            Compatibility.CheckForOldModVersions();

            MRConfigLoader.LoadConfigValues();

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony("io.smallblue.ModelReplacement");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Reptile.CharacterConstructor))]
    [HarmonyPatch(nameof(Reptile.CharacterConstructor.CreateCharacterFbx))]
    class FbxCreationPatcher
    {
        static GameObject Postfix(GameObject returnValue, Characters character, CharacterConstructor __instance)
        {
            var log = BepInEx.Logging.Logger.CreateLogSource(nameof(FbxCreationPatcher));

            
            if (Utils.IsReplacableCharacter(character, out MRConfigLoader.charaToReplace charaStruct))
            {
                log.LogInfo("Character replaced!");
                return CreateCompatibleGameObject(returnValue, charaStruct.prefab);
            } else {
                return returnValue;
            }
        }

        static GameObject CreateCompatibleGameObject(GameObject intendedReturnValue, GameObject prefab)
        {
            // Get the Chara prefab from our assetbundle
            Transform baseTransform = Object.Instantiate(prefab).transform; //SavedVariables.charaPrefab.transform;

            // Get the transform from the character the game actually wants to show
            Transform intendedGameObjectTransform = intendedReturnValue.transform;

            // Compare the gameobjects, if theres an object we dont have in our prefab, we add it for compatibility
            foreach (Transform child in intendedGameObjectTransform.GetAllChildren())
            {
                if(baseTransform.Find(child.name) == null && child.name != "root"){
                    child.parent = baseTransform;
                }
            }

            // Set the animator to the intended character's
            baseTransform.GetComponent<Animator>().runtimeAnimatorController = intendedReturnValue.GetComponent<Animator>().runtimeAnimatorController;

            // Spooky ghosts begone
            GameObject.Destroy(intendedReturnValue);

            return baseTransform.gameObject;
        }
    }

    [HarmonyPatch(typeof(Reptile.CharacterConstructor))]
    [HarmonyPatch(nameof(Reptile.CharacterConstructor.CreateCharacterMaterial))]
    class MaterialCreatorPatcher
    {
        static Material Postfix(Material returnValue, Characters character, int outfit, CharacterConstructor __instance)
        {

            if (Utils.IsReplacableCharacter(character, out MRConfigLoader.charaToReplace charaStruct))
            {
                Material targetMat =  Object.Instantiate(charaStruct.prefab.GetComponentInChildren<SkinnedMeshRenderer>().material);
                
                if(!charaStruct.overwriteShaders){
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

    [HarmonyPatch(typeof(Reptile.CharacterVisual))]
    [HarmonyPatch("SetInlineSkatesPropsMode")]
    class InlineSkatesLoaderPatch{
        static void Postfix(CharacterVisual.MoveStylePropMode mode, PlayerMoveStyleProps ___moveStyleProps, CharacterVisual __instance){
            var playerCharacter = Traverse.Create(__instance.GetComponentInParent<Player>(true)).Field("character").GetValue();
            if(mode == CharacterVisual.MoveStylePropMode.ACTIVE && Utils.IsReplacableCharacter((Characters)playerCharacter , out MRConfigLoader.charaToReplace charaStruct)){
                ___moveStyleProps.skateL.transform.localPosition = charaStruct.leftSkateVectors[0];
                ___moveStyleProps.skateL.transform.localRotation = Quaternion.Euler(charaStruct.leftSkateVectors[1]);
                ___moveStyleProps.skateL.transform.localScale = charaStruct.leftSkateVectors[2];

                ___moveStyleProps.skateR.transform.localPosition = charaStruct.leftSkateVectors[0];
                ___moveStyleProps.skateR.transform.localRotation = Quaternion.Euler(charaStruct.leftSkateVectors[1]);
                ___moveStyleProps.skateR.transform.localScale = charaStruct.leftSkateVectors[2];
            }
        }

    }
}