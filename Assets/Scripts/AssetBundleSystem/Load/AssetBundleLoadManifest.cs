using UnityEngine;
using System.Collections;

public class AssetBundleLoadManifest : AssetBundleLoadAsset
{
	public AssetBundleLoadManifest (string bundleName, string assetName, System.Type type) : base (bundleName, assetName, type)
	{
	}

	public override bool LoadRequest ()
	{
		base.LoadRequest ();
		
		if (_request != null && _request.isDone) {
			AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest> ();
			return false;
		} else {
			return true;
		}
	}
}