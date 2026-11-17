# Sprzedaż netto i sprzedawcy

## Ilości i wyszukiwanie (24.09.2026)

- Zaznacz części, następnie podaj dla każdej ilość sztuk i cenę **za sztukę**. Wartość pozycji to zaokrąglona jednostkowa cena netto pomnożona przez ilość. W przypadku brutto najpierw przeliczana jest cena jednostkowa.
- Pole „Wyszukaj części” filtruje podczas pisania, bez rozróżniania wielkości liter i polskich znaków. Sekcja „Wybrane części” pozostaje widoczna niezależnie od wyszukiwania.
- Ilości od 1 do 10 000, wyłącznie całkowite. `POST /api/sales/batch` przyjmuje `items: [{ partId, price, quantity }]`; brak quantity oznacza 1 dla zgodności z poprzednim API.
- `Sales.Price` pozostaje jednostkową ceną netto. Historia API zwraca dodatkowo `quantity` i `totalNet`. Raport `partsCount` oraz liczniki zestawień sumują sztuki, a `salesCount` liczy pozycje zapisane w tabeli Sales. Nie jest liczbą odrębnych zamówień.
- Migracja `SaleQuantities` nadaje starym pozycjom ilość 1, bez zmiany ich cen. Nie cofaj jej po zapisaniu ilości większych od 1 bez kopii bazy — utracisz informacje o ilościach.
- Katalog grupuje istniejące samochody według marek, bez rozróżniania wielkości liter. Raport na stronie i PDF nie pokazuje już średniej.
- Testy: `./Test-ilosci.ps1` (PowerShell 7, przy działającym lokalnym serwerze).

## Obsługa

- Administrator podczas dodawania sprzedaży wybiera: Krystian, Adam, Damian, Paweł albo „sprzedaż na miejscu”. To pozycje do raportowania, nie nowe konta logowania.
- Pole „Rodzaj wpisywanej ceny” pozwala wpisać netto lub brutto. Brutto dzielimy przez 1,23 i zaokrąglamy do dwóch miejsc po przecinku. Formularz pokazuje wynik; backend wykonuje niezależnie to samo obliczenie.
- Historia i raporty, także PDF, pokazują wyłącznie kwoty netto. Sumowane są zaokrąglone kwoty poszczególnych sprzedaży.
- W historii listy „Sprzedawca” i „Samochód” filtrują natychmiast; mogą działać razem. Wyszukiwanie i ręczne daty zatwierdza przycisk „Filtruj”.
- Skróty okresów obejmują pełne miesiące i tygodnie od poniedziałku do niedzieli. Obie daty graniczne są wliczone.
- Wszystkie części są dostępne dla każdego samochodu; przypisywanie części usunięto. Katalog aut sortuje marki i modele alfabetycznie bez rozróżniania wielkości liter. Volkswagen zapisuje się jako „vw”.

## Dane i migracja

Migracja EF `NetPricesAndSalespeople` wykonuje jednorazowo:

1. Dodanie tabeli `Salespeople` oraz opcjonalnej relacji `Sales.SalespersonId`.
2. Zachowanie dotychczasowych cen brutto w `Sales.OriginalGrossPrice` i przeliczenie `Sales.Price` na netto (`ROUND(Price / 1.23, 2)`). Dotychczasowy formularz opisywał ceny jako brutto.
3. Ujednolicenie nazwy marki Volkswagen/vw do `vw`.

Dotychczasowe sprzedaże zachowują pierwotnych sprzedawców; są oni nadal widoczni w filtrze historii. Nie przypisujemy starych transakcji arbitralnie do nowych osób. `UserId` zawsze oznacza rzeczywiste konto wprowadzające sprzedaż. Pracownik nie może przekazać `SalespersonId` — API zwraca 403. Administrator musi wybrać istniejącą osobę. Stara tabela `CarParts` pozostaje dla zachowania danych, ale nie ogranicza katalogu ani sprzedaży.

Migracja uruchamia się automatycznie przy starcie backendu. Ponowny start nie przelicza cen kolejny raz. Przed wdrożeniem na innym serwerze wykonaj kopię bazy. Cofanie migracji po nowych sprzedażach usuwa przypisania nowych sprzedawców, więc wymaga wcześniejszej kopii.

## Uruchomienie

Uruchom `Uruchom-serwer.cmd`, a potem otwórz http://127.0.0.1:5180/ na serwerze. Na innych komputerach użyj adresu serwera w sieci lokalnej. Po aktualizacji odśwież stronę (Ctrl+F5) i zaloguj się ponownie.

Po zmianach frontendu: `npm --prefix frontend run build`, następnie w PowerShell `Copy-Item frontend/dist/* backend/AutoPartsSales.Api/wwwroot -Recurse -Force`. Po zmianach C# zatrzymaj ten backend, wykonaj `dotnet build backend/AutoPartsSales.Api` i uruchom go ponownie.

## Testy

- PowerShell 7: `./Test-netto.ps1` — logowanie, uprawnienia, walidacja, ceny, pięciu sprzedawców, wspólne części, filtry i raporty. Wymaga działającego serwera. Tworzy tymczasowe sprzedaże i usuwa wyłącznie je w `finally`.
- `node --experimental-strip-types frontend/test-dates.mjs` — zakresy kalendarzowe, granica roku, rok przestępny.
- W folderze frontend: `node --experimental-strip-types test-pdf.mjs` — PDF rzeczywisty, pusty i wielostronicowy w `tmp/pdfs`.

API: `GET /api/salespeople` (admin), `GET /api/sales/sellers` (admin), `GET /api/sales?seller=seller:1&carId=1&from=2026-09-01&to=2026-09-30`. `POST /api/sales` przyjmuje `priceType: "net" | "gross"`, `price` i (tylko admin) `salespersonId`. `price` w odpowiedziach API zawsze oznacza netto.
