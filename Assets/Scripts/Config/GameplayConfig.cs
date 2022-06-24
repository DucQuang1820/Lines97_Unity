using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configurations/Gameplay", fileName = "New Gameplay Config")]
public class GameplayConfig : ScriptableObject
{
    [SerializeField] private uint queuedCount = 3;
    [SerializeField] private uint scorePerExploded = 1;
    [SerializeField] private uint explodeCount = 5;

    [SerializeField] private uint initGrowUpCount = 3;
    [SerializeField] private uint maxScore = 99999;

    public uint QueuedCount => queuedCount;
    public uint ScorePerExploded => scorePerExploded;
    public uint ExplodeCount => explodeCount;
    public uint InitGrowUpCount => initGrowUpCount;
    public uint MaxScore => maxScore;
}
