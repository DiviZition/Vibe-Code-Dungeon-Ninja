using UnityEngine;

public static class RectTransformExtensions
{
    /// <summary>
    /// Устанавливает родителя и вписывает RectTransform в него полностью (растягивает по anchors).
    /// </summary>
    public static void FitToParent(this RectTransform rectTransform, RectTransform newParent)
    {
        rectTransform.SetParent(newParent, false); // не сохраняем мировые координаты

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
    }

    /// <summary>
    /// Перемещает RectTransform к новому родителю, сохраняя визуальное положение, поворот и масштаб.
    /// </summary>
    public static void ReparentPreservingWorld(this RectTransform rectTransform, RectTransform newParent)
    {
        // Сохраняем мировые координаты
        Vector3 worldPos = rectTransform.position;
        Quaternion worldRot = rectTransform.rotation;
        Vector3 worldScale = rectTransform.lossyScale; // мировой scale

        // Меняем родителя БЕЗ сохранения мировых координат
        rectTransform.SetParent(newParent, false);

        // Восстанавливаем мировые координаты
        rectTransform.position = worldPos;
        rectTransform.rotation = worldRot;

        // Компенсируем scale родителя
        if (newParent != null)
        {
            Vector3 parentScale = newParent.lossyScale;
            rectTransform.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z
            );
        }
        else
        {
            rectTransform.localScale = worldScale;
        }
    }
}
