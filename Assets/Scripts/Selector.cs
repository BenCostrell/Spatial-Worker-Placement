using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{

    [HideInInspector]
    public Tile currentSelectedTile;
    private int inputLastFrame;
    public float timeToWaitBeforeRepeatingInput;
    private float timeSinceLastUniqueInput;

    // Use this for initialization
    void Start()
    {
        timeSinceLastUniqueInput = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    public void PlaceOnTile(Tile tile)
    {
        transform.position = tile.hex.ScreenPos();
        currentSelectedTile = tile;
    }

    void ProcessInput()
    {
        float xInput = Input.GetAxis("Horizontal_P" + Services.main.currentActivePlayer.playerNum);
        float yInput = Input.GetAxis("Vertical_P" + Services.main.currentActivePlayer.playerNum);
        float mag = new Vector2(xInput, yInput).magnitude;
        if (mag < 0.1f) inputLastFrame = -1;
        else 
        {
            float angle = Mathf.Atan2(yInput, xInput) * Mathf.Rad2Deg;
            float convertedAngle = (angle + 330) % 360;
            int directionIndex = 5 - Mathf.FloorToInt(convertedAngle / 60f);
            if (directionIndex != inputLastFrame || timeSinceLastUniqueInput > timeToWaitBeforeRepeatingInput)
            {
                Tile tile;
                if (Services.MapManager.map.TryGetValue(currentSelectedTile.hex.Neighbor(directionIndex), out tile))
                {
                    PlaceOnTile(tile);
                    Debug.Log("raw angle: " + angle);
                    Debug.Log("converted angle: " + convertedAngle);
                    Debug.Log("direction: " + directionIndex);
                }
                timeSinceLastUniqueInput = 0;
            }
            else timeSinceLastUniqueInput += Time.deltaTime;
            inputLastFrame = directionIndex;
        }
    }
}