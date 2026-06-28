using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.UI.Minimap.Markers
{
[Serializable]
public class MarkerData
{
    public string markerName;
    public Sprite markerIcon;
}

[CreateAssetMenu(fileName = "MinimapMarkerList", menuName = "SO/Minimap/MiniMapMarkerList", order = 0)]
public class MarkerIconListSO : ScriptableObject
{
    public List<MarkerData> markerList = new List<MarkerData>();
}

}