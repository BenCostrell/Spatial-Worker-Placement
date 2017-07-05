﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Tile
{
    public readonly Hex hex;
    public GameObject obj;

    public Tile(Hex hex_)
    {
        hex = hex_;
        obj = GameObject.Instantiate(Services.Prefabs.Tile, hex.ScreenPos(Services.MapManager.layout), Quaternion.identity);
    }
}