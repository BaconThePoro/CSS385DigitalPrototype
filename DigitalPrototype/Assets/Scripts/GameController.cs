using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using UnityEngine.SceneManagement;
using System;

public class GameController : MonoBehaviour
{
    // we need a pointer to both Player and Enemy controller. Must be connected via unity editor. 
    public GameObject playerControllerObj = null;
    private PlayerController playerController = null;
    public GameObject enemyControllerObj = null;
    private EnemyController enemyController = null;
    public GameObject savedPlayerCharsObj = null; 
    private savedPlayerChars savedPlayerChars = null;
    public GameObject mainCameraObj = null;
    private Camera mainCamera = null;
    public GameObject Mapmode = null;
    public GameObject Battlemode = null;
    public GameObject damageTXTPanel = null;
    private GameObject damageTXTObj = null;
    private TMPro.TextMeshProUGUI damageTXT = null;
    public GameObject charInfoPanelL = null;
    public GameObject charInfoPanelR = null;
    public GameObject VictoryScreen = null;
    public GameObject DefeatScreen = null;
    public GameObject turnPanel = null;
    public GameObject gearNumPanel = null;
    private GameObject gearNumPlus = null;
    public GameObject upgradeMenu = null;
    public TMPro.TMP_Text turnModeTXT = null;
    public Button endTurnButton = null;
    private Button speed1 = null;
    private Button speed2 = null;
    private Button speed4 = null;
    private Button speed8 = null;
    public Button upgradeButton = null; 
    public Grid currGrid = null;
    public Tilemap currTilemap = null;
    public Tile hoverTile = null;
    // stuff for battlemode
    private Vector3 leftBattlePos1 = new Vector3(-1.5f, 0, -1);
    private Vector3 rightBattlePos1 = new Vector3(1.5f, 0, -1);
    private Vector3 leftBattlePos2 = new Vector3(-2.5f, 0, -1);
    private Vector3 rightBattlePos2 = new Vector3(2.5f, 0, -1);
    private Vector3 camBattlePos = new Vector3(0, 1.25f, -50);
    private float camBattleSize = 2;
    private Quaternion leftBattleQua = new Quaternion();
    private Quaternion rightBattleQua = new Quaternion(0, 180, 0, 1);
    private Vector3 savedPosLeft;
    private Vector3 savedPosRight;
    private Vector3 savedPosCam;
    private Quaternion savedQuaLeft;
    private Quaternion savedQuaRight;
    private float savedCamSize;
    private float inbetweenAttackDelay = 0f;
    private float animationDuration = 0.3f;
    private Vector3 damageTextOffset = new Vector3(0, 0.8f, 0);
    // stat panel stuff
    private GameObject LmovLeftTXT = null;
    private GameObject LmovLeftNUMObj = null;
    private TMPro.TextMeshProUGUI LcharNameTXT = null;
    private TMPro.TextMeshProUGUI LhpNUM = null;
    private TMPro.TextMeshProUGUI LstrNUM = null;
    private TMPro.TextMeshProUGUI LmagNUM = null;
    private TMPro.TextMeshProUGUI LspdNUM = null;
    private TMPro.TextMeshProUGUI LdefNUM = null;
    private TMPro.TextMeshProUGUI LresNUM = null;
    private TMPro.TextMeshProUGUI LmovNUM = null;
    private TMPro.TextMeshProUGUI LmovLeftNUM = null;
    private GameObject RmovLeftTXT = null;
    private GameObject RmovLeftNUMObj = null;
    private TMPro.TextMeshProUGUI RcharNameTXT = null;
    private TMPro.TextMeshProUGUI RhpNUM = null;
    private TMPro.TextMeshProUGUI RstrNUM = null;
    private TMPro.TextMeshProUGUI RmagNUM = null;
    private TMPro.TextMeshProUGUI RspdNUM = null;
    private TMPro.TextMeshProUGUI RdefNUM = null;
    private TMPro.TextMeshProUGUI RresNUM = null;
    private TMPro.TextMeshProUGUI RmovNUM = null;
    private TMPro.TextMeshProUGUI RmovLeftNUM = null;
    // upgrade panel stuff
    private TMPro.TMP_InputField charName = null;
    private Image charImage = null;
    private Dropdown bodyDropdown = null;
    private Dropdown weaponDropdown = null;
    private TMPro.TextMeshProUGUI hpNUM = null;
    private TMPro.TextMeshProUGUI strNUM = null;
    private TMPro.TextMeshProUGUI magNUM = null;
    private TMPro.TextMeshProUGUI spdNUM = null;
    private TMPro.TextMeshProUGUI defNUM = null;
    private TMPro.TextMeshProUGUI resNUM = null;
    private TMPro.TextMeshProUGUI movNUM = null;
    private TMPro.TextMeshProUGUI hpMOD = null;
    private TMPro.TextMeshProUGUI strMOD = null;
    private TMPro.TextMeshProUGUI magMOD = null;
    private TMPro.TextMeshProUGUI spdMOD = null;
    private TMPro.TextMeshProUGUI defMOD = null;
    private TMPro.TextMeshProUGUI resMOD = null;
    private TMPro.TextMeshProUGUI movMOD = null;
    private TMPro.TextMeshProUGUI hpCOST = null;
    private TMPro.TextMeshProUGUI strCOST = null;
    private TMPro.TextMeshProUGUI magCOST = null;
    private TMPro.TextMeshProUGUI spdCOST = null;
    private TMPro.TextMeshProUGUI defCOST = null;
    private TMPro.TextMeshProUGUI resCOST = null;
    private TMPro.TextMeshProUGUI movCOST = null;
    private Image weaponIMG = null;
    private Button hpButton = null;
    private Button strButton = null;
    private Button magButton = null;
    private Button defButton = null;
    private Button resButton = null;
    private Button spdButton = null;
    private Button movButton = null;
    private GameObject weaponStatsPanel = null;
    private TMPro.TextMeshProUGUI weaponStats1 = null;
    private TMPro.TextMeshProUGUI weaponStats2 = null;
    private TMPro.TextMeshProUGUI weaponRange = null;
    public GameObject weaponSprites = null; 

