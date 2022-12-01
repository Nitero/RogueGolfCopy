using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class dreamloLeaderBoard : MonoBehaviour {

	string dreamloWebserviceURL = "http://dreamlo.com/lb/";

	public bool IUpgradedAndGotSSL = false;
	public string privateCode = "";
	public string publicCode = "";
	
	string highScores = "";

	public Action EventRequestFinished;
	
	////////////////////////////////////////////////////////////////////////////////////////////////
	
	// A player named Carmine got a score of 100. If the same name is added twice, we use the higher score.
 	// http://dreamlo.com/lb/(your super secret very long code)/add/Carmine/100

	// A player named Carmine got a score of 1000 in 90 seconds.
 	// http://dreamlo.com/lb/(your super secret very long code)/add/Carmine/1000/90
	
	// A player named Carmine got a score of 1000 in 90 seconds and is Awesome.
 	// http://dreamlo.com/lb/(your super secret very long code)/add/Carmine/1000/90/Awesome
	
	////////////////////////////////////////////////////////////////////////////////////////////////
	
	
	public struct Score {
		public string playerName;
		public int score;
		public int seconds;
		public string shortText;
		public string dateString;
	}

	void Awake()
	{
		if (IUpgradedAndGotSSL) {
			dreamloWebserviceURL = "https://www.dreamlo.com/lb/";
		}
		else {
#if UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID
		Debug.LogWarning("dreamlo may require https for WEBGL / IOS / ANDROID builds.");
#endif
		}

		this.highScores = "";
	}
	
	public static dreamloLeaderBoard GetSceneDreamloLeaderboard()
	{
		var go = GameObject.Find("dreamloPrefab");
		
		if (go == null) 
		{
			Debug.LogError("Could not find dreamloPrefab in the scene.");
			return null;
		}

		return go.GetComponent<dreamloLeaderBoard>();
	}
	
	public void AddScore(string playerName, int totalScore)
	{
		playerName = Clean(playerName);
		StartCoroutine(GetRequest(dreamloWebserviceURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(playerName) + "/" + totalScore.ToString()));
	}
	public void AddScore(string playerName, int totalScore, int totalSeconds)
	{
		playerName = Clean(playerName);
		StartCoroutine(GetRequest(dreamloWebserviceURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(playerName) + "/" + totalScore.ToString()+ "/" + totalSeconds.ToString()));
	}
	public void AddScore(string playerName, int totalScore, int totalSeconds, string shortText)
	{
		playerName = Clean(playerName);
		shortText = Clean(shortText);
		StartCoroutine(GetRequest(dreamloWebserviceURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(playerName) + "/" + totalScore.ToString() + "/" + totalSeconds.ToString()+ "/" + shortText));
	}
	
	// This function saves a trip to the server. Adds the score and retrieves results in one trip.
	public void AddScoreWithPipe(string playerName, int totalScore)
	{
		playerName = Clean(playerName);
		StartCoroutine(GetRequest(dreamloWebserviceURL + privateCode + "/add-pipe/" + UnityWebRequest.EscapeURL(playerName) + "/" + totalScore.ToString()));
	}
	public void AddScoreWithPipe(string playerName, int totalScore, int totalSeconds)
	{
		playerName = Clean(playerName);
		StartCoroutine(GetRequest(dreamloWebserviceURL + privateCode + "/add-pipe/" + UnityWebRequest.EscapeURL(playerName) + "/" + totalScore.ToString()+ "/" + totalSeconds.ToString()));
	}
	public void AddScoreWithPipe(string playerName, int totalScore, int totalSeconds, string shortText)
	{
		playerName = Clean(playerName);
		shortText = Clean(shortText);
		StartCoroutine(GetRequest(dreamloWebserviceURL + privateCode + "/add-pipe/" + UnityWebRequest.EscapeURL(playerName) + "/" + totalScore.ToString() + "/" + totalSeconds.ToString()+ "/" + shortText));
	}
	
	public void GetScores()
	{
		highScores = "";
		StartCoroutine(GetRequest(dreamloWebserviceURL +  publicCode  + "/pipe"));
	}
	
	public void GetSingleScore(string playerName)
	{
		highScores = "";
		StartCoroutine(GetRequest(dreamloWebserviceURL +  publicCode  + "/pipe-get/" + UnityWebRequest.EscapeURL(playerName)));
	}

	IEnumerator GetRequest(string url)
	{
		// Something not working? Try copying/pasting the url into your web browser and see if it works.
		// Debug.Log(url);

		using (UnityWebRequest www = UnityWebRequest.Get(url))
		{
			yield return www.SendWebRequest();
			highScores = www.downloadHandler.text;
			EventRequestFinished?.Invoke();
		}
	}
	
	
	public string[] ToStringArray()
	{
		if (this.highScores == null) return null;
		if (this.highScores == "") return null;
		
		var rows = this.highScores.Split(new char[] {'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
		return rows;
	}
	
	public List<Score> ToListLowToHigh()
	{
		var scoreList = this.ToScoreArray();
		
		if (scoreList == null) return new List<Score>();
		
		var genericList = new List<Score>(scoreList);
			
		genericList.Sort((x, y) => x.score.CompareTo(y.score));
		
		return genericList;
	}
	
	public List<Score> ToListHighToLow()
	{
		var scoreList = this.ToScoreArray();
		
		if (scoreList == null) return new List<Score>();

		List<Score> genericList = new List<Score>(scoreList);
			
		genericList.Sort((x, y) => y.score.CompareTo(x.score));
		
		return genericList;
	}
	
	public Score[] ToScoreArray()
	{
		var rows = ToStringArray();
		if (rows == null) return null;
		
		int rowcount = rows.Length;
		
		if (rowcount <= 0) return null;
		
		var scoreList = new Score[rowcount];
		
		for (int i = 0; i < rowcount; i++)
		{
			var values = rows[i].Split(new char[] {'|'}, System.StringSplitOptions.None);
			
			var current = new Score();
			current.playerName = values[0];
			current.score = 0;
			current.seconds = 0;
			current.shortText = "";
			current.dateString = "";
			if (values.Length > 1) current.score = CheckInt(values[1]);
			if (values.Length > 2) current.seconds = CheckInt(values[2]);
			if (values.Length > 3) current.shortText = values[3];
			if (values.Length > 4) current.dateString = values[4];
			scoreList[i] = current;
		}
		
		return scoreList;
	}
	
	
	// Keep pipe and slash out of names
	
	string Clean(string s)
	{
		s = s.Replace("/", "");
		s = s.Replace("|", "");
		return s;
		
	}
	
	int CheckInt(string s)
	{
		int x = 0;
		
		int.TryParse(s, out x);
		return x;
	}
	
}