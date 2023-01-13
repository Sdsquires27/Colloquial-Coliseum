using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Action : ScriptableObject
{
    [System.NonSerialized]
    public int timeRecharging;

    public int rechargeTime;
    public int range;
    public int damage;

    public abstract string type { get; }

    public abstract int size { get; }


    // does the action attack enemies or benefit allies?
    public abstract bool targetsEnemy { get; }
    public abstract bool isAoe { get; }


    // what tiles does the attack affect?
    public abstract void use(TileObject tileToAffect);

    public abstract void use(Vector3Int startTile, PlayerController curPlayer);


    public string filePath()
    {
        return "Sprites/Spells/Icons/" + type + "/" + name;
    }





    public class Attack : Action
    {
        int piercing;
        public Attack()
        {
            this.name = "Attack";
        }
        public Attack(int range, int piercing, int damage)
        {
            this.name = "Attack";
            this.range = range;
            this.piercing = piercing;
            this.damage = damage;
        }

        public Attack(Unit unit)
        {
            this.name = "Attack";
            this.range = unit.range;
            this.piercing = unit.piercing;
            this.damage = unit.dmg;
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
                return 0;
            }
        }
        public override bool isAoe
        {
            get
            {
                return false;
            }
        }


        public override string type
        {
            get
            {
                return "Attack";
            }
        }

        public override void use(TileObject toAttack)
        {
            int rand = Random.Range(0, 10);
            rand += piercing;
            if (toAttack.unit.armor <= rand)
            {
                toAttack.takeDamage(damage);
            }
        }
        public override void use(Vector3Int tileToAttack, PlayerController curPlayer)
        {

        }
    }
}
   
