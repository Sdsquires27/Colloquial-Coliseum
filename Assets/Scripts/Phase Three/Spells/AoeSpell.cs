using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "New Spell/Aoe Spell")]
public class AoeSpell : Action
{

    public int spread;

    public override string type
    {
        get
        {
            return "AoE";
        }
    }
    public override bool targetsEnemy
    {
        get
        {
            return true;
        }
    }

    public override int size
    {
        get
        {
            return spread;
        }
    }


    public override bool isAoe
    {
        get
        {
            return true ;
        }
    }
    [System.NonSerialized] public Vector3Int[] rangeTiles;

    public override void use(TileObject tileToAffect)
    {
        throw new System.NotImplementedException();
    }

    public override void use(Vector3Int startTile, PlayerController playerController)
    {
        Debug.Log("Aoe spell used");
        foreach (TileObject tileObject in LevelScript.objectsInTiles(LevelScript.tilesInRange(startTile, range)))
        {
            // don't deal damage to own player
            if (tileObject.playerController != playerController) 
            {
                Debug.Log("Did damage");
                tileObject.takeDamage(damage); 
            }

        }
    }
}
