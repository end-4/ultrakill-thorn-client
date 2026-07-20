using System;
using UnityEngine;
using System.Collections.Generic;
using ThornClient.Core;
using ThornClient.Core.DataTypes;

namespace ThornClient.Modules.World;

public class EnemyTracers : Module {
    public Setting<Color> TracerColor;
    public Setting<float> LineThickness;
    public Setting<int> EnemyCountThreshold;
    public Setting<EnemyList> ForceTraceEnemies;

    public override string IconName => "point_dot";

    public EnemyTracers() : base("thorn.enemyTracers", "Enemy Tracers", "Draws lines from you to enemies",
        ModuleCategory.World) {
        TracerColor = RegisterSetting("tracerColor", "Tracer Color", "Color used for the trace lines",
            new Color(0.65f, 0.95f, 0.89f, 0.5f));
        LineThickness = RegisterSetting("lineThickness", "Line Thickness", "The pixel width of the tracer lines", 2f);
        EnemyCountThreshold = RegisterSetting("enemyCountThreshold", "Enemy Count Threshold",
            "Display tracers when there are this many enemies left", 5);
        ForceTraceEnemies = RegisterSetting("forceTraceEnemies", "Force Trace Enemies",
            "Always trace these enemy types regardless of the total count/threshold. Useful for e.g. Mindflayers",
            new EnemyList());
    }

    private readonly Dictionary<int, Collider> _colliderCache = new();
    private readonly Queue<int> _cacheHistory = new();
    private const int MaxCacheSize = 1000;

    private Vector3 GetEnemyPosition(EnemyIdentifier enemy) {
        if (enemy == null || enemy.transform == null) return Vector3.zero;

        var enemyId = enemy.GetInstanceID();
        if (!_colliderCache.TryGetValue(enemyId, out var collider)) {
            collider = enemy.GetComponent<Collider>();
            if (collider != null) {
                _colliderCache[enemyId] = collider;
                _cacheHistory.Enqueue(enemyId);

                if (_cacheHistory.Count > MaxCacheSize) {
                    var oldestKey = _cacheHistory.Dequeue();
                    _colliderCache.Remove(oldestKey);
                }
            }
        }

        if (collider == null) return enemy.transform.position;

        var enemyHeight = (collider.bounds.center - enemy.transform.position).y + collider.bounds.extents.y;
        return enemy.transform.position + enemyHeight / 2 * Vector3.up;
    }

    private bool IsEnemyMeaningful(EnemyIdentifier enemy) {
        return enemy != null && !enemy.dead && enemy.gameObject != null &&
               enemy.gameObject.activeInHierarchy;
    }

    public override void OnRender() {
        var tracker = EnemyTracker.Instance;
        if (tracker == null) return;

        var mainCam = Camera.main;
        if (mainCam == null) return;

        var tracerMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        tracerMaterial.SetPass(0);

        // Render directly in 3D space
        GL.PushMatrix();
        GL.MultMatrix(mainCam.worldToCameraMatrix);
        GL.LoadProjectionMatrix(mainCam.projectionMatrix);

        GL.Begin(GL.QUADS);
        GL.Color(TracerColor.Value);

        // Scale down or it'll be too thick
        float thickness = LineThickness.Value * 0.01f;

        // Place origin slightly in front of camera, so enemies behind would be perceptible
        Vector3 cameraPos = mainCam.transform.position;
        Vector3 cameraForward = mainCam.transform.forward;
        Vector3 tracerOrigin = cameraPos + (cameraForward * 1.5f);

        int targetCount = tracker.enemies.Count;
        int remainingCount = 0;
        for (int i = 0; i < targetCount; i++) {
            var enemy = tracker.enemies[i];
            remainingCount += IsEnemyMeaningful(enemy) ? 1 : 0;
        }

        for (int i = 0; i < targetCount; i++) {
            var enemy = tracker.enemies[i];

            // Skip if not meaningful or not in non-threshold-triggered whitelist
            if (!IsEnemyMeaningful(enemy) || (remainingCount >= EnemyCountThreshold.Value && !ForceTraceEnemies.Value.Includes(enemy.enemyType))) {
                continue;
            }

            Vector3 enemyPos = GetEnemyPosition(enemy);
            Vector3 lineDirection = enemyPos - tracerOrigin;

            // Find vector perpendicular to both the line and camera's direction
            Vector3 camToOriginDir = (tracerOrigin - cameraPos).normalized;
            Vector3 perpendicular = Vector3.Cross(lineDirection, camToOriginDir).normalized * (thickness / 2f);

            // Draw it
            GL.Vertex(tracerOrigin - perpendicular);
            GL.Vertex(tracerOrigin + perpendicular);
            GL.Vertex(enemyPos + perpendicular);
            GL.Vertex(enemyPos - perpendicular);
        }

        GL.End();
        GL.PopMatrix();
    }
}
