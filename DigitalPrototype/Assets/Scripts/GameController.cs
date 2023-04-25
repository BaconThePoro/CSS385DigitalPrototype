using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    // we need a pointer to both Player and Enemy controller. Must be connected via unity editor. 
    public GameObject playerControllerObj = null;
    private PlayerController playerController = null;
    public GameObject enemyControllerObj = null;
    private EnemyController enemyController = null;
    public GameObject mainCameraObj = null;
    private Camera mainCamera = null;
    public GameObject Mapmode = null;
    public GameObject Battlemode = null;
    public GameObject leftHPUI = null;
    public GameObject rightHPUI = null;

    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { PlayerTurn, EnemyTurn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displayhing, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    // Must be connected via unity editor
    public GameObject turnPanel = null;
    public TMPro.TMP_Text turnModeTXT = null;
    public Button endTurnButton = null;

    // Must be connected via unity editor
    public Grid currGrid = null;
    public Tilemap currTilemap = null;
    public Tile hoverTile = null;

    // defaulted to this so the hover doesnt overwrite an existing tile on first deselection
    private Vector3Int previousMousePos = new Vector3Int(0, 0, -999);

    // stuff for battlemode
    private Vector3 leftBattlePos = new Vector3(-2, 0, -1);
    private Vector3 rightBattlePos = new Vector3(2, 0, -1);
    private Vector3 camBattlePos = new Vector3(0, 0.5f, -50);
    private float camBattleSize = 2;
    private Quaternion leftBattleQua = new Quaternion();
    private Quaternion rightBattleQua = new Quaternion(0, 180, 0, 1);
    private Vector3 savedPosLeft;
    private Vector3 savedPosRight;
    private Vector3 savedPosCam;
    private Quaternion savedQuaLeft;
    private Quaternion savedQuaRight;
    private float savedCamSize;
    private enum doubleAttack { neitherDouble, leftDoubles, rightDoubles };
    private int doubleRequirement = 4;
    //

    // set these ones 
    public float tileX = 1;
    public float tileY = 1;

    // world limit is limit camera should be movable
    float worldLimX = 10f;
    float worldLimY = 5f;
    float camMoveAmount = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        playerController = playerControllerObj.GetComponent<PlayerController>();
        enemyController = enemyControllerObj.GetComponent<EnemyController>();
        mainCamera = mainCameraObj.GetComponent<Camera>();

        changeTurn(turnMode.PlayerTurn);
        changeMode(gameMode.MapMode);

        updateTurnText();
    }

    // Update is called once per frame
    void Update()
    {
        // Map mode only
        if (currGameMode == gameMode.MapMode)
        {
            // camera move up
            if (Input.GetKey(KeyCode.W) && mainCamera.transform.position.y < worldLimY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + camMoveAmount, mainCamera.transform.position.z); 
            }
            // camera move down
            if (Input.GetKey(KeyCode.S) && mainCamera.transform.position.y > -worldLimY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - camMoveAmount, mainCamera.transform.position.z);
            }
            // camera move left
            if (Input.GetKey(KeyCode.A) && mainCamera.transform.position.x > -worldLimX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x - camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }
            // camera move right
            if (Input.GetKey(KeyCode.D) && mainCamera.transform.position.x < worldLimX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x + camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }

            // mouse hover over effect
            Vector3Int mousePos = GetMousePosition();

            // adjust pos to z -5, see below
            mousePos = new Vector3Int(mousePos.x, mousePos.y, -6);

            if (!mousePos.Equals(previousMousePos))
            {
                // select layer is z: -6 (cause I randomly decided)
                Vector3Int mousePosZHover = new Vector3Int(mousePos.x, mousePos.y, -6);
                currTilemap.SetTile(previousMousePos, null); // Remove old hoverTile
                currTilemap.SetTile(mousePosZHover, hoverTile);
                previousMousePos = mousePos;
            }
        }
    }

    // turn true = player turn, turn false = enemy turn
    public void startBattle(GameObject leftChar, GameObject rightChar, bool turn)
    {
        Debug.Log("starting battle");

        Character leftStats = leftChar.GetComponent<Character>();
        Character rightStats = rightChar.GetComponent<Character>();

        // go to battlemode
        turnPanel.SetActive(false);
        playerController.deselectTarget();
        playerController.deactivateChildren();
        enemyController.deactivateChildren();
        Mapmode.SetActive(false);
        Battlemode.SetActive(true);
        savedCamSize = mainCamera.orthographicSize;
        mainCamera.orthographicSize = camBattleSize;
        leftHPUI.SetActive(true);
        rightHPUI.SetActive(true);
        leftHPUI.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "HP: " + leftStats.hpLeft + " / " + leftStats.HP;
        rightHPUI.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "HP: " + rightStats.hpLeft + " / " + rightStats.HP;
        //

        // reactivate participants
        leftChar.SetActive(true);
        rightChar.SetActive(true);

        // save position and rotation for both participants (and camera) before we move them
        savedPosLeft = leftChar.transform.position;
        savedPosRight = rightChar.transform.position;
        savedPosCam = mainCamera.transform.position;
        savedQuaLeft = leftChar.transform.rotation;
        savedQuaRight = rightChar.transform.rotation;
        //

        // move both participants (and camera) to position for battle
        leftChar.transform.position = leftBattlePos;
        rightChar.transform.position = rightBattlePos;
        mainCamera.transform.position = camBattlePos;
        leftChar.transform.rotation = leftBattleQua;
        rightChar.transform.rotation = rightBattleQua;
        //


        // do battle stuff like animations and damage

        // figure out if one battler is double attacking or not
        doubleAttack whoDoubles;
        if (leftStats.SPD > rightStats.SPD + doubleRequirement)
        {
            whoDoubles = doubleAttack.leftDoubles;
        }
        else if (rightStats.SPD > leftStats.SPD + doubleRequirement)
        {
            whoDoubles = doubleAttack.rightDoubles;
        }
        else
            whoDoubles = doubleAttack.neitherDouble;

        //if (turn )



        // this is only here to stall while battle doesnt do anything
        StartCoroutine(waitCoroutine(leftChar, rightChar, turn));
    }

    // this is only here to stall while battle doesnt do anything
    IEnumerator waitCoroutine(GameObject leftChar, GameObject rightChar, bool turn)
    {
        yield return new WaitForSeconds(3);
        endBattle(leftChar, rightChar, turn);
    }

    // turn true = player turn, turn false = enemy turn
    public void endBattle(GameObject leftChar, GameObject rightChar, bool turn)
    {
        // return them to prior positions
        leftChar.transform.position = savedPosLeft;
        rightChar.transform.position = savedPosRight;
        mainCamera.transform.position = savedPosCam;
        mainCamera.orthographicSize = savedCamSize;
        leftChar.transform.rotation = savedQuaLeft;
        rightChar.transform.rotation = savedQuaRight;
        //

        // return to mapmode
        leftHPUI.SetActive(false);
        rightHPUI.SetActive(false);
        turnPanel.SetActive(true);
        playerController.activateChildren();
        enemyController.activateChildren();
        Mapmode.SetActive(true);
        Battlemode.SetActive(false);
        changeMode(gameMode.MapMode);
        //

        // return to either player or enemy turn
        if (turn == true)
            playerController.ourTurn = true;
        else
            playerController.ourTurn = false;
    }

    // for hovering effect
    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return currGrid.WorldToCell(mouseWorldPos);
    }

    public void changeTurn(turnMode newTurn)
    {
        turnMode prevTurnMode = currTurnMode;
        currTurnMode = newTurn;

        // make sure to update turn text as well
        updateTurnText();

        if (currTurnMode == newTurn)
        {
            Debug.Log("turnMode changed from " + prevTurnMode + " to " + newTurn);
           
            // if player turn
            if (currTurnMode == turnMode.PlayerTurn)
            {
                playerController.resetAllMove();
                playerController.ourTurn = true;

                // give player back their end turn button
                endTurnButton.gameObject.SetActive(true);
            }
            // if enemy turn
            else
            {
                playerController.ourTurn = false;

                // turn off end turn button for player since it isnt their turn
                endTurnButton.gameObject.SetActive(false);

                enemyController.enemyTurn();
            }
        }
        else
        {
            Debug.Log("!!! Failed to change turnMode from " + prevTurnMode + " to " + newTurn);
            
        }
    }

    public void changeMode(gameMode newMode)
    {
        gameMode prevGameMode = currGameMode; 
        currGameMode = newMode;

        if (currGameMode == newMode)
        {
            Debug.Log("gameMode changed from " + prevGameMode + " to " + newMode);
        }
        else
        {
            Debug.Log("!!! Failed to change gameMode from " + prevGameMode + " to " + newMode);
        }
    }

    public void updateTurnText()
    {
        if (currTurnMode == turnMode.PlayerTurn)
        {
            turnModeTXT.text = "Player Turn";    
        }
        else 
        {
            turnModeTXT.text = "Enemy Turn";
        }
    }

    public void endTurnButtonPressed()
    {
        if (currTurnMode == turnMode.PlayerTurn)
            changeTurn(turnMode.EnemyTurn);
        else
            Debug.Log("!!! The end turn button was pressed BUT it isnt currently the players turn");
    }
}
