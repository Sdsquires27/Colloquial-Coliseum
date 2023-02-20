using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;

public class LevelScript : MonoBehaviour
{
    private Tilemap tilemap;
    private PlayerController curPlayer;
    private GameObject actionHolder;

    private Vector3Int selectedTile;
    private Vector3Int hoveredTile;
    private bool onTilemap = false;
    private bool tileSelected = false;
    Vector2 mousePos;

    private List<Vector3Int> objectPos = new List<Vector3Int>();
    private List<TileObject> tileObjects = new List<TileObject>();
    private TileObject selectedObj;

    private List<Vector3Int> placeables = new List<Vector3Int>();
    private Unit unitToPlace;
    public GameObject tileObjectPrefab;
    public TextMeshProUGUI text;
    public GameObject actionImagePrefab;


    // Start is called before the first frame update
    void Start()
    {
        actionHolder = GameObject.FindGameObjectWithTag("ActionHolder");
    }

    public void startPhase(List<Unit[]> unitLists, GameManager gameManager)
    {
        int playerNum = unitLists.Count;
        GameObject grid = GameObject.FindGameObjectWithTag("Grid");
        Tilemap[] tilemaps = Resources.LoadAll<Tilemap>("Tilemaps/" + playerNum);
        tilemap = Instantiate(tilemaps[Random.Range(0, tilemaps.Length)], grid.transform);

        StartCoroutine(placeUnits(unitLists, gameManager));
    }

    private IEnumerator placeUnits(List<Unit[]> unitLists, GameManager gameManager)
    {
        int i = 0;

        foreach (Unit[] unitList in unitLists)
        {
            placeables = tilemap.GetComponent<TilemapScript>().getVector3List(i);
            curPlayer = gameManager.currentPlayer(i);
            foreach(Unit unit in unitList)
            {

                unitToPlace = unit;
                yield return new WaitUntil(() => unitToPlace == null);
            }
            i++;

        }
        placeables.Clear();
        gameManager.phaseThreeTurn();
    }

    public void playTurn(PlayerController player)
    {
        foreach(TileObject tileObject in tileObjects)
        {
            if (tileObject.playerController == player)
            {
                tileObject.startTurn();
            }
            else
            {
                tileObject.deactivate();
            }
        }

    }

    public void nextTurn()
    {
        GameManager.instance.phaseThreeTurn();
    }

    public void endGame()
    {
        GameManager.instance.endGame();
    }

    public void onHover(InputAction.CallbackContext context)
    {
        Camera camera = FindObjectOfType<Camera>(); // grab the camera
        mousePos = context.ReadValue<Vector2>();
        RaycastHit2D hit = Physics2D.Raycast(camera.ScreenToWorldPoint(mousePos), Vector2.down); // get the ray hit based off of position

        if (hit.collider != null) // if Raycast hit something
        {
            if (hit.collider.GetComponent<Tilemap>()) // if hit was a tilemap
            {
                onTilemap = true;
                Vector3Int cellPos = tilemap.WorldToCell(hit.point); // get the cell position of the cell hit
                hoveredTile = cellPos;
 
            }
        }
        else // if the tilemap exists but nothing was hit, reset the colors
        {
            onTilemap = false;
        }
    }

