﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject {
    [SerializeField]
    private GameObject[] scenes;
    public GameObject[] Scenes { get { return scenes; } }

    [SerializeField]
    private GameObject tile;
    public GameObject Tile { get { return tile; } }

    [SerializeField]
    private GameObject worker;
    public GameObject Worker { get { return worker; } }

    [SerializeField]
    private GameObject selector;
    public GameObject Selector { get { return selector; } }

    [SerializeField]
    private GameObject resource;
    public GameObject Resource { get { return resource; } }

    [SerializeField]
    private GameObject item;
    public GameObject Item { get { return item; } }

    [SerializeField]
    private GameObject building;
    public GameObject Building { get { return building; } }

    [SerializeField]
    private GameObject resourceGainText;
    public GameObject ResourceGainText { get { return resourceGainText; } }

    [SerializeField]
    private GameObject tooltip;
    public GameObject Tooltip { get { return tooltip; } }

    [SerializeField]
    private GameObject itemTooltip;
    public GameObject ItemTooltip { get { return itemTooltip; } }

    [SerializeField]
    private GameObject buildingTooltip;
    public GameObject BuildingTooltip { get { return buildingTooltip; } }

    [SerializeField]
    private GameObject zoneTooltip;
    public GameObject ZoneTooltip { get { return zoneTooltip; } }

    [SerializeField]
    private GameObject turnMarker;
    public GameObject TurnMarker { get { return turnMarker; } }

    [SerializeField]
    private GameObject roundMarker;
    public GameObject RoundMarker { get { return roundMarker; } }

    [SerializeField]
    private GameObject towerTrackerElement;
    public GameObject TowerTrackerElement { get { return towerTrackerElement; } }
}
