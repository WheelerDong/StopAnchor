using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelLibrary", menuName = "Game/LevelLibrary")]
public class LevelLibrary : ScriptableObject
{
    public List<LevelMono> levels = new List<LevelMono>();
}
