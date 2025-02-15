using Invector.vCharacterController.AI.FSMBehaviour;
using UnityEngine;
using UnityEngine.AI;

public class CheckValidDestination : vStateDecision
{
    public override string categoryName
    {
        get { return "Movement/"; }
    }
    public override string defaultName
    {
        get { return "Check Valid Destination"; }
    }

    [SerializeField]
    private string[] _areaNames;

    public override bool Decide(vIFSMBehaviourController fsmBehaviour)
    {
        var aiController = fsmBehaviour.aiController;
        
        if (!aiController.isMoving || aiController.isInDestination)
            return true;

        int areaMask = NavMesh.AllAreas;
        foreach (var area in _areaNames)
        {
            areaMask |= 1 << NavMesh.GetAreaFromName(area);
        }

        var path = new NavMeshPath();
        bool validPath = NavMesh.CalculatePath(fsmBehaviour.gameObject.transform.position, aiController.targetDestination, areaMask, path);
        return validPath;
    }
}
