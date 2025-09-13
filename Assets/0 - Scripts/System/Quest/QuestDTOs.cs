// QuestDTOs.cs
using System;
using System.Collections.Generic;

[Serializable]
public class IntPair
{
    public int key;
    public int value;
}

[Serializable]
public class QuestInstanceDTO
{
    public string questId;
    public bool isAccepted;
    public bool isCompleted;
    public int currentStepIndex;
    public List<IntPair> stepProgressList; // remplace Dictionary pour JsonUtility
}
