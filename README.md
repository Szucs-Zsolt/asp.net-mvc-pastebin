# asp.net-mvc-pastebin
Alapelvek: ASP.NET Core 6.0, MVC, EntityFramework, Identity, CRUD, Git

## Pastebinhez hasonló weboldal megvalósítása. 
   - Adminisztrátor törölhet, szerkeszthet felhasználókat, admin jogokat adhat nekik
   - Felhasználó beregisztrálhat, ekkor joga van üzenetet írni.
   - Bejelentkezett felhasználó a saját üzeneteit szerkesztheti, törölheti. (CRUD megvalósítása)
   - Ha nincs bejelentkezve, csak olvasni tudja azt az üzenetet, aminek tudja az azonosítóját
   - Az üzenetek (scriptek, stb.) első olvasáskor / 1, 4, 8 óra, 1 nap, hét, hónap múlva törlődnek.
   - Rendszeres időközönként törli az adatbázisból a már lejárt üzeneteket.

