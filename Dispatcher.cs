using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Dispatcher{
    public static readonly int SIMBLOCKSIZE = 32;
    public bool isFinished;

    private readonly int rule;
    private readonly int kernelID;
    private ComputeShader simulator;
    private Datastructure data;
    private ComputeBuffer[] buffers;
    private int paddedWidth;

    public Dispatcher(ComputeShader simulator, byte rule){
        this.simulator = simulator;
        this.kernelID = this.simulator.FindKernel("CAsim");
        this.rule = (int)rule;
        this.isFinished = false;
    }

    public Datastructure GetData(){
        return this.data;
    }

    public void Initialize(int width, int generations, List<uint[]> initData){
        if(Datastructure.MAXDATASIZE < Datastructure.EstimateSize(width, generations, initData.Count)){ 
            Debug.Log("Initialization Failed: Resulting Datastructure too large!");
        }
        else if(width > 0 && generations > 0 && initData.Count > 0){
            this.paddedWidth = (int)(Mathf.Ceil((float)width / (float)Dispatcher.SIMBLOCKSIZE) * Dispatcher.SIMBLOCKSIZE);
            this.data = new Datastructure(width, generations, initData.Count, initData);
            this.simulator.SetInt("rule", this.rule);
            this.simulator.SetInt("rightEdge", width - 1);
            this.buffers = new ComputeBuffer[generations];
            for(int i = 0; i < generations; i++){
                this.buffers[i] = new ComputeBuffer(this.paddedWidth, sizeof(int) * 2);
                if(!this.buffers[i].IsValid()){ Debug.Log("Something went wrong with initializing Computebuffers!"); }
            }
        }
        else {
            Debug.Log("Initialization Failed: One or more parameters too small!");
        }
    }

    public void Simulate(){
        float timeTaken = Time.time;
        for(int i = 0; i < this.data.initData.Count; i++){
            this.buffers[0].SetData(this.data.ParseInitializer(i, this.paddedWidth), 0, 0, this.paddedWidth);
            for(int j = 1; j < this.data.boardGenerations; j++){
                this.simulator.SetBuffer(this.kernelID, "input", this.buffers[j - 1]);
                this.simulator.SetBuffer(this.kernelID, "output", this.buffers[j]);
                this.simulator.Dispatch(this.kernelID, this.paddedWidth / SIMBLOCKSIZE, 1, 1);
            }
            for(int k = 0; k < this.data.boardGenerations; k++){
                this.buffers[k].GetData(this.data.boards[i].board, this.data.boardWidth * k, 0, this.data.boardWidth);
            }
        }
        foreach(var buffer in this.buffers){ buffer.Release(); }
        this.isFinished = true;
        Debug.Log("Simulation completed in: " + (Time.time - timeTaken).ToString() + " seconds.");
    }
}
