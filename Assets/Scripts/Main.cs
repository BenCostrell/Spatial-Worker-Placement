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
        PlaceInitialWorkers();
    }

    void InitializeMainServices()
    {
        Services.MapManager = Services.GameManager.sceneRoot.GetComponentInChildren<MapManager>();
    }

    void PlaceInitialWorkers()
    {
        for (int i = 0; i < Services.GameManager.players.Count; i++)
        {
            for (int j = 0; j < Services.GameManager.numInitialWorkers; j++)
            {
                Vector3 spawnPoint = Services.GameManager.workerSpawns[i+(j*2)];
                Services.GameManager.players[i].AddWorker(
                    Services.MapManager.map[new Hex(
                        Mathf.RoundToInt(spawnPoint.x),
                        Mathf.RoundToInt(spawnPoint.y),
                        Mathf.RoundToInt(spawnPoint.z))]);
            }
            
        }
    }
}
