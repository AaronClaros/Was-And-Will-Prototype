using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParadoxWallInstancer : MonoBehaviour
{
    public static ParadoxWallInstancer instance;
    public GameObject paradoxWallPrefab;
    public LayerMask paradoxWallLayer;
    [SerializeField]
    private List<Vector2> paradoxWallPosList;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        paradoxWallPosList = new List<Vector2>();
    }
    public void InstanceWallOnPosition(Vector2 position, int tickExecuteValue)
    {
        //CommandManager.instance.ExecuteCommand(new ParadoxWallCreateCommand(position, gameObject, tickExecuteValue));
    }
    public void DeleteWallOnPosition(Vector2 position, int tickExecuteValue)
    {
        //CommandManager.instance.ExecuteCommand(new ParadoxWallCreateCommand(position, gameObject, tickExecuteValue));
    }
    public void CheckParadoxExistInPosition(Vector2 position, int tickValue)
    {
        Collider2D[] deleteWallList = Physics2D.OverlapBoxAll(position, Vector2.one * 0.8f, 0f);
        if (deleteWallList.Length > 1)
        {
            foreach (Collider2D collider in deleteWallList)
            {
                MoveScript moveScript = collider.GetComponent<MoveScript>();
                if (moveScript != null)
                {
                    if (!collider.gameObject.layer.Equals(3))//3: Player Layer
                    {
                        CommandManager.instance.ExecuteCommand(new ParadoxWallCreateCommand(position, moveScript, tickValue));
                    }
                }
            }
        }
    }
    public void CMDInstanceWallAtPos(Vector2 position)
    {
        bool positionIsDuplicated = paradoxWallPosList.Contains(position);
        if (!positionIsDuplicated)
        {
            Instantiate(paradoxWallPrefab, position, Quaternion.identity);
            paradoxWallPosList.Add(position);
        }
    }
    public void CMDDeleteWallAtPos(Vector2 position)
    {
        bool positionExist = paradoxWallPosList.Contains(position);
        if (positionExist)
        {
            Collider2D deleteWall = Physics2D.OverlapBox(position, Vector2.one * 0.8f, 0f, paradoxWallLayer);
            if (deleteWall != null)
            {
                Destroy(deleteWall.gameObject);
                paradoxWallPosList.Remove(position);
            }
        }
    }
}
public class ParadoxWallCreateCommand : ICommand
{
    private ParadoxWallInstancer wallInstancerScript;
    private Vector2 instancePosition;
    private int tickExecuteValue;
    private MoveScript paradoxStarterRef;
    public ParadoxWallCreateCommand(Vector2 instPosition, MoveScript starterMoveScript, int tickExecute = 0)
    {
        wallInstancerScript = ParadoxWallInstancer.instance;
        instancePosition = instPosition;
        tickExecuteValue = tickExecute;
        paradoxStarterRef = starterMoveScript;
    }
    
    public void Execute()
    {
        wallInstancerScript.CMDInstanceWallAtPos(instancePosition);
        paradoxStarterRef.SetupSpiteAndColliderActive(false);
    }

    public int GetTickExecutedValue()
    {
        return tickExecuteValue;
    }

    public void Undo()
    {
        wallInstancerScript.CMDDeleteWallAtPos(instancePosition);
        paradoxStarterRef.SetupSpiteAndColliderActive(true);
    }
}