    // enum for whose turn it is currently, the players or the enemies.
    public enum turnMode { PlayerTurn, EnemyTurn };
    private turnMode currTurnMode;

    // enum for what the game is currently doing/displayhing, a menu, the map, or a battle. 
    public enum gameMode { MenuMode, MapMode, BattleMode };
    private gameMode currGameMode;

    // defaulted to this so the hover doesnt overwrite an existing tile on first deselection
    private Vector3Int previousMousePos = new Vector3Int(0, 0, -999);

    // world limit is limit camera should be movable
    public float worldLimPlusX = 18f;
    public float worldLimPlusY = 11f;
    public float worldLimMinusX = 18f;
    public float worldLimMinusY = 11f;
    public int springAmount = 0;
    float camMoveAmount = 0.02f;
    float targetZoom;
    float sensitivity = 1;
    float camSpeed = 3;
    float maxZoom = 11;
    float minZoom = 2;

    bool isFocused = true;

    // pause menu stuff
    public GameObject settingsPanel = null; 
    public GameObject pauseMenu = null;
    public RawImage pC = null; 
    public Slider pR = null;
    public Slider pG = null;
    public Slider pB = null;
    private Color pColor = Color.white;
    private Color eColor = Color.red;

    // level 0 is testScene
    static int currLevel = 1; 

