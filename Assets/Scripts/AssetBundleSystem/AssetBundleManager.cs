using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AssetBundle Manager
/// 
/// ・アセットバンドルのロードとアンロード
/// ・依存ファイルの自動ロード(マニフェストファイルから設定とロードとアンロード)
/// ・バリアントのロード
/// </summary>
public class AssetBundleManager : MonoBehaviour
{
	/// <summary>
	/// AssetBundleのマニフェストファイル
	/// </summary>
	private static AssetBundleManifest _assetBundleManifest = null;

	/// <summary>
	/// ダウンロード済みリスト
	/// </summary>
	private static Dictionary<string, DownloadedAssetBundle> _downloadedAssetBundleDic = new Dictionary<string, DownloadedAssetBundle> ();

	/// <summary>
	/// ダウンロード実行中リスト
	/// → 中身が入った段階Update処理開始
	/// </summary>
	private static Dictionary<string, WWW> _downloadingWWWDic = new Dictionary<string, WWW> ();

	/// <summary>
	/// ダウンロードエラーが発生したリスト
	/// </summary>
	private static Dictionary<string, string> _downloadingErrorDic = new Dictionary<string, string> ();

	/// <summary>
	/// 依存ファイルリスト
	/// </summary>
	private static Dictionary<string, string[]> _dependencies = new Dictionary<string, string[]> ();

	/// <summary>
	/// ロード経過チェック用リスト
	/// </summary>
	private static List<AssetBundleLoadBase> _inProgressList = new List<AssetBundleLoadBase> ();

	/// <summary>
	/// 自分自身のGameObject
	/// </summary>
	private static GameObject _myselfObject;

	/// <summary>
	/// AssetBundleManifest
	/// ・依存ファイル
	/// ・バリアント
	/// </summary>
	/// <value>The asset bundle manifest object.</value>
	public static AssetBundleManifest AssetBundleManifestObject {
		set { _assetBundleManifest = value; }
	}

