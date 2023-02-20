using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    public Unit unit;
    public int currentRange;
    public bool canAttack;
    public PlayerController playerController;
    public Sprite sprite;
    public Action action { get { return (actionIndex != -1) ? actions[actionIndex] : null; } }
    public int attackRange;
    public bool isAoe { get { return (action == null) ? false : action.isAoe; } }

    public Action[] actions;
    public int actionIndex;


    public int aoeSize()
    {
        int size = 0;

        size = action.size;

        return size;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = unit.sprite;
        Action attack = new Action.Attack(unit);
        actions = new Action[unit.spells.Count + 1];
        for(int i = 0; i < unit.spells.Count; i++) // starts at 1 because 0 is already taken by the attack action
        {
            actions[i+1] = Resources.Load<Action>("Spells/"+unit.spells[i].name);
        }
        actions[0] = attack;
        setAction(attack);
    }

    public void startTurn()
    {
        currentRange = unit.moveSpeed;
        canAttack = true;
    }

    public void deactivate()
    {
        canAttack = false;
        currentRange = 0;
        attackRange = 0;
    }

    public void useAction(Vector3Int tileAffected, PlayerController curPlayer, TileObject objectAffected)
    {
        if (action.isAoe)
        {
            action.use(tileAffected, curPlayer);
        }
        else
        {
            action.use(objectAffected);
        }

    }

    public void setAction(Action action)
    {
        if(action != null)
        {
            attackRange = action.range;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] == action)
                {
                    actionIndex = i;
                }

            }
        }
        else
        {
            attackRange = 0;
            actionIndex = -1;
        }

    }

    public void takeDamage(int damage)
    {
        unit.hp -= damage;

        if (unit.hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
