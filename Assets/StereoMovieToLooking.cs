using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereoMovieToLooking : MonoBehaviour
{
    private RenderTexture renderTexture;

    public HoloPlay.Quilt quilt;

    private Camera renderCam;

    public Vector2Int tileNumber = new Vector2Int(2,1);
    public int outNum = 10;
    public Vector2Int renderTextureSize = new Vector2Int(4096, 4096);

    public Texture2D stereoImage;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // create RenderTexture
        renderTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 0);
        renderCam = this.GetComponent<Camera>();
        quilt.overrideQuilt = renderTexture;
        renderCam.targetTexture = renderTexture;
        renderCam.orthographicSize = renderTexture.height * 0.5f;

        // setup
        meshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
        meshRenderer = meshFilter.GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = stereoImage;
        UpdateTiling();
    }
    private void OnDestroy()
    {
        renderTexture.Release();
    }

    [ContextMenu("UpdateTiling")]
    void UpdateTiling()
    {
        var newTiling = new HoloPlay.Quilt.Tiling("streoView", tileNumber.x, tileNumber.y,
            renderTexture.width, renderTexture.height);
        quilt.tiling = newTiling;

        // UpdateMesh
        var mesh = CreateMesh(tileNumber.x, tileNumber.y);
        meshFilter.mesh = mesh;

    }

    Mesh CreateMesh(int x,int y)
    {
        Mesh mesh = new Mesh();
        mesh.Clear();

        Vector3[] vertices = new Vector3[ x * y * 4];
        Vector2[] uvs = new Vector2[x * y * 4];
        Color[] colors = new Color[x * y * 4];
        int[] triangles = new int[x * y * 6];

        int num = y * x;
        for (int i = 0; i < num; ++i)
        {
            Rect r = GetRect(quilt.tiling, i);
            float val = 0.0f;
            if( i * 2 > num){
                val = 1.0f;
            }
            float visibleVal = 1.0f;
            if( i < outNum || num -i <= outNum){visibleVal = 0.0f;}

            Color col = new Color( val,visibleVal , 1.0f);
            SetData(i, r, col, vertices, uvs, triangles, colors);
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.UploadMeshData(true);
        return mesh;
    }

    Rect GetRect(HoloPlay.Quilt.Tiling tiling,int view)
    {
        
        // copy to fullsize rt
        int ri = tiling.numViews - view - 1;
        int x = (view % tiling.tilesX) * tiling.tileSizeX;
        int y = (ri / tiling.tilesX) * tiling.tileSizeY;
            // the padding is necessary because the shader takes y from the opposite spot as this does
        Rect rtRect = new Rect(x, y + tiling.paddingY, tiling.tileSizeX, tiling.tileSizeY);
        return rtRect;
    }

    void SetData(int idx,Rect r, Color col, Vector3[] vertices , Vector2[] uvs,int []triangles , Color[] colors)
    {
        vertices[idx * 4 + 0] = new Vector3(r.x , r.y, 0f);
        vertices[idx * 4 + 1] = new Vector3(r.x , r.y + r.height, 0f);
        vertices[idx * 4 + 2] = new Vector3(r.x + r.width  , r.y + r.height, 0f);
        vertices[idx * 4 + 3] = new Vector3(r.x + r.width  , r.y , 0f);

        for( int i = 0; i < 4; ++i)
        {
            vertices[idx * 4 + i].x -= renderTextureSize.x * 0.5f;
            vertices[idx * 4 + i].y -= renderTextureSize.y * 0.5f;
        }


        uvs[idx * 4 + 0] = new Vector2(0f, 0f);
        uvs[idx * 4 + 1] = new Vector2(0f, 1f);
        uvs[idx * 4 + 2] = new Vector2(1f, 1f);
        uvs[idx * 4 + 3] = new Vector2(1f, 0f);

        triangles[idx * 6 + 0] = 0 + idx * 4;
        triangles[idx * 6 + 1] = 1 + idx * 4;
        triangles[idx * 6 + 2] = 2 + idx * 4;
        triangles[idx * 6 + 3] = 0 + idx * 4;
        triangles[idx * 6 + 4] = 2 + idx * 4;
        triangles[idx * 6 + 5] = 3 + idx * 4;

        for (int i = 0; i < 4; ++i)
        {
            colors[idx *4 + i] = col;
        }

    }
}
