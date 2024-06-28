using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_WSA
using UnityEngine.Windows;
using Directory = UnityEngine.Windows.Directory;
using File = UnityEngine.Windows.File;
#endif

namespace Sprimate.JsonConfig
{
    public class JsonConfigService
    {
        private const string versionKey = "CONFIG_VERSION";
        private const string overwriteKey = "CONFIG_VERSION_OVERWRITE";

        /// <summary>
        /// Internal contract resolver class for JSON serialization.
        /// This is used to enforce writable-only properties are pulled
        /// from serialized classes.
        /// </summary>
        private class SaveContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }

        private string dataPath;
        private string fileName;
        private string fullPath
        {
            get
            {
                return System.IO.Path.Combine(dataPath, fileName);
            }
        }

        private JsonSerializer serializer;
        private JObject data;
        private JObject defaultData;
        private List<string> overwriteKeys;

        Task saveTask;

        public bool Dirty { get; private set; }

        public override string ToString()
        {
            return base.ToString() + "[" + fullPath + "]";
        }

        public JsonConfigService(string path, string name = "preferences.json", string defaultConfig = null)
        {
            // Define default settings for JSON serializer.
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new SaveContractResolver()
            };

            // Initialize JSON serializer with custom settings.
            serializer = JsonSerializer.Create(settings);

            saveTask = Task.CompletedTask;

            if (!Directory.Exists(path))
                throw new IOException("Directory is invalid.");

            dataPath = path;
            fileName = name;

            // Load default config if available.
            if (defaultConfig != null)
            {
                LoadConfiguration(ref defaultData, defaultConfig);

                if (defaultData.ContainsKey(overwriteKey))
                {
                    try
                    {
                        // Get the overwrite keys from the config.
                        overwriteKeys = defaultData[overwriteKey].ToObject<List<string>>();
                    }
                    catch (JsonException)
                    {
                        overwriteKeys = new List<string>();
                    }

                    // Remove the overwrite list.
                    defaultData.Remove(overwriteKey);
                }
            }

            if (!File.Exists(fullPath))
            {
                if (defaultData != null)
                {
                    ApplyDefaultConfig();
                }
                else
                {
                    ClearConfiguration();
                }

                SaveConfigurationSync();
            }

            Dirty = false;
        }

        /// <summary>
        /// Generates the merge JObject from the defaultData by
        /// filtering in new entries and allowing overwrite keys.
        /// </summary>
        /// <returns>Filtered JObject for merging.</returns>
        private JObject GetMergeData()
        {
            JObject mergeData = new JObject();

            foreach (KeyValuePair<string, JToken> entry in defaultData)
            {
                if (entry.Key == versionKey || !data.ContainsKey(entry.Key)
                    || overwriteKeys.Contains(entry.Key))
                {
                    mergeData.Add(entry.Key, entry.Value);
                }
            }

            return mergeData;
        }

        /// <summary>
        /// Loads the specified JSON config file
        /// into data.
        /// </summary>
        /// <param name="path">Path to config file.</param>
        private void LoadConfigFile(string path)
        {
            Debug.Log("Loading Config File: " + path);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new JsonConfigLoadException(
                    JsonConfigLoadException.Type.Nonexistent,
                    $"Specified config file does not exist at '{path}'");
            }

#if UNITY_WSA
            try
            {
                string fileData = System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(path));

               // UnityEngine.Debug.LogError(fileData);

                data = JObject.Parse(fileData);
            }
#else
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(path);
                data = JObject.Parse(sr.ReadToEnd());
            }
