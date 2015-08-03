using UnityEngine;
using System.Collections;

public class GameHUD : MonoBehaviour {

	public GUISkin Skin;

	public void OnGUI()
	{
		GUI.skin = Skin;
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		{
			GUILayout.BeginVertical(Skin.GetStyle("GameHud"));
			{
				GUILayout.Label(string.Format("Points: {0}",GameManager.Instace.Points),Skin.GetStyle("PointsText"));
				var time = LevelManager.Instace.RunningTime;
				GUILayout.Label(string.Format(
					"{0:00}:{1:00} with {2} bonus",
				    time.Minutes + (time.Hours * 60),
				    time.Seconds,
				    LevelManager.Instace.CurruntTimeBonus),Skin.GetStyle("TimeText"));
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndArea ();
	}
}
