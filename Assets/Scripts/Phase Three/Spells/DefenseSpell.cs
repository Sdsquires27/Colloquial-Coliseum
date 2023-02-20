using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "New Spell/DefenseSpell")]
public class DefenseSpell : Action
{
    public int defense;

    public override string type
    {
        get
        {
            return "Defense";
        }
    }
    public override int size
    {
        get
        {
            return 0;
        }
    }

    public override bool targetsEnemy
    {
        get
        {
            return true;
        }
    }

    public override bool isAoe
    {
        get
        {
            return false;
        }
    }


    public override void use(TileObject tileToAffect)
    {

    }
    public override void use(Vector3Int tileToAffect, PlayerController playerController)
    {
        throw new System.NotImplementedException();
    }
}
