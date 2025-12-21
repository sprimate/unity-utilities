using UnityEngine;

namespace HitTrax.CoreUtilities
{
    public static class ObjectExtensions
    {
        public static TRet Return<T, TRet>(this T item, TRet returnVal) => returnVal;
        
        public static string ToJson(this object obj, bool prettyPrint = true) => JsonUtility.ToJson(obj, prettyPrint);
    }
}
