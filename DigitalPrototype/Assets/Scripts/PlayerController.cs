using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // Must be connected via unity editor
    public GameObject gameController = null;
    public GameObject enemyControllerObj = null; 
    private EnemyController enemyController;
    private GameObject currTargeted = null;
    public GameObject charInfoPanel = null;

    public bool ourTurn = false;
    public bool isTargetEnemy = false; 

    // Must be connected via unity editor
    public Grid currGrid = null;
    public Tilemap currTilemap = null;
    public Tile targetTile = null;
    private Vector3Int prevTarget = Vector3Int.zero; 
    // dont set in here, they grab value from GameController
    private float tileX = 0;
    private float tileY = 0;

    private GameObject[] playerUnits;

    // Start is called before the first frame update
    void Start()
    {
        enemyController = enemyControllerObj.GetComponent<EnemyController>();

        tileX = gameController.GetComponent<GameController>().tileX;
        tileY = gameController.GetComponent<GameController>().tileY;

        // get a handle on each child for PlayerController
        playerUnits = new GameObject[transform.childCount];
        int i = 0;
        foreach(Transform child in transform)
        {
            playerUnits[i] = child.gameObject;

            Vector3 startPos = new Vector3(3f, -3f + i, -1f);
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

                //Debug.Log("Clicked here: " + mousePos);
                //Debug.Log("currTargeted is " + currTargeted.name);
                //Debug.Log("childCount is " + transform.childCount);
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (mousePos == playerUnits[i].transform.position)
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
                    else if (mousePos == enemyController.enemyUnits[i].transform.position)
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
                if (currTargeted != null && inMovementRange(mousePos) && currTargeted.GetComponent<Character>().movLeft > 0 && isTargetEnemy != true)
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
        //Debug.Log("battle time");
        ourTurn = false;

        // can only fight once per turn, reduce movement to 0
        currTargeted.GetComponent<Character>().movLeft = 0;

        gameController.GetComponent<GameController>().changeMode(GameController.gameMode.BattleMode);
        gameController.GetComponent<GameController>().startBattle(currTargeted, enemyController.enemyUnits[i], true);
    }

    IEnumerator waitBattle(int i)
    {
        yield return new WaitForSeconds(0.5f);
        beginBattle(i);
    }

    bool unitHere(Vector3Int pos)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (playerUnits[i].transform.position == pos)
            {
                return true;
            }
        }

        if (enemyController.GetComponent<EnemyController>().enemyHere(pos))
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
        currTargeted = playerUnits[i];
        isTargetEnemy = false;
        //Debug.Log("currTargeted is " + currTargeted.name);

        currTargeted.transform.GetChild(0).gameObject.SetActive(true);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(true);

        charInfoPanel.gameObject.SetActive(true);
        updateCharInfo();
    }

    public void deselectTarget()
    {
        if (currTargeted == null)
            return;
        currTargeted.transform.GetChild(0).gameObject.SetActive(false);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(false);
        currTargeted = null;
        charInfoPanel.gameObject.SetActive(false);
    }

    void moveAlly(Vector3Int mousePos)
    {
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

        currTargeted.GetComponent<Character>().movLeft = (int)(currTargeted.GetComponent<Character>().movLeft - Mathf.Abs(distanceTraveled.x));
        currTargeted.GetComponent<Character>().movLeft = (int)(currTargeted.GetComponent<Character>().movLeft - Mathf.Abs(distanceTraveled.y));

        //Debug.Log("moveUsedX: " + Mathf.Abs(distanceTraveled.x));
        //Debug.Log("moveUsedY: " + Mathf.Abs(distanceTraveled.y));
        //Debug.Log("moveLeft: " + moveLeft);
        updateCharInfo();
    }

    void targetEnemy(int i)
    {
        Debug.Log("Clicked enemy");
        //Debug.Log("i: " + i);
        //Debug.Log("playerUnit @ " + i + " is " + playerUnits[i].transform.name);
        currTargeted = enemyController.enemyUnits[i];
        isTargetEnemy = true;
        //Debug.Log("currTargeted is " + currTargeted.name);

        currTargeted.transform.GetChild(0).gameObject.SetActive(true);
        //currTargeted.transform.GetChild(1).gameObject.SetActive(true);

        charInfoPanel.gameObject.SetActive(true);
        updateCharInfo();
    }

    bool inMovementRange(Vector3Int mousePos)
    {
        Vector3 distanceTraveled = mousePos - currTargeted.transform.position;
        if (Mathf.Abs(distanceTraveled.x) + Mathf.Abs(distanceTraveled.y) <= currTargeted.GetComponent<Character>().movLeft)
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
            playerUnits[i].GetComponent<Character>().resetMove();
        }
    }

    public void updateCharInfo()
    {
        charInfoPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "Name: " + currTargeted.GetComponent<Character>().charName;
        charInfoPanel.transform.GetChild(9).GetComponent<TMPro.TextMeshProUGUI>().text = "" + currTargeted.GetComponent<Character>().STR;
        charInfoPanel.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>().text = "" + currTargeted.GetComponent<Character>().MAG;
        charInfoPanel.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>().text = "" + currTargeted.GetComponent<Character>().DEF;
        charInfoPanel.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>().text = "" + currTargeted.GetComponent<Character>().RES;
        charInfoPanel.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>().text = "" + currTargeted.GetComponent<Character>().SPD;

        if (isTargetEnemy == false)
        {
            charInfoPanel.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>().text = "" + currTargeted.GetComponent<Character>().MOV;
            charInfoPanel.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>().text = "" + currTargeted.GetComponent<Character>().movLeft;
            charInfoPanel.transform.GetChild(8).gameObject.SetActive(true);
            charInfoPanel.transform.GetChild(15).gameObject.SetActive(true);
        }
        else
        {
            charInfoPanel.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>().text = ""
                + currTargeted.GetComponent<Character>().MOV;
            charInfoPanel.transform.GetChild(8).gameObject.SetActive(false);
            charInfoPanel.transform.GetChild(15).gameObject.SetActive(false);
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
            playerUnits[i].gameObject.SetActive(true);
        }
    }
}
