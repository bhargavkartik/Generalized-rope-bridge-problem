using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RopeBridge
{
    class Program
	{
		// Important SMT params to obtain the optimal solution
		const int MaxTime = 100; // Bound value
		const int MinCrossings = 11;
		const int MaxCrossings = 15;
		const int Adventurers = 7;

		// PATH to Yices-SMT.exe
		const string Path = @"C:\Users\Administrator\Documents\Q5-TUE\Automated Reasoning\yices-2.6.2-x86_64-pc-mingw32-static-gmp\yices-2.6.2\bin\yices-smt.exe";

		// variables for the SMT function names
		const string PL = "PL";
		const string P = "P";
		const string A = "A";
		const string T = "T";

		// storing the time values taken by different adventurers
		static int[] TimeArray = { 0 };

		static TextWriter output;
		static Process proc;

		public static void startWrite(string funs, string preds, string logic)
		{
			Console.WriteLine("start++");

			write("(benchmark ropebridge.smt");
			write(":logic {0}", logic);

			if (!string.IsNullOrWhiteSpace(funs)) write(":extrafuns ({0})", funs);
			if (!string.IsNullOrWhiteSpace(preds)) write(":extrapreds ({0})", preds);

			write(":formula (and");
		}

		public static void endWrite()
		{
			write("))");
			write("");
		}

		public static void write(string s, params object[] p)
		{
			output.WriteLine(s, p);
			Console.WriteLine(s, p);
		}

		public static string NotInCross(int t, int a1)
		{
			Console.WriteLine("NIC++");
			return NotInCross(t, a1, -1);
		}

		public static string NotInCross(int t, int a1, int a2)
		{
			Console.WriteLine("NotInCross++");

			StringBuilder sb = new StringBuilder();
			for (int i = 1; i <= TimeArray.Length - 1; i++)
			{
				if (i != a1 && i != a2)
					sb.Append(Util.equals(Util.array(P, i, t), Util.array(P, i, t + 1)));
			}
			return sb.ToString();
		}

		public static void DoubleCross(int t, int a1, int a2)
		{
			Console.WriteLine("DoubleCross++");
			
			write(
				Util.and(
					Util.equals(Util.array(PL, t), Util.array(P, a1, t)), Util.equals(Util.array(PL, t), Util.array(P, a2, t)),
					Util.not(Util.equals(Util.array(PL, t), Util.array(PL, t + 1))), Util.not(Util.equals(Util.array(P, a1, t),
					Util.array(P, a1, t + 1))), Util.not(Util.equals(Util.array(P, a2, t), Util.array(P, a2, t + 1))),
					NotInCross(t, a1, a2),
					Util.equals(Util.array(T, t + 1), Util.plus(Util.array(T, t), Math.Max(TimeArray[a1], TimeArray[a2]).ToString()))
				)
			);
		}

		public static void Solution(int t)
		{
			Console.WriteLine("Solution++");

			write(
				Util.and(
					Util.ForJoin(1, TimeArray.Length, i => Util.not(Util.array(P, i, t))),
					Util.lessEqual(Util.array(T, t), MaxTime.ToString())
				)
			);
		}

		static void startProcess(bool model)
		{
			Console.WriteLine("startProcess++");

			ProcessStartInfo start = new ProcessStartInfo();

			if (model)
			{
				start.Arguments = "-m ";
			}

			start.FileName = Path;

			start.WindowStyle = ProcessWindowStyle.Hidden;
			start.RedirectStandardInput = true;
			start.RedirectStandardOutput = true;
			start.RedirectStandardError = true;
			start.UseShellExecute = false;
			start.CreateNoWindow = true;

			proc = Process.Start(start);

			output = proc.StandardInput;
		}

		static bool unsat(int crossings)
		{
			Console.WriteLine("unsat++");
			
			startProcess(false);

			startWrite("(T Int Int) (A Int Int) (Trans Int Int Int) ", "(P Int Int) (PL Int)", "QF_UFLIA");

			write(Util.equals(Util.array(T, 0), "0"));

			for (int i = 1; i <= TimeArray.Length - 1; i++)
			{
				write(Util.equals(Util.array(A, i), TimeArray[i].ToString()));
				write(Util.array(P, i, 0));
			}
			write(Util.array(PL, 0));

			for (int i = 0; i < crossings; i++)
			{
				write("(or ");
				for (int a1 = 1; a1 <= TimeArray.Length - 1; a1++)
					for (int a2 = 1; a2 <= a1; a2++)
						DoubleCross(i, a1, a2);
				write(")");
			}

			write("(or ");
			for (int i = 0; i <= crossings; i++)
			{
				Solution(i);
			}
			write(")");

			endWrite();

			output.Close();

			proc.WaitForExit();

			string error = proc.StandardError.ReadToEnd();
			if (!string.IsNullOrEmpty(error))
				throw new Exception(error);
			string result = proc.StandardOutput.ReadToEnd();
			if (result.Contains("unsat"))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		static void ShowResult(int crossings)
		{
			Console.WriteLine("ShowResult++");
			
			startProcess(true);

			startWrite("(T Int Int) (A Int Int) (Trans Int Int Int) ", "(P Int Int) (PL Int)", "QF_UFLIA");

			write(Util.equals(Util.array(T, 0), "0"));

			for (int i = 1; i <= TimeArray.Length - 1; i++)
			{
				write(Util.equals(Util.array(A, i), TimeArray[i].ToString()));
				write(Util.array(P, i, 0));
			}
			write(Util.array(PL, 0));

			for (int i = 0; i < crossings; i++)
			{
				write("(or ");
				for (int a1 = 1; a1 <= TimeArray.Length - 1; a1++)
					for (int a2 = 1; a2 <= a1; a2++)
						DoubleCross(i, a1, a2);
				write(")");
			}

			write("(or ");
			for (int i = 0; i <= crossings; i++)
			{
				Solution(i);
			}

			write(")");
			endWrite();

			output.Close();
			proc.WaitForExit();

			string result = proc.StandardOutput.ReadToEnd();
			Console.WriteLine(result);
		}

		static void Main(string[] args)
		{
			Console.WriteLine("Main++");
			
			List<int> times = new List<int>();
			times.Add(0);
			for (int i = 0; i < Adventurers; i++)
			{
				times.Add(Convert.ToInt32(Math.Round(Math.Pow(2, i))));
			}
			TimeArray = times.ToArray();

			int crossings = MinCrossings - 1;

			do
			{
				crossings++;
				Console.WriteLine("Number of crossings: {0}", crossings);
			}
			while (crossings < MaxCrossings && unsat(crossings));

			if (crossings == MaxCrossings)
			{
				Console.WriteLine("Formula not satisfiable in {0} crossings", MaxCrossings);
				Console.ReadKey();
			}
			else
			{
				ShowResult(crossings);
				Console.WriteLine("Number of crossings needed for satisfiability: {0}", crossings);
				Console.ReadKey();
			}
		}
	}
}
