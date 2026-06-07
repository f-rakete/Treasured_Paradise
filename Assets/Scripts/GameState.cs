using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameState : MonoBehaviour
    {
        //Completed jobs (fast lookup)
        public HashSet<string> completedJobsIDs = new HashSet<string>();
        
        //Unlocked world regions (fast lookup)
        public HashSet<string> unlockedRegionIDs = new HashSet<string>();
        
        //Discovered lore (fast lookup - mirrors PlayerData for save convenience
        public HashSet<string> discoveredLoreIDs = new HashSet<string>();
        
        //Lore entry content
        public Dictionary<int, LoreEntry> loreEntries = new Dictionary<int, LoreEntry>();
        
        //Transactional update - inventory change fires currency + journal
        public void AddItemToInventory(PlayerData player, Item item)
        {
            player.Inventory.Add(item);
            if (item is Artifact artifact && artifact.isCivilizationTied)
            {
                UnlockLoreEntry(player, artifact.loreIndex);
            }

            OnInventoryChanged?.Invoke(player);
        }

        public void UnlockLoreEntry(PlayerData player, int loreIndex)
        {
            string id = loreIndex.ToString();
            if (discoveredLoreIDs.Add(id))
            {
                player.discoveredLoreEntries.Add(id);
                OnLoreUnlocked?.Invoke(loreIndex);
            }
        }
        
        //Events
        public System.Action<PlayerData> OnInventoryChanged;
        public System.Action<int> OnLoreUnlocked;
    }

    [System.Serializable]
    public class LoreEntry
    {
        public int loreIndex;
        public string title;
        public string bodyText;
        public string civilizationOrigin;
    }
}