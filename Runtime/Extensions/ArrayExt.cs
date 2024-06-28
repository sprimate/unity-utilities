public static class ArrayExt
{
	public static string ToStringExt<T>(this T[] array, string delimiter = ",")
	{
		if (array == null)
			return "NULL";
		else if (array.Length == 0)
			return "EMPTY";
		else
		{
			string s = "";
			for (int i = 0; i < array.Length; i++)
			{
				s += array[i].ToString();
				if (i < array.Length - 1)
					s += delimiter;
			}

			return s;
		}
	}

	public static bool TryGetCastedValue<T>(this object[] objectArray, int index, out T ret)
	{
		if (objectArray != null && objectArray.Length > index && objectArray[index] is T val)
		{
			ret = val;
			return true;
		}
		
		ret = default(T);
		return false;
	}
}
