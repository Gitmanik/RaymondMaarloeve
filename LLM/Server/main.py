import traceback
from flask import Flask, request, jsonify
from llama_cpp import Llama

app = Flask(__name__)

# Global dictionary to store multiple LLaMA model instances.
# Each key is a unique string model_id and the value is its Llama instance.
models = {}


@app.route("/load", methods=["POST"])
def load_model():
    """
    Load a LLaMA model using llama-cpp-python under a specific model ID.

    Expected JSON payload:
    {
         "model_id": "unique_model_identifier",   // required: used to reference the model later
         "model_path": "path/to/ggml-model.bin",      // required: path to the model file
         "n_ctx": 1024,                             // optional: context window size (default: 1024)
         "n_parts": -1,                             // optional: number of model parts, -1 auto-detects parts
         "seed": 42,                                // optional: RNG seed (default: 42)
         "f16_kv": false                            // optional: whether to use fp16 key-value caching
    }
    """
    global models
    data = request.get_json()
    if not data:
        return jsonify({"error": "No input data provided."}), 400

    model_id = data.get("model_id")
    model_path = data.get("model_path")

    if not model_id or not model_path:
        return jsonify({"error": "Missing required parameters: 'model_id' and 'model_path'."}), 400

    if model_id in models:
        return jsonify({"error": f"Model with ID '{model_id}' is already loaded."}), 400

    # Use provided parameters or default values.
    n_ctx = data.get("n_ctx", 1024)
    n_parts = data.get("n_parts", -1)
    seed = data.get("seed", 42)
    f16_kv = data.get("f16_kv", False)

    try:
        model = Llama(
            model_path=model_path,
            n_ctx=n_ctx,
            n_parts=n_parts,
            seed=seed,
            f16_kv=f16_kv
        )
        models[model_id] = model
        return jsonify({"message": f"Model '{model_id}' loaded successfully from {model_path}."}), 200
    except Exception as e:
        return jsonify({
            "error": f"Failed to load model '{model_id}': {str(e)}",
            "trace": traceback.format_exc()
        }), 500


@app.route("/predict", methods=["POST"])
def predict():
    """
    Generate text using a specified loaded LLaMA model.

    Expected JSON payload:
    {
       "model_id": "unique_model_identifier",  // required: specifies which model to use
       "prompt": "Your prompt here",             // required: text prompt for generation
       "max_tokens": 100,                        // optional: maximum tokens to generate (default: 100)
       "temperature": 0.8,                       // optional: sampling temperature (default: 0.8)
       "top_p": 0.95                             // optional: nucleus sampling top_p (default: 0.95)
    }
    """
    global models
    data = request.get_json()
    if not data:
        return jsonify({"error": "No input data provided."}), 400

    model_id = data.get("model_id")
    prompt = data.get("prompt", "")
    if not model_id or prompt == "":
        return jsonify({"error": "Missing required parameters: 'model_id' and 'prompt'."}), 400

    # Check if the specified model is loaded.
    model = models.get(model_id)
    if model is None:
        return jsonify({"error": f"No loaded model found for model_id '{model_id}'."}), 400

    # Optional parameters with default values.
    max_tokens = data.get("max_tokens", 100)
    temperature = data.get("temperature", 0.8)
    top_p = data.get("top_p", 0.95)

    try:
        # Generate response using the loaded LLaMA model.
        response = model(
            prompt,
            max_tokens=max_tokens,
            temperature=temperature,
            top_p=top_p
        )
        generated_text = ""
        if "choices" in response and response["choices"]:
            generated_text = response["choices"][0]["text"]
        return jsonify({"response": generated_text.strip(), "raw": response}), 200
    except Exception as e:
        return jsonify({
            "error": f"Prediction failed for model '{model_id}': {str(e)}",
            "trace": traceback.format_exc()
        }), 500


@app.route("/unload", methods=["POST"])
def unload_model():
    """
    Unload (delete) the specified LLaMA model to free up resources.

    Expected JSON payload:
    {
         "model_id": "unique_model_identifier"   // required: specifies which model to unload
    }
    """
    global models
    data = request.get_json()
    if not data:
        return jsonify({"error": "No input data provided."}), 400

    model_id = data.get("model_id")
    if not model_id:
        return jsonify({"error": "Missing required parameter: 'model_id'."}), 400

    if model_id not in models:
        return jsonify({"error": f"Model with ID '{model_id}' is not loaded."}), 400

    try:
        # Unload the model by removing it from the dictionary. The garbage collector
        # will later reclaim the memory.
        models.pop(model_id)
        return jsonify({"message": f"Model '{model_id}' has been unloaded successfully."}), 200
    except Exception as e:
        return jsonify({"error": f"Failed to unload model '{model_id}': {str(e)}"}), 500


@app.route("/status", methods=["GET"])
def status():
    """
    Returns the status of all loaded models. The response includes model IDs and
    a simple message indicating whether the model is loaded.
    """
    global models
    # Build a dictionary with model IDs.
    status_dict = {model_id: "loaded" for model_id in models.keys()}
    return jsonify(status_dict), 200


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
