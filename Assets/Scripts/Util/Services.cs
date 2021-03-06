﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Services {
    public static GameManager GameManager { get; set; }
    public static EventManager EventManager { get; set; }
    public static TaskManager TaskManager { get; set; }
    public static PrefabDB Prefabs { get; set; }
    public static SceneStackManager<TransitionData> SceneStackManager { get; set; }
    public static InputManager InputManager { get; set; }
    public static MapManager MapManager { get; set; }
    public static Main main { get; set; }
    public static ItemConfig ItemConfig { get; set; }
    public static ZoneConfig ZoneConfig { get; set; }
    public static UIManager UIManager { get; set; }
}
