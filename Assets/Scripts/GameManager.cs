using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public GameObject sceneRoot;

    public int numPlayers;
    [HideInInspector]
    public List<Player> players;
    public Vector3[] workerSpawns;
    public Color[] playerColors;
    public string[] playerNames;

	void Awake()
    {
        InitializeServices();
    }
    
    // Use this for initialization
	void Start () {
        InitializePlayers();
        Services.EventManager.Register<Reset>(Reset);
        Services.SceneStackManager.PushScene<TitleScreen>();
    }
	
	// Update is called once per frame
	void Update () {
        Services.InputManager.GetInput();
        Services.TaskManager.Update();
	}

    void InitializeServices()
    {
        Services.GameManager = this;
        Services.EventManager = new EventManager();
        Services.TaskManager = new TaskManager();
        Services.Prefabs = Resources.Load<PrefabDB>("Prefabs/Prefabs");
        Services.SceneStackManager = new SceneStackManager<TransitionData>(sceneRoot, Services.Prefabs.Scenes);
        Services.InputManager = new InputManager();
        Services.ItemConfig = Resources.Load<ItemConfig>("ItemConfig/Item Config");
    }

    void InitializePlayers()
    {
        players = new List<Player>();
        for (int i = 0; i < numPlayers; i++) players.Add(new Player(i + 1));
    }

    void Reset(Reset e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //TaskTree TestBranch(int branchNum)
    //{
    //    TaskTree testBranch = new TaskTree(
    //        new DebugTask("starting test branch " + branchNum),
    //        new TaskTree(new DebugTask("immediate test branch " + branchNum + " child 1")),
    //        new TaskTree(new DebugTask("immediate test branch " + branchNum + " child 2")),
    //        new TaskTree(new DebugTask("immediate test branch " + branchNum + " child 3"), 
    //            new TaskTree(new WaitTask(1f), 
    //                new TaskTree(new DebugTask("delayed test branch " + branchNum  + " child 3 subchild"))))
    //        );
    //    return testBranch;
    //}
}

//public class DebugTask : Task
//{
//    public string debugMessage;

//    public DebugTask(string _debugMessage)
//    {
//        debugMessage = _debugMessage;
//    }

//    protected override void Init()
//    {
//        Debug.Log(debugMessage + " at time " + Time.time);
//        SetStatus(TaskStatus.Success);
//    }
//}