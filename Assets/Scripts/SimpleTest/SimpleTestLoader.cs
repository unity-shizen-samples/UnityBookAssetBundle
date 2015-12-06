using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class SimpleTestLoader : MonoBehaviour
{
	public string assetName = "cube0";

	private IEnumerator Start ()
	{
		string filePath = System.IO.Path.Combine (AssetBundleUtility.GetDownloadPath (), assetName);
		yield return StartCoroutine (SimpleAssetBundleDownloader.Load (filePath, assetName));

		// NoChache
		/*yield return StartCoroutine (SimpleAssetBundleDownloader.LoadNoCache (filePath, assetName));*/

		if (SimpleAssetBundleDownloader.IsLoadSuccess == false) {
			Debug.LogError ("download failed");
		} else {
			GameObject prefab = SimpleAssetBundleDownloader.LoadedPrefab;
			if (prefab != null) {
				GameObject go0 = Instantiate (prefab);
				go0.name = SimpleAssetBundleDownloader.LoadedPrefab.name;
			} else {
				Debug.LogError ("prefab missing!!");
			}
		}
	}
}
