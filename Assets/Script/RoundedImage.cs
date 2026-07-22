using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RoundedImage : MaskableGraphic
{
    [SerializeField] private float radius = 20f;
    [SerializeField] private int segments = 8; // 角の滑らかさ

    public float Radius
    {
        get => radius;
        set { radius = value; SetVerticesDirty(); }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float width = rect.width;
        float height = rect.height;

        // 半径が大きすぎる場合の制限
        float maxRadius = Mathf.Min(width, height) * 0.5f;
        float r = Mathf.Clamp(radius, 0f, maxRadius);

        if (r <= 0)
        {
            // 通常の四角形を描画
            AddQuad(vh, rect.min, rect.max, color);
            return;
        }

        // 四隅の中心座標
        Vector2[] centers = new Vector2[]
        {
            new Vector2(rect.xMax - r, rect.yMax - r), // 右上
            new Vector2(rect.xMin + r, rect.yMax - r), // 左上
            new Vector2(rect.xMin + r, rect.yMin + r), // 左下
            new Vector2(rect.xMax - r, rect.yMin + r)  // 右下
        };

        // メッシュ生成（中心点＋扇形）
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        // 中心頂点
        int centerIndex = vh.currentVertCount;
        vert.position = rect.center;
        vh.AddVert(vert);

        // 外周の頂点生成
        for (int i = 0; i < 4; i++)
        {
            float startAngle = i * 90f;
            for (int j = 0; j <= segments; j++)
            {
                float angle = (startAngle + (90f / segments) * j) * Mathf.Deg2Rad;
                Vector2 pos = centers[i] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;

                vert.position = pos;
                vh.AddVert(vert);
            }
        }

        // インデックス（三角形）の定義
        int vertCount = vh.currentVertCount;
        for (int i = 1; i < vertCount - 1; i++)
        {
            vh.AddTriangle(centerIndex, i, i + 1);
        }
        vh.AddTriangle(centerIndex, vertCount - 1, 1);
    }

    private void AddQuad(VertexHelper vh, Vector2 min, Vector2 max, Color32 color)
    {
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        int startIndex = vh.currentVertCount;

        vert.position = new Vector3(min.x, min.y); vh.AddVert(vert);
        vert.position = new Vector3(min.x, max.y); vh.AddVert(vert);
        vert.position = new Vector3(max.x, max.y); vh.AddVert(vert);
        vert.position = new Vector3(max.x, min.y); vh.AddVert(vert);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }
}