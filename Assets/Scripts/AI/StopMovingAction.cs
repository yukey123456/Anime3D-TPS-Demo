using Invector.vCharacterController.AI.FSMBehaviour;

public class StopMovingAction : vStateAction
{
    public override string categoryName
    {
        get { return "Movement/"; }
    }
    public override string defaultName
    {
        get { return "Stop"; }
    }

    public override void DoAction(vIFSMBehaviourController fsmBehaviour, vFSMComponentExecutionType executionType = vFSMComponentExecutionType.OnStateUpdate)
    {
        fsmBehaviour.aiController.Stop();
    }
}
