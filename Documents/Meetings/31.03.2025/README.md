## Memory Weighting System

### Weight Categories
Each weight category will be scaled by 10 to avoid potential issues with fractional values.

#### Recency
- Measures how recently the memory was formed.
- Starts with a weight of **10**, and every hour, the current value is multiplied by **0.995** (adjustable).

#### Importance
- Represents the significance of an action without context.
- Fixed value between **1 and 10**:
  - **1** – Mundane actions (e.g., making the bed, seeing a bird).
  - **5** – Moderately significant (e.g., suspicious behavior from a neighbor—optional, as it may skew the range).
  - **10** – Extreme events (e.g., witnessing or committing a murder).

#### Relevance
- Represents how significant the action is given the context and existing memories.
- Fixed value between **1 and 10**:
  - Considers connection to previous memories (e.g., an argument with a neighbor followed by suspicious behavior).
  - Relation to the current environment (e.g., someone asking for the restroom in a tavern).
  - Importance of the event to the game narrative (e.g., events tied to a murder investigation).
  - If a new memory connects to an old one, the **old memory’s Relevance value increases**.

### Memory Weight Calculation
- **Total Weight** = Recency + Importance + Relevance
- **Maximum possible value:** 30

### Memory Format
```
(date hour) - (Memory content) - (total weight) - (each weight separately)
```

---

## Command Format
**COMMAND:** Output only ratings (number only). Use only memory in **Memory:** section.

### Example

#### **Memory:**
```
Memory: Argument with your neighbor about their loud dog
```

#### **Importance Rating:**
On a scale of **1 to 10**, where **1** is mundane and **10** is extremely poignant, rate the memory's importance.
```
Rating: <fill in>
```

#### **Relevance Rating:**
On a scale of **1 to 10**, where **1** is completely irrelevant and **10** is extremely relevant, rate the memory's relevance.
```
Rating: <fill in>
```