    void clickFunction()
    {
        Camera camera = FindObjectOfType<Camera>(); // grab the camera
        RaycastHit2D hit = Physics2D.Raycast(camera.ScreenToWorldPoint(mousePos), Vector2.down); // get the ray hit based off of position

        if (hit.collider != null) // if Raycast hit something
        {
            if (hit.collider.GetComponent<Tilemap>()) // if hit was a tilemap
            {
                tilemap = hit.collider.GetComponent<Tilemap>(); // get the tilemap being hit
                Vector3Int cellPos = tilemap.WorldToCell(hit.point); // get the cell position of the cell hit

                // if there are limited placeable tiles, the cell clicked on is placeable, and the cell clicked on does not already have something
                if (placeables != null && placeables.Contains(cellPos) && !objectPos.Contains(cellPos)) // placeables will only have a count when placing units
                {
                    // generate the unit, add it to the lists.
                    GameObject x = Instantiate(tileObjectPrefab);
                    x.GetComponent<TileObject>().unit = unitToPlace;
                    x.GetComponent<TileObject>().playerController = curPlayer;
                    unitToPlace = null;
                    tileObjects.Add(x.GetComponent<TileObject>());
                    objectPos.Add(cellPos);
                }
                else
                {
                    if (selectedObj != null && selectedObj.canAttack && selectedObj.isAoe)
                    {
                        objectPos[tileObjects.IndexOf(selectedObj)] = farthestTileInRange(objectPos[tileObjects.IndexOf(selectedObj)], cellPos, selectedObj.currentRange, selectedObj.attackRange);
                        selectedObj.useAction(objectPos[tileObjects.IndexOf(selectedObj)], curPlayer, tileObjects[objectPos.IndexOf(selectedTile)]);
                        selectedObj.deactivate();

                        PlayerController winningPlayer = selectedObj.playerController; // the winner if every single tile has the same player controller
                        bool playerWon = true;
                        List<TileObject> winningTiles = new List<TileObject>();

                        tileSelected = false;
                        selectedObj = null;

                        List<TileObject> toRemove = new List<TileObject>();

                        foreach (TileObject tileObject in tileObjects)
                        {
                            if (!tileObject.gameObject.activeSelf)
                            {

                                objectPos.RemoveAt(tileObjects.IndexOf(tileObject));
                                toRemove.Add(tileObject);

                            }
                            else
                            {

                                playerWon = tileObject.playerController == winningPlayer ? playerWon : false;
                                if (playerWon) winningTiles.Add(tileObject);

                            }
                        }

                        foreach (TileObject tileObject in toRemove)
                        {
                            tileObjects.Remove(tileObject);
                        }

                        if (playerWon)
                        {
                            FindObjectOfType<GameManager>().endPhaseThree(winningTiles.ToArray());
                        }
                    }
                    // if the player just clicked on the selected tile
                    if (selectedTile == cellPos && tileSelected)
                    {
                        // unselect the selected tile
                        tileSelected = false;
                        selectedObj = null;
                    }
                    else
                    {
                        // set the new selected tile
                        selectedTile = cellPos;
                        tileSelected = true;

                        bool hasObj = selectedObj != null; // if there was already an object selected
                        bool noOthers = !objectPos.Contains(cellPos); // if there isn't already an object there
                        bool withinRange = tileWithinRange(objectPos[tileObjects.IndexOf(selectedObj)], selectedTile, selectedObj.currentRange); // if the tile is within range

                        if (hasObj && noOthers && withinRange)
                        {
                            // move the tile to the new position
                            selectedObj.currentRange -= distanceBetween(cellPos, objectPos[tileObjects.IndexOf(selectedObj)]);
                            objectPos[tileObjects.IndexOf(selectedObj)] = cellPos;
                            tileSelected = false;
                            selectedObj = null;

                        }
                        else if (!noOthers && tileWithinRange(objectPos[tileObjects.IndexOf(selectedObj)], selectedTile, selectedObj.currentRange + selectedObj.attackRange))
                        {
                            if (selectedObj.canAttack && tileObjects[objectPos.IndexOf(selectedTile)].playerController != selectedObj.playerController)
                            {
                                int attackRange = selectedObj.attackRange;
                                selectedObj.useAction(objectPos[tileObjects.IndexOf(selectedObj)], curPlayer, tileObjects[objectPos.IndexOf(selectedTile)]);
                                objectPos[tileObjects.IndexOf(selectedObj)] = farthestTileInRange(objectPos[tileObjects.IndexOf(selectedObj)], cellPos, selectedObj.currentRange, attackRange);
                                selectedObj.deactivate();

                                PlayerController winningPlayer = selectedObj.playerController; // the winner if every single tile has the same player controller
                                bool playerWon = true;
                                List<TileObject> winningTiles = new List<TileObject>();

                                tileSelected = false;
                                selectedObj = null;

                                List<TileObject> toRemove = new List<TileObject>();

                                foreach (TileObject tileObject in tileObjects)
                                {
                                    if (!tileObject.gameObject.activeSelf)
                                    {

                                        objectPos.RemoveAt(tileObjects.IndexOf(tileObject));
                                        toRemove.Add(tileObject);
                                        
                                    }
                                    else
                                    {
                                        playerWon = tileObject.playerController == winningPlayer ? playerWon : false;
                                        if (playerWon) winningTiles.Add(tileObject);

                                    }
                                }

                                foreach (TileObject tileObject in toRemove)
                                {
                                    tileObjects.Remove(tileObject);
                                }

                                if (playerWon)
                                {
                                    FindObjectOfType<GameManager>().endPhaseThree(winningTiles.ToArray());
                                }

                            }

                        }
                        else
                        {
                            selectedObj = null;
                        }
                    }
                }
            }
        }
    }

