using UnityEngine;
using System.Collections;
using System;
using System.Text;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;
using SimpleJSON;

public struct turn_data {
	string player_id;
	int score;
}

public class MultiplayerController : MonoBehaviour {


	bool mWaitingForAuth = true;

	private const int MinOpponents = 1;
	private const int MaxOpponents = 7;
	const int Variant = 0;

	private TurnBasedMatch currentMatch;

	private bool firstTurn = true;
	private bool isPlayerTwo = false;

	private bool turnIsFinished = false;
	private bool mWaiting = false;

	// Use this for initialization
	void Start () {
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			.EnableSavedGames()
			.WithInvitationDelegate(handleInvitation)
			.WithMatchDelegate(handleTurnBasedNotification)
				.Build ();

		PlayGamesPlatform.InitializeInstance(config);
		PlayGamesPlatform.DebugLogEnabled = true;

		GooglePlayGames.PlayGamesPlatform.Activate();
		if(!Social.localUser.authenticated) {
			mWaitingForAuth = true;
			Social.localUser.Authenticate((bool success) => {
				mWaitingForAuth = false;
				if(success) {
					Debug.Log ("Success in authenticating");
				} else {
					Debug.Log ("Did not authenticate successfully");
				}
			});
		}
//		if(!Social.localUser.authenticated) {
//			mWaitingForAuth = true;
//			Social.localUser.Authenticate((bool success) => {
//				mWaitingForAuth = false;
//				if(success) {
//					Debug.Log ("Success in authenticating");
//				} else {
//					Debug.Log ("Did not authenticate successfully");
//				}
//			});
//		} else {
//			PlayGamesPlatform.Instance.TurnBased.CreateWithInvitationScreen(MinOpponents, MaxOpponents, Variant, OnMatchStarted);
//		}
	}

	// Update is called once per frame
	void Update () {

	}

	public void onPlayClick() {
		Debug.Log ("Inside onPlayClick");
		if(!Social.localUser.authenticated) {
			mWaitingForAuth = true;
			Social.localUser.Authenticate((bool success) => {
				mWaitingForAuth = false;
				if(success) {
					Debug.Log ("Success in authenticating");
				} else {
					Debug.Log ("Did not authenticate successfully");
				}
			});
		} else {
			PlayGamesPlatform.Instance.TurnBased.CreateWithInvitationScreen(MinOpponents, MaxOpponents, Variant, OnMatchStarted);
		}
	}

	void handleInvitation(Invitation invitation, bool shouldAutoAccept) {
		Debug.Log ("Handle Invite");
		isPlayerTwo = true;
		PlayGamesPlatform.Instance.TurnBased.AcceptInvitation(invitation.InvitationId, OnMatchStarted);
	}

	void handleTurnBasedNotification(TurnBasedMatch t, bool b) {
		Debug.Log ("Handle Turn Notification");
		if(turnIsFinished && !(mWaiting)) {
			// Finish game here
			var d = t.Data;

			int other_score = BitConverter.ToInt32(d, 0);
			Debug.Log ("Score===> " + other_score);
			if(t.TurnStatus == TurnBasedMatch.MatchTurnStatus.MyTurn) {
				finishMatch(t, other_score);
			}
//			Application.LoadLevel("Finish");
		} else {
			mWaiting = false;
		}
	}

	private void finishMatch(TurnBasedMatch match, int other_score) {
		MatchOutcome outcome = new MatchOutcome();

		var score = PlayerPrefs.GetInt("score");
		bool selfWon = false;
		if(score < other_score) {
			selfWon = true;
		}
		foreach(Participant p in match.Participants) {
			MatchOutcome.ParticipantResult result;
			uint placement;
			if(p.ParticipantId.Equals(match.SelfParticipantId)) {
				if(selfWon) { 
					result = MatchOutcome.ParticipantResult.Win;
					placement = 1;
				} else {
					result = MatchOutcome.ParticipantResult.Loss;
					placement = 2;
				}
			} else {
				if(selfWon) {
					result = MatchOutcome.ParticipantResult.Loss;
					placement = 2;
				} else {
					result = MatchOutcome.ParticipantResult.Win;
					placement = 1;
				}
			}

			var gm = GameObject.Find ("MPController");
			GameObject.DontDestroyOnLoad(gm);

			outcome.SetParticipantResult(p.ParticipantId, result, placement);
		}

		string winner;

		if(selfWon && !(isPlayerTwo)) {
			winner = "Player One Wins!";
		} else {
			winner = "Player Two Wins!";
		}

		PlayerPrefs.SetString("winner", winner);
		Debug.Log ("In finish, winner ==> " + winner);
		Byte[] finalData = Encoding.ASCII.GetBytes(winner);

		PlayGamesPlatform.Instance.TurnBased.Finish (match, finalData, outcome, (bool success) => {
			if(success) {
				Debug.Log ("Game over!");
				StartCoroutine(resetMap());

				var myout = outcome.GetResultFor(match.SelfParticipantId);
				if(myout == MatchOutcome.ParticipantResult.Win) {
					PlayerPrefs.SetString("winner", "You Won, score==> " + score);
				} else {
					PlayerPrefs.SetString("winner", "You Lost, score==> " + score);
				}
				Application.LoadLevel("gameover");

			}
		});
	}

