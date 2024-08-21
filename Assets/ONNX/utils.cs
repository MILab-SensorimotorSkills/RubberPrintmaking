using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static readonly Dictionary<int, string> myDictionary = new Dictionary<int, string>
    {
        { 0, "No force" },
        { 1, "Applying Force" },
        { 2, "Deforming" },
        { 3, "Releasing Force" }

    };

}
