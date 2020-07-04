using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SwallowNest.Assortment
{
	public static class AppStateManager
	{
		#region private member

		//ステートを取得する時間の間隔
		private static readonly TimeSpan interval = TimeSpan.FromMilliseconds(1000);

		#endregion private member

		#region public member

		/// <summary>
		/// <paramref name="stateReader"/>がExitを返したら終了するタスクを作成し、開始します。
		/// </summary>
		/// <param name="stateReader"></param>
		/// <returns></returns>
		public static Task RunAsync(Func<AppState> stateReader)
		{
			Setting setting = new Setting(stateReader);
			return RunAsync(setting);
		}

		/// <summary>
		/// readStateがExitを返したら終了するタスクを返します。
		/// </summary>
		/// <param name="setting"></param>
		/// <returns></returns>
		public static Task RunAsync(Setting setting)
		{
			return Task.Factory.StartNew(() =>
			{
				//SharedLogger.Print($"{nameof(AppState)}を開始します。", LogLevel.INFO);
				AppState state;

				do
				{
					Task.Delay(interval).Wait();
					state = setting.StateReader();
				} while (state is AppState.Running);

				setting.Finalizer?.Invoke();

				//SharedLogger.Print($"{nameof(AppState)}を終了します。", LogLevel.INFO);
			}, default, default, TaskScheduler.Default);
		}

		#region readメソッド作成のためのヘルパーメソッド

		/// <summary>
		/// <paramref name="input"/>をAppStateに変換します。
		/// </summary>
		/// <param name="input">入力文字列</param>
		/// <param name="exitCode">Exitに相当する文字列</param>
		/// <param name="ignoreCase">input, exitCodeのケースを無視するかどうか</param>
		/// <param name="strict">input, exitCodeが完全一致とするか、部分一致とするか</param>
		/// <returns></returns>
		public static AppState ToAppState(string input, string exitCode, bool ignoreCase, bool strict)
		{
			//アルファベットのケースを無視する場合
			if (ignoreCase)
			{
				input = input.ToUpper();
				exitCode = exitCode.ToUpper();
			}

			//完全一致か部分一致か
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
		/// 標準入力からAppStateを取得する設定を返します。
		/// </summary>
		/// <param name="exitCode">終了文字列</param>
		/// <param name="ignoreCase">終了文字列のケースを無視するか否か</param>
		/// <param name="strict">文字列が完全一致か部分一致か</param>
		/// <returns></returns>
		public static Setting GetSettingUsingStdInput(
			string exitCode, bool ignoreCase = true, bool strict = false)
		{
			AppState readState()
			{
				string input = Console.ReadLine();
				return ToAppState(input, exitCode, ignoreCase, strict);
			}

			return new Setting(readState);
		}

		/// <summary>
		/// ファイルからAppStateを取得する設定を返します。
		/// </summary>
		/// <param name="stateFilePath">AppStateを読み取るファイルのパス</param>
		/// <param name="exitCode">終了文字列</param>
		/// <param name="ignoreCase">文字列のケースを無視するか否か</param>
		/// <param name="strict">文字列が完全一致か部分一致か</param>
		/// <returns></returns>
		public static Setting GetSettingUsingFile(
			string stateFilePath, string exitCode = "exit", bool ignoreCase = true, bool strict = false)
		{
			//AppStateを取得する関数
			AppState readState()
			{
				if (File.Exists(stateFilePath))
				{
					string input = File.ReadAllText(stateFilePath);
					return ToAppState(input, exitCode, ignoreCase, strict);
				}
				else
				{
					//ステートファイル作成
					File.WriteAllText(stateFilePath, "RUNNING");
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

			return new Setting(readState, finalize);
		}

		#endregion readメソッド作成のためのヘルパーメソッド

		#endregion public member

		#region inner class, enum

		/// <summary>
		/// 設定項目をまとめたクラスです。
		/// </summary>
		public class Setting
		{
			/// <summary>
			/// AppStateを取得するための関数です。
			/// </summary>
			public Func<AppState> StateReader { get; set; }

			/// <summary>
			/// 終了処理を行う関数です。
			/// </summary>
			public Action? Finalizer { get; set; }

			/// <summary>
			/// 設定項目を作成します。
			/// </summary>
			/// <param name="stateReader">アプリ状態を取得する関数</param>
			/// <param name="finalizer">終了処理を行う関数</param>
			public Setting(Func<AppState> stateReader, Action? finalizer = null)
			{
				StateReader = stateReader;
				Finalizer = finalizer;
			}
		}

		/// <summary>
		/// アプリの状態を表します。
		/// </summary>
		public enum AppState
		{
			/// <summary>
			/// アプリが実行中であることを表します。
			/// </summary>
			Running,

			/// <summary>
			/// アプリを終了することを表します。
			/// </summary>
			Exit,
		}

		#endregion inner class, enum
	}
}
