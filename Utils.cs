using BepInEx;
using Reptile;


namespace ModelReplacement
{
    public static class Utils
    {
        public static string GetLastLetterOfString(string target){
            return target[target.Length - 1].ToString();
        }

        public static bool IsReplacableCharacter(Characters possibleCharacter, out MRConfigLoader.charaToReplace result){
            foreach (MRConfigLoader.charaToReplace replacableChara in MRConfigLoader.replacableCharas)
            {
                if(replacableChara.replacedChara == possibleCharacter){
                    result = replacableChara;
                    return true;
                }
            }

            result = new MRConfigLoader.charaToReplace();
            return false;

        }
    }



}