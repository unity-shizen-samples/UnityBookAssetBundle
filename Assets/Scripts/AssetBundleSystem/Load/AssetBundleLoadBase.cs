using UnityEngine;
using System.Collections;

/// <summary>
/// AssetBundleLoader基底クラス
/// </summary>
public abstract class AssetBundleLoadBase : IEnumerator
{
	public object Current {
		get {
			return null;
		}
	}

	/// <summary>
	/// Moves the next.
	/// falseになると、コルーチンが終了する
	/// </summary>
	/// <returns><c>true</c>, if next was moved, <c>false</c> otherwise.</returns>
	public bool MoveNext ()
	{
		return IsDone () == false;
	}

	public void Reset ()
	{
	}

	public abstract bool LoadRequest ();

	public abstract bool IsDone ();

	/// <summary>
	/// Assetの取得
	/// </summary>
	/// <returns>The asset.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public abstract T GetAsset<T> () where T : UnityEngine.Object;
}