	/// <summary>
	/// AssetBundleManifestの初期化
	/// </summary>
	public static AssetBundleLoadManifest DownloadManifest ()
	{
		// 自身のGameObject生成
		if (_myselfObject == null) {
			_myselfObject = new GameObject ("AssetBundleManager", typeof(AssetBundleManager));
		}

		// マニフェストファイルのダウンロード
		string manifestAssetBundleName = AssetBundleUtility.GetPlatformFolder ();
		_DownloadAssetBundle (manifestAssetBundleName, true);
		var loadManifest = new AssetBundleLoadManifest (manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
		_inProgressList.Add (loadManifest);
		return loadManifest;
	}

	/// <summary>
	/// AssetBundleパスからAssetロードする
	/// 1. AssetBundleをパスからダウンロード
	/// 2. ダウンロード成功したら、ロード経過のチェックするリストに追加
	/// 3. リクエストを返す
	/// </summary>
	/// <returns>The asset async.</returns>
	/// <param name="assetBundleName">Asset bundle name.</param>
	/// <param name="assetName">Asset name.</param>
	/// <param name="type">Type.</param>
	public static AssetBundleLoadBase LoadAssetAsync (string assetBundlePath, string assetName, System.Type type)
	{
		AssetBundleLoadBase loadRequest = null;
		// ダウンロード開始予定リストに追加
		DownloadAssetBundle (assetBundlePath);
		// リクエスト取得
		loadRequest = new AssetBundleLoadAsset (assetBundlePath, assetName, type);
		// ロード経過チェック用リストにリクエストを追加
		_inProgressList.Add (loadRequest);

		// リクエスト処理を返す
		return loadRequest;
	}


	/// <summary>
	/// ダウンロード済みAssetBundleの取得
	/// ・毎フレーム実行
	/// ・依存ファイルのチェックを実行
	/// </summary>
	/// <returns>The downloaded asset bundle.</returns>
	/// <param name="assetBundlePath">Asset bundle path.</param>
	/// <param name="error">Error.</param>
	public static DownloadedAssetBundle GetDownloadedAssetBundle (string assetBundlePath, out string error)
	{
		// エラーリストに該当のAssetBundleがあった場合は終了
		if (_downloadingErrorDic.TryGetValue (assetBundlePath, out error)) {
			Debug.LogError (assetBundlePath + " error: " + error);
			return null;
		}

		// ダウンロード済みリストに、該当のAssetBundleが無ければ終了
		DownloadedAssetBundle downloadedAB = null;
		if (_downloadedAssetBundleDic.TryGetValue (assetBundlePath, out downloadedAB) == false) {
			return null;
		}

		// -------------------------
		// ダウンロード済みリストにある
		// -------------------------

		// assetBundleNameの依存ファイルチェック。
		string[] dependencies = null;
		if (_dependencies.TryGetValue (assetBundlePath, out dependencies) == false) {
			// 依存ファイルが無ければそのままloadedAssetBundleを返す
			return downloadedAB;
		}

		// -------------------------
		// 依存ファイルのチェック
		// -------------------------

		// assetBundleNameの依存ファイルを抽出
		foreach (var dependency in dependencies) {
			// エラーリストに該当のAssetBundleがあった場合
			if (_downloadingErrorDic.TryGetValue (assetBundlePath, out error)) {
				// downloadedABを返して処理終了
				return downloadedAB;
			}

			// ロード済み依存ファイル チェック
			DownloadedAssetBundle dependentBundle;
			_downloadedAssetBundleDic.TryGetValue (dependency, out dependentBundle);
			if (dependentBundle == null) {
				return null;
			}
		}

		return downloadedAB;
	}

	/// <summary>
	/// AssetBundleのアンロード
	/// → 依存ファイルもアンロード
	/// </summary>
	/// <param name="assetBundleName">Asset bundle name.</param>
	public static void UnloadAssetBundle (string assetBundleName)
	{
		// アセットバンドルのアンロード
		_UnloadAssetBundle (assetBundleName);
		// 依存ファイルもあればアンロードする
		UnloadDependencies (assetBundleName);
	}


	/// <summary>
	/// AssetBundleのダウンロード
	/// → あれば、依存ファイルもダウンロードする
	/// </summary>
	/// <param name="assetBundleName">Asset bundle name.</param>
	/// <param name="isLoadingAssetBundleManifest">If set to <c>true</c> is loading asset bundle manifest.</param>
	private static void DownloadAssetBundle (string assetBundleName)
	{
		// ダウンロード済チェック
		bool isAlreadyProcessed = _DownloadAssetBundle (assetBundleName, false);

		// 初めてのダウンロードの場合は、依存ファイルをロードする
		if (isAlreadyProcessed == false) {
			DownloadDependencies (assetBundleName);
		}
	}


	/// <summary>
	/// ダウンロード処理
	/// trueで既にダウンロード済み
	/// falseで初めてのダウンロード
	/// </summary>
	/// <returns><c>true</c>, if asset bundle was downloaded, <c>false</c> otherwise.</returns>
	/// <param name="assetBundleName">Asset bundle name.</param>
	/// <param name="isDownloadingABManifest">If set to <c>true</c> is downloading AB manifest.</param>
	private static bool _DownloadAssetBundle (string assetBundleName, bool isDownloadingABManifest)
	{
		/*
		// キャッシュ削除(Test用)
		if (Caching.CleanCache ()) {
			Debug.Log ("cache clear");
		}
		*/

		// 既にダウンロード済み
		DownloadedAssetBundle downloadedAB = null;
		_downloadedAssetBundleDic.TryGetValue (assetBundleName, out downloadedAB);
		if (downloadedAB != null) {
			// 既にダウンロード済みだが、再度リクエストあった扱いなので、参照カウントをUpする
			downloadedAB.referencedCount++;
			return true;
		}

		// ダウンロード実行中のリストをチェックする
		if (_downloadingWWWDic.ContainsKey (assetBundleName)) {
			return true;
		}

		// ダウンロード実行中リストへ追加
		WWW downloadQue = null;
		string url = AssetBundleUtility.GetDownloadPath () + assetBundleName;

		// マニフェストファイル or AssetBundle
		if (isDownloadingABManifest) {
			downloadQue = new WWW (url);
		} else {
			downloadQue = WWW.LoadFromCacheOrDownload (url, _assetBundleManifest.GetAssetBundleHash (assetBundleName), 0);
		}

		// ダウンロード実行中リストに追加
		_downloadingWWWDic.Add (assetBundleName, downloadQue);

		// 初ダウンロード
		return false;
	}


	/// <summary>
	/// 依存ファイルのロード
	/// </summary>
	/// <param name="assetBundleName">Asset bundle name.</param>
	private static void DownloadDependencies (string assetBundleName)
	{
		if (_assetBundleManifest == null) {
			Debug.LogError ("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
			return;
		}

		// マニフェストファイルから指定AssetBundleの依存ファイルを取得する
		string[] dependencies = _assetBundleManifest.GetAllDependencies (assetBundleName);
		if (dependencies.Length == 0) {
			return;
		}

		// Dictionaryに保存
		_dependencies.Add (assetBundleName, dependencies);
		foreach (var dependence in dependencies) {
			_DownloadAssetBundle (dependence, false);
		}
	}

	/// <summary>
	/// 依存ファイルのアンロード
	/// </summary>
	/// <param name="assetBundleName">Asset bundle name.</param>
	private static void UnloadDependencies (string assetBundleName)
	{
		string[] dependencies = null;
		if (_dependencies.TryGetValue (assetBundleName, out dependencies) == false) {
			return;
		}
		foreach (var dependency in dependencies) {
			_UnloadAssetBundle (dependency);
		}
		_dependencies.Remove (assetBundleName);
		Debug.Log ("Unload " + assetBundleName);
	}

	/// <summary>
	/// AssetBundleのアンロード処理
	/// </summary>
	/// <param name="assetBundleName">Asset bundle name.</param>
	private static void _UnloadAssetBundle (string assetBundleName)
	{
		// ロード済みのAssetBundleチェック
		string error;
		DownloadedAssetBundle downloadedBundle = GetDownloadedAssetBundle (assetBundleName, out error);
		if (downloadedBundle == null) {
			return;
		}

		// AssetBundleの参照カウントチェック
		if (--downloadedBundle.referencedCount == 0) {
			downloadedBundle.assetBundle.Unload (false);
			// ロード済みAssetBundleリストから該当分を削除する
			_downloadedAssetBundleDic.Remove (assetBundleName);
		}
	}

	void Update ()
	{
		DownloadingCheck ();
	}

	/// <summary>
	/// ダウンロードのリストに追加
	/// (キューを追加するイメージ)
	/// </summary>
	private void DownloadingCheck ()
	{
		var removeKeyList = new List<string> ();

		// ダウンロード実行中リストからValue(Key)を取得
		foreach (var wwwValue in _downloadingWWWDic) {
			WWW downloadWWW = wwwValue.Value;

			// ダウンロードに失敗した場合
			if (string.IsNullOrEmpty (downloadWWW.error) == false) {
				_downloadingErrorDic.Add (wwwValue.Key, downloadWWW.error);
				// 失敗したもののKeyを削除予定リストへ
				removeKeyList.Add (wwwValue.Key);
				continue;
			}

			// ダウンロードに成功したら
			if (downloadWWW.isDone) {
				// ダウンロード済リストへ保存
				_downloadedAssetBundleDic.Add (wwwValue.Key, new DownloadedAssetBundle (downloadWWW.assetBundle));
				// ダウンロード済みしたもののKeyを削除予定リストへ
				removeKeyList.Add (wwwValue.Key);
			}
		}

		// ダウンロード実行中リストから削除予定Keyで削除
		foreach (var key in removeKeyList) {
			WWW download = _downloadingWWWDic [key];
			_downloadingWWWDic.Remove (key);
			download.Dispose ();
		}


		// ロード経過リストをチェック
		for (int i = 0; i < _inProgressList.Count;) {
			if (_inProgressList [i].LoadRequest () == false) {
				_inProgressList.RemoveAt (i);
			} else {
				i++;
			}
		}
	}
}