    public void onClick(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            clickFunction();
        }

    }

    Vector3Int farthestTileInRange(Vector3Int curPos, Vector3Int movePos, int range, int attackRange)
    {
        // finds and returns the farthest legal tile to a targeted tile within a certain range of a point.
        // used for moving closer when attacker clicks straight on object rather than moving closer themselves.

        // closest tile to return
        Vector3Int closestTile = new Vector3Int();

        // list of tiles that are within the range of the moving unit
        List<Vector3Int> tiles = tilesInRange(curPos, range);
        List<Vector3Int> availableAttackTiles = tilesInRange(movePos, attackRange);


        //remove available attack tiles that can't be moved to
        List<Vector3Int> tilesToRemove = new List<Vector3Int>();
        foreach (Vector3Int tile in availableAttackTiles)
        {
            if (!tiles.Contains(tile))
            {
                tilesToRemove.Add(tile);
            }
        }
        foreach(Vector3Int tile in tilesToRemove)
        {
            availableAttackTiles.Remove(tile);
        }

        // ? allows the float to be set to null. Shortest distance found up to this point in code
        float longestDistance = 0;

        // a list of all farthest distances (will go up to 4)

        // cycle through tiles that are possible
        foreach(Vector3Int tile in availableAttackTiles)
        {
            // find distance between the two tiles
            float distance = Vector3Int.Distance(tile, movePos);
            if (distance != 0) // if it is 0, we don't want it because that's not a space they can go to.
            {
                // if this is the new shortest distance
                if (distance > longestDistance)
                {
                    // reset the list and make this the new shortest distance

                    longestDistance = distance;
                    closestTile = tile;
                }
            }
        }


        return closestTile;
    }

    int distanceBetween(Vector3Int pos1, Vector3Int pos2)
    {
        // returns distance between two points in grid movement.
        Vector3Int differencePos = pos1 - pos2;
        return Mathf.Abs(differencePos.x) + Mathf.Abs((differencePos.y));
    }

    bool tileWithinRange(Vector3Int startPos, Vector3Int testPos, int range)
    {
        // returns if the a tile is within the range of another tile.

        bool inRange = false;

        List<Vector3Int> tiles = tilesInRange(startPos, range);

        if (tiles.Contains(testPos))
        {
            inRange = true;
        }

        return inRange;
    }


    // Update is called once per frame
    void Update()
    {
        // controlling the tilemap
        if (tilemap != null)
        {

            tilemap.RefreshAllTiles(); // reset all tiles

            if(placeables.Count != 0)
            {


                foreach (Vector3Int placeable in placeables)
                {
                    tilemap.SetColor(placeable, Color.blue); // set the color of the tile            }
                }
            }


                // if there is a list of tile objects and their positions, find where they are
                if (tileObjects != null && objectPos != null)
            {
                for (int i = 0; i < tileObjects.Count; i++)
                {
                    TileObject tileObject = tileObjects[i];
                    tileObject.gameObject.transform.position = tilemap.GetCellCenterWorld(objectPos[i]);

                    if (objectPos[i] == selectedTile && tileSelected) // if there is a tile selected and it lines up with an object
                    {
                        selectedObj = tileObject; // the selected object is that object
                    }
                }


            }
            // if there is a selected object
            if (selectedObj != null)
            {
                int range = selectedObj.currentRange + selectedObj.attackRange;

                // grab the range, highlight the tiles within it
                List<Vector3Int> highlightTiles = tilesInRange(objectPos[tileObjects.IndexOf(selectedObj)], range);
                foreach (Vector3Int tile in highlightTiles)
                {
                
                    tilemap.RemoveTileFlags(tile, TileFlags.LockColor); // make it so the tile can be changed colors
                    tilemap.SetColor(tile, Color.magenta); // set the color of the tile
                }
                range -= selectedObj.attackRange;
                highlightTiles = tilesInRange(objectPos[tileObjects.IndexOf(selectedObj)], range);
                foreach (Vector3Int tile in highlightTiles)
                {
                    tilemap.SetColor(tile, Color.yellow); // set the color of the tile
                }
            }

            // if the mouse is on the tilemap
            if (onTilemap)
            {
                // check if there is a limit on the tiles you can place on, if there is, check you're in it.
                if (placeables.Count == 0 || placeables.Contains(hoveredTile))
                {
                    tilemap.RemoveTileFlags(hoveredTile, TileFlags.LockColor); // make it so the tile can be changed colors
                    tilemap.SetColor(hoveredTile, Color.red); // set the color of the tile
                    if(selectedObj!= null)
                    {
                        if (selectedObj.isAoe)
                        {

                            int range = selectedObj.currentRange + selectedObj.attackRange;

                            if (tileWithinRange(objectPos[tileObjects.IndexOf(selectedObj)], hoveredTile, range))
                            {

                                foreach (Vector3Int tile in tilesInRange(hoveredTile, selectedObj.aoeSize()))
                                {
                                    tilemap.RemoveTileFlags(tile, TileFlags.LockColor); // make it so the tile can be changed colors
                                    tilemap.SetColor(tile, Color.cyan);


                                }
                            }
                        }
                    }
                }

            }
            // if a tile is currently selected, change its color
            if (tileSelected)
            {
                tilemap.RemoveTileFlags(selectedTile, TileFlags.LockColor); // make it so the tile can be changed colors
                if (tilemap.GetColor(selectedTile) == Color.red)
                {
                    tilemap.SetColor(selectedTile, Color.blue);
                }
                else
                {
                    tilemap.SetColor(selectedTile, Color.green); // set the color of the tile
                }
            }
        }
        updateUI();

    }

    private void updateUI()
    {
        if(selectedObj != null && selectedObj.canAttack)
        {
            string t = string.Format("{0:D}\n\nHP: {1:D}\nARMOR: {2:D}\nPIERCING: {3:D}\nDAMAGE: {4:D}\nSPEED: {5:D}", selectedObj.unit.word, selectedObj.unit.hp, selectedObj.unit.armor, selectedObj.unit.piercing, selectedObj.unit.dmg, selectedObj.unit.moveSpeed);
            if(text.text != t)
            {
                text.text = t;
                foreach (Transform transform in actionHolder.transform)
                {
                    Destroy(transform.gameObject);
                }


                for (int i = 0; i < selectedObj.actions.Length; i++)
                {
                    if (selectedObj.actions[i] != null)
                    {
                        Action action = selectedObj.actions[i];
                        ActionChoice x = Instantiate(actionImagePrefab, actionHolder.transform).GetComponent<ActionChoice>();
                        string y = action.filePath();
                        Image image = x.gameObject.GetComponent<Image>();
                        image.sprite = Resources.Load<Sprite>(y);
                        x.setAction(action);
                        if (i == selectedObj.actionIndex)
                        {
                            x.selected = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Object is null");
                    }
                }
            }

        }
        else
        {
            text.text = "";
            foreach (Transform transform in actionHolder.transform)
            {
                Destroy(transform.gameObject);
            }
        }

    }

    public static List<Vector3Int> tilesInRange(Vector3Int startPos, int range)
    {
        // returns a list of tiles within the given range of a given tile

        // the list of tiles that are going to be available
        List<Vector3Int> tiles = new List<Vector3Int>();


        // the sequence to generate the range:

        // start at the very top (however high up the range is)
        Vector3Int curPos = startPos + Vector3Int.up * range;
        int numInRow = 1; // the number of items in that row, starts at one and goes up by two

        for(int i = 0; i < range * 2 + 1; i++)
        {

            for (int j = 0; j < numInRow; j++)
            {
                // add the tiles on the row
                tiles.Add(curPos);
                curPos += Vector3Int.right;
            }
            // move left and down from where you started the last row
            curPos += Vector3Int.left * (numInRow + 1);
            curPos += Vector3Int.down;

            // increase until you've reached the point in a range where the width starts decreasing
            if (i < range)
            {
                numInRow += 2;
            }
            else
            {
                numInRow -= 2;
                curPos += Vector3Int.right * 2;
            }
        }
        // remove the start position to prevent its highlight.
        tiles.Remove(startPos);

        return tiles; // return the list
    }

    public static List<TileObject> objectsInTiles(List<Vector3Int> tiles)
    {
        List<TileObject> objects = new List<TileObject>();
        LevelScript levelScript = FindObjectOfType<LevelScript>();

        foreach(Vector3Int tile in tiles)
        {
            if (levelScript.objectPos.Contains(tile))
            {
                objects.Add(levelScript.tileObjects[levelScript.objectPos.IndexOf(tile)]);
            }
        }

        return objects;
    }
    public void changeSelectedAction(Action action)
    {
        if (selectedObj != null)
        {
            selectedObj.setAction(action != selectedObj.action ? action : null);
            foreach (Transform t in actionHolder.transform)
            {
                t.gameObject.GetComponent<ActionChoice>().selected = false;
            }
            if(selectedObj.actionIndex != -1)
            {
                actionHolder.transform.GetChild(selectedObj.actionIndex).gameObject.GetComponent<ActionChoice>().selected = true;
            }

        }
    }
}
