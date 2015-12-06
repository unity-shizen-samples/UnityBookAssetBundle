using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AssetBundleのパスリストからAssetBundleを取得する
/// </summary>
public class AssetLoaderBase : MonoBehaviour
{
	/// <summary>
	/// ダウンロード予定のリスト
	/// (ファイル名のパスが入る)
	/// </summary>
	public string[] dowonloadABList = { "" };

	/// <summary>
	/// ロード済みGameObjectリスト
	/// </summary>
	protected List<GameObject> _loadedObjectList;

	/// <summary>
	/// AssetBundleのロード
	/// 1. ダウンロード (WWWを使用する)
	/// 2. ダウンロード済みからアセットをロードする
	/// </summary>
	/// <param name="assetBundleName">Asset bundle name.</param>
	/// <param name="assetName">Asset name.</param>
	protected virtual IEnumerator Load (string assetBundleName, string assetName)
	{
		yield return null;
	}

	/// <summary>
	/// マニフェストファイルのダウンロード
	/// </summary>
	/// <returns>The manifest.</returns>
	protected IEnumerator DownloadManifest ()
	{
		// ダウンロード開始
		AssetBundleLoadBase request = AssetBundleManager.DownloadManifest ();

		if (request == null) {
			yield break;
		}
		// マニフェストファイルダウンロードの経過が完了したらコルーチン終了
		yield return StartCoroutine (this.CoroutineTimeOutCheck (request, 5f));
	}
}
