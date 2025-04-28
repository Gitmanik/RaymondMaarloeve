using System;

[Serializable]
public class LoadModelDTO
{
    public string model_id;
    public string model_path;
    public int n_ctx;
    public int n_parts;
    public int seed;
    public bool f16_kv;
    public int n_gpu_layers;
}
