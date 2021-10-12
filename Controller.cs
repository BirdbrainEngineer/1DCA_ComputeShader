//Attach this script to Main Camera
//Attach Simulator.compute and Viewport.compute to simulationShader and viewportshader respectively in the inspector

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public static readonly int VIEWPORTGPUTHREADSSIDE = 8;
    public static int renderMode = 0;
    public ComputeShader simulationShader;
    public ComputeShader viewportShader;
    public byte rule = 0;
    public int width = 0;
    public int generations = 0;
    public int boardToView = 0;

    private Datastructure data;
    private List<uint[]> initData;
    private int kernelID;
    private Dispatcher dispatcher;
    private RenderTexture temp;
    private Texture2D activeBoard;
    private int boardViewed = -1;

    void Start()
    {
        this.initData = GetRandomData(width, 16);
        //this.initData = FillRange(0xffff, 16);
        this.kernelID = this.viewportShader.FindKernel("Viewport");
        this.dispatcher = new Dispatcher(simulationShader, rule);
        this.dispatcher.Initialize(width, generations, this.initData);
        this.dispatcher.Simulate();
        this.data = this.dispatcher.GetData();
        this.viewportShader.SetFloats("ruleColors", ruleColors);
        this.viewportShader.SetInts("inputDims", new int[2]{width, generations});
        this.viewportShader.SetInt("renderMode", renderMode);
        this.activeBoard = new Texture2D(width, generations, TextureFormat.RGBAFloat, false);
        this.activeBoard.filterMode = FilterMode.Point;
        this.viewportShader.SetTexture(this.kernelID, "input", this.activeBoard);
        this.temp = new RenderTexture(width, generations, 24);
        this.temp.enableRandomWrite = true;
        this.temp.filterMode = FilterMode.Point;
        this.temp.wrapMode = TextureWrapMode.Clamp;
        this.temp.Create();
        this.viewportShader.SetTexture(this.kernelID, "output", this.temp);
        this.viewportShader.SetInts("outputDims", new int[2]{width, generations});
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest){
        if(this.boardViewed != boardToView){
            this.activeBoard.SetPixelData(this.data.boards[boardToView].BoardToTexture(), 0, 0);
            this.activeBoard.Apply();
        }
        if(this.data.boards[boardToView].board == null){
            Graphics.Blit(src, dest);
            Debug.Log("Data null!");
        }
        else {
            this.viewportShader.Dispatch(this.kernelID, width / VIEWPORTGPUTHREADSSIDE, generations / VIEWPORTGPUTHREADSSIDE, 1);
            Graphics.Blit(this.temp, dest);
        }
    }

    public List<uint[]> GetRandomData(int width, int boards){
        List<uint[]> result = new List<uint[]>();
        int elements = (int)Mathf.Ceil((float)width / (float)32);
        for(int i = 0; i < boards; i++){
            result.Add(new uint[elements]);
            for(int j = 0; j < elements; j++){
                result[i][j] = (uint)Random.Range(0x00000000, 0xffffffff);
            }
        }
        return result;
    }

    public List<uint[]> FillRange(int range, int shiftFactor){
        List<uint[]> result = new List<uint[]>();
        for(int i = 0; i < range; i++){
            result.Add(new uint[1]{(uint)(i << shiftFactor)});
        }
        return result;
    }
    

    public float[] ruleColors = new float[8 * 4]{
        0.0f, 0.0f, 0.0f, 1.0f,
        1.0f, 0.0f, 0.0f, 1.0f,
        1.0f, 1.0f, 0.0f, 1.0f,
        0.0f, 1.0f, 0.0f, 1.0f,
        0.0f, 1.0f, 1.0f, 1.0f,
        0.0f, 0.0f, 1.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f,
    };
}
