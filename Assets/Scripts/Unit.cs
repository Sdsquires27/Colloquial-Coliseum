using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Unit
{
    // holds unit data
    public string word;
    public LetterScript[] letterScripts;
    public List<Letter> letters = new List<Letter>();
    public int worth;
    public int hp;
    public int armor;
    public int piercing;
    public int pierceMin = 0;
    public int moveSpeed;
    public int range;
    public int dmg;
    public int hpMin = 1;
    public int armorMin = 0;
    public int moveSpeedMin = 1;
    public int dmgMin = 1;
    public int spellSlots = 1;
    public List<SpellTile> spellTiles = new List<SpellTile>();
    public List<Action> spells = new List<Action>();

    public Unit(string setWord, LetterScript[] setLetters)
    {
        // create a standard unit
        letterScripts = setLetters;
        word = setWord;
        hp = 1;
        armor = 0;
        moveSpeed = 1;
        range = 1;
        dmg = 1;
        piercing = 0;
        getLetters();
        getStats();
    }

    public void generateSpells()
    {
        foreach(SpellTile spellTile in spellTiles)
        {
            spells.Add(Resources.Load<Action>("Spells/"+spellTile.name));
            Debug.Log("Spells/" + spellTile.name);
            
        }
        foreach(Action spell in spells)
        {
            Debug.Log(spell.name);
        }
    }

    public void printDescription()
    {
        string toPrint = "UNIT " + word + "\n";

        toPrint += "Hp: " + hp + "\n" +
            "Dmg: " + dmg + "\n" +
            "Armor: " + hp + "\n" +
            "MoveSpeed: " + hp + "\n" +
            "Piercing: " + hp + "\nSpells:";
        foreach (Action spell in spells)
        {
            toPrint += "\n" + spell;
        }

        Debug.Log(toPrint);
        

    }

    private void getLetters()
    {
        foreach (LetterScript letter in letterScripts)
        {
            letters.Add(letter.letter);
        }
    }

    private void getStats()
    {
        foreach (Letter letter in letters)
        {
            if(letter.worth == 1)
            {
                worth += 1;
            }
            else if (letter.worth == 2)
            {
                worth += 1;
                giveRandomStat();
            }
            else if (letter.worth == 3)
            {
                worth += 2;
            }
            else if (letter.worth == 4)
            {
                spellSlots += 1 ;
            }
        }
        if(word == "SETH")
        {
            worth += 10; // hehe
        }
    }

    public void giveRandomStat()
    {
        int rand = Random.Range(0, 4);

        if (rand == 0)
        {
            hp += 1;
            hpMin += 1;
        }
        else if(rand == 1)
        {
            armor += 1;
            armorMin += 1;
        }
        else if (rand == 2)
        {
            dmg += 1;
            dmgMin += 1;
        }
        else if (rand == 3)
        {
            moveSpeed += 1;
            moveSpeedMin += 1;
        }
        else if (rand == 4)
        {
            piercing += 1;
            pierceMin += 1;
        }
           
        
    }
}
