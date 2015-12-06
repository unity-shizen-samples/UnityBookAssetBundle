using UnityEngine;
using System.Collections;

public class AssetBundleLoadAssetEditorSimulation : AssetBundleLoadBase
{
	Object _simulatedObject;

	public AssetBundleLoadAssetEditorSimulation (Object simulatedObject)
	{
		_simulatedObject = simulatedObject;
	}

	public override T GetAsset<T> ()
	{
		return _simulatedObject as T;
	}

	public override bool LoadRequest ()
	{
		return false;
	}

	public override bool IsDone ()
	{
		return true;
	}
}