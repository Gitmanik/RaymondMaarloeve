# Raymond Maarloeve - Unity

## Struktura projektu
- **SampleScene** – ta scena służy jako poligon doświadczalny (sandbox). Wszystkie nowe pomysły i testy można tutaj wprowadzać bez obaw, że wpłyną na ostateczną wersję gry.
- **Docelowa scena** – planujemy stworzyć osobną scenę (lub sceny) przeznaczone na finalną wersję gry. Szczegóły i terminy przenoszenia funkcjonalności zostaną ustalone w trakcie rozwoju projektu.

## Zarządzanie współpracą
1. **Unikaj jednoczesnej edycji tych samych elementów** – jeśli jedna osoba wprowadza zmiany w danym elemencie (np. prefab, skrypt), staraj się w tym samym czasie go nie modyfikować. Dzięki temu unikniemy konfliktów przy merge.
2. **Częste komity** – regularnie przesyłaj zmiany, aby reszta zespołu mogła je zobaczyć i uniknąć duplikowania pracy.
3. **Komunikacja** – informuj pozostałych członków zespołu o tym, nad czym pracujesz. Używaj do tego systemu śledzenia zadań i/lub kanału do komunikacji grupowej.

## Struktura UI
- W scenie SampleScene znajduje się obiekt `_UI` z komponentem `Canvas`, który jest głównym kontenerem dla wszystkich elementów interfejsu użytkownika.
- Każda nowa funkcjonalność UI powinna mieć swój **osobny obiekt** (GameObject) jako dziecko obiektu `_UI`. Dzięki temu łatwiej zarządzać poszczególnymi elementami i uniknąć konfliktów.
- Pamiętaj, aby korzystać z **TextMeshPro** dla wszelkich tekstów w interfejsie – zapewni to lepszą jakość wyświetlania oraz dodatkowe opcje stylizacji.

## Konwencje i dobre praktyki
- **Nazewnictwo obiektów**: Staraj się używać czytelnych nazw opisujących funkcję obiektu (np. `MenuPanel`, `PlayButton`, `ScoreText`).
- **Prefaby**: Jeśli dany obiekt może się przydać w wielu scenach, warto go zapisać jako prefab. Umożliwi to łatwiejsze aktualizacje i ponowne wykorzystanie.
- **Skrypty**: Każda nowa funkcjonalność powinna być w miarę możliwości umieszczona w osobnym skrypcie, z zachowaniem zasady pojedynczej odpowiedzialności (Single Responsibility Principle).
- **Kontrola wersji**: Unikaj wprowadzania wielu radykalnych zmian w jednym commitcie. Lepiej zrobić kilka mniejszych, opisowych committów.

## Jak zacząć?
1. Sklonuj repozytorium lokalnie.
2. Otwórz projekt w **Unity**.
3. W eksploratorze Unity znajdź scenę `SampleScene` i uruchom ją, aby zobaczyć bieżące testowe elementy.
4. Dodawaj swoje elementy – pamiętając o podanych wyżej zasadach współpracy i konwencjach.

## Plan rozwoju
1. Dodawanie kolejnych modułów i testowanie ich w `SampleScene`.
2. Stopniowe przenoszenie dopracowanych elementów do finalnej sceny (lub tworzenie nowej sceny pod kątem właściwej gry).
3. Optymalizacja, testy i refaktoryzacja kodu przed wydaniem pierwszej oficjalnej wersji.

---

Jeśli masz dodatkowe pytania lub sugestie dotyczące organizacji projektu, dodaj je w sekcji Issue lub skontaktuj się z resztą zespołu. Dobra komunikacja to klucz do uniknięcia problemów z łączeniem zmian! Powodzenia w dalszym rozwoju projektu.
