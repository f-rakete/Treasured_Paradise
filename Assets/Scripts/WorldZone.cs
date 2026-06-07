
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldZone
{
    public string zoneID;
    public string zoneName;
    public bool isUnlocked;
    public List<string> prerequisiteAritfactsID = new List<string>();
    public int requiredGearType;    //Minimum gear requirements
    public ZoneTone tone;
}

public enum ZoneTone { Warm, Mysterious, Ancient }

//Lost city region - extends WorldZone
[System.Serializable]
public class LogicCityZone : WorldZone
{
    public string civilizationName;     // "Lemuria", "Atlantis", "Tartaria"
    public bool isFullyRevealed;
    public List<string> loreEntryIDs = new List<string>();
}

// Map Marker -- player-placed or story triggered
[System.Serializable]
public class MapMarker
{
    public string markerID;
    public Vector3 worldPosition;
    public string label;
    public MarkerType type;
    public bool isStoryMarker;
}

public enum MarkerType { Job, Artifact, POI, LostCity, Personal }