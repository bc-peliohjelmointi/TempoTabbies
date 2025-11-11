using UnityEngine;
using static CardDataScript;
using static PlayerStatsScript;
using static GameManager;

public class ComboLeechEffect : MonoBehaviour
{
    /*public RhythmGameManager Game;

    /// <summary>
    /// K‰ynnist‰‰ efektin (esim. kun kortti valitaan).
    /// </summary>


    [Header("Effect Settings")]
    public string ownerPlayer = "PlayerA";
    public string targetPlayer = "PlayerB";
    public float duration = 5f;      // efektin kesto sekunteina
    public float interval = 1f;      // kuinka usein leechaus tapahtuu
    public int comboDrainAmount = 5; // montako comboa siirret‰‰n / sekunti

    private bool isActive;

    public void Activate()
    {
        if (!isActive && Game != null)
            StartCoroutine(LeechRoutine());
        else
            Debug.LogWarning("ComboLeechScript: GameManager ei ole asetettu!");
    }

    private IEnumerator LeechRoutine()
    {
        isActive = true;
        float elapsed = 0f;

        // PlayerStats owner = RhythmGameManager.Instance.GetPlayer("PlayerA");
        // PlayerStats target = RhythmGameManager.Instance.GetPlayer("PlayerB");
        var owner = Game.GetPlayer(ownerPlayer);
        var target = Game.GetPlayer(targetPlayer);

        if (owner == null || target == null)
        {
            Debug.LogError("ComboLeechEffect: Player reference missing!");
            yield break;
        }

        Debug.Log($"[ComboLeech] {owner.PlayerName} started draining {target.PlayerName}");

        while (elapsed < duration)
        {
            // siirr‰ comboa
            int drained = Mathf.Min(comboDrainAmount, target.Combo);
            target.AddCombo(-drained);
            player.AddCombo(drained);

            Debug.Log($"[ComboLeech] {player.PlayerName} leeched {drained} combo from {target.PlayerName}");

            RhythmGameManager.Instance.ShowComboStatus();

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        Debug.Log($"[ComboLeech] Effect ended for {player.PlayerName}");
        isActive = false;
    }*/
}
