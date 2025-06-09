using System;

[Serializable]
/// <summary>
/// Represents a request to load a model into the system.
/// </summary>
public class LoadModelDTO
{
    /// <summary>
    /// The unique identifier of the model to be loaded.
    /// </summary>
    public string model_id;
    /// <summary>
    /// The path to the model file to be loaded.
    /// </summary>
    public string model_path;
    
    /// <summary>
    /// Context size for the model, which determines how many tokens can be processed at once.
    /// </summary>
    public int n_ctx;
    /// <summary>
    /// Number of parts to split the model into for loading.
    /// </summary>
    public int n_parts;
    /// <summary>
    /// Seed for random number generation, used for reproducibility in model loading.
    /// </summary>
    public int seed;
    /// <summary>
    /// Whether to use quantization for the model, which can reduce memory usage and improve performance.
    /// </summary>
    public bool f16_kv;

    /// <summary>
    /// Whether to use GPU acceleration for the model, which can significantly speed up processing, -1 for uinlimited GPU, 0 for CPU, and positive integers for specific GPU IDs.
    /// </summary>
    public int n_gpu_layers;
}
