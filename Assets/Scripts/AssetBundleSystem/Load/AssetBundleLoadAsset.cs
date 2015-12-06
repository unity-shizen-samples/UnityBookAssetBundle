using UnityEngine;
using System.Collections;

/// <summary>
/// AssetBundleからAssetをロードする
/// </summary>
public class AssetBundleLoadAsset : AssetBundleLoadBase
{
	/// <summary>
	/// AssetBundleの名前
	/// </summary>
	protected string _assetBundlePath;
	/// <summary>
	/// AssetBudleにあるAssetの名前
	/// </summary>
	protected string _assetName;
	protected string _downloadingError;
	protected System.Type _type;

	/// <summary>
	/// AssetBundleのリクエスト
	/// ・進捗状況などを確認する
	/// </summary>
	protected AssetBundleRequest _request = null;

	public AssetBundleLoadAsset (string bundlePath, string assetName, System.Type type)
	{
		_assetBundlePath = bundlePath;
		_assetName = assetName;
		_type = type;
	}


	/// <summary>
	/// Assetの取得
	/// </summary>
	/// <returns>The asset.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public override T GetAsset<T> ()
	{
		if (_request != null && _request.isDone) {
			return _request.asset as T;
		} else {
			return null;
		}
	}


	/// <summary>
	/// AssetBundleのダウンロードを監視する
	/// → ダウンロード成功(requestに入る) falseが返る
	/// </summary>
	public override bool LoadRequest ()
	{
		// すでにリクエストが有る場合は無視する
		if (_request != null) {
			return false;
		}

		// アセットバンドルのダウンロードを監視
		DownloadedAssetBundle downloadedBundle = AssetBundleManager.GetDownloadedAssetBundle (_assetBundlePath, out _downloadingError);
		// ダウンロード成功したら
		if (downloadedBundle != null) {
			// ダウンロードしたアセットバンドルから、名前とタイプでアセットをロードする(リクエストを返す)
			_request = downloadedBundle.assetBundle.LoadAssetAsync (_assetName, _type);
			return false;
		} else {
			return true;
		}
	}

	/// <summary>
	/// 完了したかチェックする
	/// (IEnumeratorインターフェースのMoveNext時に呼ばれる)
	/// </summary>
	/// <returns><c>true</c> if this instance is done; otherwise, <c>false</c>.</returns>
	public override bool IsDone ()
	{
		if (_request == null && _downloadingError != null) {
			Debug.LogError ("_downloadingError: " + _downloadingError);
			return true;
		}

		return _request != null && _request.isDone;
	}
}