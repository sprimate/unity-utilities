namespace System.Collections.Generic
{
	public static class DictionaryExtensions
	{
		public static string ToStringFull<T0, T1>(this Dictionary<T0, T1> dict)
		{
			var sb = new Text.StringBuilder();

			if (dict == null)
			{
				return "NULL Dictionary!!";
			}

			sb.AppendLine("Count: " + dict.Count);

			foreach (var item in dict)
			{
				sb.AppendFormat("Key: {0}    Value: {1}\n", item.Key, item.Value);
			}

			return sb.ToString();
		}

        public static T2 Get<T1, T2>(this Dictionary<T1, T2> dict, T1 key)
        {
            if (dict.ContainsKey(key))
                return dict[key];

            return default(T2);
        }
        public static bool Has<T1, T2>(this Dictionary<T1, T2> dict, T1 key)
        {
            return (dict.ContainsKey(key));
        }
        public static void Set<T1, T2>(this Dictionary<T1, T2> dict, T1 key, T2 val)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = val;
            }
            else
            {
                dict.Add(key, val);
            }
        }

    }
}
