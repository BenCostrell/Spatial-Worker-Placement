using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : Scene<TransitionData> {

	// Use this for initialization
	void Start () {
        Services.EventManager.Register<ButtonPressed>(StartGame);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void StartGame(ButtonPressed e)
    {
        Services.EventManager.Unregister<ButtonPressed>(StartGame);
        Services.SceneStackManager.Swap<Main>();
    }
}
