using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // most public stuff needs to be connected through unity editor
    public GameObject gameControllerObj = null;
    private GameController gameController = null;
    public GameObject enemyControllerObj = null; 
    private EnemyController enemyController = null;
    private GameObject currTargeted = null;
    private Character currTargetedStats = null;
    public GameObject charInfoPanel = null;
    public GameObject upgradePanel = null;
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

    public float mapBoundPlusX = 35;
    public float mapBoundPlusY = 20;
    public float mapBoundMinusX = -5f;
    public float mapBoundMinusY = -5f;

    // movement area thingies
    public Pathfinding pathfinding = null;
    public Tilemap collisionGrid = null;
    public GameObject moveAreaParent = null;
    public GameObject attackAreaParent = null;
    private GameObject[] moveAreas;
    private GameObject[] attackAreas;
    [System.NonSerialized] public GameObject[] playerUnits;
    [System.NonSerialized] public Character[] playerStats;
    public Vector3[] allyStartPos;
    public Character.bodyType[] bodysList;
    public Character.weaponType[] weaponsList;

    // context menu stuff
    public GameObject contextMenu = null;
    private Button moveButton = null;
    private Button attackButton = null;
    private Button upgradeButton = null;
    private Button inspectButton = null;
    private Button deselectButton = null;
    private int enemyNum = 0;
    private Vector3 menuOffset = new Vector3(2f, 1, 0);
    private Vector3 lastClickPos = Vector3.zero;

    private int gearAmount = 0;
    private float delay = 0f; 

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

        moveButton = contextMenu.transform.GetChild(0).GetComponent<Button>();
        attackButton = contextMenu.transform.GetChild(1).GetComponent<Button>();
        upgradeButton = contextMenu.transform.GetChild(2).GetComponent<Button>();
        inspectButton = contextMenu.transform.GetChild(3).GetComponent<Button>();
        deselectButton = contextMenu.transform.GetChild(4).GetComponent<Button>();

        // get a handle on each child for PlayerController
        playerUnits = new GameObject[transform.childCount];
        playerStats = new Character[transform.childCount];

        int i = 0;
        foreach(Transform child in transform)
        {
            playerUnits[i] = child.gameObject;
            playerStats[i] = playerUnits[i].GetComponent<Character>();          
            playerUnits[i].transform.position = allyStartPos[i];
            playerStats[i].changeBody(bodysList[i]);
            playerStats[i].changeWeapon(weaponsList[i]);

            i += 1;      
        }

        moveAreas = new GameObject[moveAreaParent.transform.childCount];
        i = 0;
        foreach (Transform child in moveAreaParent.transform)
        {
            moveAreas[i] = child.gameObject;
            i += 1;
        }

        attackAreas = new GameObject[attackAreaParent.transform.childCount];
        i = 0;
        foreach (Transform child in attackAreaParent.transform)
        {
            attackAreas[i] = child.gameObject;
            i += 1;
        }

        pathfinding = new Pathfinding(17, 11, collisionGrid);
    }

    // Update is called once per frame
    void Update()
    {
        if (ourTurn)
        {
            // if player left clicks
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("Clicked UI, ignoring");
                    return;
                }

                // adjust to z level for units
                Vector3Int mousePos = GetMousePosition();
                mousePos = new Vector3Int(mousePos.x, mousePos.y, 0);

                // checking if an ally was clicked
                for (int i = 0; i < playerUnits.Length; i++)
                {
                    // an ally was clicked
                    if (mousePos == playerUnits[i].transform.position && playerStats[i].getIsDead() == false)
                    {
                        // if nothing currently targeted
                        if (currTargeted == null)
                            targetAlly(i);

                        // if clicking ally you already selected
                        else if (currTargeted.transform.position == mousePos)
                        {
                            openContextMenu(mousePos);
                            moveButton.interactable = false;
                            attackButton.interactable = false;
                            upgradeButton.interactable = true;
                            inspectButton.interactable = true;
                            deselectButton.interactable = true;
                        }

                        // another ally clicked
                        else
                        {
                            deselectTarget();
                            targetAlly(i);
                        }

                        return;
                    }
                }

                // checking if an enemy was clicked
                for (int i = 0; i < enemyController.enemyUnits.Length; i++)
                {
                    // an enemy was clicked
                    if (mousePos == enemyController.enemyUnits[i].transform.position && enemyController.enemyStats[i].getIsDead() == false)
                    {
                        // if nothing currently targeted
                        if (currTargeted == null)
                            targetEnemy(i);

                        // if clicking enemy you already selected
                        else if (currTargeted.transform.position == mousePos)
                        {
                            openContextMenu(mousePos);
                            moveButton.interactable = false;
                            attackButton.interactable = false;
                            upgradeButton.interactable = false;
                            inspectButton.interactable = true;
                            deselectButton.interactable = true;
                        }

                        // if you have an ally clicked and click an enemy already in attack range
                        else if (currTargeted != null && isTargetEnemy == false && currTargetedStats.getCanAttack() == true && inAttackRange(mousePos, currTargeted))
                        {
                            openContextMenu(mousePos);
                            moveButton.interactable = false;
                            attackButton.interactable = true;
                            upgradeButton.interactable = true;
                            inspectButton.interactable = true;
                            deselectButton.interactable = true;
                            enemyNum = i; 
                        }

/*                        // if an ally is clicked and click an enemy that can be moved to and attacked
                        else if (currTargeted != null && isTargetEnemy == false && currTargetedStats.getCanAttack() == true 
                            && inRange(Vector3Int.FloorToInt(enemyController.enemyUnits[i].transform.position)))
                        {
                            openContextMenu(mousePos);
                            moveButton.interactable = false;
                            attackButton.interactable = true;
                            upgradeButton.interactable = true;
                            inspectButton.interactable = true;
                            deselectButton.interactable = true;
                            lastClickPos = mousePos;
                        }*/
                                
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
                    openContextMenu(mousePos);
                    moveButton.interactable = true;
                    attackButton.interactable = false;
                    upgradeButton.interactable = true;
                    inspectButton.interactable = true;
                    deselectButton.interactable = true;
                    lastClickPos = mousePos;
                }

                // clicked nothing
                else
                {
                    // if have ally selected
                    if (currTargeted != null && currTargetedStats.getIsEnemy() == false)
                    {
                        openContextMenu(mousePos);
                        moveButton.interactable = false;
                        attackButton.interactable = false;
                        upgradeButton.interactable = true;
                        inspectButton.interactable = true;
                        deselectButton.interactable = true;
                    }
                    // if you have anything selected
                    else if (currTargeted != null)
                    {
                        openContextMenu(mousePos);
                        moveButton.interactable = false;
                        attackButton.interactable = false;
                        upgradeButton.interactable = false;
                        inspectButton.interactable = true;
                        deselectButton.interactable = true;
                    }

                }                            
            }
        }
    }

    public void openContextMenu(Vector3 mousePos)
    {
        contextMenu.SetActive(true);
        contextMenu.transform.position = Camera.main.WorldToScreenPoint(mousePos + menuOffset);
        
    }

    public void moveButtonPressed()
    {
        List<PathNode> vectorPath = new List<PathNode>();
        vectorPath = pathfinding.FindPath((int)currTargeted.transform.position.x, (int)currTargeted.transform.position.y,
            (int)lastClickPos.x, (int)lastClickPos.y, currTargetedStats.movLeft);

        if (vectorPath != null)
        {
            StartCoroutine(movePath(vectorPath));
            pathfinding.resetCollision();
            /* for (int i = 0; i < vectorPath.Count; i++)
                Debug.Log(vectorPath[i]);*/
        }

        contextMenu.SetActive(false);
    }

    public void attackButtonPressed()
    {
        int g = 0;
        List<PathNode> vectorPath = new List<PathNode>();
        if (!inAttackRange(Vector3Int.FloorToInt(lastClickPos), currTargeted))
        {
            vectorPath = enemyController.findBestForOne(currTargeted, enemyController.enemyUnits[enemyNum], ref g);
        }

        if (vectorPath != null)
        {
            for (int i = 0; i < vectorPath.Count; i++)
                Debug.Log("VectorPath[" + i + "]: " + vectorPath[i]);
        }

        if (inAttackRange(Vector3Int.FloorToInt(lastClickPos), currTargeted))
        {
            beginBattle(enemyNum);
            contextMenu.SetActive(false);
        }
    }

    public void upgradeButtonPressed()
    {
        gameController.upgradeButtonPressed();
        contextMenu.SetActive(false);
    }

    public void inspectButtonPressed()
    {
        charInfoPanel.gameObject.SetActive(true);
        updateCharInfo();
        hideArea();
        showArea(currTargeted);

        contextMenu.SetActive(false);
    }

    public void deselectButtonPressed()
    {
        deselectTarget(); 
        contextMenu.SetActive(false);
    }

    public IEnumerator movePath(List<PathNode> vectorPath)
    {
        // stop player from ending turn during movement
        gameController.changeMode(GameController.gameMode.MenuMode);

        for (int i = 1; i < vectorPath.Count; i++)
        {
            currTargeted.transform.position = new Vector3(vectorPath[i].x, vectorPath[i].y, 0);
            currTargetedStats.movLeft--;
            hideArea();
            showArea(currTargeted);
            updateCharInfo();
            yield return new WaitForSeconds(delay);
        }

        gameController.changeMode(GameController.gameMode.MapMode);
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

        // calculate range of this battle
        int battleRange = 0;
        Vector3 distance = enemyController.enemyUnits[i].transform.position - currTargeted.transform.position;
        if ((Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 0)
            || (Mathf.Abs(distance.x) == 0 && Mathf.Abs(distance.y) == 1))
        {
            battleRange = 1;
        }
        else if ((Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 1)
            || (Mathf.Abs(distance.x) == 2 && Mathf.Abs(distance.y) == 0)
            || (Mathf.Abs(distance.x) == 0 && Mathf.Abs(distance.y) == 2))
        {
            battleRange = 2;
        }

        // if facing to the right or down then put ally on the left 
        if (battleDirection == direction.right || battleDirection == direction.down)
            StartCoroutine(gameController.startBattle(currTargeted, enemyController.enemyUnits[i], true, battleRange));
        // else put ally on the right
        else
            StartCoroutine(gameController.startBattle(enemyController.enemyUnits[i], currTargeted, true, battleRange));
    }

    IEnumerator waitBattle(int i)
    {
        hideArea();       
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

    public bool unitHere(Vector3Int pos)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (playerUnits[i].transform.position == pos && playerStats[i].getIsDead() == false)
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
        if (playerStats[i].getIsDead() == true)
            return;

        currTargeted = playerUnits[i];
        currTargetedStats = currTargeted.GetComponent<Character>();
        isTargetEnemy = false;
        //Debug.Log("currTargeted is " + currTargeted.name);

        currTargeted.transform.GetChild(0).gameObject.SetActive(true);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(true);

        //charInfoPanel.gameObject.SetActive(true);
        //updateCharInfo();
        showArea(currTargeted);
        //upgradePanel.SetActive(true);
        gameController.updateUpgradeMenu(currTargeted);
    }

    public void showArea(GameObject unit)
    {
        Character unitStats = unit.GetComponent<Character>();

        // 1 Range
        if (unitStats.getAttackRange() == 1)
        {
            if (unitStats.movLeft < 0 || unitStats.movLeft > moveAreas.Length || unitStats.movLeft >= attackAreas.Length)
            {
                Debug.Log("movLeft out of range in showArea!!!");
                hideArea();
            }
            else if (unitStats.movLeft == 0)
            {
                hideArea();
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
            else
            {
                moveAreas[unitStats.movLeft - 1].SetActive(true);
                moveAreas[unitStats.movLeft - 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
        }
        // 2 Range
        else if (unitStats.getAttackRange() == 2)
        {
            if (unitStats.movLeft > moveAreas.Length || unitStats.movLeft + 1 >= attackAreas.Length)
            {
                Debug.Log("movLeft out of range in showArea!!!");
                hideArea();
            }
            else if (unitStats.movLeft == 0)
            {
                hideArea();
                attackAreas[unitStats.movLeft + 1].SetActive(true);
                attackAreas[unitStats.movLeft + 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
            }
            else
            {
                moveAreas[unitStats.movLeft - 1].SetActive(true);
                moveAreas[unitStats.movLeft - 1].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft].SetActive(true);
                attackAreas[unitStats.movLeft].transform.position = unit.transform.position;
                attackAreas[unitStats.movLeft + 1].SetActive(true);
                attackAreas[unitStats.movLeft + 1].transform.position = unit.transform.position;
            }
        }

    }

    public void hideArea()
    {
        for (int i = 0; i < moveAreas.Length; i++)
            moveAreas[i].SetActive(false);

        for (int i = 0; i < attackAreas.Length; i++)
            attackAreas[i].SetActive(false);
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
        hideArea();

        contextMenu.SetActive(false);

        /*        if (isTargetEnemy == false)
                {
                    upgradePanel.SetActive(false);
                }*/

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
        hideArea();
        showArea(currTargeted);
    }

    void targetEnemy(int i)
    {
        Debug.Log("Clicked enemy");
        Debug.Log("i: " + i);
        currTargeted = enemyController.enemyUnits[i];
        currTargetedStats = currTargeted.GetComponent<Character>();
        isTargetEnemy = true;
        //Debug.Log("currTargeted is " + currTargeted.name);

        currTargeted.transform.GetChild(0).gameObject.SetActive(true);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(true);

        //charInfoPanel.gameObject.SetActive(true);
        //updateCharInfo();
        //hideArea();
        //showArea(currTargeted);
    }

    public bool inRange(Vector3Int mousePos)
    {
        float maxDistance = currTargetedStats.movLeft + currTargetedStats.getAttackRange();
        if ((mousePos - currTargeted.transform.position).magnitude < maxDistance)
            return true;
        else
            return false;

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

    bool inAttackRange(Vector3Int mousePos, GameObject unit)
    {
        Character unitStats = unit.GetComponent<Character>();

        // 1 Range
        if (unitStats.getAttackRange() == 1)
        {
            Vector3Int distance = mousePos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) == 1 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) == 1))
                return true;
        }
        // 2 Range
        else if (unitStats.getAttackRange() == 2)
        {
            Vector3Int distance = mousePos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) <= 2 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) <= 2) || (Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 1)) 
                return true;
        }

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
    
    public void resetAllAttack()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            playerStats[i].setAttack(true);
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
            if (playerStats[i].getIsDead() == false)
                playerUnits[i].gameObject.SetActive(true);
        }
    }

    public void comeBackToLife()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            playerStats[i].undie();
        }
    }

    public bool allDead()
    {
        for (int i = 0; i < playerStats.Length; i++)
        {
            if (playerStats[i].getIsDead() == false)
                return false;
        }

        return true;
    }

    public void moveToAttack(Vector3Int mousePos, int i)
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

    public void changedName(string s)
    {
        currTargetedStats.charName = s;
        currTargeted.name = s;
    }

    public void changedBody(Dropdown d)
    {
        int val = d.value;
        currTargetedStats.changeBody((Character.bodyType)val);
        gameController.updateUpgradeMenu(currTargeted);
    }

    public void changedWeapon(Dropdown d)
    {
        hideArea();
        int val = d.value;
        currTargetedStats.changeWeapon((Character.weaponType)val);
        gameController.updateUpgradeMenu(currTargeted);
        showArea(currTargeted);
    }

    public void hpButtonPressed()
    {
        if (getGearNum() >= currTargetedStats.HPCost && currTargetedStats.baseHP < currTargetedStats.getHPMAX())
        {
            giveGearNum(-currTargetedStats.HPCost);
            currTargetedStats.baseHP = currTargetedStats.baseHP + 1;
            currTargetedStats.hpLeft = currTargetedStats.hpLeft + 1;
            gameController.updateUpgradeMenu(currTargeted);
            currTargetedStats.updateStats();
        }
    }

    public void strButtonPressed()
    {
        if (getGearNum() >= currTargetedStats.STRCost && currTargetedStats.baseSTR < currTargetedStats.getSTRMAX())
        {
            giveGearNum(-currTargetedStats.STRCost);
            currTargetedStats.baseSTR = currTargetedStats.baseSTR + 1;
            gameController.updateUpgradeMenu(currTargeted);
            currTargetedStats.updateStats();
        }
    }

    public void magButtonPressed()
    {
        if (getGearNum() >= currTargetedStats.MAGCost && currTargetedStats.baseMAG < currTargetedStats.getMAGMAX())
        {
            giveGearNum(-currTargetedStats.MAGCost);
            currTargetedStats.baseMAG = currTargetedStats.baseMAG + 1;
            gameController.updateUpgradeMenu(currTargeted);
            currTargetedStats.updateStats();
        }
    }

    public void defButtonPressed()
    {
        if (getGearNum() >= currTargetedStats.DEFCost && currTargetedStats.baseDEF < currTargetedStats.getDEFMAX())
        {
            giveGearNum(-currTargetedStats.DEFCost);
            currTargetedStats.baseDEF = currTargetedStats.baseDEF + 1;
            gameController.updateUpgradeMenu(currTargeted);
            currTargetedStats.updateStats();
        }
    }

    public void resButtonPressed()
    {
        if (getGearNum() >= currTargetedStats.RESCost && currTargetedStats.baseRES < currTargetedStats.getRESMAX())
        {
            giveGearNum(-currTargetedStats.RESCost);
            currTargetedStats.baseRES = currTargetedStats.baseRES + 1;
            gameController.updateUpgradeMenu(currTargeted);
            currTargetedStats.updateStats();
        }
    }

    public void spdButtonPressed()
    {
        if (getGearNum() >= currTargetedStats.SPDCost && currTargetedStats.baseSPD < currTargetedStats.getSPDMAX())
        {
            giveGearNum(-currTargetedStats.SPDCost);
            currTargetedStats.baseSPD = currTargetedStats.baseSPD + 1;
            gameController.updateUpgradeMenu(currTargeted);
            currTargetedStats.updateStats();
        }
    }

    public void movButtonPressed()
    {
        if (getGearNum() >= currTargetedStats.MOVCost && currTargetedStats.baseMOV < currTargetedStats.getMOVMAX())
        {
            giveGearNum(-currTargetedStats.MOVCost);
            hideArea();
            currTargetedStats.baseMOV = currTargetedStats.baseMOV + 1;
            currTargetedStats.movLeft = currTargetedStats.movLeft + 1;
            showArea(currTargeted);
            gameController.updateUpgradeMenu(currTargeted);
            currTargetedStats.updateStats();            
        }
    }

    public int getGearNum()
    {
        return gearAmount;
    }

    public void setGearNum(int i)
    {
        gearAmount = i;
        gameController.updateGearNumPanel();
    }

    public void giveGearNum(int i)
    {
        gearAmount = gearAmount + i;
        gameController.updateGearNumPanel();
    }

    public GameObject getCurrTargeted()
    {
        return currTargeted;
    }

    public void setDelay(float num)
    {
        delay = num;
    }
}

