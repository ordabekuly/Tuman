using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ConfigureShooting
{
    [MenuItem("Tuman/Configure Shooting")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Courtyard.unity") return;
        var player = GameObject.Find("Player");
        var enemy = GameObject.Find("Enemy");
        var safe = GameObject.Find("Mobile Canvas/Safe Area");
        if (!player || !enemy || !safe) { Debug.LogError("Shooting requires Player, Enemy and Mobile Canvas/Safe Area."); return; }
        var shooter = player.GetComponent<PlayerShooting>();
        if (!shooter) shooter = player.AddComponent<PlayerShooting>();
        if (!enemy.GetComponent<EnemyHealth>()) enemy.AddComponent<EnemyHealth>();
        var existing = safe.transform.Find("Fire Button");
        if (!existing)
        {
            var obj = new GameObject("Fire Button", typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(safe.transform, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-220, 210); rect.sizeDelta = new Vector2(280, 200);
            var image = obj.GetComponent<Image>(); image.color = new Color(.5f, .22f, .1f, .9f);
            var button = obj.GetComponent<Button>(); button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            UnityEventTools.AddPersistentListener(button.onClick, shooter.Fire);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(rect, false);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>(); text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Arial.ttf");
            text.fontSize = 48; text.text = "FIRE"; text.color = new Color(1, .95f, .8f);
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            // Keep the death overlay above the fire button.
            var death = safe.transform.Find("Death Panel"); if (death) death.SetAsLastSibling();
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Shooting configured: FIRE / Space, nearest-enemy aim, 3-hit enemy health.");
    }
}
