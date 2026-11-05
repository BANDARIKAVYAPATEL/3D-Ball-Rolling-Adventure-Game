using UnityEngine;
using UnityEngine.UI;

public class BallAdventureGame : MonoBehaviour {
 public static BallAdventureGame I; public int level=1, score=0; public bool done=false, paused=false;
 Transform root,ball; Rigidbody rb; Text hud,msg; AudioSource audio; AudioClip coin,win,bump; float start;
 void Awake(){ I=this; Application.targetFrameRate=60; Screen.SetResolution(1600,900,false);
 audio=gameObject.AddComponent<AudioSource>(); audio.volume=1; coin=Tone(new float[]{880,1175,1568},.24f); win=Tone(new float[]{523,659,784,1046},.7f); bump=Tone(new float[]{160},.16f); UI(); Build(1); }
 void Update(){ if(Input.GetKeyDown(KeyCode.R)) Build(level); if(Input.GetKeyDown(KeyCode.N)&&done&&level<3) Build(level+1);
 if(Input.GetKeyDown(KeyCode.P)&&!done){paused=!paused;Time.timeScale=paused?0:1;msg.text=paused?"PAUSED - Press P":"";}
 hud.text="LEVEL "+level+"   "+(level==1?"GREEN VALLEY":level==2?"DESERT CANYON":"NEON SPACEWAY")+"     COINS "+score+"     TIME "+(Time.time-start).ToString("0")+"s\nWASD / ARROWS MOVE     P PAUSE     R RESTART"; }
 void Build(int n){Time.timeScale=1;paused=false;done=false;level=Mathf.Clamp(n,1,3);start=Time.time;if(root)Destroy(root.gameObject);if(ball)Destroy(ball.gameObject);root=new GameObject("Level "+level).transform;
 Camera c=Camera.main;if(c){c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=level==1?new Color(.4f,.75f,.98f):level==2?new Color(.95f,.38f,.17f):new Color(.025f,.015f,.13f);c.fieldOfView=62;}
 RenderSettings.fog=level!=1;RenderSettings.fogColor=c?c.backgroundColor:Color.black;RenderSettings.fogDensity=level==2?.008f:.012f;
 int sections=level==1?14:level==2?18:22;float width=level==1?10:level==2?7.5f:6;
 Cube("Safety Floor",new Vector3(0,-.8f,sections*6),new Vector3(80,1,sections*12+25),Color.gray,false);
 for(int i=0;i<sections;i++){float z=i*12+6,x=level==1?0:Mathf.Sin(i*.55f)*(level==2?1:1.8f);Cube("Track "+i,new Vector3(x,0,z),new Vector3(width,.55f,12.1f),level==1?new Color(.25f,.7f,.3f):level==2?new Color(.72f,.4f,.2f):new Color(.2f,.15f,.5f),false);}
 int obs=level==1?7:level==2?15:24, coins=level==1?14:level==2?20:26;
 for(int i=0;i<obs;i++){int sec=1+i*(sections-2)/obs;float x=(i%2==0?-1:1)*(level==1?2.7f:level==2?2.2f:1.7f);Cube("Obstacle "+i,new Vector3(x,.95f,sec*12+5+(i%3)*1.4f),new Vector3(1.5f,1.9f,1.3f),new Color(.9f,.12f,.12f),true);}
 for(int i=0;i<coins;i++){float z=8+i*((sections*12f-12)/coins),x=(i%2==0?-1:1)*1.4f;GameObject g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Gold Coin";g.transform.position=new Vector3(x,1.35f,z);g.transform.localScale=new Vector3(.85f,.13f,.85f);g.transform.rotation=Quaternion.Euler(90,0,0);Paint(g,Color.yellow);g.GetComponent<Collider>().isTrigger=true;g.AddComponent<Coin>();g.transform.SetParent(root);}
 float end=sections*12-2;Cube("Finish Platform",new Vector3(0,0,end),new Vector3(width+2,.6f,8),Color.green,false);GameObject gate=Cube("Finish Gate",new Vector3(0,2,end+1.5f),new Vector3(4,4,.5f),Color.green,false);gate.GetComponent<Collider>().isTrigger=true;gate.AddComponent<Finish>();
 for(int i=0;i<28;i++){float x=i%2==0?-12:12;GameObject deco=Cube("Scenery",new Vector3(x,2,i*9+3),new Vector3(level==2?4:2,level==2?6:5,2),level==1?new Color(.1f,.4f,.12f):level==2?new Color(.4f,.18f,.1f):Color.cyan,false);Destroy(deco.GetComponent<Collider>());}
 GameObject b=GameObject.CreatePrimitive(PrimitiveType.Sphere);b.name="Player Ball";b.transform.position=new Vector3(0,1.2f,0);b.transform.localScale=Vector3.one*1.25f;Paint(b,Color.cyan);rb=b.AddComponent<Rigidbody>();rb.mass=1.4f;rb.drag=.22f;rb.angularDrag=.08f;b.AddComponent<BallMove>();ball=b.transform;
 if(c){Follow f=c.GetComponent<Follow>();if(!f)f=c.gameObject.AddComponent<Follow>();f.target=ball;}
 msg.text="LEVEL "+level+"\nCollect coins and reach the finish gate!";
 }
 GameObject Cube(string n,Vector3 p,Vector3 s,Color col,bool obstacle){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.position=p;o.transform.localScale=s;Paint(o,col);if(obstacle)o.AddComponent<Hit>();o.transform.SetParent(root);return o;}
 void Paint(GameObject o,Color c){var r=o.GetComponent<Renderer>();var sh=Shader.Find("Universal Render Pipeline/Lit");if(!sh)sh=Shader.Find("Standard");if(sh){var m=new Material(sh);m.color=c;r.material=m;}}
 void UI(){var go=new GameObject("HUD Canvas");var c=go.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;var sc=go.AddComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1600,900);go.AddComponent<GraphicRaycaster>();hud=Text(go.transform,"HUD",new Vector2(.5f,1),new Vector2(0,-30),new Vector2(1500,100),21);msg=Text(go.transform,"Message",new Vector2(.5f,.55f),Vector2.zero,new Vector2(1100,180),34);}
 Text Text(Transform p,string n,Vector2 anchor,Vector2 pos,Vector2 size,int fs){var g=new GameObject(n);g.transform.SetParent(p,false);var r=g.AddComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;var t=g.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");t.fontSize=fs;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;return t;}
 AudioClip Tone(float[] notes,float dur){int rate=44100,each=Mathf.CeilToInt(rate*dur/notes.Length);float[] a=new float[each*notes.Length];for(int n=0;n<notes.Length;n++)for(int i=0;i<each;i++)a[n*each+i]=Mathf.Sin(2*Mathf.PI*notes[n]*i/rate)*(1f-(float)i/each)*.5f;var clip=AudioClip.Create("SFX",a.Length,1,rate,false);clip.SetData(a,0);return clip;}
 public void GotCoin(){score+=10;audio.PlayOneShot(coin,1);msg.text="+10 COINS!";CancelInvoke();Invoke(nameof(Clear),.5f);}void Clear(){if(!done&&!paused)msg.text="";}
 public void Bump(){audio.PlayOneShot(bump,.8f);}public void FinishLevel(){if(done)return;done=true;audio.PlayOneShot(win,1);msg.text=level<3?"LEVEL COMPLETE!\nPress N for next level or R to replay":"YOU WON ALL 3 LEVELS!\nFinal coins: "+score+"\nPress R to replay";}
}
public class BallMove:MonoBehaviour{Rigidbody r;void Start(){r=GetComponent<Rigidbody>();}void FixedUpdate(){var g=BallAdventureGame.I;if(g==null||g.paused||g.done)return;float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical");r.AddForce(new Vector3(x,0,z)*22,ForceMode.Acceleration);r.velocity=new Vector3(Mathf.Clamp(r.velocity.x,-8,8),r.velocity.y,Mathf.Clamp(r.velocity.z,-12,12));if(transform.position.y< -5)g.SendMessage("Build",g.level);}}
public class Follow:MonoBehaviour{public Transform target;void LateUpdate(){if(!target)return;transform.position=Vector3.Lerp(transform.position,target.position+new Vector3(0,7.5f,-10.5f),Time.deltaTime*5);transform.LookAt(target.position+Vector3.forward*4);}}
public class Coin:MonoBehaviour{void Update(){transform.Rotate(0,160*Time.deltaTime,0,Space.World);}void OnTriggerEnter(Collider c){if(c.GetComponent<BallMove>()){BallAdventureGame.I.GotCoin();Destroy(gameObject);}}}
public class Finish:MonoBehaviour{void OnTriggerEnter(Collider c){if(c.GetComponent<BallMove>())BallAdventureGame.I.FinishLevel();}}
public class Hit:MonoBehaviour{void OnCollisionEnter(Collision c){if(c.gameObject.GetComponent<BallMove>())BallAdventureGame.I.Bump();}}
