using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace KamiUnity
{
    public sealed class KamiGame : MonoBehaviour
    {
        Level[] levels;
        PuzzleBoard board;
        int current, selected = 1;
        bool menu;
        Vector2 scroll;
        AudioSource effects, music;
        readonly Dictionary<int, Texture2D> tiles = new Dictionary<int, Texture2D>();
        readonly Color[] fallback = { Color.white, new Color(.85f,.24f,.15f), new Color(.96f,.82f,.56f), new Color(.18f,.25f,.26f), new Color(.4f,.65f,.62f), new Color(.8f,.65f,.3f) };
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (FindObjectOfType<KamiGame>() == null) new GameObject("KAMI Game").AddComponent<KamiGame>();
        }
        void Awake()
        {
            if (Camera.main == null)
            {
                var cameraObject = new GameObject("Main Camera");
                var camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.12f,.12f,.12f);
            }
            if (FindObjectOfType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            levels = JsonUtility.FromJson<LevelCatalog>(Resources.Load<TextAsset>("levels").text).levels;
            effects = gameObject.AddComponent<AudioSource>();
            music = gameObject.AddComponent<AudioSource>();
            music.clip = Resources.Load<AudioClip>("Music"); music.loop = true; music.volume = .25f;
            if (music.clip) music.Play();
            Load(Mathf.Clamp(PlayerPrefs.GetInt("kami.current", 0), 0, levels.Length - 1));
        }
        void Load(int index)
        {
            current = index; board = new PuzzleBoard(levels[index]); selected = board.Cells[0]; menu = false;
            PlayerPrefs.SetInt("kami.current", index); PlayerPrefs.Save();
            foreach (var t in tiles.Values) Destroy(t);
            tiles.Clear();
            string theme = levels[index].theme.Replace("_three", "");
            Texture2D atlas = Resources.Load<Texture2D>("themeTextures_png/" + theme + "_full");
            TextAsset xml = Resources.Load<TextAsset>("themeTextures_png/" + theme + "_full");
            if (!atlas || !xml) return;
            foreach (var e in XDocument.Parse(xml.text).Root.Elements("SubTexture"))
            {
                string n = (string)e.Attribute("name");
                if (!n.EndsWith("_static_0000")) continue;
                int c = int.Parse(n.Substring(2, n.IndexOf('_') - 2));
                int x = (int)e.Attribute("x"), y = (int)e.Attribute("y"), w = (int)e.Attribute("width"), h = (int)e.Attribute("height");
                var tile = new Texture2D(w,h,TextureFormat.RGBA32,false);
                tile.SetPixels(atlas.GetPixels(x,atlas.height-y-h,w,h)); tile.Apply(); tiles[c] = tile;
            }
        }
        void OnGUI()
        {
            if (board == null) return;
            float scale = Mathf.Max(.1f, Mathf.Min(Screen.width/1100f, Screen.height/800f));
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1100*scale)/2,(Screen.height-800*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            GUI.Box(new Rect(0,0,1100,800), "");
            GUI.Label(new Rect(30,15,800,28), "KAMI  /  " + levels[current].stage + "  /  " + (current+1) + "    " + levels[current].id);
            if (GUI.Button(new Rect(920,12,150,32), "Levels")) menu = !menu;
            if (menu) { DrawMenu(); return; }
            GUI.Label(new Rect(30,52,1000,28), "Choose a colour, then click a connected paper region. Make the whole board one colour.");
            const float cell=60;
            float ox=(1100-board.Level.width*cell)/2, oy=100;
            for (int i=0;i<board.Cells.Length;i++)
            {
                Rect r = new Rect(ox+i%board.Level.width*cell,oy+i/board.Level.width*cell,cell,cell);
                DrawTile(r,board.Cells[i]);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none) && board.Paint(i,selected))
                {
                    var clip=Resources.Load<AudioClip>("sounds/fold_start_1"); if (clip) effects.PlayOneShot(clip,.5f);
                    if (board.Solved)
                    {
                        string key="kami.best."+board.Level.id;
                        if (board.Moves<PlayerPrefs.GetInt(key,int.MaxValue)) { PlayerPrefs.SetInt(key,board.Moves); PlayerPrefs.Save(); }
                    }
                }
            }
            var colours = new SortedSet<int>(); foreach(char c in board.Level.colours) colours.Add(c-'0');
            int slot=0;
            foreach(int c in colours)
            {
                Rect r=new Rect(30+slot++*66,720,56,48);
                DrawTile(r,c);
                if(GUI.Button(r,c==selected ? "["+c+"]" : c.ToString())) selected=c;
            }
            GUI.Label(new Rect(390,715,480,28), "Moves: "+board.Moves+"    Gold: "+board.Level.gold+"    "+board.Medal);
            int best=PlayerPrefs.GetInt("kami.best."+board.Level.id,0);
            GUI.Label(new Rect(390,744,280,28), best>0 ? "Best: "+best : "No completion yet");
            if(GUI.Button(new Rect(720,730,100,36),"Undo")) board.Undo();
            if(GUI.Button(new Rect(830,730,100,36),"Reset")) board.Reset();
            GUI.enabled=current+1<levels.Length;
            if(GUI.Button(new Rect(940,730,130,36),"Next")) Load(current+1);
            GUI.enabled=true;
        }
        void DrawTile(Rect rect,int colour)
        {
            if (tiles.TryGetValue(colour,out var tile)) GUI.DrawTexture(rect,tile,ScaleMode.StretchToFill);
            else { Color previous=GUI.color; GUI.color=fallback[Mathf.Min(colour,fallback.Length-1)]; GUI.DrawTexture(rect,Texture2D.whiteTexture); GUI.color=previous; }
        }
        void DrawMenu()
        {
            scroll=GUI.BeginScrollView(new Rect(30,65,1040,700),scroll,new Rect(0,0,1000,Mathf.Ceil(levels.Length/3f)*64));
            for(int i=0;i<levels.Length;i++)
            {
                int best=PlayerPrefs.GetInt("kami.best."+levels[i].id,0);
                string title=(i+1)+". "+levels[i].id+(best>0 ? " / "+best+" moves" : "");
                if(GUI.Button(new Rect(i%3*330,i/3*64,320,54),title)) Load(i);
            }
            GUI.EndScrollView();
        }
    }
}
