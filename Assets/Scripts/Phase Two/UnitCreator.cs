using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitCreator : MonoBehaviour
{
    [System.NonSerialized] public List<Unit> units = new List<Unit>(); // list of all units
    public Unit unit; // current unit
    private int index = 0;
    [SerializeField] private TextMeshProUGUI text;
    private TileHolder tileHolder;
    [SerializeField] private UIWorldTileScript endTurnTile;

    // Start is called before the first frame update
    void Start()
    {
        tileHolder = GetComponentInChildren<TileHolder>();

    }

    public void addSpell(SpellTile spell)
    {
        if (unit != null)
        {
            if (unit.spellSlots > unit.spellTiles.Count)
            {
                spell.spellHolder.removeTile(spell);
                spell.transform.SetParent(tileHolder.transform);
                unit.spellTiles.Add(spell);
            }
        }

    }

    public void clearUnits()
    {
        foreach (SpellTile spell in unit.spellTiles)
        {
            spell.gameObject.SetActive(false);
        }
        units.Clear();
        unit = null;
    }

    public bool canEndTurn()
    {
        if (units.Count == 0) return false;
        bool canEnd = true;
        foreach(Unit unit in units)
        {
            bool needSpells = unit.spellSlots != unit.spellTiles.Count;
            if ((unit.worth != 0) || (needSpells && !GameManager.instance.spellHolder.hasNoTiles()))
            {
                Debug.Log(unit.worth);
                Debug.Log(needSpells && GameManager.instance.spellHolder.hasNoTiles());
                canEnd = false;
            }
        
        }

        Debug.Log(canEnd);
        return canEnd;
    }

    public void removeSpell(SpellTile spell)
    {
        unit.spellTiles.Remove(spell);
    }

    public void addUnit(Unit newUnit)
    {
        units.Add(newUnit);
        index = units.Count - 1;
        unit = units[index];
    }

    public void changeIndex(bool up)
    {
        index += up ? 1 : -1;
        if (index == units.Count)
        {
            index = 0;
        }
        else if (index == -1)
        {
            index = units.Count - 1;
        }
        foreach (SpellTile spell in unit.spellTiles)
        {
            spell.gameObject.SetActive(false);
        }
        unit = units[index];
        foreach(SpellTile spell in unit.spellTiles)
        {
            spell.gameObject.SetActive(true);
        }
    }

    public void upStat(string stat)
    {
        if (unit.worth > 0)
        {
            unit.worth--;
            if (stat == "HP")
            {
                unit.hp++;
            }
            else if (stat == "DMG")
            {
                unit.dmg++;
            }
            else if (stat == "SPD")
            {
                unit.moveSpeed++;
            }
            else if (stat == "ARMOR")
            {
                unit.armor++;
            }
            else if (stat == "PRC")
            {
                unit.piercing++;
            }
        }

    }

    public void downStat(string stat)
    {
        if (stat == "HP" && unit.hp > unit.hpMin)
        {
            unit.hp--;
            unit.worth++;
        }
        else if (stat == "DMG" && unit.dmg > unit.dmgMin)
        {
            unit.dmg--;
            unit.worth++;
        }
        else if (stat == "SPD" && unit.moveSpeed > unit.moveSpeedMin)
        {
            unit.moveSpeed--;
            unit.worth++;
        }
        else if (stat == "ARMOR" && unit.armor > unit.armorMin)
        {
            unit.armor--;
            unit.worth++;
        }
        else if (stat == "PRC" && unit.piercing > unit.pierceMin)
        {
            unit.piercing--;
            unit.worth++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (unit != null)
        {
            text.text = unit.word;
        }
        else
        {
            text.text = "";
        }
        if (canEndTurn())
        {
            endTurnTile.setActive(true);
        }
        else
        {
            endTurnTile.setActive(false);
        }
    }
}
