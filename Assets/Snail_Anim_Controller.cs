using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snail_Anim_Controller : MonoBehaviour
{
    public enum AnimState
    {
        Idle,
        Fly,
        Move
    }
    AnimState currentState;

    public Snail_Go_Anim_2 idle, fly, move, active, next;

    public bool animate;
    // Start is called before the first frame update

    public void IdleOrMoving(bool isMoving)
    {
        PrepareNextAnimState(isMoving ? AnimState.Move: AnimState.Idle);
    }

    public void PrepareNextAnimState(AnimState state)
    {
        if (state == currentState)
        {
            return;
        }

        if (active != null)
        {
            active.PrepareToStop();
        }

        switch (state)
        {
            case AnimState.Idle:
                next = idle;
                break;
            case AnimState.Fly:
                next = fly;
                break;
            case AnimState.Move:
                next = move;
                break;
        }

        if(active == null)
        {
            OnTransition();
        }
    }

    void AllStatesOff()
    {
        fly.gameObject.SetActive(false);
        idle.gameObject.SetActive(false);
        move.gameObject.SetActive(false);
    }

    public void OnTransition()
    {
        AllStatesOff();
        next.gameObject.SetActive(true);

        if(next == idle)
        {
            currentState = AnimState.Idle;
        }
        else if(next == fly)
        {
            currentState = AnimState.Fly;
        }
        else
        {
            currentState = AnimState.Move;
        }

        active = next;
        next = null;
    }
}
