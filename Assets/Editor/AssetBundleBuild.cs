using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class AssetBundleBuild
{
	[MenuItem ("AssetBundles/Build for iOS")]
	private static void BuildABiOS ()
	{
		BuildAB (BuildTarget.iOS);
	}

	[MenuItem ("AssetBundles/Build for Android")]
	private static void BuildABAndroid ()
	{
		BuildAB (BuildTarget.Android);
	}

	[MenuItem ("AssetBundles/Build for iOS and Android")]
	private static void BuildABiOSAndAndroid ()
	{
		BuildTarget[] buildTargetList = new BuildTarget[]{ BuildTarget.iOS, BuildTarget.Android };
		foreach (var buildTarget in buildTargetList) {
			BuildAB (buildTarget);
		}
	}

	private static void BuildAB (BuildTarget target)
	{
		// 出力ルートフォルダ + PlatForm名をそのまま最終出力フォルダとして使用する。
		string resultOutPutPath = Application.streamingAssetsPath + "/" + target.ToString ();

		// 指定フォルダ存在チェック 無ければ作成
		if (Directory.Exists (resultOutPutPath) == false) {
			Directory.CreateDirectory (resultOutPutPath);
		}

		// プラットフォーム名(Android・iOS)のマニフェストのアセットバンドルが作られる。
		BuildPipeline.BuildAssetBundles (resultOutPutPath, BuildAssetBundleOptions.None, target);
	}


	[MenuItem ("Assets/Set AssetBundle name")]
	private static void SelectedAsset ()
	{
		// アクティブな選択オブジェクトを取得する
		Object[] selection = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);

		foreach (var obj in selection) {
			SetAssetBundleName (obj);
		}
	}

	private static void SetAssetBundleName (Object obj)
	{
		// AssetImporter
		AssetImporter importer = AssetImporter.GetAtPath (AssetDatabase.GetAssetPath (obj));
		importer.assetBundleName = obj.name;

		// バリアントは未使用
		/*importer.assetBundleVariant = "";*/
	}
}
