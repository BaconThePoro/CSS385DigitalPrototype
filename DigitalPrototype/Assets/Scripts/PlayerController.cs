using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

    private float mapBoundPlusX = 16f;
    private float mapBoundPlusY = 10f;
    private float mapBoundMinusX = -16f;
    private float mapBoundMinusY = -10f;

    // movement area thingies
    public GameObject moveAreaParent = null;
    public GameObject attackAreaParent = null;
    private GameObject[] moveAreas;
    private GameObject[] attackAreas;

    public GameObject[] playerUnits;
    public Character[] playerStats;
    public Vector3[] allyStartPos;

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

        // get a handle on each child for PlayerController
        playerUnits = new GameObject[transform.childCount];
        playerStats = new Character[transform.childCount];

        int i = 0;
        foreach(Transform child in transform)
        {
            playerUnits[i] = child.gameObject;
            playerStats[i] = playerUnits[i].GetComponent<Character>();          
            playerUnits[i].transform.position = allyStartPos[i];

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
                mousePos = new Vector3Int(mousePos.x, mousePos.y, 0);

                Debug.Log("Clicked here: " + mousePos);
                //Debug.Log("currTargeted is " + currTargeted.name);
                //Debug.Log("childCount is " + transform.childCount);
                for (int i = 0; i < transform.childCount; i++)
                {
                    // clicked ally 
                    if (mousePos == playerUnits[i].transform.position && playerStats[i].isDead == false)
                    {
                        // target ally 
                        if (currTargeted == null)
                        {
                            targetAlly(i);
                        }
                        // someone already selected, deselct and retarget them
                        else
                        {
                            deselectTarget();
                            targetAlly(i);
                        }

                        return;
                    }
                    // clicked enemy 
                    else if (mousePos == enemyController.enemyUnits[i].transform.position && enemyController.enemyStats[i].isDead == false)
                    {
                        // no ally selected target the enemy
                        if (currTargeted == null)
                        {
                            targetEnemy(i);
                        }
                        // ally selected and in range, attack
                        else if (currTargeted != null && isTargetEnemy == false && currTargetedStats.canAttack == true && inAttackRange(mousePos, currTargeted))
                        {
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
        showArea(currTargeted);

    }

    public void showArea(GameObject unit)
    {
        Character unitStats = unit.GetComponent<Character>();

        // sword + axe
        if (unitStats.weapon == 1 || unitStats.weapon == 3)
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
        // bow
        else if (unitStats.weapon == 2)
        {
            if (unitStats.movLeft < 0 || unitStats.movLeft > moveAreas.Length || unitStats.movLeft + 1 >= attackAreas.Length)
            {
                Debug.Log("movLeft out of range in showArea!!!");
                hideArea();
            }
            else if (unitStats.movLeft == 0)
            {
                hideArea();
                attackAreas[unitStats.movLeft + 1].SetActive(true);
                attackAreas[unitStats.movLeft + 1].transform.position = unit.transform.position;
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
        hideArea();
        showArea(currTargeted);
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

        // sword + axe
        if (unitStats.weapon == 1 || unitStats.weapon == 3)
        {
            Vector3Int distance = mousePos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) == 1 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) == 1))
                return true;
        }
        // bow
        else if (unitStats.weapon == 2)
        {
            Vector3Int distance = mousePos - Vector3Int.FloorToInt(unit.transform.position);
            if ((Mathf.Abs(distance.x) == 2 && distance.y == 0) || (distance.x == 0 && Mathf.Abs(distance.y) == 2) || (Mathf.Abs(distance.x) == 1 && Mathf.Abs(distance.y) == 1)) 
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
}

