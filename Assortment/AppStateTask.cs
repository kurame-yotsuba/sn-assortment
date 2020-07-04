using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SwallowNest.Assortment
{
	public class AppStateTask
	{
		#region private member

		//ステートを取得する時間の間隔
		private static readonly TimeSpan interval = TimeSpan.FromMilliseconds(1000);

		#endregion private member

		#region public member

		/// <summary>
		/// AppStateを取得するための関数です。
		/// </summary>
		public Func<AppState> StateReader { get; }

		/// <summary>
		/// 終了処理を行う関数です。
		/// </summary>
		public Action? Finalizer { get; set; }

		/// <summary>
		/// <paramref name="stateReader"/>がAppState.Exitを返したら
		/// 終了するインスタンスを作成します。
		/// </summary>
		/// <param name="stateReader">アプリ状態を取得する関数</param>
		public AppStateTask(Func<AppState> stateReader)
		{
			if (stateReader is null) { throw new ArgumentNullException(nameof(stateReader)); }
			StateReader = stateReader;
		}

		/// <summary>
		/// AppState.Exitを返したら終了するタスクを作成し、開始します。
		/// </summary>
		/// <returns></returns>
		public Task RunAsync()
		{
			return Task.Factory.StartNew(() =>
			{
				//SharedLogger.Print($"{nameof(AppState)}を開始します。", LogLevel.INFO);
				AppState state;

				do
				{
					Task.Delay(interval).Wait();
					state = StateReader();
				} while (state is AppState.Running);

				Finalizer?.Invoke();

				//SharedLogger.Print($"{nameof(AppState)}を終了します。", LogLevel.INFO);
			});
		}

		#endregion public member
	}
}
