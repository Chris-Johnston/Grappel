using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SegmentDifficulty : int
{
    EASY = 0, MEDIUM = 1, HARD = 2
}

/// <summary>
/// Represents a single segment prefab to be used by the dynamic swing up scene
/// </summary>
public class DynamicSwingUpSegment : MonoBehaviour
{
    /// <summary>
    /// Which category of difficulty is this segment?
    /// </summary>
    public SegmentDifficulty Difficulty = SegmentDifficulty.EASY;

    /// <summary>
    /// How much should the camera offset for this center
    /// for example, if this section is sloping to the right, then the center of the segment should be 
    /// towards the right
    /// </summary>
    public Vector3 SegmentCenter = new Vector3(0, 0, 0);

    /// <summary>
    /// How should the speed of the water be modified during this segement so that it stays fair?
    /// If this were a segemnt that was slanted towards the right, or very tricky, then the water should
    /// probably slow down.
    /// </summary>
    [Range(0, 3)]
    public float SegmentWaterMultiplier = 1.0f;
}
