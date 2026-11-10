# Projekt aplikacji – Zarządzanie sprzedażą części samochodowych

## 1. Nazwa i autorzy

- **Nazwa projektu:** Aplikacja do zarządzania sprzedażą części samochodowych.
- **Autor:** Andrzej Hebda.

## 2. Cel aplikacji

Celem aplikacji jest stworzenie systemu internetowego umożliwiającego rejestrowanie, kontrolowanie i analizowanie sprzedaży części samochodowych.

Aplikacja jest przeznaczona dla:
- **Pracowników (sprzedawców)** – którzy będą dodawać informacje o sprzedanych częściach i przeglądać swoją historię sprzedaży.
- **Administratora** – który będzie zarządzać pracownikami, sprzedażą, samochodami oraz generować raporty i statystyki.

Aplikacja rozwiązuje problem ręcznego zapisywania i kontrolowania sprzedaży. Ułatwia sprawdzanie, jakie części zostały sprzedane, z jakich samochodów pochodzą, w jakiej ilości i przez którego pracownika.

**Przykład użycia:**

Pracownik sprzedaje lewe lusterko samochodowe. Loguje się do aplikacji, wybiera samochód z listy, wpisuje nazwę części, ilość oraz cenę. System automatycznie przelicza cenę brutto na netto i zapisuje sprzedaż w historii. Administrator może później sprawdzić tę transakcję i uwzględnić ją w raporcie sprzedaży.

## 3. Planowane funkcje

1. **Logowanie do aplikacji** – administrator i pracownicy mogą logować się na swoje indywidualne konta. System rozpoznaje ich uprawnienia i wyświetla odpowiedni panel.

2. **Dodawanie sprzedaży** – pracownik może dodać nową sprzedaż, wybierając samochód, wpisując nazwę części, ilość oraz cenę netto lub brutto.

3. **Przeglądanie historii sprzedaży** – pracownik widzi swoje sprzedaże, natomiast administrator ma dostęp do historii wszystkich pracowników.

4. **Filtrowanie sprzedaży** – użytkownik może wyszukiwać sprzedaże według daty, samochodu lub nazwy części. Administrator może dodatkowo filtrować według pracownika.

5. **Zarządzanie pracownikami** – administrator może dodawać, edytować, dezaktywować i usuwać konta użytkowników oraz resetować ich hasła.

6. **Zarządzanie samochodami** – administrator może dodawać nowe samochody, edytować ich dane i dezaktywować nieaktualne pozycje.

7. **Podpowiedzi nazw części** – podczas wpisywania nazwy części system wyświetla podpowiedzi na podstawie wcześniej zapisanych sprzedaży.

8. **Statystyki i raporty** – administrator może sprawdzać wyniki sprzedaży pracowników, najczęściej sprzedawane części oraz wartość sprzedaży w wybranym okresie.

9. **Eksport raportów** – administrator może pobierać raporty w formatach PDF, Excel i CSV.

10. **Automatyczne przeliczanie cen** – system przelicza ceny brutto na netto według ustawionej stawki VAT, domyślnie 23%.

## 4. Technologie i dane

Do stworzenia aplikacji zostaną wykorzystane:

- **HTML** – do tworzenia struktury strony internetowej.
- **CSS** – do wyglądu i stylizacji aplikacji.
- **JavaScript** – do obsługi interakcji i funkcjonalności po stronie użytkownika.
- **PHP** – do obsługi logiki aplikacji, komunikacji z bazą danych i przetwarzania danych.
- **MySQL** – do przechowywania wszystkich informacji w centralnej bazie danych.

W bazie danych będą przechowywane:
- Dane użytkowników i ich uprawnienia.
- Lista samochodów.
- Informacje o sprzedaży.
- Nazwy części wykorzystywane do podpowiedzi.
- Historia zmian i operacji wykonywanych w systemie.

Za obsługę danych będzie odpowiadał serwer PHP, który będzie komunikował się z bazą MySQL.

## 5. Zagrożenia i zabezpieczenia

| Zagrożenie | Zabezpieczenie |
|---|---|
| Pracownik może próbować przeglądać sprzedaż innych pracowników. | Kontrola uprawnień po stronie serwera. |
| Nieuprawniona osoba może próbować zalogować się na konto administratora. | Bezpieczne hashowanie haseł i indywidualne konta użytkowników. |
| Użytkownik może próbować zmienić lub usunąć cudzą sprzedaż. | Sprawdzanie uprawnień przed każdą operacją. |
| Ktoś może przesłać błędną ilość części, np. liczbę ujemną. | Walidacja danych i dopuszczanie wyłącznie poprawnych wartości. |
| Użytkownik może wpisać błędną lub ujemną cenę. | Sprawdzanie poprawności ceny i blokowanie wartości niedozwolonych. |
| Ktoś może próbować wprowadzić złośliwy kod w formularzu. | Walidacja i oczyszczanie danych oraz stosowanie zapytań parametryzowanych. |
| Może dojść do przypadkowego usunięcia ważnych danych. | Tworzenie kopii zapasowych i archiwizacja zamiast trwałego usuwania. |
| Ktoś może próbować zmienić dane sprzedaży bez pozostawienia śladu. | Rejestrowanie historii zmian wraz z informacją o osobie wykonującej operację. |

## 6. Plan pracy

### Etap 1 – Podstawowa aplikacja
- Przygotowanie projektu i bazy danych.
- Stworzenie systemu logowania.
- Dodanie podziału na administratora i pracownika.
- Stworzenie paneli użytkowników.
- Dodawanie i zarządzanie kontami.
- Zarządzanie listą samochodów.
- Dodawanie nowych sprzedaży.

### Etap 2 – Historia i automatyzacja
- Stworzenie historii sprzedaży.
- Dodanie filtrowania i wyszukiwania.
- Wprowadzenie podpowiedzi nazw części.
- Dodanie automatycznego przeliczania cen brutto i netto.
- Dodanie możliwości edytowania i usuwania sprzedaży.

### Etap 3 – Statystyki i raporty
- Stworzenie panelu statystyk administratora.
- Dodanie raportów sprzedaży pracowników.
- Dodanie raportów dotyczących części i samochodów.
- Przygotowanie wykresów sprzedaży.
- Dodanie eksportowania raportów do PDF, Excel i CSV.

### Etap 4 – Testowanie i wdrożenie
- Dostosowanie aplikacji do telefonów i tabletów.
- Testowanie poprawności działania funkcji.
- Testowanie zabezpieczeń i uprawnień.
- Poprawienie błędów i optymalizacja aplikacji.
- Wdrożenie aplikacji na serwerze.
- Konfiguracja regularnych kopii zapasowych.

## Cel końcowy projektu

Stworzenie prostej i bezpiecznej aplikacji internetowej, która usprawni zarządzanie sprzedażą części samochodowych, pozwoli pracownikom szybko rejestrować transakcje, a administratorowi kontrolować wyniki sprzedaży i analizować działalność firmy.