    // Start is called before the first frame update
    void Start()
    {
        // getting a billion components
        playerControllerObj = GameObject.Find("PlayerController");
        playerController = playerControllerObj.GetComponent<PlayerController>();
        enemyController = enemyControllerObj.GetComponent<EnemyController>();
        savedPlayerCharsObj = GameObject.Find("savedPlayerChars");
        savedPlayerChars = savedPlayerCharsObj.GetComponent<savedPlayerChars>();
        mainCamera = mainCameraObj.GetComponent<Camera>();
        damageTXTObj = damageTXTPanel.transform.GetChild(0).gameObject;
        damageTXT = damageTXTObj.GetComponent<TMPro.TextMeshProUGUI>();
        LcharNameTXT = charInfoPanelL.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        LmovLeftTXT = charInfoPanelL.transform.GetChild(9).gameObject;
        LhpNUM = charInfoPanelL.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        LstrNUM = charInfoPanelL.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        LmagNUM = charInfoPanelL.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        LspdNUM = charInfoPanelL.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        LdefNUM = charInfoPanelL.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        LresNUM = charInfoPanelL.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        LmovNUM = charInfoPanelL.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        LmovLeftNUMObj = charInfoPanelL.transform.GetChild(17).gameObject;
        LmovLeftNUM = LmovLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();
        RcharNameTXT = charInfoPanelR.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftTXT = charInfoPanelR.transform.GetChild(9).gameObject;
        RhpNUM = charInfoPanelR.transform.GetChild(10).GetComponent<TMPro.TextMeshProUGUI>();
        RstrNUM = charInfoPanelR.transform.GetChild(11).GetComponent<TMPro.TextMeshProUGUI>();
        RmagNUM = charInfoPanelR.transform.GetChild(12).GetComponent<TMPro.TextMeshProUGUI>();
        RspdNUM = charInfoPanelR.transform.GetChild(13).GetComponent<TMPro.TextMeshProUGUI>();
        RdefNUM = charInfoPanelR.transform.GetChild(14).GetComponent<TMPro.TextMeshProUGUI>();
        RresNUM = charInfoPanelR.transform.GetChild(15).GetComponent<TMPro.TextMeshProUGUI>();
        RmovNUM = charInfoPanelR.transform.GetChild(16).GetComponent<TMPro.TextMeshProUGUI>();
        RmovLeftNUMObj = charInfoPanelR.transform.GetChild(17).gameObject;
        RmovLeftNUM = RmovLeftNUMObj.GetComponent<TMPro.TextMeshProUGUI>();
        gearNumPlus = gearNumPanel.transform.GetChild(2).gameObject;
        //
        charName = upgradeMenu.transform.GetChild(1).transform.GetChild(3).GetComponent<TMPro.TMP_InputField>();
        charImage = upgradeMenu.transform.GetChild(1).transform.GetChild(6).GetComponent<Image>();
        bodyDropdown = upgradeMenu.transform.GetChild(1).transform.GetChild(8).GetComponent<Dropdown>();
        weaponDropdown = upgradeMenu.transform.GetChild(1).transform.GetChild(10).GetComponent<Dropdown>();
        hpNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(18).GetComponent<TMPro.TextMeshProUGUI>();
        strNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(19).GetComponent<TMPro.TextMeshProUGUI>();
        magNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(20).GetComponent<TMPro.TextMeshProUGUI>();
        spdNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(21).GetComponent<TMPro.TextMeshProUGUI>();
        defNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(22).GetComponent<TMPro.TextMeshProUGUI>();
        resNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(23).GetComponent<TMPro.TextMeshProUGUI>();     
        movNUM = upgradeMenu.transform.GetChild(1).transform.GetChild(24).GetComponent<TMPro.TextMeshProUGUI>();
        hpMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(25).GetComponent<TMPro.TextMeshProUGUI>();
        strMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(26).GetComponent<TMPro.TextMeshProUGUI>();
        magMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(27).GetComponent<TMPro.TextMeshProUGUI>();
        spdMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(28).GetComponent<TMPro.TextMeshProUGUI>();
        defMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(29).GetComponent<TMPro.TextMeshProUGUI>();
        resMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(30).GetComponent<TMPro.TextMeshProUGUI>();
        movMOD = upgradeMenu.transform.GetChild(1).transform.GetChild(31).GetComponent<TMPro.TextMeshProUGUI>();
        hpCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(34).GetComponent<TMPro.TextMeshProUGUI>();
        strCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(36).GetComponent<TMPro.TextMeshProUGUI>(); 
        magCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(38).GetComponent<TMPro.TextMeshProUGUI>();
        defCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(40).GetComponent<TMPro.TextMeshProUGUI>();
        resCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(42).GetComponent<TMPro.TextMeshProUGUI>();
        spdCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(44).GetComponent<TMPro.TextMeshProUGUI>();
        movCOST = upgradeMenu.transform.GetChild(1).transform.GetChild(46).GetComponent<TMPro.TextMeshProUGUI>();
        weaponIMG = upgradeMenu.transform.GetChild(1).transform.GetChild(47).GetComponent<Image>();
        hpButton = upgradeMenu.transform.GetChild(1).transform.GetChild(32).GetComponent<Button>();
        strButton = upgradeMenu.transform.GetChild(1).transform.GetChild(35).GetComponent<Button>();
        magButton = upgradeMenu.transform.GetChild(1).transform.GetChild(37).GetComponent<Button>();
        defButton = upgradeMenu.transform.GetChild(1).transform.GetChild(39).GetComponent<Button>();
        resButton = upgradeMenu.transform.GetChild(1).transform.GetChild(41).GetComponent<Button>();
        spdButton = upgradeMenu.transform.GetChild(1).transform.GetChild(43).GetComponent<Button>();
        movButton = upgradeMenu.transform.GetChild(1).transform.GetChild(45).GetComponent<Button>();
        weaponStatsPanel = upgradeMenu.transform.GetChild(1).transform.GetChild(48).gameObject;
        weaponStats1 = weaponStatsPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        weaponStats2 = weaponStatsPanel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        weaponRange = weaponStatsPanel.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>();
        //
        speed1 = turnPanel.transform.GetChild(3).GetComponent<Button>();
        speed2 = turnPanel.transform.GetChild(4).GetComponent<Button>();
        speed4 = turnPanel.transform.GetChild(5).GetComponent<Button>();
        speed8 = turnPanel.transform.GetChild(6).GetComponent<Button>();

        speed1ButtonPressed();

        targetZoom = mainCamera.orthographicSize;

        changeTurn(turnMode.PlayerTurn);
        changeMode(gameMode.MapMode);

        updateTurnText();

        //playerController.setGearNum(springAmount);

        savedPlayerChars.loadChars();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isFocused != isFocused)
        {
            isFocused = !isFocused;
            if (isFocused)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
        }

