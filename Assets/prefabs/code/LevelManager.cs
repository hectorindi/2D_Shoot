using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class LevelManager : MonoBehaviour {

	public static LevelManager Instace { get; private set; }
	
	public Player Player { get; private set;}
	public CameraControl Camera { get; private set;}
	public TimeSpan RunningTime{get {return DateTime.UtcNow -_started;}}


	private List<CheckPoint> _checkPoints;
	private int _CurruntCheckPointIndex;
	private DateTime _started;
	private int _savedPoint;


	public CheckPoint DebugSpawn;
	public int BonusCutOffSeconds;
	public int BonusSecondMultiplier;

	public int CurruntTimeBonus 
	{
		get
		{
			var secondDiffrence = (int)(BonusCutOffSeconds - RunningTime.TotalSeconds);
			return Mathf.Max (0, secondDiffrence) * BonusSecondMultiplier;
		}
	}

	public void Awake()
	{
		Instace = this;
	}

	public void Start()
	{
		_checkPoints = FindObjectsOfType<CheckPoint> ().OrderBy (t => t.transform.position.x).ToList ();
		_CurruntCheckPointIndex = _checkPoints.Count > 0 ? 0 : -1;

		Player = FindObjectOfType<Player>();
		Camera = FindObjectOfType<CameraControl>();

		_started = DateTime.UtcNow;

#if UNITY_EDITOR	
		if(DebugSpawn != null)
			DebugSpawn.spawnPlayer(Player);
		else if (_CurruntCheckPointIndex != -1)
			_checkPoints[_CurruntCheckPointIndex].spawnPlayer(Player);
#else
		if(_CurruntCheckPointIndex != -1)
			_checkPoints[_CurruntCheckPointIndex].spawnPlayer(Player);
#endif
	}

	public void Update()
	{
		var isAtLastCheckPoint = _CurruntCheckPointIndex + 1 > _checkPoints.Count;
		if (isAtLastCheckPoint)
			return;
		var distenceToNextCheckPoint = _checkPoints [_CurruntCheckPointIndex + 1].transform.position.x - Player.transform.position.x;
		if (distenceToNextCheckPoint >= 0)
			return;
		_checkPoints [_CurruntCheckPointIndex].PlayerLeftCheckPoint ();
		_CurruntCheckPointIndex++;
		_checkPoints [_CurruntCheckPointIndex].PlayerHitCheckPoint ();

		// TODO TimeBonus
		GameManager.Instace.AddPoints (CurruntTimeBonus);
		_savedPoint = GameManager.Instace.Points;
		_started = DateTime.UtcNow;
	}

	public void KillPlayer()
	{
		StartCoroutine (KillPlayerCo());
	}

	private IEnumerator KillPlayerCo()
	{
		Player.Kill ();
		Camera.IsFollowing = false;
		yield return new WaitForSeconds(2f);

		Camera.IsFollowing = true;
		if (_CurruntCheckPointIndex != -1)
			_checkPoints [_CurruntCheckPointIndex].spawnPlayer (Player);

		_started = DateTime.UtcNow;
		GameManager.Instace.ResetPoints (_savedPoint);
	}
}
