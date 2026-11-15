using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KamiUnity.Editor
{
    public sealed class PaperImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/themeTextures_png/")) return;
            var importer=(TextureImporter)assetImporter;
            importer.isReadable=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=4096;
            importer.mipmapEnabled=false;
        }
    }
    public static class KamiProject
    {
        [MenuItem("KAMI/Open Game")]
        public static void OpenGame()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Kami.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Kami.unity",true)};
            PlayerSettings.productName="KAMI Unity";
            PlayerSettings.companyName="KamiPort";
        }
        // Unity -batchmode -projectPath ... -executeMethod KamiUnity.Editor.KamiProject.SmokeTest -quit
        public static void SmokeTest()
        {
            OpenGame();
            var catalog=JsonUtility.FromJson<LevelCatalog>(Resources.Load<TextAsset>("levels").text);
            int solved=0;
            foreach(var level in catalog.levels)
            {
                var board=new PuzzleBoard(level);
                TextAsset xml=null;
                foreach(var candidate in Resources.LoadAll<TextAsset>("puzzles"))
                    if(candidate.name==level.id) { xml=candidate; break; }
                if(xml==null) throw new System.Exception("Missing XML: "+level.id);
                var doc=System.Xml.Linq.XDocument.Parse(xml.text);
                foreach(var turn in doc.Root.Element("Solution").Elements("Turn"))
                {
                    int index=turn.Attribute("num")!=null ? (int)turn.Attribute("num") :
                        (int)turn.Attribute("y")*level.width+(int)turn.Attribute("x");
                    board.Paint(index,(int)turn.Attribute("colour"));
                }
                if(!board.Solved) throw new System.Exception("Unsolved: "+level.id);
                solved++;
                board.Undo();
                if(board.Solved) throw new System.Exception("Undo failed: "+level.id);
                board.Reset();
                if(board.Moves!=0) throw new System.Exception("Reset failed: "+level.id);
            }
            Debug.Log("KAMI_SMOKE_PASSED levels="+solved);
        }
    }
}
