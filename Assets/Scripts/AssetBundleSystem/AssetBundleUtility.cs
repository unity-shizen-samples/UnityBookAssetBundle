using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// AssetBundle ユーティリティクラス
/// 主にパスなどはこのクラスから取得できます
/// </summary>
public class AssetBundleUtility
{
	public static string GetDownloadPath ()
	{
		return GetRelativePath () + "/" + GetPlatformFolder () + "/";
	}

	public static string GetPlatformFolder ()
	{
		#if UNITY_EDITOR
		return EditorPlatformFolder (EditorUserBuildSettings.activeBuildTarget);
		#else
		return PlatformFolder(Application.platform);
		#endif
	}

	private static string GetRelativePath ()
	{
		return "file://" + Application.streamingAssetsPath;
	}

	#if UNITY_EDITOR
	private static string EditorPlatformFolder (BuildTarget target)
	{
		switch (target) {
		case BuildTarget.Android:
			return "Android";
		case BuildTarget.iOS:
			return "iOS";
		case BuildTarget.WebPlayer:
			return "WebPlayer";
		default:
			Debug.LogWarning ("GetPlatformFolder " + target.ToString ());
			return null;
		}
	}
	#endif

	private static string PlatformFolder (RuntimePlatform platform)
	{
		switch (platform) {
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.WindowsWebPlayer:
		case RuntimePlatform.OSXWebPlayer:
			return "WebPlayer";
		default:
			Debug.LogWarning ("GetPlatformFolder " + platform.ToString ());
			return null;
		}
	}
}
