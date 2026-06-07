using System.Collections.Generic;
using NUnit.Framework;

[System.Serializable]
public class PlayerData
{
    public string PlayerName;
        
    //Inventory
    public List<Item> Inventory = new List<Item>();
    public Dictionary<string, GearItem> EquippedGear = new Dictionary<string, GearItem>();

    //Ecomony
    public float currencyBalance;
        
    //Oxygen
    public float OxygenMax;
    public float OxygenCurrent;
    public readonly OxygenTier CurrentOxygenTier = OxygenTier.Snorkel;
        
    //Health Level
    public float healthMeter;
    
    //Journal + Lore
    public HashSet<string> discoveredLoreEntries = new HashSet<string>();
    
    //Map
    public List<MapMarker> mapMarkers = new List<MapMarker>();
}
    
public enum OxygenTier { Snorkel, BasicScuba, Rebreather }