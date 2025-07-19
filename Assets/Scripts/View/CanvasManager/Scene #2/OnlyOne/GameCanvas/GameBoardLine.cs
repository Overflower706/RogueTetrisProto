using UnityEngine;
using UnityEngine.UI;
using OVFL.ECS;
using Minomino;
using System.Collections.Generic;
using TMPro;

public class GameLine : MonoBehaviour
{
    public Context Context { get; set; }
    [SerializeField] private int lineNumber = 0;


    public void Init(Context context)
    {
        Context = context;
    }

    public void OnButtonDownBake()
    {
        // CommandRequestComponent 찾기
        var commandRequestEntity = GetCommandRequestEntity();

        var commandRequest = commandRequestEntity.GetComponent<CommandRequestComponent>();

        Debug.Log("입력: 굽기 " + lineNumber);
        commandRequest.Requests.Enqueue(new CommandRequest
        {
            Type = CommandType.Bake,
            PayLoad = lineNumber
        });
    }
    
        public void OnButtonDownTrash()
    {
        // CommandRequestComponent 찾기
        var commandRequestEntity = GetCommandRequestEntity();

        var commandRequest = commandRequestEntity.GetComponent<CommandRequestComponent>();

            Debug.Log("입력: 버리기 " + lineNumber);
            commandRequest.Requests.Enqueue(new CommandRequest
            {
                Type = CommandType.Trash,
                PayLoad = lineNumber
            });
    }
    

        /// <summary>
    /// CommandRequestComponent가 있는 엔티티를 찾기
    /// </summary>
    private Entity GetCommandRequestEntity()
    {
        var commandRequestEntities = Context.GetEntitiesWithComponent<CommandRequestComponent>();

        if (commandRequestEntities.Count == 0)
        {
            return null;
        }
        else if (commandRequestEntities.Count > 1)
        {
            Debug.LogWarning("CommandRequestComponent가 여러 엔티티에 존재합니다. 첫 번째 엔티티를 사용합니다.");
        }

        return commandRequestEntities[0];
    }

}