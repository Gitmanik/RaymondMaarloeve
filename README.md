# Raymond Maarloeve sp. z o.o.

## Wprowadzenie

### Opis
Projekt, nad którym pracujemy, to gra komputerowa, w której kluczową rolę odgrywa sztuczna inteligencja (AI). Jej główną cechą jest dynamiczny świat, w którym postacie niezależne (NPC) posiadają własne osobowości, harmonogramy dnia oraz zdolność do reagowania na działania gracza. Dzięki wykorzystaniu AI każda rozgrywka jest unikalna, a zachowanie NPC wpływa zarówno na fabułę, jak i na decyzje podejmowane przez gracza.

To, co wyróżnia tę grę, to brak klasycznego, sztywnego oskryptowania wydarzeń. Zamiast tego świat gry rozwija się w sposób organiczny, a interakcje między NPC oraz graczem determinują przebieg śledztwa detektywistycznego, które stanowi centralny element rozgrywki. Proceduralnie generowane otoczenie oraz złożone systemy decyzyjne NPC sprawiają, że każda sesja gry jest niepowtarzalnym doświadczeniem.

### Stack technologiczny
- **Silnik gry:** Unity 6 (6000.0.38f1) + C#
- **Sztuczna inteligencja:** Large Language Model (LLM), pathfinding NavMeshPlus
- **Grafika:** Darmowe assety ([Itch.io Free Assets](https://itch.io/game-assets/free/tag-isometric))

### Grupa
- **Lider:** Cyprian Zasada
- **Zastępca lidera:** Marek Nijakowski
- **Zespół:**
  - Paweł Reich
  - Paweł Dolak
  - Maciej Pitucha
  - Maciej Włudarski
  - Karol Rzepiński
  - Kamil Włodarczyk
  - Łukasz Jastrzębski
  - Michał Eisler
  - Łukasz Czarzasty 

---

## Założenia

### Rozgrywka
- **Cel gry:**
  Gracz wciela się w detektywa badającego sprawę morderstwa w małej społeczności NPC. Śledztwo opiera się na analizie poszlak oraz rozmowach z NPC sterowanymi przez AI. Na końcu gracz rekonstruuje przebieg wydarzeń, co decyduje o sukcesie lub porażce.
- **Metryki sukcesu:**
  - Gra umożliwia swobodną eksplorację świata i interakcje z NPC.
  - Morderstwo następuje w pewnym momencie gry (np. trzeciego dnia).
  - Istnieją co najmniej 2 źródła poszlak (np. rozmowy z NPC + dowody fizyczne).
  - Gracz może prezentować swoją teorię w finale gry poprzez interaktywny system układania sekwencji wydarzeń.

### Postacie NPC
- **Zachowanie:**
  NPC posiadają unikalne osobowości, harmonogramy dnia oraz zdolność dynamicznej reakcji na działania gracza.
- **Metryki sukcesu:**
  - Liczba NPC w grze: minimum 6, docelowo 10.
  - NPC generują odpowiedzi i decyzje dzięki LLM.
  - NPC mogą dynamicznie zmieniać swoje trasy w odpowiedzi na interakcje z graczem (docelowo, w odpowiedzi na interakcje z innymi NPC).

### Mapa
- **Wygląd:**
  W grze każdy NPC ma swój dom, a ich rozłożenie jest generowane proceduralnie.

---

## Wykonanie

### Podział prac
#### Unity:
- Paweł Reich
- Marek Nijakowski
- Paweł Dolak

#### LLM:
- Maciej Pitucha
- Maciej Włudarski
- Karol Rzepiński
- Kamil Włodarczyk
- Łukasz Jastrzębski
- Michał Eisler
- Łukasz Czarzasty

### Milestones
#### 1. Milestone - Prototyp gry (bez LLM)
- Istnieje baza gry, w której postacie się poruszają.
- Akcje NPC są podejmowane losowo lub według predefiniowanych schematów.

#### 2. Milestone - Integracja LLM z Unity
- Do gry wprowadzono system dnia i nocy.
- Dataset posiada 50% zamierzonych promptów.


#### 3. Milestone - Ostatnie szlify
- Menu główne
- Ścieżka dźwiękowa
- Napisy końcowe
- Wykonano fine-tuning modelu LLM.

### Podział zadań
📌 [Harmonogram Gantta](https://docs.google.com/spreadsheets/d/1uFGMCmiO6wAubyI_MKR1ynXz4QdD-30tejBS1lcy7w8/edit?usp=sharing)

