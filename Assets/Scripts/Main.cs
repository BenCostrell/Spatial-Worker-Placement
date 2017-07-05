using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : Scene<TransitionData> {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    internal override void Init()
    {
        InitializeMainServices();
        Services.MapManager.CreateHexGrid();
    }

    void InitializeMainServices()
    {
        Services.MapManager = Services.GameManager.sceneRoot.GetComponentInChildren<MapManager>();
    }
}
