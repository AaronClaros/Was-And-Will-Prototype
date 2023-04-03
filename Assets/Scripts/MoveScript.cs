using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MoveColor
{
    White,
    Red,
    Green,
    Blue,
    Multicolor
}
public class MoveScript : MonoBehaviour
{
    public MoveColor defaultMoveColor;
    [SerializeField]
    private MoveColor actualMoveColor;
    [SerializeField]
    private LayerMask blockMovementLayer;
    [SerializeField]
    private LayerMask pushMovementLayer;
    [SerializeField]
    private float movePaceDistance = 1;
    [SerializeField]
    private Vector2 actualExpectedPosition;
    private SpriteRenderer spriteRef;
    private BoxCollider2D colliderRef;
    public UnityEvent moveEventStarted;
    public UnityEvent moveEventCompleted;
    public UnityEvent moveEventCanceled;
    private void Start()
    {
        spriteRef = GetComponent<SpriteRenderer>();
        colliderRef = GetComponent<BoxCollider2D>();
        actualExpectedPosition = transform.position;
    }
    public void MoveToDir(Vector2 direction, int tickCommand)
    {
        moveEventStarted.Invoke();
        if (!CheckNextPositionIsFree(direction))
        {
            RaycastHit2D hitPush = Physics2D.Raycast((Vector2)transform.position + (direction), direction, movePaceDistance / 4, pushMovementLayer);
            if (hitPush.collider != null)
            {
                MoveScript pushable = hitPush.collider.GetComponent<MoveScript>();
                if (pushable != null)
                {
                    //Debug.Log("trying to push: " + hitPush.collider.gameObject.name, gameObject);
                    if (pushable.CheckNextPositionIsFree(direction))
                    {
                        ICommand selfMove = new MoveCommand(this, direction, transform.position, tickCommand);
                        ICommand syncMove = new MoveCommand(pushable, direction, pushable.transform.position, tickCommand);
                        CommandManager.instance.ExecuteCommand(syncMove);
                        CommandManager.instance.ExecuteCommand(selfMove);
                    }
                    else
                    {
                        moveEventCanceled.Invoke();
                    }
                }
                else
                {
                    moveEventCanceled.Invoke();
                }
            }
        }
        else
        {
            CommandManager.instance.ExecuteCommand(new MoveCommand(this, direction, transform.position, tickCommand));
        }
        //ParadoxWallInstancer.instance.CheckParadoxExistInPosition(actualExpectedPosition, tickCommand);
    }
    public void SetupMoveData(MoveColorData moveData)
    {
        actualMoveColor = moveData.moveColor;
        blockMovementLayer = moveData.blockMovementLayer;
        pushMovementLayer = moveData.pushMovementLayer;
    }
    public void ExecuteSelfMove(Vector2 direction)
    {
        Vector3 newPos = transform.position + (Vector3)(direction.normalized * movePaceDistance);
        actualExpectedPosition = newPos;
        transform.position = newPos;
        moveEventCompleted.Invoke();
    }
    public void ForceNewPosition(Vector2 newPos)
    {
        transform.position = newPos;
        actualExpectedPosition = newPos;
    }
    public bool CheckNextPositionIsFree(Vector2 dir)
    {
        Collider2D blockCollider = Physics2D.OverlapBox((Vector2)transform.position + (dir * movePaceDistance), Vector2.one/2, 0f, blockMovementLayer);
        if (blockCollider == null)
        {
            //Debug.Log("target dir is free for : " + gameObject.name, gameObject);
            return true;
        } else
        {
            //Debug.Log("target dir is ocuppied for : " + gameObject.name, gameObject);
            return false;
        }
    }
    public void SetupSpiteAndColliderActive(bool isActive)
    {
        spriteRef.enabled = isActive;
        colliderRef.enabled = isActive;
    }
    
    public bool CheckIfEntityIsMerged()
    {
        return false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null)
        {
            //collision.collider.isTrigger = true;
            //isOverlap = true;
            transform.position = actualExpectedPosition;
            ParadoxWallInstancer.instance.CheckParadoxExistInPosition(actualExpectedPosition, CommandManager.instance.GetCurrenteTickExecuteValue());
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            //if (collision.gameObject.layer
            transform.position = actualExpectedPosition;
            ParadoxWallInstancer.instance.CheckParadoxExistInPosition(actualExpectedPosition, CommandManager.instance.GetCurrenteTickExecuteValue());
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        //if (collision != null)
        //collision.collider.isTrigger = false;
        //isOverlap = false;
        //transform.position = actualExpectedPosition;
    }
}
public class MoveCommand : ICommand
{
    private MoveScript selfMove;
    private Vector2 moveDirection;
    private Vector2 moveDirectionInverse;
    private readonly int tickExecuteValue;
    private Vector2 actualPosition;
    public MoveCommand(MoveScript moveScriptRef, Vector2 direction, Vector2 actualPos, int tickExecute = 0)
    {
        selfMove = moveScriptRef;
        moveDirection = direction;
        moveDirectionInverse = direction*-1;
        actualPosition = actualPos;
        tickExecuteValue = tickExecute;
    }
    public void Execute()
    {
        selfMove.ExecuteSelfMove(moveDirection);
    }

    public int GetTickExecutedValue()
    {
        return tickExecuteValue;
    }

    public void Undo()
    {
        //selfMove.ExecuteSelfMove(moveDirection);
        selfMove.ForceNewPosition(actualPosition); 
    }
}

