namespace System.Collections.Generic
{
    public static class ListExtensions
	{
		public static string ToStringFull<T>(this List<T> list)
		{
			if (list == null)
				return "NULL LIST type: " + typeof(T).ToString();

			var sb = new Text.StringBuilder();
			sb.AppendLine("Count: " + list.Count);

			for (int i = 0; i < list.Count; i++)
			{
				sb.AppendFormat("[{0}]: {1}", i, list[i]);
			}

			return sb.ToString();
		}

        public static string LinesToString<T>(this List<T> list)
        {
            if (list == null)
                return "NULL";

            if (list.Count == 0)
                return "EMPTY";

            var sb = new Text.StringBuilder();
            foreach (var item in list)
            {
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }

        public static T GetCircular<T>(this List<T> list, ref int i)
        {
            i = i % (list.Count);
            if (i < 0)
            {
                i += list.Count;
            }

            if (list != null && list.Count > 0 && i >= 0)
            {
                return list[i];
            }

            return default(T);
        }

        public static T GetCircular<T>(this List<T> list, int i)
        {
            return GetCircular(list, ref i);
        }

        public static T GetNextCircular<T>(this List<T> list, T item)
        {
            int indexOf = list.IndexOf(item);
            if (indexOf >= 0)
            {
                return GetCircular(list, indexOf + 1);
            }
            return default(T);
        }

        public static T GetRandom<T>(this List<T> list)
        {
            if (list != null && list.Count > 0)
            {
                var randoIndex = UnityEngine.Random.Range(0, list.Count);
                return list[randoIndex];
            }

            return default(T);
        }

        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}