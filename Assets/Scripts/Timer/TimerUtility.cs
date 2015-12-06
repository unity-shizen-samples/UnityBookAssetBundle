using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 時間処理 Utilityクラス
/// </summary>
public static class TimerUtility
{
	/// <summary>
	/// コルーチンに制限時間を付ける
	/// 制限時間を超えた場合は、強制的に yield break する
	/// 
	/// 使用例:
	/// yield return StartCoroutine(component.CoroutineTimeOutCheck(coroutine, 5f));
	/// 
	/// </summary>
	/// <returns>The time out check.</returns>
	/// <param name="component">Component.</param>
	/// <param name="coroutine">Coroutine.</param>
	/// <param name="limitTime">Limit time.</param>
	public static IEnumerator CoroutineTimeOutCheck (this MonoBehaviour component, IEnumerator coroutine, float limitTime)
	{
		DateTime startTime = DateTime.Now;
		component.StartCoroutine (coroutine);
		while (coroutine.MoveNext ()) {
			if (startTime.IsElapsedTime (limitTime) == false) {
				Debug.LogError ("coroutine Force Break");
				yield break;
			}
			yield return null;
		}
	}

	/// <summary>
	/// 経過時間チェック falseで経過した
	/// Updateやコルーチンの「ループ内」で使用する
	/// 
	/// 使用例:
	/// bool isbool = startTime.IsElapsedTime(checkTime);
	/// 
	/// </summary>
	/// <returns><c>true</c> if this instance is elapsed time the specified startTime checkTime; otherwise, <c>false</c>.</returns>
	/// <param name="startTime">Start time.</param>
	/// <param name="checkTime">Check time.</param>
	public static bool IsElapsedTime (this DateTime startTime, float checkTime)
	{
		return (DateTime.Now - startTime).TotalSeconds < checkTime;
	}
}
