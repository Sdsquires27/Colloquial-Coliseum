using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class GameManager : MonoBehaviour
{
    // a class for controlling movement during and between scenes

    #region Variable Declaration
    [Header("Components")]
    [SerializeField] private LetterScript letterPrefab; // prefab for the letter that is generated
    [SerializeField] private GameObject layoutPrefab; // prefab for the horizontal layout used in phase one (used as prefab for customization)
    [SerializeField] private PlayerController[] playerList; // an array of the players
    [SerializeField] private SpellTile spellTilePrefab;
    [SerializeField] private GameObject personalMatPrefab;

    [Header("Settings")]
    [SerializeField] private int lettersPerPlayer;
    [SerializeField] private int lettersPerRow; // used in phase one drafting, number of letters in group
    [SerializeField] private int spellNum; // number of spells generated to choose from in phase one
    [SerializeField] private int spellsPerPack; // number of spells generated per pack

    // accessed by other clases
    [System.NonSerialized] public List<string> wordList = new List<string>(); // list of letters created during phase one
    [System.NonSerialized] public List<List<string>> spellList = new List<List<string>>();
    [System.NonSerialized] public PlayerController curPlayer; // the player's turn
    [System.NonSerialized] public int numLetters; // number of letters that will be generated

    // private variables

    // ALL PHASE VARIABLES
    private static GameManager _instance;
    public static GameManager instance { get { return _instance; } }
    private int numPlayers; // number of players
    private int playerNum = 0; // current player ID
    private int score;
    private int numRounds;
    private int bonusPerRound;
    private bool draftingOn;
    private bool buyingScore;
    private bool handicapOff;

    // MODE SELECT VARS
    [SerializeField] private UIWorldTileScript[] wordTileScripts; // settings set by word tile scripts (drafting type, buying type, handicap on)
    [SerializeField] private SlideSettingManager[] slideSettingManagers; // settings set by slide setting managers (num players, num rounds, bonus per round)

    // PHASE ONE VARIABLES
    private bool nextScene = false;
    List<GameObject> horizontalLayouts = new List<GameObject>(); // horizontal layouts used in phases one
    [System.NonSerialized] public GameObject canvas; // phase one canvas
    private List<char> chars = new List<char>(); // list of all the characters that can be drawn from (multiple per letter). Used in phase one generation
    private int curTileGroup = 0; // current tile group that has been revealed, phase one
    int numPicks = 0; // number of picks that have been grabbed this reveal, phase one

    //PHASE TWO VARIABLES
    private LetterHolder letterHolder; // phase two holder of letters not used
    [System.NonSerialized] public LetterHolder spellHolder; // phase two spell holder of spells not used
    private WordMaker wordMaker; // holds the words that are created
    private Transform horizontalLayout; // the horizontal layout used in phase two (split automatically by letter holder)
    private Transform spellHorizontalLayout; // the horizontal layout used in phase two (split automatically by letter holder)
    private UnitCreator curCreator; // the current unit creator being used
    // text for the unit stats:
    private TextMeshProUGUI hpText;
    private TextMeshProUGUI pierceText;
    private TextMeshProUGUI armorText;
    private TextMeshProUGUI dmgText;
    private TextMeshProUGUI spdText;

    // PHASE THREE VARIABLES
    private LevelScript levelScript; // access to the script that controls everything in phase 3
    private GameObject panel; // end of game panel

    #endregion

    #region Unity Methods
    private void Awake()
    {
        // add the onSceneLoad function to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            _instance = this;
        }
        else
        {
            
            Debug.Log("Duplicate Game Managers");
            Destroy(instance.gameObject);
            _instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "ModeSelect")
        {
            if (wordTileScripts[0].activated)
            {
                for(int i = 1; i < wordTileScripts.Length; i++)
                {
                    wordTileScripts[i].enabled = false;
                    wordTileScripts[i].GetComponent<Image>().color = Color.gray;
                }
                slideSettingManagers[2].enabled = false;
                foreach(Image image in slideSettingManagers[2].transform.GetComponentsInChildren<Image>())
                {
                    image.color = Color.gray;
                }
            }
            else
            {
                for (int i = 1; i < wordTileScripts.Length; i++)
                {
                    wordTileScripts[i].enabled = true;
                    wordTileScripts[i].GetComponent<Image>().color = Color.white;
                }
                slideSettingManagers[2].enabled = true;
                foreach (Image image in slideSettingManagers[2].transform.GetComponentsInChildren<Image>())
                {
                    image.color = Color.white;
                }
            }
            numPlayers = slideSettingManagers[0].curNum;
            numRounds = slideSettingManagers[1].curNum;
            bonusPerRound = slideSettingManagers[2].curNum;
            draftingOn = wordTileScripts[0].activated;
            buyingScore = wordTileScripts[1].activated;
            handicapOff = wordTileScripts[2].activated;



        }
        else if (SceneManager.GetActiveScene().name == "Phase Two")
        {
            // set the text on screen to current stats
            if (curCreator == null)
            {
                hpText.text = "0";
                armorText.text = "0";
                dmgText.text = "0";
                spdText.text = "0";
                pierceText.text = "0";
            }
            else if (curCreator.unit == null)
            {
                hpText.text = "0";
                armorText.text = "0";
                dmgText.text = "0";
                spdText.text = "0";
                pierceText.text = "0";
            }
            else
            {

                hpText.text = curCreator.unit.hp.ToString();
                armorText.text = curCreator.unit.armor.ToString();
                spdText.text = curCreator.unit.moveSpeed.ToString();
                dmgText.text = curCreator.unit.dmg.ToString();
                pierceText.text = curCreator.unit.piercing.ToString();
            }
        }
    }
    #endregion

    #region Scene transition methods
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // when a new scene starts, initialize the scene
        canvas = GameObject.FindGameObjectWithTag("canvas");
        if(scene.name == "Phase One")
        {
            startPhaseOne();
        }
        if (scene.name == "Phase Two")
        {
            startPhaseTwo();
            phaseTwoTurn(playerNum);
        }
        if (scene.name == "Phase Three")
        {
            startPhaseThree();
        }
    }

    public void loadNextScene()
    {
        // loads the next scene in the sequence based on the current scene
        if (SceneManager.GetActiveScene().name == "Phase One")
        {
            SceneManager.LoadScene("Phase Two");
        }
        else if (SceneManager.GetActiveScene().name == "Phase Two")
        {
            SceneManager.LoadScene("Phase Three");
        }
    }

    #endregion

    #region Turn handlers
    void startPhaseOne()
    {
        int j = 0;
        foreach (PlayerController player in playerList)
        {
            Image tempSr = GameObject.FindGameObjectWithTag("Mat" + (j + 1)).GetComponent<Image>();
            tempSr.color = playerList[j].color;
            player.gameObject.SetActive(true);
            GameObject y = Instantiate(personalMatPrefab, GameObject.FindGameObjectWithTag("Mat" + (j + 1)).GetComponent<Transform>());
            y.GetComponent<Image>().color = playerList[j].color;
            j++;
        }

        numLetters = lettersPerPlayer * (numPlayers + 1);
        curPlayer = playerList[0];
        for (int i = numPlayers; i < playerList.Length; i++)
        {
            playerList[i].enabled = false;
            GameObject y = GameObject.FindGameObjectWithTag("Mat" + (i+1));
            y.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            y.transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 0);


        }

        List<char>[] levelOneChars = new List<char>[numPlayers];
        for(int i = 0; i < levelOneChars.Length; i++)
        {
            levelOneChars[i] = new List<char>();
        }

        //create a vertical layout group that holds the horizontal layout groups which hold the tiles
        GameObject verticalLayout = new GameObject();
        VerticalLayoutGroup verticalLayoutGroup = verticalLayout.AddComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.spacing = 10;

        // add new vertical layout to 
        verticalLayout.transform.SetParent(canvas.transform);
        verticalLayout.transform.localPosition = new Vector3(0, 380, 0);
        verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;

        // grab the letters from the folder in resources
        Letter[] files = Resources.LoadAll<Letter>("Letters");

        // for each letter, add a corresponding number of characters to the pot
        foreach (Letter letter in files)
        {
            for (int i = 0; i < letter.amount; i++)
            {
                chars.Add(letter.character);
            }
            if(letter.worth == 1)
            {
                foreach(List<char> list in levelOneChars)
                {
                    list.Add(letter.character);

                }
            }
        }

        // make new horizontal layout to hold tiles
        for (int i = 0; i < numLetters / lettersPerRow; i++)
        {
            GameObject temp = Instantiate(layoutPrefab, verticalLayout.transform);
            horizontalLayouts.Add(temp);
        }
        int layoutChildren = 0;
        int curLayout = 0;
        for (int i = 0; i < numLetters; i++)
        {
            // once horizontal layout is full, create new layout
            if (layoutChildren == lettersPerRow)
            {
                curLayout++;
                layoutChildren = 0;
            }

            // generate random letter for tile and remove that letter from the list
            int randNum = Random.Range(0, chars.Count - 1);
            char randChar = chars[randNum];
            chars.RemoveAt(randNum);

            layoutChildren++;
            // create new letter
            generateLetter(randChar, horizontalLayouts[curLayout].transform);
        }


        horizontalLayouts[0].GetComponent<TileHolder>().flipTiles();

        // generate spells
        Sprite[] spellIcons = Resources.LoadAll<Sprite>("Sprites/Spells/Packs");
        List<Sprite> chosenSpells = new List<Sprite>();
        GameObject x = Instantiate(layoutPrefab, verticalLayout.transform);
        for (int i = 0; i < spellNum; i++)
        {
            generateSpell(x, spellIcons[Random.Range(0, spellIcons.Length - 1)]);

        }

        for (int i = 0; i < numPlayers; i++)
        {
            GameObject y = GameObject.FindGameObjectWithTag("Mat" + (i + 1));
            Transform tileHolder = y.transform.GetChild(0).GetChild(0);
            List<char> list = levelOneChars[i];

            for (int l = 0; l < 4; l++)
            {
                int rand = Random.Range(0, list.Count - 1);
                generateLetter(list[rand], tileHolder);
                list.RemoveAt(rand);
            }



        }
        activatePlayerMat();

    }

    public void endPhaseThree(TileObject[] winningTiles)
    {
        panel.SetActive(true);

        TextMeshProUGUI unitText = GameObject.FindGameObjectWithTag("Unit Text").GetComponent<TextMeshProUGUI>();

        unitText.text = "UNITS:";
        
        foreach(TileObject tileObject in winningTiles)
        {
            unitText.text += "\n" + tileObject.unit.word;
        }

    }

    public void endGame()
    {
        Debug.Log("Game Over");
    }

    private void startPhaseTwo()
    {
        // player turn is 0
        playerNum = 0;
        // find the letter holder in this scene
        letterHolder = GameObject.FindGameObjectWithTag("LetterHolder").GetComponent<LetterHolder>();
        spellHolder = GameObject.FindGameObjectWithTag("SpellHolder").GetComponent<LetterHolder>();
        // grab the horizonatal layouts outside of the holder
        horizontalLayout = letterHolder.transform.GetChild(0);
        spellHorizontalLayout = spellHolder.transform.GetChild(0);
        wordMaker = FindObjectOfType<WordMaker>();

        curCreator = FindObjectOfType<UnitCreator>(); 
        if (curCreator == null) Debug.Log("Something went wrong");

        hpText = GameObject.FindGameObjectWithTag("HpText").GetComponent<TextMeshProUGUI>();
        pierceText = GameObject.FindGameObjectWithTag("PierceText").GetComponent<TextMeshProUGUI>();
        armorText = GameObject.FindGameObjectWithTag("ArmorText").GetComponent<TextMeshProUGUI>();
        spdText = GameObject.FindGameObjectWithTag("SpeedText").GetComponent<TextMeshProUGUI>();
        dmgText = GameObject.FindGameObjectWithTag("DmgText").GetComponent<TextMeshProUGUI>();

    }

    private void startPhaseThree()
    {
        levelScript = FindObjectOfType<LevelScript>();
        List<Unit[]> unitLists = new List<Unit[]>();
        foreach(PlayerController player in playerList)
        {
            if (player.enabled)
            {
                Unit[] x = player.unitList.ToArray();
                unitLists.Add(x);
            }

        }

        levelScript.startPhase(unitLists, this);
    }

    public void phaseThreeTurn()
    {
        playerNum++;
        if (playerNum == playerList.Length || !playerList[playerNum].enabled)
        {
            playerNum = 0;
        }
        curPlayer = playerList[playerNum];
        levelScript.playTurn(curPlayer);
    }

    void phaseTwoTurn(int playerID)
    {
        // set the current player
        curPlayer = playerList[playerID];
        letterHolder.GetComponent<Image>().color = curPlayer.color;
        spellHolder.GetComponent<Image>().color = curPlayer.color;

        // grab the letters to generate
        string letters = wordList[playerID];

        // create the letters
        foreach (char letter in letters)
        {
            generateLetter(letter, horizontalLayout, true, true);
        }

        foreach (string spellPack in spellList[playerID])
        {
            Sprite[] spritePack = Resources.LoadAll<Sprite>("Sprites/Spells/Icons/" + spellPack);
            for (int i = 1; i < spellsPerPack; i++)
            {
                generateSpell(spellHorizontalLayout.gameObject, spritePack[Random.Range(0, spritePack.Length - 1)]);

            }
        }

    }

    public void switchTurns()
    {

        if (SceneManager.GetActiveScene().name == "Phase One")
        {
            // switch from player one to player to and vice versa
            playerNum += 1;
            // resets count if at the end of the current list of players
            if (playerNum == playerList.Length || !playerList[playerNum].enabled)
            {
                playerNum = 0;
                if (nextScene)
                {
                    string[] strings = new string[numPlayers];
                    int i = 0;
                    foreach (PlayerController player in playerList)
                    {
                        
                        if (player.enabled)
                        {
                            spellList.Add(player.getSpells());

                            Debug.Log(i);
                            strings[i] += player.getLetters();
                            for (int j = 0; j < strings.Length; j++)
                            {
                                if (j != i)
                                {
                                    strings[i] += GameObject.FindGameObjectWithTag("Mat" + (j + 1)).transform.GetChild(0).GetChild(0).GetComponent<TileHolder>().getLetters();
                                }
                            }
                            wordList.Add(strings[i]);

                            i++;

                        }
                    }
                    loadNextScene();
                }
            }
            activatePlayerMat();
            curPlayer = playerList[playerNum];

            numPicks++;
            // use this instead of below because of options beyond picking tiles
            if (numPicks == lettersPerRow - 2)
            {
                numPicks = 0;
                curTileGroup++;
                if (curTileGroup == horizontalLayouts.Count)
                {
                    nextScene = true;
                }
                else
                {

                    horizontalLayouts[curTileGroup].GetComponent<TileHolder>().flipTiles();
                }


                /*               if (playerNum == 0 && nextScene)
                               {
                                   foreach (PlayerController player in playerList)
                                   {
                                       if (player.enabled)
                                       {
                                           spellList.Add(player.getSpells());
                                           wordList.Add(player.getLetters());

                                       }
                                   }
                                   loadNextScene();
                               }
                               if (curTileGroup == horizontalLayouts.Count)
                               {
                                   nextScene = true;
                               }
                               else
                               {
                                   numPicks = 0;
                                   curTileGroup++;
                                   horizontalLayouts[curTileGroup].GetComponent<TileHolder>().flipTiles();
                               }
                */
            }
        }

        else if (SceneManager.GetActiveScene().name == "Phase Two")
        {
            if (FindObjectOfType<UnitCreator>().canEndTurn())
            {
                // save the units created
                UnitCreator unitCreator = FindObjectOfType<UnitCreator>();
                List<Unit> units = new List<Unit>(unitCreator.units);

                playerList[playerNum].unitList = units;
                foreach(Unit unit in playerList[playerNum].unitList)
                {
                    unit.generateSpells();
                    unit.printDescription();
                }

                // playerList[playerNum].unitList = unitCreator.units; THIS WORKS

                // clear the unit lists
                unitCreator.clearUnits();

                // delete all the tiles from the screen
                spellHolder.deleteLetters();
                letterHolder.deleteLetters();
                //move to the next turn
                playerNum++;

                // if the player is not the last player
                if (playerList[playerNum].enabled)
                {
                    // next turn
                    phaseTwoTurn(playerNum);
                }
                // if the player is the last player
                else
                {
                    // load phase three
                    loadNextScene();
                }
            }

        }
    }
    #endregion

    #region tile handlers
    void generateSpell(GameObject layoutHolder, Sprite icon)
    {
        // create a spell tile
        SpellTile tempSpell = Instantiate(spellTilePrefab, layoutHolder.transform);
        tempSpell.initialize(this, layoutHolder.GetComponent<TileHolder>(), icon);
        tempSpell.name = icon.name;
    }

    void generateLetter(char letterName, Transform parent, bool flip = false, bool teleport = false)
    {
        // makes a game object letterscript

        LetterScript x = Instantiate(letterPrefab, transform.position, Quaternion.identity);
        // find which letter matches the character
        Letter letter = Resources.Load<Letter>("Letters/" + letterName);
        // give the new letter tile that letter
        x.letter = letter;

        x.transform.SetParent(parent);
        parent.GetComponent<TileHolder>().letterScripts.Add(x);
        // initialize the letter
        x.initialize(this, parent.GetComponent<TileHolder>(), teleport);

        if (flip) x.flip(); // flip is instructed
    }

    public void handleTileClick(LetterScript letter)
    {
        // used in phase one
        if(SceneManager.GetActiveScene().name == "Phase One")
        {
            // scene one tile click

            // add the tile to the current player's store of letters
            curPlayer.addTile(letter);
            letter.tileHolder.removeTile(letter);

            // next turn
            switchTurns();
        }
        // used in phase two
        else if (SceneManager.GetActiveScene().name == "Phase Two")
        {
            // if the letter currently has a tile holder
            if (letter.tileHolder != null)
            {
                // add the tile to the word maker
                letter.tileHolder.removeTile(letter);
                letter.transform.SetParent(wordMaker.transform);
            }
            // if the letter currently does not have a tile holder
            else
            {
                // take the tile out of the word maker and back to the letter bank
                horizontalLayout.GetComponent<TileHolder>().addTile(letter);
            }
        }
    }

    public void handleTileClick(SpellTile spell)
    {
        if(SceneManager.GetActiveScene().name == "Phase One")
        {
            // scene one tile click

            // add spell to the current player's spells
            curPlayer.addTile(spell);
            spell.spellHolder.removeTile(spell);
            // next turn
            switchTurns();
        }
        else if (SceneManager.GetActiveScene().name == "Phase Two")
        {
            // if the spell holder does not have a holder (is not part of a unit)
            if (spell.spellHolder != null)
            {
                // place the spell in the current unit
                FindObjectOfType<UnitCreator>().addSpell(spell);
            }
            else
            {
                // take the spell out of the current unit and into the holder
                spellHorizontalLayout.GetComponent<TileHolder>().addTile(spell);
                FindObjectOfType<UnitCreator>().removeSpell(spell);
            }
        }
    }
    #endregion

    public static List<string> readTextFile(string file_path, string linebreak)
    {
        List<string> ret = new List<string>();
        StreamReader inp_stm = new StreamReader(file_path);

        while (!inp_stm.EndOfStream)
        {
            string toAdd = "";
            string inp_ln = inp_stm.ReadLine();
            if (inp_ln != linebreak)
            {
                toAdd += inp_ln;
            }
            else
            {
                ret.Add(toAdd);
                toAdd = "";
            }

        }

        inp_stm.Close();
        return ret;
    }
    public static List<string> readTextFile(string file_path)
    {
        List<string> ret = new List<string>();
        StreamReader inp_stm = new StreamReader(file_path);

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            ret.Add(inp_ln);

        }

        inp_stm.Close();
        return ret;
    }

    public static string RemoveSpecialCharacters(string str)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in str)
        {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public List<string> curSettings()
    {
        List<string> settings = new List<string>
        {
            draftingOn.ToString(),
            numRounds.ToString()
        };
        if (draftingOn)
        {
            settings.Add("null");
            settings.Add("null");
            settings.Add("null");

        }
        else
        {
            settings.Add(buyingScore.ToString());
            settings.Add(bonusPerRound.ToString());
            settings.Add(handicapOff.ToString());
        }

        return settings;
    }
    
    public void newSettings(string[] settings)
    {
        wordTileScripts[0].setValue(settings[0].ToLower().Trim() == "true" ? true : false);
        slideSettingManagers[1].setNum(int.Parse(settings[1]));

        if (!wordTileScripts[0].activated)
        {
            wordTileScripts[1].setValue(settings[2].ToLower().Trim() == "true" ? true : false);
            slideSettingManagers[2].setNum(int.Parse(settings[3]));
            wordTileScripts[2].setValue(settings[4].ToLower().Trim() == "true" ? true : false);
        }


    }
    private void activatePlayerMat()
    {
        for(int i = 0; i < 4; i++)
        {
            
            Image tempSr = GameObject.FindGameObjectWithTag("Mat" + (i + 1)).GetComponent<Image>();
            if (tempSr.color.a != 0f)
            {
                tempSr.color = new Color(tempSr.color.r, tempSr.color.g, tempSr.color.b, .5f);
                Image tempChildSr = tempSr.transform.GetChild(0).GetComponent<Image>();
                tempChildSr.color = new Color(tempChildSr.color.r, tempChildSr.color.g, tempChildSr.color.b, .5f);
                TileHolder tempTileHolder = tempChildSr.transform.GetChild(0).GetComponent<TileHolder>();
                tempTileHolder.hideTiles();

            }
        }
        Image sr = GameObject.FindGameObjectWithTag("Mat" + (playerNum + 1)).GetComponent<Image>();
        Image childSr = sr.transform.GetChild(0).GetComponent<Image>();
        childSr.color = new Color(childSr.color.r, childSr.color.g, childSr.color.b, 1);
        TileHolder tileHolder = childSr.transform.GetChild(0).GetComponent<TileHolder>();
        tileHolder.revealTiles();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
    }

    public void setPanel(GameObject panel)
    {
        panel.SetActive(false);
        this.panel = panel;
        
    }
    public PlayerController currentPlayer(int playerID)
    {
        return playerList[playerID];
    }
}