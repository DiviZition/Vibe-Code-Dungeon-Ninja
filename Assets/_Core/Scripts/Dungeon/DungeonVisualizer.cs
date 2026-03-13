using UnityEngine;
using UnityEditor;

namespace Dungeon
{
    [CustomEditor(typeof(DungeonGenerator))]
    public class DungeonVisualizer : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = (DungeonGenerator)target;

            if (GUILayout.Button("Generate"))
            {
                generator.Generate();
            }
        }

        private void OnSceneGUI()
        {
            var generator = (DungeonGenerator)target;
            var data = generator.Data;

            if (data == null || data.Rooms == null)
                return;

            // Draw corridors first (under rooms)
            if (data.Corridors != null)
            {
                foreach (var corridor in data.Corridors)
                {
                    DrawCorridor(corridor);
                }
            }

            // Draw rooms
            foreach (var room in data.Rooms)
            {
                DrawRoom(room);
            }
        }

        private void DrawRoom(Room room)
        {
            Vector3 position = new Vector3(room.GridX, room.GridY, 0);
            Vector3 size = new Vector3(room.Width, room.Height, 0);

            // Get color based on room type
            Color color = GetRoomColor(room.Type);

            // Draw filled rectangle
            Handles.color = color.WithAlpha(0.3f);
            Handles.DrawAAConvexPolygon(
                position,
                position + new Vector3(size.x, 0, 0),
                position + new Vector3(size.x, size.y, 0),
                position + new Vector3(0, size.y, 0)
            );

            // Draw outline
            Handles.color = color;
            Handles.DrawWireCube(
                position + size * 0.5f,
                size
            );

            // Draw room info label
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;

            Handles.Label(
                position + new Vector3(1, size.y - 1, 0),
                $"{room.Type}\n({room.ZoneX},{room.ZoneY})",
                style
            );
        }

        private void DrawCorridor(Corridor corridor)
        {
            Color corridorColor = Color.cyan;

            Vector3 position = new Vector3(corridor.PointFrom.x, corridor.PointTo.y, 0);

            // Draw filled rectangle
            Handles.color = corridorColor.WithAlpha(0.5f);
            Handles.DrawAAConvexPolygon(new Vector3[] { corridor.PointFrom, corridor.PointTo });
        }

        private Color GetRoomColor(RoomType type)
        {
            return type switch
            {
                RoomType.Boss => Color.red,
                RoomType.Normal => Color.white,
                _ => Color.gray
            };
        }
    }

    // Extension method for color alpha
    public static class ColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
