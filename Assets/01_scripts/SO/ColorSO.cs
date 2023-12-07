using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallClass
{
    public int layerMask;
    public Color color;
}

[CreateAssetMenu(menuName =("SO/Color"))]
public class ColorSO : ScriptableObject
{
    public List<WallClass> RandomMapList;
}
