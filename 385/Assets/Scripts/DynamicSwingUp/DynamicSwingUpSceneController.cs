using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicSwingUpSceneController : MonoBehaviour
{
    /// <summary>
    /// Collection of all of the map sections that are easy
    /// </summary>
    public List<GameObject> EasySections = new List<GameObject>();

    /// <summary>
    /// Collection of all the map sections that are considered "medium" difficulty
    /// </summary>
    public List<GameObject> MediumSections = new List<GameObject>();

    /// <summary>
    /// Collection of all of the map sections that are considered "hard" in difficutly
    /// </summary>
    public List<GameObject> HardSections = new List<GameObject>();

    /// <summary>
    /// How far away should each of the origin sections be placed
    /// </summary>
    public Vector3 SectionSpacing = new Vector3(0, 10f, 0);

    /// <summary>
    /// Reference to the player game object reference
    /// </summary>
    public GameObject PlayerReference;
}
