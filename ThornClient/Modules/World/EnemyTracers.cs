using UnityEngine;
using ThornClient.Core;
using ThornClient.Core.DataTypes;

namespace ThornClient.Modules.World;

public class EnemyTracers : Module {
    public Setting<bool> FromCrosshair;
    public Setting<Color> TracerColor;
    public Setting<float> LineThickness;
    public Setting<int> EnemyCountThreshold;

    public override string IconName => "point_dot";

    public EnemyTracers() : base("thorn.enemyTracers", "Enemy Tracers", "Draws lines from you to enemies",
        ModuleCategory.World) {
        TracerColor = RegisterSetting("tracerColor", "Tracer Color", "Color used for the trace lines",
            new Color(0.65f, 0.95f, 0.89f, 0.5f));
        FromCrosshair = RegisterSetting("fromCrosshair", "From Crosshair", "Draw tracers from the center of the screen instead of the bottom", true);
        LineThickness = RegisterSetting("lineThickness", "Line Thickness", "The pixel width of the tracer lines", 2f);
        EnemyCountThreshold = RegisterSetting("enemyCountThreshold", "Enemy Count Threshold",
            "Display tracers when there are this many enemies left", 5);
    }

    public override void OnRender() {
        var tracker = EnemyTracker.Instance;
        if (tracker == null) return;

        var mainCam = Camera.main;
        if (mainCam == null) return;

        GL.Begin(GL.QUADS);
        GL.Color(TracerColor.Value);

        Vector3 screenOrigin = FromCrosshair.Value
            ? new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
            : new Vector3(Screen.width / 2f, 0f, 0f);

        float halfThickness = LineThickness.Value / 2f;

        int targetCount = tracker.enemies.Count;

        for (int i = 0; i < targetCount; i++) {
            var enemy = tracker.enemies[i];

            // Skip drawing if not meaningful
            if (enemy == null || enemy.dead || enemy.gameObject == null || !enemy.gameObject.activeInHierarchy) {
                continue;
            }

            Vector3 screenPos = mainCam.WorldToScreenPoint(enemy.transform.position);
            Vector3 targetPos = new Vector3(screenPos.x, screenPos.y, 0f);

            if (screenPos.z > 0 || true) {
                // GL.Vertex(screenOrigin);
                // GL.Vertex(new Vector3(screenPos.x, screenPos.y, 0f));

                Vector3 direction = (targetPos - screenOrigin).normalized;
                Vector3 normal = new Vector3(-direction.y, direction.x, 0f) * halfThickness;

                GL.Vertex(screenOrigin - normal);
                GL.Vertex(screenOrigin + normal);
                GL.Vertex(targetPos + normal);
                GL.Vertex(targetPos - normal);
            }
        }

        GL.End();
    }
}
