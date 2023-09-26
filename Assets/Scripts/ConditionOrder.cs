using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConditionOrder", menuName = "ScriptableObjects/ConditionOrder", order = 1)]
public class ConditionOrder : ScriptableObject
{
    [SerializeField] private List<string> conditions = new List<string>() { "condition 1"};
    [SerializeField] private List<int> conditionOrderList = new List<int>() { 0 };
    [SerializeField] private int participantsPerCondition = 10;
    [SerializeField] private int currParticipantNumber = 0;

    public string GetCurrentCondition(int ppid = -1)
    {
        if (ppid == -1) ppid = currParticipantNumber;
        if (conditionOrderList.Count == 0) return null;
        
        if (ppid >= 0 && ppid < conditionOrderList.Count)
            return conditions[conditionOrderList[ppid]];
        else
        {
            Debug.LogError("Participant condition not found in list. Using default condition.");
            return null;
        }
    }

    public void GenerateList()
    {
        var list = new List<int>();

        for (var j = 0; j < conditions.Count; j++)
        for (var i=0; i < participantsPerCondition; i++)
            list.Add(j);

        conditionOrderList = list;
    }
    
    public void RandomizeList()
    {
        conditionOrderList = ExtensionMethods.Shuffle(conditionOrderList);
    }

    public void SetParticipantNumber(int num)
    {
        currParticipantNumber = num;
    }
}