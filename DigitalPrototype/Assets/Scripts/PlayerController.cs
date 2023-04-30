using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Must be connected via unity editor
    public GameObject gameControllerObj = null;
    private GameController gameController = null;
    public GameObject enemyControllerObj = null; 
    private EnemyController enemyController = null;
    private GameObject currTargeted = null;
    private Character currTargetedStats = null;
    public GameObject charInfoPanel = null;
    private GameObject movLeftTXT = null;
    private GameObject movLeftNUMObj = null;
    private TMPro.TextMeshProUGUI charNameTXT = null;
    private TMPro.TextMeshProUGUI hpNUM = null;
    private TMPro.TextMeshProUGUI strNUM = null;
    private TMPro.TextMeshProUGUI magNUM = null;
    private TMPro.TextMeshProUGUI spdNUM = null;
    private TMPro.TextMeshProUGUI defNUM = null;
    private TMPro.TextMeshProUGUI resNUM = null;
    private TMPro.TextMeshProUGUI movNUM = null;
    private TMPro.TextMeshProUGUI movLeftNUM = null;

    public bool ourTurn = false;
    public bool isTargetEnemy = false; 
    private enum direction { left, right, up, down };

    // Must be connected via unity editor
    public Grid currGrid = null;
    public Tilemap currTilemap = null;
    public Tile targetTile = null;
    private Vector3Int prevTarget = Vector3Int.zero; 

    // dont set in here, they grab value from GameController
    private float tileX = 0;
    private float tileY = 0;
    private float mapBoundPlusX = 8f;
    private float mapBoundPlusY = 4f;
    private float mapBoundMinusX = -9f;
    private float mapBoundMinusY = -5f;

    // movement area thingies
    public GameObject oneMovArea = null;
    public GameObject twoMovArea = null;
    public GameObject threeMovArea = null;

    private GameObject[] playerUnits;
    private Character[] playerStats;

    // Start is called before the first frame update
    void Start()
    {
        gameController = gameControllerObj.GetComponent<GameController>();
        enemyController = enemyControllerObj.GetComponent<EnemyController>();
        
        charNameTXT = charInfoPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        movLeftTXT = charInfoPanel.transform.GetChild(9).gameObject;
        hpNUM = charInfoPanel.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        strNUM = charInfoPanel.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        magNUM = charInfoPanel.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        spdNUM = charInfoPanel.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        defNUM = charInfoPanel.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        resNUM = charInfoPanel.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        movNUM = charInfoPanel.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        movLeftNUMObj = charInfoPanel.transform.GetChild(17).gameObject;
        movLeftNUM = movLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();
        
        tileX = gameController.tileX;
        tileY = gameController.tileY;

        // get a handle on each child for PlayerController
        playerUnits = new GameObject[transform.childCount];
        playerStats = new Character[transform.childCount];

        int i = 0;
        foreach(Transform child in transform)
        {
            playerUnits[i] = child.gameObject;
            playerStats[i] = playerUnits[i].GetComponent<Character>();

            Vector3 startPos = new Vector3(0f, -1f + i, -1f);
            playerUnits[i].transform.position = startPos;
           
            i += 1;      
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ourTurn)
        {
            // if player left clicks
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                // adjust to z level for units
                Vector3Int mousePos = GetMousePosition();
                mousePos = new Vector3Int(mousePos.x, mousePos.y, -1);

                Debug.Log("Clicked here: " + mousePos);
                //Debug.Log("currTargeted is " + currTargeted.name);
                //Debug.Log("childCount is " + transform.childCount);
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (mousePos == playerUnits[i].transform.position && playerStats[i].isDead == false)
                    {
                        // target ally
                        if (currTargeted == null)
                        {
                            targetAlly(i);
                        }
                        else
                        {
                            deselectTarget();
                            targetAlly(i);
                        }

                        return;
                    }
                    else if (mousePos == enemyController.enemyUnits[i].transform.position && enemyController.enemyStats[i].isDead == false)
                    {
                        // no ally selected target enemy
                        if (currTargeted == null)
                        {
                            targetEnemy(i);
                        }
                        // ally selected and in range, attack
                        else if (currTargeted != null && isTargetEnemy == false && inMovementRange(mousePos))
                        {                          
                            // if you clicked an enemy but arent next to them yet, then move next to them
                            Vector3Int distanceFrom = mousePos - Vector3Int.FloorToInt(currTargeted.transform.position);
                            //Debug.Log("initial distanceFrom = " + distanceFrom);

                            // to far diagonally
                            if (Mathf.Abs(distanceFrom.x) >= 1 && Mathf.Abs(distanceFrom.y) >= 1)
                            {
                                // just copies the vertical movement stuff
                                Vector3Int temp;

                                if (distanceFrom.y < 0) // if the distance is negative
                                    temp = new Vector3Int(0, -1, 0);
                                else // its positive
                                    temp = new Vector3Int(0, 1, 0);

                                distanceFrom = distanceFrom - temp;
                                Debug.Log("Initiated combat within mov range but not adjacent. Moving: " + distanceFrom);

                                // if theres an ally already in the space your moving to
                                if (unitHere(Vector3Int.FloorToInt(currTargeted.transform.position) + distanceFrom))
                                { // try move horizontally

                                    distanceFrom = distanceFrom + temp;
                                    if (distanceFrom.x < 0) // if the distance is negative
                                        temp = new Vector3Int(-1, 0, 0);
                                    else // its positive
                                        temp = new Vector3Int(1, 0, 0);

                                    distanceFrom = distanceFrom - temp;
                                    Debug.Log("Initiated combat within mov range but not adjacent. Moving: " + distanceFrom);

                                    // both sides occupied just cant attack
                                    if (unitHere(Vector3Int.FloorToInt(currTargeted.transform.position) + distanceFrom))
                                    {
                                        deselectTarget();
                                        targetEnemy(i);
                                        return;
                                    }
                                }

                                moveAlly(Vector3Int.FloorToInt(currTargeted.transform.position) + distanceFrom);

                                // small delay before beginning battle so user can see character move
                                StartCoroutine(waitBattle(i));
                                return;
                            }

                            // to far horizontally
                            else if (Mathf.Abs(distanceFrom.x) > 1)
                            {                               
                                Vector3Int temp;

                                if (distanceFrom.x < 0) // if the distance is negative
                                    temp = new Vector3Int(-1, 0, 0);
                                else // its positive
                                    temp = new Vector3Int(1, 0, 0);

                                distanceFrom = distanceFrom - temp;
                                //Debug.Log("new distanceFrom = " + distanceFrom);
                                //Debug.Log("Initiated combat within mov range but not adjacent. Moving: " + distanceFrom);

                                // if theres an ally already in the space your moving to
                                if (unitHere(Vector3Int.FloorToInt(currTargeted.transform.position) + distanceFrom))
                                {
                                    deselectTarget();
                                    targetEnemy(i);
                                    return;
                                }

                                moveAlly(Vector3Int.FloorToInt(currTargeted.transform.position) + distanceFrom);

                                // small delay before beginning battle so user can see character move
                                StartCoroutine(waitBattle(i));
                                return;
                            }
                            // to far vertically
                            else if (Mathf.Abs(distanceFrom.y) > 1)
                            {
                                Vector3Int temp;
                               
                                if (distanceFrom.y < 0) // if the distance is negative
                                    temp = new Vector3Int(0, -1, 0);
                                else // its positive
                                    temp = new Vector3Int(0, 1, 0);

                                distanceFrom = distanceFrom - temp;
                                //Debug.Log("Initiated combat within mov range but not adjacent. Moving: " + distanceFrom);

                                // if theres an ally already in the space your moving to
                                if (unitHere(Vector3Int.FloorToInt(currTargeted.transform.position) + distanceFrom))
                                {
                                    deselectTarget();
                                    targetEnemy(i);
                                    return;
                                }

                                moveAlly(Vector3Int.FloorToInt(currTargeted.transform.position) + distanceFrom);

                                // small delay before beginning battle so user can see character move
                                StartCoroutine(waitBattle(i));
                                return;
                            }

                            beginBattle(i);
                        }
                        // ally selected but not in range, reselect enemy instead
                        else
                        {
                            deselectTarget();
                            targetEnemy(i);
                        }

                        return;
                    }
                }

                // clicked in move range, move ally
                if (currTargeted != null && inMovementRange(mousePos) && currTargetedStats.movLeft > 0 && isTargetEnemy != true)
                {
                    moveAlly(mousePos);
                }
                // clicked nothing and/or outside of moverange, deselect target
                else
                {
                    deselectTarget();
                }                            
            }
        }
    }

    void beginBattle(int i)
    {
        Debug.Log("battle time");
        ourTurn = false;

        // can only fight once per turn, reduce movement to 0
        currTargetedStats.movLeft = 0;

        gameController.changeMode(GameController.gameMode.BattleMode);

        // figure out which way to face (ally on left or right)
        direction battleDirection = facingWhere(currTargeted.transform.position, enemyController.enemyUnits[i].transform.position);

        // if facing to the right or down then put ally on the left 
        if (battleDirection == direction.right || battleDirection == direction.down)
            StartCoroutine(gameController.startBattle(currTargeted, enemyController.enemyUnits[i], false, true));
        // else put ally on the right
        else
            StartCoroutine(gameController.startBattle(enemyController.enemyUnits[i], currTargeted, true, true));
    }

    IEnumerator waitBattle(int i)
    {
        hideMovArea();       
        yield return new WaitForSeconds(0.25f);
        beginBattle(i);
    }

    direction facingWhere(Vector3 allyPos, Vector3 enemyPos)
    {
        Vector3 difference = enemyPos - allyPos;

        if (difference.x < 0)
            return direction.left;
        else if (difference.x > 0)
            return direction.right;

        if (difference.y < 0)
            return direction.down;
        else if (difference.y > 0)
            return direction.up;

        Debug.Log("facingWhere() is broke, units somehow standing on top of each other");
        return direction.left;
    }

    bool unitHere(Vector3Int pos)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (playerUnits[i].transform.position == pos && playerStats[i].isDead == false)
            {
                return true;
            }
        }

        if (enemyController.enemyHere(pos))
        {
            return true;
        }

        return false; 
    }

    void targetAlly(int i)
    {
        //Debug.Log("Clicked ally");
        //Debug.Log("i: " + i);
        //Debug.Log("playerUnit @ " + i + " is " + playerUnits[i].transform.name);
        if (playerStats[i].isDead == true)
            return;

        currTargeted = playerUnits[i];
        currTargetedStats = currTargeted.GetComponent<Character>();
        isTargetEnemy = false;
        //Debug.Log("currTargeted is " + currTargeted.name);

        currTargeted.transform.GetChild(0).gameObject.SetActive(true);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(true);

        charInfoPanel.gameObject.SetActive(true);
        updateCharInfo();
        showMovArea(currTargeted);

    }

    public void showMovArea(GameObject unit)
    {
        Character unitStats = unit.GetComponent<Character>();

        if (unitStats.movLeft == 0)
        {
            hideMovArea();
        }
        else if (unitStats.movLeft == 1)
        {
            oneMovArea.SetActive(true);
            oneMovArea.transform.position = unit.transform.position;
        }
        else if (unitStats.movLeft == 2)
        {
            twoMovArea.SetActive(true);
            twoMovArea.transform.position = unit.transform.position;
        }
        else if (unitStats.movLeft == 3)
        {
            threeMovArea.SetActive(true);
            threeMovArea.transform.position = unit.transform.position;
        }
    }

    public void hideMovArea()
    {
        oneMovArea.SetActive(false);
        twoMovArea.SetActive(false);
        threeMovArea.SetActive(false);
    }

    public void deselectTarget()
    {
        if (currTargeted == null)
            return;
        currTargeted.transform.GetChild(0).gameObject.SetActive(false);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(false);
        currTargeted = null;
        currTargetedStats = null;
        charInfoPanel.gameObject.SetActive(false);
        hideMovArea();
    }

    void moveAlly(Vector3Int mousePos)
    {
        if (mousePos.x > mapBoundPlusX || mousePos.x < mapBoundMinusX || mousePos.y > mapBoundPlusY || mousePos.y < mapBoundMinusY)
        {
            Debug.Log("Movement area out of bounds, cancelling movement");
            return;
        }

        Vector3 distanceTraveled = mousePos - currTargeted.transform.position;
        currTargeted.transform.position = mousePos;

        // Flip image based on the movement direction (if you move left sprite should face left
        //Debug.Log("distance traveled.x = " + distanceTraveled.x);
        // face left
        if (distanceTraveled.x < 0f)
        {
            currTargeted.transform.rotation = new Quaternion(0f, 180f, 0f, 1f);
        }
        // face right
        else if (distanceTraveled.x > 0f)
        {
            currTargeted.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
        }

        currTargetedStats.movLeft = (int)(currTargetedStats.movLeft - Mathf.Abs(distanceTraveled.x));
        currTargetedStats.movLeft = (int)(currTargetedStats.movLeft - Mathf.Abs(distanceTraveled.y));

        //Debug.Log("moveUsedX: " + Mathf.Abs(distanceTraveled.x));
        //Debug.Log("moveUsedY: " + Mathf.Abs(distanceTraveled.y));
        //Debug.Log("moveLeft: " + moveLeft);
        updateCharInfo();
        hideMovArea();
        showMovArea(currTargeted);
    }

    void targetEnemy(int i)
    {
        Debug.Log("Clicked enemy");
        //Debug.Log("i: " + i);
        //Debug.Log("playerUnit @ " + i + " is " + playerUnits[i].transform.name);
        currTargeted = enemyController.enemyUnits[i];
        currTargetedStats = currTargeted.GetComponent<Character>();
        isTargetEnemy = true;
        //Debug.Log("currTargeted is " + currTargeted.name);

        currTargeted.transform.GetChild(0).gameObject.SetActive(true);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(true);

        charInfoPanel.gameObject.SetActive(true);
        updateCharInfo();
        hideMovArea();
        showMovArea(currTargeted);
    }

    bool inMovementRange(Vector3Int mousePos)
    {
        Vector3 distanceTraveled = mousePos - currTargeted.transform.position;
        if (Mathf.Abs(distanceTraveled.x) + Mathf.Abs(distanceTraveled.y) <= currTargetedStats.movLeft)
        {             
            return true;
        }
        else
            return false;
    }

    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }

    public void resetAllMove()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            playerStats[i].resetMove();
        }
    }

    public void updateCharInfo()
    {
        charNameTXT.text = "Name: " + currTargetedStats.charName;
        hpNUM.text = "" + currTargetedStats.hpLeft + " / " + currTargetedStats.HP;
        strNUM.text = "" + currTargetedStats.STR;
        magNUM.text = "" + currTargetedStats.MAG;
        defNUM.text = "" + currTargetedStats.DEF;
        resNUM.text = "" + currTargetedStats.RES;
        spdNUM.text = "" + currTargetedStats.SPD;

        if (isTargetEnemy == false)
        {
            movNUM.text = "" + currTargetedStats.MOV;
            movLeftNUM.text = "" + currTargetedStats.movLeft;
            movLeftTXT.SetActive(true);
            movLeftNUMObj.SetActive(true);
        }
        else
        {
            movNUM.text = "" + currTargetedStats.MOV;
            movLeftTXT.SetActive(false);
            movLeftNUMObj.SetActive(false);
        }
    }

    public void deactivateChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            playerUnits[i].gameObject.SetActive(false);
        }
    }

    public void activateChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (playerStats[i].isDead == false)
                playerUnits[i].gameObject.SetActive(true);
        }
    }

    public void comeBackToLife()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            playerStats[i].isDead = false;
        }
    }

    public bool allDead()
    {
        for (int i = 0; i < playerStats.Length; i++)
        {
            if (playerStats[i].isDead == false)
                return false;
        }

        return true;
    }
}

