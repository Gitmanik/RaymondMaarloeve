# Model Testing Report – Mathstral-7B

**Tester:** Kamil Włodarczyk  
**Date:** 08.04.2025

---

## Model Name: Mistral 7B  
## Purpose:  
Test the model's ability to evaluate the importance of memories without fine-tuning.

---

### Prompt Template:
PROMPT:

You will receive a short memory.
Your task is to rate how important the memory is, on a scale from 1 to 10.

Scale:
1 = very minor things (e.g., making the bed, seeing a bird)
10 = very serious things (e.g., witnessing or committing a murder)

Rules:

-Only use the information in the Memory section

-Output a single number (no words, no explanation)

-Always output only the number

Memory: <Your input>

Importance Rating:
---

### Memories and Model Output:

1. **Saw a raven land on the church roof at dawn** – 1  
2. **Lost a copper coin while buying apples at the market** – 1  
3. **Helped the blacksmith carry tools to his forge** – 3  
4. **Heard strange howling outside the village walls at night** – 5  
5. **The innkeeper’s cat gave birth to kittens behind the bar** – 1  
6. **Noticed bloodstains near the old well but told no one** – 6  
7. **Found your neighbor's goat wandering alone in the woods** – 3  
8. **A drunk man at the tavern spoke of the forest being cursed** – 5  
9. **Helped bury a traveler who died of sickness** – 4  
10. **Saw the town guard speaking secretly with a cloaked stranger** – 7  
11. **Witnessed the baker's son sneaking out just before curfew** – 5  
12. **Discovered a torn piece of cloak near the latest murder site** – 8  
13. **Fought with your sibling over inheritance of your father’s land** – 9  
14. **The priest warned you in private to stop asking questions** – 6  
15. **Your best friend vanished without a word two nights ago** – 10  
16. **Overheard the mayor bribing a guard late at night** – 8  
17. **Found a hidden dagger buried in your garden** – 7  
18. **Were attacked by a hooded figure but managed to escape** – 9  
19. **Saw the murderer’s face but were too scared to speak** – 10  
20. **Killed someone you believed was the murderer—and now you're not sure** – 10

---

### Notes:

The model has performed well in evaluating the importance of the memories. The responses align with the expected ratings for various levels of significance, from trivial occurrences to life-altering events.

At this stage, there is no immediate need for fine-tuning, as the model's output is satisfactory for the task. Further testing could be conducted if more nuanced responses are required, but currently, the model shows consistent and accurate judgment of importance.