        // Map mode only
        if (currGameMode == gameMode.MapMode)
        {
            // camera move up
            if (Input.GetKey(KeyCode.W) && mainCamera.transform.position.y < worldLimPlusY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + camMoveAmount, mainCamera.transform.position.z);
            }
            // camera move down
            if (Input.GetKey(KeyCode.S) && mainCamera.transform.position.y > worldLimMinusY)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - camMoveAmount, mainCamera.transform.position.z);
            }
            // camera move left
            if (Input.GetKey(KeyCode.A) && mainCamera.transform.position.x > worldLimMinusX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x - camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }
            // camera move right
            if (Input.GetKey(KeyCode.D) && mainCamera.transform.position.x < worldLimPlusX)
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x + camMoveAmount, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }

            targetZoom -= Input.mouseScrollDelta.y * sensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            float newSize = Mathf.MoveTowards(mainCamera.orthographicSize, targetZoom, camSpeed * Time.deltaTime);
            mainCamera.orthographicSize = newSize;

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

            // if user presses right click, end turn
            if (Input.GetKey(KeyCode.Q) && currTurnMode == turnMode.PlayerTurn)
            {
                endTurnButtonPressed();
            }
        }
    }

    public void updateGearNumPanel()
    {
        gearNumPanel.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + playerController.getGearNum();
    }

    // playerSide = FALSE, player is on the left
    // playerSide = TRUE, player is on the right
    // playerTurn = False, this was not a playerTurn battle
    // playerTurn = true, this was a playerTurn battle

    // playerSide and Turn in conjunction tell who should strike first (whoevers turn it is ie. playerTurn attack means player attacks first)
    // if the attack occurs on a playerTurn they need control returned to them as well
    public IEnumerator startBattle(GameObject leftChar, GameObject rightChar, bool playerTurn, int battleRange)
    {
        //Debug.Log("starting battle");
        Character leftStats = leftChar.GetComponent<Character>();
        Character rightStats = rightChar.GetComponent<Character>();
        // go to battlemode
        turnPanel.SetActive(false);
        gearNumPanel.SetActive(false);
        settingsPanel.SetActive(false);
        playerController.deselectTarget();
        playerController.deactivateChildren();
        enemyController.deactivateChildren();
        Mapmode.SetActive(false);
        Battlemode.SetActive(true);
        savedCamSize = mainCamera.orthographicSize;
        mainCamera.orthographicSize = camBattleSize;
        charInfoPanelL.SetActive(true);
        charInfoPanelR.SetActive(true);
        updateBattleStats(leftStats, rightStats);
        // reactivate participants
        leftChar.SetActive(true);
        rightChar.SetActive(true);
        // save position and rotation for both participants (and camera) before we move them
        savedPosLeft = leftChar.transform.position;
        savedPosRight = rightChar.transform.position;
        savedPosCam = mainCamera.transform.position;
        savedQuaLeft = leftChar.transform.rotation;
        savedQuaRight = rightChar.transform.rotation;
        // move both participants (and camera) to position for battle
        if (battleRange == 1)
        {
            leftChar.transform.position = leftBattlePos1;
            rightChar.transform.position = rightBattlePos1;
        }
        else if (battleRange == 2)
        {
            leftChar.transform.position = leftBattlePos2;
            rightChar.transform.position = rightBattlePos2;
        }
        mainCamera.transform.position = camBattlePos;
        leftChar.transform.rotation = leftBattleQua;
        rightChar.transform.rotation = rightBattleQua;

        // delay for 1.5s so user can see before battle starts
        yield return new WaitForSeconds(inbetweenAttackDelay * 3);

        // Determine who should attack first
        GameObject firstAttacker;
        Character firstStats;
        GameObject secondAttacker;
        Character secondStats;

        // left is faster
        if (leftStats.SPD > rightStats.SPD)
        {
            firstAttacker = leftChar;
            secondAttacker = rightChar;
            firstStats = leftStats;
            secondStats = rightStats;
        }
        // speed tie
        else if (leftStats.SPD == rightStats.SPD)
        {
            // coin flip who goes first
            if (UnityEngine.Random.value >= 0.5)
            {
                firstAttacker = leftChar;
                secondAttacker = rightChar;
                firstStats = leftStats;
                secondStats = rightStats;
            }
            else
            {
                firstAttacker = rightChar;
                secondAttacker = leftChar;
                firstStats = rightStats;
                secondStats = leftStats;
            }
        }
        // right is faster
        else
        {
            firstAttacker = rightChar;
            secondAttacker = leftChar;
            firstStats = rightStats;
            secondStats = leftStats;
        }

        // first attacks
        if (firstStats.getAttackRange() >= battleRange)
        {
            yield return StartCoroutine(LerpPosition(firstAttacker, firstAttacker.transform.position + firstAttacker.transform.right, animationDuration));
            yield return new WaitForSeconds(inbetweenAttackDelay);
            yield return StartCoroutine(updateDamageTXT(secondAttacker, Attack(firstStats, secondStats))); // person taking damage and damage value
            updateBattleStats(leftStats, rightStats);
        }
 
        // delay
        yield return new WaitForSeconds(inbetweenAttackDelay);

        // second attack
        if (secondStats.getAttackRange() >= battleRange)
        {
            yield return StartCoroutine(LerpPosition(secondAttacker, secondAttacker.transform.position + secondAttacker.transform.right, animationDuration));
            yield return new WaitForSeconds(inbetweenAttackDelay);
            yield return StartCoroutine(updateDamageTXT(firstAttacker, Attack(secondStats, firstStats))); // person taking damage and damage value
            updateBattleStats(leftStats, rightStats);
        }

        // delay again for 1.5s so user can see result of battle before leaving battlemode
        yield return new WaitForSeconds(inbetweenAttackDelay * 4);   
        
        // return them to prior positions
        leftChar.transform.position = savedPosLeft;
        rightChar.transform.position = savedPosRight;
        mainCamera.transform.position = savedPosCam;
        mainCamera.orthographicSize = savedCamSize;
        leftChar.transform.rotation = savedQuaLeft;
        rightChar.transform.rotation = savedQuaRight;
        // return to mapmode
        charInfoPanelL.SetActive(false);
        charInfoPanelR.SetActive(false);
        turnPanel.SetActive(true);
        gearNumPanel.SetActive(true);
        settingsPanel.SetActive(true);
        playerController.activateChildren();
        enemyController.activateChildren();
        Mapmode.SetActive(true);
        Battlemode.SetActive(false);
        changeMode(gameMode.MapMode); 
        // return to either player or enemy turn
        if (playerTurn == true)
        {
            playerController.ourTurn = true;
            changeTurn(turnMode.PlayerTurn);
            leftStats.setAttack(false);
            rightStats.setAttack(false);
        }
        else
        {
            playerController.ourTurn = false;
        }

        // see if player gets some gears for killing something
        if (firstStats.getIsDead() == true && firstStats.getIsEnemy() == true
            || secondStats.getIsDead() == true && secondStats.getIsEnemy() == true)
        {
            playerController.giveGearNum(4);

            
            StartCoroutine(plusAnimation());
        }
    }

    public IEnumerator plusAnimation()
    {
        float time = 0;
        RawImage rI = gearNumPlus.GetComponent<RawImage>();
        Vector3 originalPos = gearNumPlus.transform.position;

        gearNumPlus.SetActive(true);
        while (time <= 0.2f)
        {
            gearNumPlus.transform.position = gearNumPlus.transform.position + new Vector3(0, 0.25f, 0);
            if (time > 0.1f)
                rI.color = new Color(1, 1, 1, rI.color.a * 0.80f);
            yield return new WaitForSeconds(0.01f);
            time = time + Time.deltaTime;
            //Debug.Log("time: " + time);
        }

        gearNumPlus.transform.position = originalPos;
        rI.color = new Color(1, 1, 1, 1);
        gearNumPlus.SetActive(false);
    }
    
    // false == left hurt, true == right hurt
    public IEnumerator updateDamageTXT(GameObject unitHurt, int damageNum)
    {
        damageTXTPanel.gameObject.SetActive(true);
        damageTXTPanel.transform.position = mainCamera.WorldToScreenPoint(unitHurt.transform.position + damageTextOffset);

        // dead char attack number
        if (damageNum == -999)
            yield return null;
        else if (damageNum == 0)
        {
            damageTXT.color = Color.yellow;
            damageTXT.text = "(" + damageNum + ")";

            yield return new WaitForSeconds(inbetweenAttackDelay * 3);

            damageTXT.text = "";
        }
        else
        {
            damageTXT.color = Color.red;
            damageTXT.text = "(-" + damageNum + ")";

            yield return new WaitForSeconds(inbetweenAttackDelay * 3);

            damageTXT.text = "";
        }

        damageTXTPanel.gameObject.SetActive(false);
    }

    public IEnumerator waitTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    public IEnumerator LerpPosition(GameObject theObject, Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector2 startPosition = theObject.transform.position;
        while (time < duration)
        {
            theObject.transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;   
        theObject.transform.position = targetPosition;

        while (time < duration)
        {
            theObject.transform.position = Vector2.Lerp(targetPosition, startPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        theObject.transform.position = startPosition;
    }
    

    public int Attack(Character attacker, Character damageTaker)
    {
        if (attacker.getIsDead() == true || damageTaker.getIsDead() == true)
            return -999;

        int damageMinusDefense = -1;
        // if attacker has a physical weapon
        if (attacker.GetWeaponType() == Character.weaponType.Sword || attacker.GetWeaponType() == Character.weaponType.Bow 
            || attacker.GetWeaponType() == Character.weaponType.Axe || attacker.GetWeaponType() == Character.weaponType.Spear)
        {
            damageMinusDefense = attacker.STR - damageTaker.DEF;
            // make sure you cant do negative damage
            if (damageMinusDefense < 0)
                damageMinusDefense = 0;
            
            damageTaker.takeDamage(damageMinusDefense);
        }
        // attacker has magic weapon
        else
        {
            damageMinusDefense = attacker.MAG - damageTaker.RES;
            // make sure you cant do negative damage
            if (damageMinusDefense < 0)
                damageMinusDefense = 0;

            damageTaker.takeDamage(damageMinusDefense);
        }

        // all player characters dead
        if (playerController.allDead())
        {
            //Debug.Log("All allies dead you lose");
            StartCoroutine(defeat());
        }
        // all enemy characters dead
        else if (enemyController.allDead())
        {
            //Debug.Log("All enemies dead you win");
            StartCoroutine(victory());
        }

        return damageMinusDefense;
    }

    public IEnumerator victory()
    {
        yield return new WaitForSeconds(2);
        VictoryScreen.SetActive(true);
        Mapmode.SetActive(false);
        turnPanel.SetActive(false);
        gearNumPanel.SetActive(false);
        charInfoPanelL.SetActive(false);
        charInfoPanelR.SetActive(false);
        playerControllerObj.SetActive(false);
        enemyControllerObj.SetActive(false);
    }

    public IEnumerator defeat()
    {
        yield return new WaitForSeconds(2);
        DefeatScreen.SetActive(true);
        Mapmode.SetActive(false);
        turnPanel.SetActive(false);
        gearNumPanel.SetActive(false);
        charInfoPanelL.SetActive(false);
        charInfoPanelR.SetActive(false);
        playerControllerObj.SetActive(false);
        enemyControllerObj.SetActive(false);
    }

    public void updateBattleStats(Character leftStats, Character rightStats)
    {
        LcharNameTXT.text = "Name: " + leftStats.charName;
        LhpNUM.text = "" + leftStats.hpLeft + " / " + leftStats.HP;
        LstrNUM.text = "" + leftStats.STR;
        LmagNUM.text = "" + leftStats.MAG;
        LdefNUM.text = "" + leftStats.DEF;
        LresNUM.text = "" + leftStats.RES;
        LspdNUM.text = "" + leftStats.SPD;
        LmovNUM.text = "" + leftStats.MOV;
        LmovLeftTXT.SetActive(false);
        LmovLeftNUMObj.SetActive(false);

        RcharNameTXT.text = "Name: " + rightStats.charName;
        RhpNUM.text = "" + rightStats.hpLeft + " / " + rightStats.HP;
        RstrNUM.text = "" + rightStats.STR;
        RmagNUM.text = "" + rightStats.MAG;
        RdefNUM.text = "" + rightStats.DEF;
        RresNUM.text = "" + rightStats.RES;
        RspdNUM.text = "" + rightStats.SPD;
        RmovNUM.text = "" + rightStats.MOV;
        RmovLeftTXT.SetActive(false);
        RmovLeftNUMObj.SetActive(false);
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
                if (playerController.ourTurn == false)
                {
                    playerController.ourTurn = true;
                }

                
                // give player back their end turn button
                endTurnButton.gameObject.SetActive(true);
            }
            // if enemy turn
            else
            {
                playerController.ourTurn = false;
                playerController.deselectTarget();

                // turn off end turn button for player since it isnt their turn
                endTurnButton.gameObject.SetActive(false);

                enemyController.resetAllMove();
                StartCoroutine(enemyController.enemyTurn());
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
        // if not in mapmode dont do anything
        if (currGameMode != gameMode.MapMode)
            return;

        if (currTurnMode == turnMode.PlayerTurn)
        {
            changeTurn(turnMode.EnemyTurn);
            playerController.resetAllAttack();
        }
        else
            Debug.Log("!!! The end turn button was pressed BUT it isnt currently the players turn");
    }

    public void upgradeButtonPressed()
    {
        // if not in mapmode dont do anything
        if (currGameMode != gameMode.MapMode)
            return;

        playerController.ourTurn = false;
        changeMode(gameMode.MenuMode);
        upgradeMenu.gameObject.SetActive(true);
        updateUpgradeMenu(playerController.getCurrTargeted());
    }

    public void closeUpgradeMenu()
    {
        if (upgradeMenu.activeSelf == true)
        {
            playerController.ourTurn = true;
            changeMode(gameMode.MapMode);
            upgradeMenu.gameObject.SetActive(false);
            playerController.updateCharInfo();
            playerController.openContextMenu(playerController.getCurrTargeted().transform.position);
        }
    }

    public void updateUpgradeMenu(GameObject character)
    {
        Character charStats = character.GetComponent<Character>();
        charName.text = charStats.name;
        charImage.sprite = character.GetComponent<SpriteRenderer>().sprite;
        charImage.transform.localScale = character.transform.localScale;

        weaponIMG.sprite = character.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite;
        weaponIMG.transform.localScale = charStats.transform.GetChild(1).localScale;
        charStats.updateCosts();

        if (bodyDropdown.options.Count == 0)
        {
            string[] bodyNames = Enum.GetNames(typeof(Character.bodyType));
            List<string> body = new List<string>(bodyNames);
            bodyDropdown.AddOptions(body);
        }
        else
        {
            bodyDropdown.value = (int)charStats.GetBodyType();
        }

        if (weaponDropdown.options.Count == 0)
        {
            List<string> weapons = new List<string>();

            for (int i = 0; i < weaponSprites.transform.childCount; i++)
            {
                weapons.Add(weaponSprites.transform.GetChild(i).name);
            }

            weaponDropdown.AddOptions(weapons);
        }
        else
        {
            weaponDropdown.value = (int)charStats.GetWeaponType();
        }

        hpNUM.text = "" + charStats.baseHP;
        strNUM.text = "" + charStats.baseSTR;
        magNUM.text = "" + charStats.baseMAG;
        defNUM.text = "" + charStats.baseDEF;
        resNUM.text = "" + charStats.baseRES;
        spdNUM.text = "" + charStats.baseSPD;
        movNUM.text = "" + charStats.baseMOV;

        weaponRange.text = "RNG: " + charStats.getAttackRange();
        weaponStats1.text = "";
        weaponStats2.text = "";

        if (charStats.HPMOD == 0 && charStats.STRMOD == 0 && charStats.MAGMOD == 0 && charStats.DEFMOD == 0
            && charStats.RESMOD == 0 && charStats.SPDMOD == 0 && charStats.MOVMOD == 0)
        {
            //weaponStatsPanel.gameObject.SetActive(false);
            weaponStats1.text = "";
            weaponStats2.text = "";
        }

        if (charStats.HPMOD == 0)
            hpMOD.text = "";
        else if (charStats.HPMOD > 0)
        {
            hpMOD.color = Color.green;
            hpMOD.text = "+ " + charStats.HPMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            weaponStats1.color = Color.green;
            weaponStats1.text = "+" + charStats.HPMOD + " HP";
        }
        else
        {
            hpMOD.color = Color.red;
            hpMOD.text = "- " + Mathf.Abs(charStats.HPMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            weaponStats1.color = Color.red;
            weaponStats1.text = "-" + Mathf.Abs(charStats.HPMOD) + " HP";
        }

        if (charStats.STRMOD == 0)
            strMOD.text = "";
        else if (charStats.STRMOD > 0)
        {
            strMOD.color = Color.green;
            strMOD.text = "+ " + charStats.STRMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.STRMOD + " STR";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.STRMOD + " STR";
            }
        }
        else
        {
            strMOD.color = Color.red;
            strMOD.text = "- " + Mathf.Abs(charStats.STRMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.STRMOD) + " STR";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.STRMOD) + " STR";
            }
        }

        if (charStats.MAGMOD == 0)
            magMOD.text = "";
        else if (charStats.MAGMOD > 0)
        {
            magMOD.color = Color.green;
            magMOD.text = "+ " + charStats.MAGMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.MAGMOD + " MAG";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.MAGMOD + " MAG";
            }
        }
        else
        {
            magMOD.color = Color.red;
            magMOD.text = "- " + Mathf.Abs(charStats.MAGMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.MAGMOD) + " MAG";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.MAGMOD) + " MAG";
            }
        }

        if (charStats.DEFMOD == 0)
            defMOD.text = "";
        else if (charStats.DEFMOD > 0)
        {
            defMOD.color = Color.green;
            defMOD.text = "+ " + charStats.DEFMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.DEFMOD + " DEF";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.DEFMOD + " DEF";
            }
        }
        else
        {
            defMOD.color = Color.red;
            defMOD.text = "- " + Mathf.Abs(charStats.DEFMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.DEFMOD) + " DEF";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.DEFMOD) + " DEF";
            }
        }

        if (charStats.RESMOD == 0)
            resMOD.text = "";
        else if (charStats.RESMOD > 0)
        {
            resMOD.color = Color.green;
            resMOD.text = "+ " + charStats.RESMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.RESMOD + " RES";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.RESMOD + " RES";
            }
        }
        else
        {
            resMOD.color = Color.red;
            resMOD.text = "- " + Mathf.Abs(charStats.RESMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.RESMOD) + " RES";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.RESMOD) + " RES";
            }
        }

        if (charStats.SPDMOD == 0)
            spdMOD.text = "";
        else if (charStats.SPDMOD > 0)
        {
            spdMOD.color = Color.green;
            spdMOD.text = "+ " + charStats.SPDMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.SPDMOD + " SPD";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.SPDMOD + " SPD";
            }
        }
        else
        {
            spdMOD.color = Color.red;
            spdMOD.text = "- " + Mathf.Abs(charStats.SPDMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.SPDMOD) + " SPD";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.SPDMOD) + " SPD";
            }
        }

        if (charStats.MOVMOD == 0)
            movMOD.text = "";
        else if (charStats.MOVMOD > 0)
        {
            movMOD.color = Color.green;
            movMOD.text = "+ " + charStats.MOVMOD;
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.green;
                weaponStats1.text = "+" + charStats.MOVMOD + " MOV";
            }
            else
            {
                weaponStats2.color = Color.green;
                weaponStats2.text = "+" + charStats.MOVMOD + " MOV";
            }
        }
        else
        {
            movMOD.color = Color.red;
            movMOD.text = "- " + Mathf.Abs(charStats.MOVMOD);
            weaponStatsPanel.gameObject.SetActive(true);
            if (weaponStats1.text == "")
            {
                weaponStats1.color = Color.red;
                weaponStats1.text = "-" + Mathf.Abs(charStats.MOVMOD) + " MOV";
            }
            else
            {
                weaponStats2.color = Color.red;
                weaponStats2.text = "-" + Mathf.Abs(charStats.MOVMOD) + " MOV";
            }
        }

        if (charStats.baseHP < charStats.getHPMAX())
        {
            hpButton.gameObject.SetActive(true);
            hpCOST.text = "x" + charStats.HPCost;
        }
        else
        {
            hpCOST.text = "MAX";
            hpButton.gameObject.SetActive(false);
        }


        if (charStats.baseSTR < charStats.getSTRMAX())
        {
            strButton.gameObject.SetActive(true);
            strCOST.text = "x" + charStats.STRCost;
        }        
        else
        {
            strCOST.text = "MAX";
            strButton.gameObject.SetActive(false);
        }

        if (charStats.baseMAG < charStats.getMAGMAX())
        {
            magButton.gameObject.SetActive(true);
            magCOST.text = "x" + charStats.MAGCost;
        }
        else
        {
            magCOST.text = "MAX";
            magButton.gameObject.SetActive(false);
        }

        if (charStats.baseDEF < charStats.getDEFMAX())
        {
            defButton.gameObject.SetActive(true);
            defCOST.text = "x" + charStats.DEFCost;
        }   
        else
        {
            defCOST.text = "MAX";
            defButton.gameObject.SetActive(false);
        }

        if (charStats.baseRES < charStats.getRESMAX())
        {
            resButton.gameObject.SetActive(true);
            resCOST.text = "x" + charStats.RESCost;
        } 
        else
        {
            resCOST.text = "MAX";
            resButton.gameObject.SetActive(false);
        }

        if (charStats.baseSPD < charStats.getSPDMAX())
        {
            spdButton.gameObject.SetActive(true);
            spdCOST.text = "x" + charStats.SPDCost;
        }       
        else
        {
            spdCOST.text = "MAX";
            spdButton.gameObject.SetActive(false);
        }

        if (charStats.baseMOV < charStats.getMOVMAX())
        {
            movButton.gameObject.SetActive(true);
            movCOST.text = "x" + charStats.MOVCost;
        }         
        else
        {
            movCOST.text = "MAX";
            movButton.gameObject.SetActive(false);
        }
    }

    public void speed1ButtonPressed()
    {
        inbetweenAttackDelay = 1;
        enemyController.setDelay(1);
        playerController.setDelay(1);

        speed1.interactable = false;
        speed2.interactable = true;
        speed4.interactable = true;
        speed8.interactable = true;
    }

    public void speed2ButtonPressed()
    {
        inbetweenAttackDelay = 0.5f;
        enemyController.setDelay(0.5f);
        playerController.setDelay(0.5f);

        speed1.interactable = true;
        speed2.interactable = false;
        speed4.interactable = true;
        speed8.interactable = true;
    }

    public void speed4ButtonPressed()
    {
        inbetweenAttackDelay = 0.2f;
        enemyController.setDelay(0.2f);
        playerController.setDelay(0.2f);

        speed1.interactable = true;
        speed2.interactable = true;
        speed4.interactable = false;
        speed8.interactable = true;
    }

    public void speed8ButtonPressed()
    {
        inbetweenAttackDelay = 0.1f;
        enemyController.setDelay(0.1f);
        playerController.setDelay(0.1f);

        speed1.interactable = true;
        speed2.interactable = true;
        speed4.interactable = true;
        speed8.interactable = false;
    }

    public void pauseButtonPressed()
    {
        Time.timeScale = 0;
        changeMode(gameMode.MenuMode);
        pauseMenu.SetActive(true);

        pC.color = pColor;
        pR.value = pC.color.r;
        pG.value = pC.color.g;
        pB.value = pC.color.b;
    }

    public void rChanged(Single s)
    {
        pColor = new Color(s, pColor.g, pColor.b, 1);
        pC.color = pColor;
    }

    public void gChanged(Single s)
    {
        pColor = new Color(pColor.r, s, pColor.b, 1);
        pC.color = pColor;
    }

    public void bChanged(Single s)
    {
        pColor = new Color(pColor.r, pColor.g, s, 1);
        pC.color = pColor;
    }

    public void continueButtonPressed()
    {
        Time.timeScale = 1;
        changeMode(gameMode.MapMode);
        pauseMenu.SetActive(false);
        //updateColors();
    }

    public void tutorialButtonPressed()
    {
        SceneManager.LoadScene("TutorialScene", LoadSceneMode.Additive);
    }

    public void exitButtonPressed()
    {
        Application.Quit();
    }

    public void updateColors()
    {
        for (int i = 0; i < playerController.playerUnits.Length; i++)
        {
            playerController.playerUnits[i].GetComponent<SpriteRenderer>().color = pColor;
        }
    }

    public void nextLevelButton()
    {
        Debug.Log("Loading next scene");
        savedPlayerChars.saveChars();
        currLevel = currLevel + 1;
        SceneManager.LoadScene(currLevel); // load scene
        //SceneManager.MoveGameObjectToScene(savedPlayerCharsObj, SceneManager.GetActiveScene());
    }

    public void retryButton()
    {
        // stats should get reset
        SceneManager.LoadScene(currLevel); // load scene
    }
}