	private IEnumerator resetMap() {
		WWWForm form = new WWWForm();
		form.AddField("playerid", PlayerPrefs.GetString("playerId"));
		form.AddField("status", -1);
		form.AddField("score", PlayerPrefs.GetInt("score"));
		
		WWW request = new WWW("http://107.170.10.115:3000/rooms/" + PlayerPrefs.GetString("roomId"), form);
		Debug.Log("Sending request to http://107.170.10.115:3000/rooms/" + PlayerPrefs.GetString("roomId"));
		Debug.Log("PlayerId=" + PlayerPrefs.GetString("playerId"));
		Debug.Log("Score=" + PlayerPrefs.GetInt("score"));
		
		yield return request;
		
		if (request.error == null) {
			Debug.Log("Updated room successfuly");
			var room = JSONNode.Parse(request.text);
			var roomStatus = room["isFinished"].AsBool;
			Debug.Log("Room is finished: " + roomStatus);
		}
	}

	private string DecideWhoIsNext(TurnBasedMatch match) {
		if(match.AvailableAutomatchSlots > 0) {
			return null;
		}
		foreach(Participant p in match.Participants) {
			if(!p.ParticipantId.Equals(match.SelfParticipantId)) {
				return p.ParticipantId;
			}
		}

		return null;
	}

	private string GetOtherPlayerID(TurnBasedMatch match) {
		foreach(Participant p in match.Participants) {
			if(!p.ParticipantId.Equals(match.SelfParticipantId)) {
				return p.Player.PlayerId;
			}
		}
		return null;
	}
	public void acceptInvite() {
		Debug.Log ("Looking Invites");
		PlayGamesPlatform.Instance.TurnBased.AcceptFromInbox(OnMatchStarted);
	}

	public void takeTurn() {
		Debug.Log ("Inside Take Turn");
		var score = PlayerPrefs.GetInt("score");
		Debug.Log ("Inside take turn, score is ==> " + score);
		byte[] scoreInBytes = BitConverter.GetBytes(score);

		string whoIsNext = DecideWhoIsNext(currentMatch);

		PlayGamesPlatform.Instance.TurnBased.TakeTurn(currentMatch, scoreInBytes, whoIsNext, (bool success) => {
			if(success) {
				Debug.Log ("Turn taken successfully");
				turnIsFinished = true;
				mWaiting = true;
			}
		});

	}

	void OnMatchStarted(bool success, TurnBasedMatch match) {
		if(success) {
//			byte[] myData = null;

			currentMatch = match;

			if(isPlayerTwo) {
				Debug.Log ("Player two setup");
				var pOneID = GetOtherPlayerID(match);
				var pTwoID = match.Self.Player.PlayerId;
//				var pOneID = DecideWhoIsNext(match);
//				var pTwoID = match.SelfParticipantId;
				PlayerPrefs.SetString("pOneID", pOneID);
				PlayerPrefs.SetString ("pTwoID", pTwoID);
				
				PlayerPrefs.SetString("playerId", pTwoID);
			} else if(firstTurn) {
				Debug.Log ("Player one setup");
				firstTurn = false;
				var pOneID = match.Self.Player.PlayerId;
				var pTwoID = GetOtherPlayerID(match);
//				var pOneID = match.SelfParticipantId;
//				var pTwoID = DecideWhoIsNext(match);
				PlayerPrefs.SetString("pOneID", pOneID);
				PlayerPrefs.SetString("pTwoID", pTwoID);

				PlayerPrefs.SetString("playerId", pOneID);

			}

			if(success && match.Status == TurnBasedMatch.MatchStatus.Complete) {
				PlayGamesPlatform.Instance.TurnBased.AcknowledgeFinished(match, (bool ack_success) => {
					if(ack_success) {
						var d = match.Data;

						var winner = Encoding.ASCII.GetString(d);
						if(winner.Equals("Player Two Wins!")) {
							winner = "You win, score==>" + PlayerPrefs.GetInt("score");
						} else {
							winner = "You lose, score==>" + PlayerPrefs.GetInt("score");
						}
						PlayerPrefs.SetString("winner", winner);
						Debug.Log ("Acknowledged Game is over");
						Application.LoadLevel("gameover");
					}
				});
			}

			Debug.Log ("Successfully Invited Someone");
//			string whoIsNext = DecideWhoIsNext(match);
			GameObject mpController = GameObject.Find("MPController");
			GameObject.DontDestroyOnLoad(mpController);
			Application.LoadLevel("one");
//			PlayGamesPlatform.Instance.TurnBased.TakeTurn(match, myData, whoIsNext, (bool successPlay) => {
//				if(successPlay) {
//					Debug.Log ("Stuff turn done");
//				} else {
//					Debug.Log ("asdfasdfasdf");
//				}
//			});
		} else {
			Debug.Log ("Fucking dumbass");
		}
	}
}
