using System;
using System.IO;

namespace SwallowNest.Assortment
{
	/// <summary>
	/// StateReader作成のためのヘルパーメソッドを集めた関数です。
	/// </summary>
	public static class AppStateCreator
	{
		public class Options
		{
			public string ExitCode { get; set; } = "exit";
			public bool IgnoreCase { get; set; } = true;
			public bool Strict { get; set; } = false;

			public void Deconstruct(out string exitCode, out bool ignoreCase, out bool strict)
			{
				exitCode = ExitCode;
				ignoreCase = IgnoreCase;
				strict = Strict;
			}
		}

		/// <summary>
		/// <paramref name="input"/>をAppStateに変換します。
		/// </summary>
		/// <param name="input">入力文字列</param>
		/// <param name="exitCode">Exitに相当する文字列</param>
		/// <param name="ignoreCase">input, exitCodeのケースを無視するかどうか</param>
		/// <param name="strict">input, exitCodeが完全一致とするか、部分一致とするか</param>
		/// <returns></returns>
		public static AppState ToAppState(string input, Options? options = null)
		{
			var (exitCode, ignoreCase, strict) = options ?? new Options();

			// アルファベットのケースを無視する場合
			if (ignoreCase)
			{
				input = input.ToUpper();
				exitCode = exitCode.ToUpper();
			}

			// 完全一致か部分一致か
			AppState state;
			if (strict && input == exitCode || !strict && input.Contains(exitCode))
			{
				state = AppState.Exit;
			}
			else
			{
				state = AppState.Running;
			}

			return state;
		}

		/// <summary>
		/// 標準入力からAppStateを取得するAppStateTaskを返します。
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static AppStateTask CreateStdInputTask(Options? options = null)
		{
			// AppStateを取得する関数
			AppState readState()
			{
				string input = Console.ReadLine();
				return ToAppState(input, options);
			}

			return new AppStateTask(readState);
		}

		/// <summary>
		/// ファイルからAppStateを取得するAppStateTaskを返します。
		/// <paramref name="stateFilePath"/>はタスク終了時に削除されます。
		/// </summary>
		/// <param name="stateFilePath">AppStateを読み取るファイルのパス</param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static AppStateTask CreateFileReadTask(string stateFilePath, Options? options = null)
		{
			// AppStateを取得する関数
			AppState readState()
			{
				if (File.Exists(stateFilePath))
				{
					string input = File.ReadAllText(stateFilePath);
					return ToAppState(input, options);
				}
				else
				{
					//ステートファイル作成
					File.WriteAllText(stateFilePath, AppState.Running.ToString());
					return AppState.Running;
				}
			}

			//終了処理を行う関数
			void finalize()
			{
				if (File.Exists(stateFilePath))
				{
					File.Delete(stateFilePath);
				}
			}

			return new AppStateTask(readState)
			{
				Finalizer = finalize,
			};
		}
	}
}