#endif
            catch (JsonReaderException)
            {
                throw new JsonConfigLoadException(JsonConfigLoadException.Type.Malformed, "Malformed JSON detected.");
            }
            catch (IOException)
            {
                throw new JsonConfigLoadException(JsonConfigLoadException.Type.Access, "Configuration file is inaccessible.");
            }
            finally
            {
#if !UNITY_WSA
                if (sr != null)
                {
                    sr.Close();
                }
#endif

                // Create new config object.
                if (data == null)
                {
                    ClearConfiguration();
                }
            }
        }

        /// <summary>
        /// Parse the specified configuration into the target JObject
        /// </summary>
        /// <param name="target">Target JObject</param>
        /// <param name="config">Configuration to parse</param>
        private void LoadConfiguration(ref JObject target, string config)
        {
            try
            {
                target = JObject.Parse(config);
            }
            catch (JsonReaderException)
            {
                throw new JsonConfigLoadException(JsonConfigLoadException.Type.Malformed, "Malformed JSON detected.");
            }
        }

        /// <summary>
        /// Serialize and save the configuration to file.
        /// </summary>
        private async void Save()
        {
#if UNITY_WSA
            throw new NotImplementedException("Async saving is not available on WSA.");
#else
            // Open file for overwriting.
            StreamWriter sw = new StreamWriter(fullPath);

            using (JsonTextWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                // Write formatted JSON info.
                await data.WriteToAsync(jw);
            }

            sw.Close();
            
            Dirty = false;
#endif
        }

        /// <summary>
        /// Run save task synchronously.
        /// </summary>
        public void SaveConfigurationSync()
        {
            while (!saveTask.IsCompleted) { }

#if UNITY_WSA
            byte[] outputData = System.Text.Encoding.ASCII.GetBytes(data.ToString());
            File.WriteAllBytes(fullPath, outputData);
#else
            StreamWriter sw = new StreamWriter(fullPath);

            using (JsonTextWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.Indented;

                // Write formatted JSON info.
                data.WriteTo(jw);
            }

            sw.Close();
#endif
            Dirty = false;
        }

        /// <summary>
        /// Save the entire data structure to the
        /// designated file asynchronously.
        /// </summary>
        public async void SaveConfiguration()
        {
            await saveTask;
            saveTask = Task.Run(Save);
        }

        /// <summary>
        /// Load the configuration from file.
        /// </summary>
        public void LoadConfiguration()
        {
            LoadConfigFile(fullPath);
        }

        /// <summary>
        /// Load the configuration from a string.
        /// </summary>
        /// <param name="config">JSON configuration to load.</param>
        public void LoadConfiguration(string config)
        {
            LoadConfiguration(ref data, config);
        }

        /// <summary>
        /// Apply the loaded default configuration to the current data.
        /// </summary>
        /// <param name="ignoreVersion">Ignore versioning restrictions.</param>
        public void ApplyDefaultConfig(bool ignoreVersion = false, bool clearData = false)
        {
            if (defaultData == null) return;

            if (data != null && !clearData)
            {
                // Apply the config if we ignore version or if there's no specified version.
                if (ignoreVersion || !data.ContainsKey(versionKey))
                {
                    data.Merge(GetMergeData());
                    Dirty = true;
                }
                else if (defaultData.ContainsKey(versionKey))
                {
                    JToken defaultVersionToken = defaultData.GetValue(versionKey);
                    JToken currentVersionToken = data.GetValue(versionKey);

                    if (defaultVersionToken.Type == JTokenType.Integer && currentVersionToken.Type == JTokenType.Integer)
                    {
                        if (defaultVersionToken.Value<int>() > currentVersionToken.Value<int>())
                        {
                            data.Merge(GetMergeData());
                            Dirty = true;
                        }
                    }
                    else
                    {
                        throw new JsonConfigException("Invalid version type detected, integer required.");
                    }
                }
            }
            else
            {
                // Setup the config as new.
                data = new JObject(defaultData);
                Dirty = true;
            }
        }

        /// <summary>
        /// Remove all properties within the config
        /// data structure.
        /// </summary>
        public void ClearConfiguration()
        {
            data = new JObject();
            Dirty = true;
        }

        /// <summary>
        /// Get the value of the specified name and type.
        /// </summary>
        /// <typeparam name="T">Type of value to get.</typeparam>
        /// <param name="name">Name (or key) of the parameter.</param>
        /// <returns>Value of the specified parameter.</returns>
        public T Get<T>(string name)
        {
            if (!typeof(T).IsSerializable)
                throw new InvalidOperationException($"{typeof(T)} is not a serializable class, so it can't be deserialized to.");

            if (!HasKey(name))
                throw new IOException($"Data has no parameter identified by '{name}'.");

            return data[name].ToObject<T>();
        }

        /// <summary>
        /// Attempt to get the value of the specified name and type.
        /// </summary>
        /// <typeparam name="T">Type of value to get.</typeparam>
        /// <param name="name">Name (or key) of the parameter.</param>
        /// <param name="find">The found value (default if returned False).</param>
        /// <returns>True if a result was found without error.</returns>
        public bool TryGet<T>(string name, out T find)
        {
            try
            {
                find = Get<T>(name);
                return true;
            }
            catch (Exception exception)
            {
                if (exception is IOException || exception is JsonSerializationException)
                {
                    find = default;
                    return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Set the value of the specified name and type.
        /// </summary>
        /// <typeparam name="T">Type of value to set.</typeparam>
        /// <param name="name">Name (or key) of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        public void Set<T>(string name, T value)
        {
            if (!typeof(T).IsSerializable)
                throw new InvalidOperationException($"{typeof(T)} is not a serializable class!");

            JToken token = JToken.FromObject(value, serializer);

            if (!HasKey(name))
            {
                data.Add(name, token);
            }
            else
            {
                data[name] = token;
            }

            Dirty = true;
        }

        /// <summary>
        /// Check if a parameter exists by name.
        /// </summary>
        /// <param name="name">Name (or key) of the parameter.</param>
        /// <returns>True if a parameter exists by that name.</returns>
        public bool HasKey(string name)
        {
            return data.ContainsKey(name);
        }
    }
}
