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
         "model_path": "path/to/ggml-model.bin",  // required: path to the model file
         "n_ctx": 1024,                           // optional: context window size (default: 1024)
         "n_parts": -1,                           // optional: number of model parts, -1 auto-detects parts
         "seed": 42,                              // optional: RNG seed (default: 42)
         "f16_kv": false,                         // optional: whether to use fp16 key-value caching
         "n_gpu_layers": -1                       // optional: number of layers to offload to GPU, -1 for all (default: -1)
    }
    """
    global models
    data = request.get_json()
    if not data:
        return jsonify({"message": "No input data provided.", "success": False}), 400

    model_id = data.get("model_id")
    model_path = data.get("model_path")

    if not model_id or not model_path:
        return jsonify({"message": "Missing required parameters: 'model_id' and 'model_path'.", "success": False}), 400

    if model_id in models:
        return jsonify({"message": f"Model with ID '{model_id}' is already loaded.", "success": False}), 400

    # Use provided parameters or default values.
    n_ctx = data.get("n_ctx", 1024)
    n_parts = data.get("n_parts", -1)
    seed = data.get("seed", 42)
    f16_kv = data.get("f16_kv", False)
    n_gpu_layers = data.get("n_gpu_layers", -1)  # -1 offloads all layers to GPU

    try:
        model = Llama(
            model_path=model_path,
            n_ctx=n_ctx,
            n_parts=n_parts,
            seed=seed,
            f16_kv=f16_kv,
            n_gpu_layers=n_gpu_layers,
        )
        models[model_id] = model
        return jsonify({
            "message": f"Model '{model_id}' loaded successfully from {model_path}.",
            "success": True
        }), 200
    except Exception as e:
        return jsonify({
            "message": f"Failed to load model '{model_id}': {str(e)}\ntrace: {traceback.format_exc()}",
            "success": False
        }), 500

@app.route("/chat", methods=["POST"])
def chat():
    """
    Generate responses using a chat-based format with user and assistant messages.

    Expected JSON payload:
    {
        "model_id": "unique_model_identifier",  // required: specifies which model to use
        "messages": [                            // required: array of message objects
            {"role": "system", "content": "You are a helpful assistant."}, // optional system message
            {"role": "user", "content": "Hello, how are you?"},            // user messages
            {"role": "assistant", "content": "I'm doing well, thank you!"}, // assistant messages
            {"role": "user", "content": "Tell me about yourself."}         // typically ends with user
        ],
        "max_tokens": 100,                       // optional: maximum tokens to generate (default: 100)
        "temperature": 0.8,                      // optional: sampling temperature (default: 0.8)
        "top_p": 0.95                            // optional: nucleus sampling top_p (default: 0.95)
    }

    Returns just the generated assistant response text.
    """
    global models
    data = request.get_json()
    if not data:
        return jsonify({"message": "No input data provided.", "success": False}), 400

    model_id = data.get("model_id")
    messages = data.get("messages", [])

    if not model_id or not messages:
        return jsonify({"message": "Missing required parameters: 'model_id' and 'messages'.", "success": False}), 400

    for msg in messages:
        if not isinstance(msg, dict) or "role" not in msg or "content" not in msg:
            return jsonify({"message": "Invalid message format. Each message must have 'role' and 'content' fields.", "success": False}), 400
        if msg["role"] not in ["system", "user", "assistant"]:
            return jsonify({"message": f"Invalid role: '{msg['role']}'. Must be 'system', 'user', or 'assistant'.", "success": False}), 400

    # Check if the specified model is loaded.
    model = models.get(model_id)
    if model is None:
        return jsonify({"message": f"No loaded model found for model_id '{model_id}'.", "success": False}), 400

    # Optional parameters with default values
    max_tokens = data.get("max_tokens", 100)
    temperature = data.get("temperature", 0.8)
    top_p = data.get("top_p", 0.95)

    try:
        # Format the messages into a prompt
        formatted_prompt = format_chat_messages(messages)
        
        # Generate response using the loaded LLaMA model
        response = model(
            formatted_prompt,
            max_tokens=max_tokens,
            temperature=temperature,
            top_p=top_p
        )
        
        # Extract the generated text
        generated_text = ""
        if "choices" in response and response["choices"]:
            generated_text = response["choices"][0]["text"]
            
        return jsonify({"response": generated_text.strip(), "success": True}), 200
    except Exception as e:
        return jsonify({
            "message": f"Chat completion failed for model '{model_id}': {str(e)}\ntrace:{traceback.format_exc()}",
            "success": False
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
        return jsonify({"message": "No input data provided.", "success": False}), 400

    model_id = data.get("model_id")
    if not model_id:
        return jsonify({"message": "Missing required parameter: 'model_id'.", "success": False}), 400

    if model_id not in models:
        return jsonify({"message": f"Model with ID '{model_id}' is not loaded.", "success": False}), 400

    try:
        # Unload the model by removing it from the dictionary. The garbage collector
        # will later reclaim the memory.
        models.pop(model_id)
        return jsonify({
            "message": f"Model '{model_id}' has been unloaded successfully.",
            "success": True
        }), 200
    except Exception as e:
        return jsonify({
            "message": f"Failed to unload model '{model_id}': {str(e)}",
            "success": False
        }), 500

@app.route("/status", methods=["GET"])
def status():
    """
    Returns ids of all loaded models.
    """
    global models
    return jsonify({"healthy": True, "models": list(models.keys())}), 200


def format_chat_messages(messages):
    """
    Format a list of chat messages into a single prompt string.
    Uses a format compatible with various LLaMA models.

    Args:
        messages: List of message dictionaries with 'role' and 'content' keys

    Returns:
        A formatted prompt string
    """
    prompt = ""

    # Extract system message if present
    system_message = None
    for msg in messages:
        if msg["role"] == "system":
            system_message = msg["content"]
            break

    # Start with system message if available
    if system_message:
        prompt += f"<system>\n{system_message}\n</system>\n\n"

    # Add conversation history
    for msg in messages:
        if msg["role"] == "system":
            continue  # Skip system message as it was already handled

        if msg["role"] == "user":
            prompt += f"<human>: {msg['content']}\n"
        elif msg["role"] == "assistant":
            prompt += f"<assistant>: {msg['content']}\n"

    # Add final assistant prompt
    prompt += "<assistant>: "

    return prompt

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)