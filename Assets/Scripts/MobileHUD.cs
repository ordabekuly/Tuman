using UnityEngine;
using UnityEngine.UI;

public sealed class MobileHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Text healthLabel;
    [SerializeField] private GameObject deathPanel;
    private void Update()
    {
        if(!health)return;
        healthLabel.text=$"HP: {health.CurrentHealth} / {health.MaxHealth}";
        deathPanel.SetActive(!health.IsAlive);
    }
}
