using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public static CommandManager instance;
    [SerializeField]
    private Stack<ICommand> mainCommandRegistryStack;

    private int registryTickCounter;
    private void Awake()
    {
        if (instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        mainCommandRegistryStack = new Stack<ICommand>();
        registryTickCounter = 0;
    }
    private void Update()
    {
        if (Input.GetButtonUp("Undo"))
        {
            UndoLastCommand();
        }
    }
    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        mainCommandRegistryStack.Push(command);
        if (command.GetTickExecutedValue() > registryTickCounter)
        {
            registryTickCounter++;
        }
    }
    public void UndoLastCommand()
    {
        if (mainCommandRegistryStack.Count == 0) return;
        if (registryTickCounter < 0) return;
        int currentTickValue = GetCurrenteTickExecuteValue();
        while (mainCommandRegistryStack.Count>0 && mainCommandRegistryStack.Peek().GetTickExecutedValue() == currentTickValue)
        {
            ICommand cmd = mainCommandRegistryStack.Pop();
            cmd.Undo();
        }
        registryTickCounter--;
    }
    public int GetCurrenteTickExecuteValue()
    {
        return registryTickCounter;
    }
}
public interface ICommand
{
    int GetTickExecutedValue();
    void Execute();
    void Undo();
}
/*
public class ExecuteSyncCommand : ICommand
{
    private readonly List<ICommand> syncCommandsList;
    private int tickExecuteValue;
    public ExecuteSyncCommand(List<ICommand> syncCommands, int tickExecute = 0)
    {
        syncCommandsList = syncCommands;
        tickExecuteValue = tickExecute;
    }
    public void Execute()
    {
        if (syncCommandsList.Count == 0) return;
        foreach (ICommand command in syncCommandsList)
        {
            command.Execute();
        }
    }

    public void Undo()
    {
        if (syncCommandsList.Count == 0) return;
        foreach (ICommand command in syncCommandsList)
        {
            command.Undo();
        }
    }
    public int GetTickExecutedValue()
    {
        return tickExecuteValue;
    }
}*/
