using System.IO;
using UnityEngine;

namespace Code.ETC
{
    public class RenderTextureExporter : MonoBehaviour
    {
        public RenderTexture targetRT; // 여기에 MinimapRenderTexture 연결

        [ContextMenu("Save to PNG")]
        public void ExportRT()
        {
            // 현재 활성화된 RT 백업
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = targetRT;

            // Texture2D 생성 (RT 해상도에 맞춤)
            Texture2D tex = new Texture2D(targetRT.width, targetRT.height, TextureFormat.RGB24, false);
        
            // RT의 데이터를 Texture2D로 읽어오기
            tex.ReadPixels(new Rect(0, 0, targetRT.width, targetRT.height), 0, 0);
            tex.Apply();

            // PNG로 변환 후 저장
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes("Assets/Work/LKW/Shader"+ "/ExportedMinimap.png", bytes);

            // RT 복구 및 메모리 정리
            RenderTexture.active = previous;
            DestroyImmediate(tex);
        
            Debug.Log("이미지 저장 완료: " + Application.dataPath + "/ExportedMinimap.png");
        }
    }
}