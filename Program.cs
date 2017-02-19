using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.REPL;

namespace MoonSharp
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Script.DefaultOptions.ScriptLoader = new ReplInterpreterScriptLoader();

			Script script = new Script(CoreModules.Preset_Complete);

			script.Globals["makestatic"] = (Func<string, DynValue>)(MakeStatic);

			if (CheckArgs(args, new ShellContext(script)))
				return;

			Banner();

			ReplInterpreter interpreter = new ReplInterpreter(script)
			{
				HandleDynamicExprs = true,
				HandleClassicExprsSyntax = true
			};


			while (true)
			{
				InterpreterLoop(interpreter, new ShellContext(script));
			}
		}

		private static DynValue MakeStatic(string type)
		{
			Type tt = Type.GetType(type);
			if (tt == null)
				Console.WriteLine("Type '{0}' not found.", type);
			else
				return UserData.CreateStatic(tt);

			return DynValue.Nil;
		}

		private static void InterpreterLoop(ReplInterpreter interpreter, ShellContext shellContext)
		{
			Console.Write(interpreter.ClassicPrompt + " ");

			string s = Console.ReadLine();

			try
			{
				DynValue result = interpreter.Evaluate(s);

				if (result != null && result.Type != DataType.Void)
					Console.WriteLine("{0}", result);
			}
			catch (InterpreterException ex)
			{
				Console.WriteLine("{0}", ex.DecoratedMessage ?? ex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("{0}", ex.Message);
			}
		}

		private static void Banner()
		{
			Console.WriteLine(Script.GetBanner("Console"));
			Console.WriteLine();
			Console.WriteLine("Type Lua code to execute it.\n");
			Console.WriteLine("Welcome.\n");
		}


		private static bool CheckArgs(string[] args, ShellContext shellContext)
		{
			if (args.Length == 0)
				return false;

			if (args.Length == 1 && args[0].Length > 0 && args[0][0] != '-')
			{
				Script script = new Script();
				script.DoFile(args[0]);
			}

			if (args[0] == "-H" || args[0] == "--help" || args[0] == "/?" || args[0] == "-?")
			{
				ShowCmdLineHelpBig();
			}

			return true;
		}

		private static void ShowCmdLineHelpBig()
		{
			Console.WriteLine("usage: moonsharp [-H | --help | <script>]");
			Console.WriteLine();
			Console.WriteLine("-H : shows this help");
			Console.WriteLine();
		}

		private static void ShowCmdLineHelp()
		{
			Console.WriteLine("usage: moonsharp [-H | --help | -X \"command\" | -W <dumpfile> <destfile> [--internals] [--vb] | <script>]");
		}
	}
}
