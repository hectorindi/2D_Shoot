using UnityEngine;
using System.Collections;

public class CheckPoint : MonoBehaviour {
	
	public void Start () {
	
	}

	public void PlayerHitCheckPoint () {
		StartCoroutine (PlayerHitCheckPointCo (LevelManager.Instace.CurruntTimeBonus)); 
	}

	private IEnumerator PlayerHitCheckPointCo(int bonus)
	{
		FloatingText.Show ("check Point!!", "CheckPointText", new CenterTextPositioner (0.2f));

		yield return new WaitForSeconds(.5f);
		FloatingText.Show (string.Format("+{0} Time Bonus",bonus), "CheckPointText", new CenterTextPositioner (0.2f));
	}

	public void PlayerLeftCheckPoint()	
	{
	}

	public void spawnPlayer(Player player)
	{
		player.ReSpawnAt (transform);
	}

	public void AssignObjectToCheckPoint()
	{
	}
}
