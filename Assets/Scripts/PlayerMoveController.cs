using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerPowerState
{
    United,
    Separated,
}
[RequireComponent(typeof(MoveScript))]
public class PlayerMoveController : MonoBehaviour
{
    public MoveScript moveMirrorCopyRed;
    public MoveScript moveMirrorCopyGreen;
    public Sprite ControlRedSprite;
    public Sprite ControlGreenSprite;
    public GameObject controlRefGO;
    public MoveScript moveSelf;
    public Vector3 mirrorCopyPositionStart;
    public bool isMirrorCopyActive = false;
    private float moveRefreshRate = 0.3f;
    private float moveRefreshTimer = 0;
    private bool isCopyRedMainControl = false;
    private SpriteRenderer controlRefSpriteRenderer;

    private void Start()
    {
        moveSelf = GetComponent<MoveScript>();
        moveMirrorCopyRed.gameObject.SetActive(false);
        moveMirrorCopyGreen.gameObject.SetActive(false);
        controlRefGO.SetActive(false);
        controlRefSpriteRenderer = controlRefGO.GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        moveRefreshTimer += 1 * Time.deltaTime;
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.magnitude > 0)
        {
            if (moveRefreshTimer >= moveRefreshRate)
            {
                if (Mathf.Abs(moveInput.x) + Mathf.Abs(moveInput.y) == 2)
                {
                    moveInput = new Vector2(moveInput.x, 0);
                }
                int tickExecuteValue = CommandManager.instance.GetCurrenteTickExecuteValue() + 1;
                if (isMirrorCopyActive)
                {
                    if (isCopyRedMainControl)
                    {
                        moveMirrorCopyRed.MoveToDir(moveInput, tickExecuteValue);
                        moveMirrorCopyGreen.MoveToDir(moveInput * -1, tickExecuteValue);
                        controlRefGO.transform.position = moveMirrorCopyRed.transform.position;
                    }
                    else
                    {
                        moveMirrorCopyRed.MoveToDir(moveInput * -1, tickExecuteValue);
                        moveMirrorCopyGreen.MoveToDir(moveInput, tickExecuteValue);
                        controlRefGO.transform.position = moveMirrorCopyGreen.transform.position;
                    }
                    //moveSelf.MoveToDir(moveInput, tickExecuteValue);
                }
                else
                {
                    moveSelf.MoveToDir(moveInput, tickExecuteValue);
                }
                moveRefreshTimer = 0;
            }
        }
        bool powerInput = Input.GetButtonUp("Jump");
        if (powerInput)
        {
            int tickExecuteValue = CommandManager.instance.GetCurrenteTickExecuteValue() + 1;
            if (isMirrorCopyActive)
            {
                CommandManager.instance.ExecuteCommand(
                    new SetupPlayerCopyCommand(this, false, moveMirrorCopyRed.transform.position, moveMirrorCopyGreen.transform.position, isCopyRedMainControl, tickExecuteValue)
                    );
                isCopyRedMainControl = !isCopyRedMainControl;
            }
            else
            {
                CommandManager.instance.ExecuteCommand(
                    new SetupPlayerCopyCommand(this, true, transform.position, transform.position, isCopyRedMainControl, tickExecuteValue)
                    );
            }
        }
    }
    public void MirrorCopyEnable(Vector2 copyRedPosition, Vector2 copyGreenPosition, bool isRedCopyOnControl)
    {
        moveMirrorCopyRed.transform.position = copyRedPosition;
        moveMirrorCopyGreen.transform.position = copyGreenPosition;
        moveSelf.SetupSpiteAndColliderActive(false);
        SetMirrorCopyActive(true);
        controlRefGO.SetActive(true);
        if (isRedCopyOnControl)
        {
            controlRefSpriteRenderer.sprite = ControlRedSprite;
            controlRefGO.transform.parent = moveMirrorCopyRed.transform;
        }
        else
        {
            controlRefSpriteRenderer.sprite = ControlGreenSprite;
            controlRefGO.transform.parent = moveMirrorCopyGreen.transform;
        }
        controlRefGO.transform.localPosition = Vector2.zero;
        isCopyRedMainControl = isRedCopyOnControl;
    }
    public void MirrorCopyDisable(bool isRedCopyOnControl)
    {
        SetMirrorCopyActive(false);
        controlRefGO.transform.parent = null;
        controlRefGO.SetActive(false);
        moveSelf.SetupSpiteAndColliderActive(true);
        if (isRedCopyOnControl)
        {
            moveSelf.transform.position = moveMirrorCopyGreen.transform.position;
        }
        else
        {
            moveSelf.transform.position = moveMirrorCopyRed.transform.position;
        }
    }
    private void SetMirrorCopyActive(bool isActive)
    {
        moveMirrorCopyRed.gameObject.SetActive(isActive);
        moveMirrorCopyGreen.gameObject.SetActive(isActive);
        isMirrorCopyActive = isActive;
       // if (isMirrorCopyActive) moveMirrorCopy.transform.position = transform.position;
    }
}
/*public class PlayerCopySyncMoveCommand : ICommand
{
    private readonly MoveScript playerMoveRef;
    private readonly MoveScript copyMoveRef;
    private readonly Vector2 direction;
    public PlayerCopySyncMoveCommand(MoveScript player, MoveScript copy, Vector2 dir)
    {
        playerMoveRef = player;
        copyMoveRef = copy;
        direction = dir;
    }
    public void Execute()
    {
        playerMoveRef.MoveToDirNoCommand(direction);
        copyMoveRef.MoveToDir(direction * -1);
    }

    public void Undo()
    {
        playerMoveRef.MoveToDirNoCommand(direction * -1);
        copyMoveRef.MoveToDirNoCommand(direction);
    }
}*/
public class SetupPlayerCopyCommand : ICommand
{
    private readonly PlayerMoveController copyMoveRef;
    private readonly bool isActive;
    private Vector2 actualCopyRedPosition;
    private Vector2 actualCopyGreenPosition;
    private int tickExecuteValue;
    private bool isRedCopyOnControl;
    public SetupPlayerCopyCommand(PlayerMoveController playerMoveRef, bool active, 
        Vector2 copyRedPosition, Vector2 copyGreenPosition, bool isRedCopyControl,int tickExecute = 0)
    {
        copyMoveRef = playerMoveRef;
        isActive = active;
        actualCopyRedPosition = copyRedPosition;
        actualCopyGreenPosition = copyGreenPosition;
        isRedCopyOnControl = isRedCopyControl;
        tickExecuteValue = tickExecute;
    }
    public void Execute()
    {
        if (isActive)
        {
            copyMoveRef.MirrorCopyEnable(actualCopyRedPosition, actualCopyGreenPosition, isRedCopyOnControl);
        }
        else
        {
            copyMoveRef.MirrorCopyDisable(isRedCopyOnControl);
        }
    }

    public int GetTickExecutedValue()
    {
        return tickExecuteValue;
    }

    public void Undo()
    {
        if (isActive)
        {
            copyMoveRef.MirrorCopyDisable(isRedCopyOnControl);
        }
        else
        {
            copyMoveRef.MirrorCopyEnable(actualCopyRedPosition, actualCopyGreenPosition, isRedCopyOnControl);
        }
    }
}
