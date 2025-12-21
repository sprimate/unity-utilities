using HitTrax.CoreUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public static class PlayerPrefsUtilities
    {
        private static string X(this string key) => $"{key}_x";
        private static string Y(this string key) => $"{key}_y";
        private static string Z(this string key) => $"{key}_z";

        private static string Index(this string key, int index) => $"{key}_{index}";
        

        private static void SetVectorForList(this string key, Vector3 value)
        {           
            SetVectorPref(key, value);
        }

        public static Vector3 SetVectorPref(this string key, Vector3 vector)
        {
            $"SET {key} {vector}".Green().Log();
            PlayerPrefs.SetFloat(key.X(), vector.x);
            PlayerPrefs.SetFloat(key.Y(), vector.y);
            PlayerPrefs.SetFloat(key.Z(), vector.z);
            return vector;
        }

        public static Vector3 GetVectorPref(this string key) => new Vector3(key.XPref(), key.YPref(), key.ZPref());
        public static Vector3 GetVectorPref(this string key, int index) => new Vector3(key.XPref(index), key.YPref(index), key.ZPref(index));

        private static bool HasVector3(this string key)
            =>
                PlayerPrefs.HasKey(key.X()) &&
                PlayerPrefs.HasKey(key.Y()) &&
                PlayerPrefs.HasKey(key.Z())
                ;

        public static float XPref(this string key) => PlayerPrefs.GetFloat(key.X());
        public static float YPref(this string key) => PlayerPrefs.GetFloat(key.Y());
        public static float ZPref(this string key) => PlayerPrefs.GetFloat(key.Z());

        public static float XPref(this string key, int index) => PlayerPrefs.GetFloat(key.Index(index).X());
        public static float YPref(this string key, int index) => PlayerPrefs.GetFloat(key.Index(index).Y());
        public static float ZPref(this string key, int index) => PlayerPrefs.GetFloat(key.Index(index).Z());

        public static void AddToPrefList(this string key, float value) => key.AddToPrefList(value, PlayerPrefs.SetFloat);
        public static void AddToPrefList(this string key, int value) => key.AddToPrefList(value, PlayerPrefs.SetInt);
        public static void AddToPrefList(this string key, Vector3 value) => key.AddToPrefList(value, SetVectorForList, HasVector3);

        public static T AddToPrefList<T>(this string key, T value, Action<string, T> setter, Func<string, bool> hasKey = null)
        {
            int index = 0;

            // To check for something custom (like a Vector) a custom query may be necessary
            if(hasKey == null)
            {
                hasKey = PlayerPrefs.HasKey;
            }

            while (hasKey(key.Index(index)))
            {                
                index++;
            }

            setter(key.Index(index), value);

            return value;
        }

        public static IEnumerable<float> GetFloatPrefsList(this string key) => key.GetPrefsList(PlayerPrefs.GetFloat);
        public static IEnumerable<int> GetIntPrefsList(this string key) => key.GetPrefsList(PlayerPrefs.GetInt);
        public static IEnumerable<Vector3> GetVector3PrefsList(this string key) => key.GetPrefsList();

        private static IEnumerable<T> GetPrefsList<T>(this string key, Func<string, T> getter)
        {
            int index = 0;
            while (PlayerPrefs.HasKey(key.Index(index)))
            {
                yield return getter(key.Index(index));
                index++;
            }
        }

        private static IEnumerable<Vector3> GetPrefsList(this string key)
        {
            int index = 0;

            while (HasVector3(key.Index(index)))
            {      
                yield return GetVectorPref(key, index);
                index++;
            }
        }

        public static void ClearList(this string key)
        {
            int index = 0;
            // Normal List
            while(PlayerPrefs.HasKey(key.Index(index)))
            {
                PlayerPrefs.DeleteKey(key.Index(index));
                index++;
            }

            index = 0;
            // Vector List
            while (HasVector3(key.Index(index)))
            {
                key.Index(index).DeleteVectorPref();   
                index++;
            }
        }

        public static void DeleteVectorPref(this string key)
        {
            PlayerPrefs.DeleteKey(key.X());
            PlayerPrefs.DeleteKey(key.Y());
            PlayerPrefs.DeleteKey(key.Z());
        }

        public static void RemoveFloatFromList(this string key, int index) => key.RemoveFromList(index, PlayerPrefs.GetFloat, AddToPrefList);
        public static void RemoveIntFromList(this string key, int index) => key.RemoveFromList(index, PlayerPrefs.GetInt, AddToPrefList);
        //public static void RemoveVector3FromList(this string key, int index) => key.RemoveFromList(index, GetVectorPref, AddToPrefList);

        // Not optimized but works
        private static void RemoveFromList<T>(this string key, int index, Func<string, T> getter, Action<string, T> addTolist)
        {
            List<T> values = new();

            // Add all items to the list of values except the one we're removing
            int searchIndex = 0;
            while (PlayerPrefs.HasKey(key.Index(searchIndex)))
            {
                if(searchIndex != index)
                {
                    values.Add(getter(key.Index(searchIndex)));
                }

                searchIndex++;
            }

            key.ClearList();

            foreach(T value in values)
            {
                addTolist(key, value);
            }
        }

        private static void RemoveVector3FromList(this string key, int index)
        {
            List<Vector3> values = new();

            // Add all items to the list of values except the one we're removing
            int searchIndex = 0;
            while (HasVector3(key.Index(searchIndex)))
            {
                if (searchIndex != index)
                {
                    values.Add(key.GetVectorPref(index));
                }

                searchIndex++;
            }

            key.ClearList();

            foreach (Vector3 value in values)
            {
                AddToPrefList(key, value);                
            }
        }
    }
}
