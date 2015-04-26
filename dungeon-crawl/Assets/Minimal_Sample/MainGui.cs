/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Multiplayer;

public class MainGui : MonoBehaviour {
    private const float FontSizeMult = 0.05f;
    private bool mWaitingForAuth = false;
    private string mStatusText = "Ready.";
	const int MinOpponents = 1;
	const int MaxOpponents = 7;
	const int Variant = 0;
    void Start () {

		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			// enables saving game progress.
			.EnableSavedGames()
				// registers a callback to handle game invitations received while the game is not running.
				.WithInvitationDelegate(handleInvitation)
				// registers a callback for turn based match notifications received while the
				// game is not running.
				.WithMatchDelegate(handleTurnBasedNotification)
				.Build();
		
		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;

        // Select the Google Play Games platform as our social platform implementation
        GooglePlayGames.PlayGamesPlatform.Activate();
    }

    void OnGUI() {
        GUI.skin.button.fontSize = (int)(FontSizeMult * Screen.height);
        GUI.skin.label.fontSize = (int)(FontSizeMult * Screen.height);

        GUI.Label(new Rect(20, 20, Screen.width, Screen.height * 0.25f),
                  mStatusText);

        Rect buttonRect = new Rect(0.25f * Screen.width, 0.10f * Screen.height,
                          0.5f * Screen.width, 0.25f * Screen.height);
        Rect imageRect = new Rect(buttonRect.x+buttonRect.width/4f,
                                  buttonRect.y + buttonRect.height * 1.1f,
                                  buttonRect.width/2f, buttonRect.width/2f);

		Rect inboxRect = new Rect(imageRect.x + imageRect.width/4f,
		                          imageRect.y + imageRect.height * 1.1f,
		                          imageRect.width/2f, imageRect.width/2f);

        if (mWaitingForAuth) {
            return;
        }

        string buttonLabel;
		string inboxLabel;

        if (Social.localUser.authenticated) {
          buttonLabel = "Sign Out";
			inboxLabel = "Check Inbox";
          if (Social.localUser.image != null) {
            GUI.DrawTexture(imageRect, Social.localUser.image,
                            ScaleMode.ScaleToFit);
          } else {
            GUI.Label(imageRect, "No image available");
          }
        } else {
          buttonLabel = "Authenticate";
          mStatusText = "Ready";
			inboxLabel = "No check";
        }

        if (GUI.Button(buttonRect, buttonLabel)) {
            if (!Social.localUser.authenticated) {
                // Authenticate
                mWaitingForAuth = true;
                mStatusText = "Authenticating...";
                Social.localUser.Authenticate((bool success) => {
                    mWaitingForAuth = false;
                    if (success) {
                      mStatusText = "Welcome " + Social.localUser.userName;
                    } else {
                      mStatusText = "Authentication failed.";
                    }
                });
            } else {
                // Sign out!
				PlayGamesPlatform.Instance.TurnBased.CreateWithInvitationScreen(MinOpponents, MaxOpponents, Variant, OnMatchStarted);
//                mStatusText = "Signing out.";
//                ((GooglePlayGames.PlayGamesPlatform) Social.Active).SignOut();
            }
        }

		if(GUI.Button(inboxRect, inboxLabel)) {
			if(!Social.localUser.authenticated) {
				mWaitingForAuth = true;
				Social.localUser.Authenticate((bool success) => {
					mWaitingForAuth = false;
					if(success) {
						mStatusText = "Welcome " + Social.localUser.userName + " You can check your inbox";
					} else {
						mStatusText = "Authentication failed, you cannot check your inbox";
					}
				});
			} else {
				PlayGamesPlatform.Instance.TurnBased.AcceptFromInbox(OnMatchStarted);
			}
		}
    }

	void handleInvitation(Invitation v, bool b) {
		Debug.Log("Handle Invite");
	}
	
	void handleTurnBasedNotification(TurnBasedMatch t, bool b) {
		Debug.Log ("Handle Turn Notification");
	}
	
	void OnMatchStarted(bool success, TurnBasedMatch match) {
		if (success) {
			// go to the game screen and play!
			Debug.Log ("Successfully Invited Someone");
		} else {
			// show error message
			Debug.Log ("Fucking dumbass");
		}
	}
}
