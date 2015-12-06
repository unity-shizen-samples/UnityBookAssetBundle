using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple cube asset loader.
/// AssetBundleのパスリストからAssetBundleを取得する
/// </summary>
public class SimpleCubeAssetLoader : AssetLoaderBase
{
	public Vector3[] cubePosList;

	private void Awake ()
	{
		dowonloadABList = new string[]{ "cube0", "cube1", "cube2", "cube3" };
	}

	IEnumerator Start ()
	{
		// マニフェストファイルのロード
		yield return StartCoroutine (DownloadManifest ());

		_loadedObjectList = new List<GameObject> ();

		foreach (var path in dowonloadABList) {
			yield return StartCoroutine (Load (path, path));

			// AssetBundle.Unload(false);
			// Unload(false)は、既にロードしたものはScene上に残る
			AssetBundleManager.UnloadAssetBundle (path);
		}
			

		// ロード済みなので使用可能 --------------------------
		int i = 0;
		foreach (var prefab in _loadedObjectList) {
			var go = Instantiate (prefab);
			go.transform.localPosition = cubePosList [i];
			// shaderの再設定
			ShaderFind (go);
			i++;
		}

		Destroy (gameObject, 1f);
	}

	/// <summary>
	/// AssetBundleのロード
	/// 1. ダウンロード (WWWを使用する)
	/// 2. ダウンロード済みからアセットをロードする
	/// </summary>
	/// <param name="assetBundleName">Asset bundle name.</param>
	/// <param name="assetName">Asset name.</param>
	protected override IEnumerator Load (string assetBundleName, string assetName)
	{
		Debug.Log ("load開始 " + assetName + " 経過frame " + Time.frameCount);

		// AssetBundleからAssetをLoadする
		AssetBundleLoadBase request = AssetBundleManager.LoadAssetAsync (assetBundleName, assetName, typeof(GameObject));
		if (request == null) {
			yield break;
		}

		// リクエスト処理(AssetBundleLoadBase)をイテレーション
		yield return StartCoroutine (this.CoroutineTimeOutCheck (request, 5f));

		// GameObject指定
		GameObject prefab = request.GetAsset<GameObject> ();
		if (prefab != null) {
			_loadedObjectList.Add (prefab);
		}
	}

	/// <summary>
	/// シェーダの再設定
	/// 注意：AssetBundle化するとシェーダはアタッチされているのに、
	/// 適用されていない事があるため。
	/// </summary>
	/// <param name="go">Go.</param>
	private void ShaderFind(GameObject go)
	{
		Renderer[] renderers = go.GetComponentsInChildren<Renderer> (true);
		foreach(Renderer renderer in renderers){
			Material mt = renderer.material;
			mt.shader = Shader.Find(mt.shader.name);
		}
	}
}
