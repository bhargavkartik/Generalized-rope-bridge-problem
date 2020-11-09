using System;
using System.Collections.Generic;
using System.Text;

namespace RopeBridge
{
    class Util
    {
		public static string array(string arrayName, params string[] index)
		{
			return string.Format("({0} {1})", arrayName, string.Join(" ", index));
		}
		public static string array(string arrayName, params int[] index)
		{
			return array(arrayName, new List<int>(index).ConvertAll<string>(i => i.ToString()).ToArray());
		}

		public static string plus(string first, string second)
		{
			return operation("+", first, second);
		}

		public static string lessEqual(string first, string second)
		{
			return operation("<=", first, second);
		}

		public static string ForJoin(int start, int end, Func<int, string> func)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = start; i < end; i++)
			{
				if (i != start)
					sb.Append(" ");
				sb.Append(func(i));
			}
			return sb.ToString();
		}

		public static string operation(string operation, params string[] attributes)
		{
			return string.Format("({1} {0})", string.Join(" ", attributes), operation);
		}

		public static string and(params string[] attributes)
		{
			return operation("and", attributes);
		}

		public static string not(params string[] attributes)
		{
			return operation("not", attributes);
		}

		public static string equals(string first, string second)
		{
			return operation("=", first, second);
		}
	}
}
