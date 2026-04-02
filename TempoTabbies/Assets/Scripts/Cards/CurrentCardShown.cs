using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentCardShown : MonoBehaviour
{
    public Image[] player1Slots;
    public Image[] player2Slots;

    private _GameManager gm;
    private PlayerScript p1;
    private PlayerScript p2;

    // Track last known counts so we only refresh when needed
    private int lastP1Count = -1;
    private int lastP2Count = -1;

    void Start()
    {
        gm = _GameManager.instance ?? FindFirstObjectByType<_GameManager>();
        TryGetPlayers();
        // start hidden if empty
        ApplySlotsVisibility(player1Slots, false);
        ApplySlotsVisibility(player2Slots, false);
    }
    void Update()
    {
        if (gm == null)
        {
            gm = _GameManager.instance ?? FindFirstObjectByType<_GameManager>();
            if (gm == null) return;
        }

        TryGetPlayers();

        if (p1 != null && p1.AllCards.Count != lastP1Count)
        {
            UpdatePlayerSlots(player1Slots, p1);
            lastP1Count = p1.AllCards.Count;
        }

        // Only show player 2 when multiplayer is enabled
        if (gm.multiplayer)
        {
            if (p2 != null && p2.AllCards.Count != lastP2Count)
            {
                UpdatePlayerSlots(player2Slots, p2);
                lastP2Count = p2.AllCards.Count;
            }
        }
        else
        {
            // make sure player2 UI is hidden in singleplayer
            ApplySlotsVisibility(player2Slots, false);
            lastP2Count = -1;
        }
    }
    private void TryGetPlayers()
    {
        if (gm == null) return;
        if (p1 == null) p1 = gm.p1;
        if (p2 == null && gm.multiplayer) p2 = gm.p2;
    }

    private void UpdatePlayerSlots(Image[] slots, PlayerScript player)
    {
        if (slots == null || slots.Length == 0)
            return;

        if (player == null || player.AllCards == null || player.AllCards.Count == 0)
        {
            // No cards -> hide all slots
            ApplySlotsVisibility(slots, false);
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            if (i < player.AllCards.Count && player.AllCards[i] != null)
            {
                slot.overrideSprite = player.AllCards[i].icon;
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot.overrideSprite = null;
                slot.gameObject.SetActive(false);
            }
        }
    }

    private void ApplySlotsVisibility(Image[] slots, bool visible)
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].gameObject.SetActive(visible);
        }
    }

    // Force refresh from other scripts (e.g. after assigning cards)
    public void ForceRefresh()
    {
        lastP1Count = -1;
        lastP2Count = -1;
        TryGetPlayers();
        if (p1 != null) UpdatePlayerSlots(player1Slots, p1);
        if (gm != null && gm.multiplayer && p2 != null) UpdatePlayerSlots(player2Slots, p2);
    }
}
