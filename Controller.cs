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

    private Datastructure data;
    private List<uint[]> initData;
    private int kernelID;
    private Dispatcher dispatcher;
    private RenderTexture temp;
    private Texture2D activeBoard;

    void Start()
    {
        this.initData = new List<uint[]>();
        uint[] randomints = new uint[60];
        for(int i = 0; i < 60; i++){randomints[i] = (uint)Random.Range(0, 0x7fffffff); }
        this.initData.Add(randomints);
        //this.initData.Add(new uint[1]{0x00008000});
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
        this.activeBoard.SetPixelData(this.data.boards[0].BoardToTexture(), 0, 0);
        for(int i = 0; i < 32; i++){print(this.data.boards[0].board[i].state);}
        this.activeBoard.Apply();
        this.viewportShader.SetTexture(this.kernelID, "input", this.activeBoard);
        this.temp = new RenderTexture(width, generations, 24);
        this.temp.enableRandomWrite = true;
        this.temp.filterMode = FilterMode.Point;
        this.temp.wrapMode = TextureWrapMode.Clamp;
        this.temp.Create();
        print("Board Size: " + this.data.boards[0].board.Length.ToString());
        print("Should be: " + width * generations);
        print((this.data.boards[0].board.Length / width).ToString());
        print("Resolution: " + this.temp.width.ToString() + " x " + this.temp.height.ToString());
        this.viewportShader.SetTexture(this.kernelID, "output", this.temp);
        this.viewportShader.SetInts("outputDims", new int[2]{width, generations});
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest){
        if(this.data.boards[0].board == null){
            Graphics.Blit(src, dest);
            Debug.Log("Data null!");
        }
        else {
            this.viewportShader.Dispatch(this.kernelID, width / VIEWPORTGPUTHREADSSIDE, generations / VIEWPORTGPUTHREADSSIDE, 1);
            Graphics.Blit(this.temp, dest);
        }
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

    public static readonly uint[] TEST = new uint[60]{
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
                0x00008000, 0x60020030, 0x00070010, 0x00000000, 0x00362000, 0x80008000,
            };
}
