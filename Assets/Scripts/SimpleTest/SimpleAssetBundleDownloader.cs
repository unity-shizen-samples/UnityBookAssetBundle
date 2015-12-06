using UnityEngine;
using System.Collections;

/// <summary>
/// ・アセットバンドルのダウンロード
/// ・アセットのロード
/// </summary>
public static class SimpleAssetBundleDownloader
{
	/// <summary>
	/// アセットバンドルからロードしたprefab
	/// </summary>
	public static GameObject LoadedPrefab {
		get {
			if (_request != null && _request.isDone) {
				return _request.GetAsset<GameObject> ();
			} else {
				return null;
			}
		}
	}

	/// <summary>
	/// ダウンロード済みのアセットバンドル
	/// </summary>
	public static AssetBundle DownloadedAssetBundle;

	/// <summary>
	/// ロード成功フラグ
	/// </summary>
	/// <value><c>true</c> if is load success; otherwise, <c>false</c>.</value>
	public static bool IsLoadSuccess{ get; private set; }

	/// <summary>
	/// アセットバンドルのリクエストデータ
	/// </summary>
	private static AssetBundleRequest _request;
	/// <summary>
	/// アセットバンドルのダウンロード リミット時間
	/// </summary>
	private readonly static float LIMIT_TIME = 5f;

	/// <summary>
	/// アセットバンドルのダウンロードとロード
	/// </summary>
	/// <param name="url">URL.</param>
	/// <param name="loadName">Load name.</param>
	public static IEnumerator Load (string url, string loadName)
	{
		// キャッシュ・クリア(テストコード)
		Caching.CleanCache ();

		// キャッシュシステムの準備が完了するのを待ちます
		while (Caching.ready == false) {
			yield return null;
		}

		// ダウンロードの開始
		// 第一引数: URL
		// 第二引数: Version
		using (WWW www = WWW.LoadFromCacheOrDownload (url, 0)) {

			// ダウンロード完了を待つ
			var startTime = Time.realtimeSinceStartup;
			AssetBundle assetBundle = null;
			while (www.isDone == false) {
				yield return 0;
				if (www.progress == 0f && Time.realtimeSinceStartup - startTime > LIMIT_TIME) {
					// timeout
					break;
				}
			}

			if (string.IsNullOrEmpty (www.error) && www.isDone) {
				// ダウンロード成功
				Debug.Log ("LoadSuccess");
				assetBundle = www.assetBundle;
				IsLoadSuccess = true;

				// 非同期Loadする (LoadAssetAsync)
				AssetBundleRequest request = assetBundle.LoadAssetAsync (loadName, typeof(GameObject));

				// リクエストの完了を待つ
				yield return request;

				_request = request;
				DownloadedAssetBundle = assetBundle;

				yield break;
			}

			// ダウンロード失敗
			if (www.isDone == false) {
				Debug.LogError ("TIME OUT");
			} else if (string.IsNullOrEmpty (www.error) == false) {
				Debug.LogError (url + "\n" + www.error);
			} else {
				Debug.LogError ("ERROR");
			}
			IsLoadSuccess = false;
		}
	}

	/// <summary>
	/// アセットバンドルのダウンロードとロード
	/// NoCache
	/// </summary>
	/// <returns>The no cache.</returns>
	/// <param name="url">URL.</param>
	/// <param name="loadName">Load name.</param>
	public static IEnumerator LoadNoCache (string url, string loadName)
	{
		Debug.Log ("NoCache");

		// ダウンロードの開始
		// 第一引数: URL
		using (WWW www = new WWW (url)) {

			// ダウンロード完了を待つ
			var startTime = Time.realtimeSinceStartup;
			AssetBundle assetBundle = null;
			while (www.isDone == false) {
				yield return 0;
				if (www.progress == 0f && Time.realtimeSinceStartup - startTime > LIMIT_TIME) {
					// timeout
					break;
				}
			}

			if (string.IsNullOrEmpty (www.error) && www.isDone) {
				// ダウンロード成功
				Debug.Log ("LoadSuccess");
				assetBundle = www.assetBundle;
				IsLoadSuccess = true;

				// 非同期Loadする (LoadAssetAsync)
				AssetBundleRequest request = assetBundle.LoadAssetAsync (loadName, typeof(GameObject));

				// リクエストの完了を待つ
				yield return request;

				_request = request;
				DownloadedAssetBundle = assetBundle;

				yield break;
			}

			// ダウンロード失敗
			if (www.isDone == false) {
				Debug.LogError ("TIME OUT");
			} else if (string.IsNullOrEmpty (www.error) == false) {
				Debug.LogError (url + "\n" + www.error);
			} else {
				Debug.LogError ("ERROR");
			}
			IsLoadSuccess = false;
		}
	}

	/// <summary>
	/// Assetの取得
	/// </summary>
	/// <returns>The asset.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T GetAsset<T> (this AssetBundleRequest request) where T : UnityEngine.Object
	{
		return request.asset as T;
	}



	/// <summary>
	/// AssetBundleのアンロード
	/// </summary>
	/// <param name="assetBundle">Asset bundle.</param>
	/// <param name="isAllLoadedObject">If set to <c>true</c> is all loaded object.</param>
	public static void UnloadAsset (AssetBundle assetBundle, bool isAllLoadedObject = false)
	{
		assetBundle.Unload (isAllLoadedObject);
	}
}
