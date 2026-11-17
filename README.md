# Aplikacja sprzedaży części samochodowych

System do rejestrowania sprzedaży części, przeglądania historii i tworzenia raportów.

## Funkcje

- Rejestracja i logowanie pracowników z tokenem JWT.
- Dodawanie sprzedaży oraz przeglądanie historii.
- Katalog samochodów i części oraz raporty dla administratora.
- Hasła przechowywane przez ASP.NET Identity w postaci bezpiecznych skrótów.

## Konfiguracja

Ustaw prywatne wartości `ConnectionStrings__DefaultConnection` oraz `Jwt__Key` jako zmienne środowiskowe lub sekrety .NET User Secrets. Klucz JWT powinien być losowy i mieć co najmniej 32 znaki. Do utworzenia konta administratora ustaw prywatne zmienne `InitialAdmin__UserName` i `InitialAdmin__Password`. Nie zapisuj prawdziwych haseł ani klucza JWT w plikach śledzonych przez Git.

Frontend używa Vite na porcie 5173 i przekazuje żądania `/api` do API na porcie 5180. Szczegóły ustawień lokalnych znajdują się w `backend/AutoPartsSales.Api/appsettings.example.json`.