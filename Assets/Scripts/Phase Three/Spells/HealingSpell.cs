using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "New Spell/Healing Spell")]
public class HealingSpell : Action
{
    public int spread;
    public bool aoe;

    public override string type
    {
        get
        {
            return "Healing";
        }
    }
    public override int size
    {
        get
        {
            return spread;
        }
    }

    public override bool targetsEnemy
    {
        get
        {
            return false;
        }
    }



    public override bool isAoe
    {
        get
        {
            return aoe;
        }
    }

    public override void use(TileObject tileToAffect)
    {

    }

    public override void use(Vector3Int tileToAffect, PlayerController curPlayer)
    {
        throw new System.NotImplementedException();
    }
}