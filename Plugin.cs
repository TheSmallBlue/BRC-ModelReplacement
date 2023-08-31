﻿using BepInEx;
using System.Collections;
using Reptile;
using UnityEngine;
using System.IO;
using HarmonyLib;
using System.CodeDom;
using UnityEngine.TextCore.Text;
using BepInEx.Configuration;

namespace ModelReplacement
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<int> configCharacterToReplace;
        public static ConfigEntry<Vector3> inlineSkatesRotL, inlineSkatesPosL, inlineSkatesScaleL;
        public static ConfigEntry<Vector3> inlineSkatesRotR, inlineSkatesPosR, inlineSkatesScaleR;
        public static ConfigEntry<bool> configOverwriteShader;

        private void Awake()
        {
            configCharacterToReplace = Config.Bind("General", "charaToReplace", -1, "Which character to replace, taken from the 'Characters' enum. See this image (The numbers are one digit more than they should be, so what would be 2 in this image is actually 1, 3 is 2, 4 is 3, and so on): https://files.catbox.moe/vhda8a.png");

            inlineSkatesRotL = Config.Bind("General", "inlineSkatesDirL", Vector3.zero, "The rotation of the left inline skate in angles. Modify this to make your left skate look correct");
            inlineSkatesPosL = Config.Bind("General", "inlineSkatesPosL", Vector3.zero, "The Position of the left inline skate relative to the leg bone. Modify this to make your left skate look correct");
            inlineSkatesScaleL = Config.Bind("General", "inlineSkatesScaleL", Vector3.zero, "The scale of the left inline skate relative to the leg bone. Modify this to make your left skate look correct");

            inlineSkatesRotR = Config.Bind("General", "inlineSkatesDirR", Vector3.zero, "The rotation of the right inline skate in angles. Modify this to make your right skate look correct");
            inlineSkatesPosR = Config.Bind("General", "inlineSkatesPosR", Vector3.zero, "The Position of the right inline skate relative to the leg bone. Modify this to make your left skate look correct");
            inlineSkatesScaleR = Config.Bind("General", "inlineSkatesScaleR", Vector3.zero, "The scale of the right inline skate relative to the leg bone.  Modify this to make your left skate look correct");

            configOverwriteShader = Config.Bind("General", "shaderOverwritten", false, "Whether or not we prioritize the shader you set to your material in the Unity editor or the base shader the game uses for outlines and cel-shading");

            SavedVariables.charaPrefab = SavedVariables.GetBundle().LoadAsset<GameObject>("Chara");
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony("io.smallblue.ModelReplacement");
            harmony.PatchAll();
        }
    }

    static class SavedVariables{

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

            if (character == (Characters)Plugin.configCharacterToReplace.Value)
            {
                log.LogInfo("Character replaced!");
                return CreateCompatibleGameObject(returnValue);
            } else {
                return returnValue;
            }
        }

        static GameObject CreateCompatibleGameObject(GameObject intendedReturnValue)
        {
            // Get the Chara prefab from our assetbundle
            Transform baseTransform = Object.Instantiate(SavedVariables.charaPrefab).transform; //SavedVariables.charaPrefab.transform;

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

            if (character == (Characters)Plugin.configCharacterToReplace.Value)
            {
                Material targetMat =  Object.Instantiate(SavedVariables.charaPrefab.GetComponentInChildren<SkinnedMeshRenderer>().material);
                
                if(!Plugin.configOverwriteShader.Value){
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
            
            if(mode == CharacterVisual.MoveStylePropMode.ACTIVE && __instance.transform.GetChild(0).name.Contains("Chara")){
                ___moveStyleProps.skateL.transform.localPosition = Plugin.inlineSkatesPosL.Value;
                ___moveStyleProps.skateL.transform.localRotation = Quaternion.Euler(Plugin.inlineSkatesRotL.Value);
                ___moveStyleProps.skateL.transform.localScale = Plugin.inlineSkatesScaleL.Value;

                ___moveStyleProps.skateR.transform.localPosition = Plugin.inlineSkatesPosR.Value;
                ___moveStyleProps.skateR.transform.localRotation = Quaternion.Euler(Plugin.inlineSkatesRotR.Value);
                ___moveStyleProps.skateR.transform.localScale = Plugin.inlineSkatesScaleR.Value;
            }
        }

    }
}