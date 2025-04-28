import pandas as pd


df = pd.read_excel("test_dataset_for_finetuning.xlsx")

df["instruction"] = df["Input"].astype(str).str.strip()
df["response"] = df["Output"].astype(str).str.strip()

df["conversations"] = df.apply(
    lambda row: [
        {"role": "user", "content": row["instruction"]},
        {"role": "assistant", "content": row["response"]}
    ],
    axis=1
)

df[["conversations"]].to_parquet("finetune_ready_sharegpt.parquet", index=False)

print("Zapisano jako finetune_ready_sharegpt.parquet")

df = pd.read_parquet("finetune_ready_npc.parquet")
print(df.head())
