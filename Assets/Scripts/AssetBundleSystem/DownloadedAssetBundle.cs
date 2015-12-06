using UnityEngine;
using System.Collections;

/// <summary>
/// ダウンロード済みのAssetBundle
/// 
/// ・アセットバンドル : assetBundle
/// ・参照カウント : referencedCount
/// </summary>
public class DownloadedAssetBundle
{
	public AssetBundle assetBundle;
	public int referencedCount;

	public DownloadedAssetBundle (AssetBundle assetBundle)
	{
		this.assetBundle = assetBundle;
		referencedCount = 1;
	}
}
