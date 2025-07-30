using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StatniSvatky;

public class StatniSvatky
{
    private record StatniSvatek(int Month, int Day, string Description, bool OpenStores);

    private static readonly IReadOnlyList<StatniSvatek> SvatkyBezVelikonoc =
[
    new(1, 1,   "Nový rok, Den obnovy samostatného českého státu", false),
    new(5, 1,   "Svátek práce", true),
    new(5, 8,   "Den vítězství", false),
    new(7, 5,   "Den slovanských věrozvěstů Cyrila a Metoděje", true),
    new(7, 6,   "Den upálení mistra Jana Husa", true),
    new(9, 28,  "Den české státnosti", false),
    new(10, 28, "Den vzniku samostatného československého státu", false),
    new(11, 17, "Den boje za svobodu a demokracii", true),
    new(12, 24, "Štědrý den", true),
    new(12, 25, "1. svátek vánoční", false),
    new(12, 26, "2. svátek vánoční", false)
];

    private static DateTime GetEasterSunday(int year)
    {
        ValidateYear(year);

        // Algoritmus pro výpočet data Velikonoc podle Gaussova vzorce
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31; // 3 = March, 4 = April
        int day = ((h + l - 7 * m + 114) % 31) + 1;

        // Velikonoční neděle
        DateTime easterSunday = new(year, month, day);
        return easterSunday;
    }

    private static IEnumerable<DateTime> GetStatniSvatkyNaRokBezVelikonoc(int year)
    {
        IEnumerable<DateTime> svatky = [.. SvatkyBezVelikonoc.Select(s => new DateTime(year, s.Month, s.Day))];

        return svatky;
    }

    private static IEnumerable<DateTime> GetStatniSvatkyNaRokBezVelikonocZavrenoVObchodech(int year)
    {
        IEnumerable<DateTime> svatky = [.. SvatkyBezVelikonoc.Where(s => !s.OpenStores).Select(s => new DateTime(year, s.Month, s.Day))];

        return svatky;
    }

    private static void ValidateYear(int year)
    {
        if (year < 1583 || year > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Rok musí být v rozmezí 1583 - 9999.");
        }
    }

    /// <summary>
    /// Vrátí datum velikonočního pondělí pro zadaný rok.
    /// </summary>
    /// <param name="year">rok</param>
    /// <returns></returns>
    public static DateTime GetVelikonocniPondeli(int year)
    {
        ValidateYear(year);

        return GetEasterSunday(year).AddDays(1);
    }

    /// <summary>
    /// Vrátí datum velkého pátku pro zadaný rok.
    /// </summary>
    /// <param name="year">rok</param>
    /// <returns></returns>
    public static DateTime GetVelkyPatek(int year)
    {
        ValidateYear(year); 

        return GetEasterSunday(year).AddDays(-2);
    }
    

    /// <summary>
    /// Vrátí oba velikonoční svátky pro zadaný rok.
    /// </summary>
    /// <param name="year"></param>
    /// <returns>Tuple(DateTime Velký Patek,DateTime Velikonoční Pondělí)</returns>
    public static (DateTime VelkyPatek, DateTime VelikonocníPondeli) GetVelikonocniSvatky(int year)
    {
        ValidateYear(year);

        DateTime easterSunday = GetEasterSunday(year);
        DateTime velkyPatek = easterSunday.AddDays(-2);
        DateTime velikonocníPondeli = easterSunday.AddDays(1);

        return (velkyPatek, velikonocníPondeli);
    }

    /// <summary>
    /// Vrátí seznam všech státních svátků v daném roce.
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public static IEnumerable<DateTime> GetStatniSvatkyNaRok(int year)
    {
        ValidateYear(year);
        List<DateTime> svatky = [];
        DateTime velikonocniNedele = GetEasterSunday(year);

        svatky.Add(velikonocniNedele.AddDays(-2));
        svatky.Add(velikonocniNedele.AddDays(1));
        svatky.AddRange(GetStatniSvatkyNaRokBezVelikonoc(year));

        return svatky;
    }

    /// <summary>
    /// Vrátí seznam všech státních svátků v daném roce,
    /// ve kterých platí zákaz prodeje. (pro 24.12. vrací false)
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public static IEnumerable<DateTime> GetStatniSvatkyNaRokZavrenoVObchodech(int year)
    {
        ValidateYear(year);
        List<DateTime> svatky = [];
        DateTime velikonocniNedele = GetEasterSunday(year);

        svatky.Add(velikonocniNedele.AddDays(1));
        svatky.AddRange(GetStatniSvatkyNaRokBezVelikonocZavrenoVObchodech(year));

        return svatky;
    }

    /// <summary>
    /// Vrátí true pokud je zadáné datum státní svátek.
    /// </summary>
    /// <param name="datum"></param>
    /// <returns></returns>
    public static bool JeSvatek(DateTime datum)
    {
        int month = datum.Month;
        int day = datum.Day;
        int year = datum.Year;
        ValidateYear(year);

        if (SvatkyBezVelikonoc.Any(s => s.Day == day && s.Month == month))
        {
            return true;
        }
        DateTime velikonocniNedele = GetEasterSunday(year);
        if (datum == velikonocniNedele.AddDays(1) || datum == velikonocniNedele.AddDays(-2))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Vrátí true pokud je zadáné datum státní svátek, 
    /// ve kterém platí zákaz prodeje. (pro 24.12. vrací false)
    /// </summary>
    /// <param name="datum"></param>
    /// <returns></returns>
    public static bool JeZavrenoVObchodech(DateTime datum)
    {
        int month = datum.Month;
        int day = datum.Day;
        int year = datum.Year;
        ValidateYear(year);

        if (SvatkyBezVelikonoc.Any(s => s.Day == day && s.Month == month && !s.OpenStores))
        {
            return true;
        }
        DateTime velikonocniNedele = GetEasterSunday(year);
        if (datum == velikonocniNedele.AddDays(1))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Vrátí název svátku, pokud datum není svátek vrací null.
    /// </summary>
    /// <param name="datum"></param>
    /// <returns></returns>
    public static string? GetPopisSvatku(DateTime datum) 
    {
        ValidateYear(datum.Year);
        if (!JeSvatek(datum)) 
        { 
            return null;
        }

        string? description = SvatkyBezVelikonoc.First(s => s.Month == datum.Month && s.Day == datum.Day).Description;
        if (description != null) 
        {
            return description;
        }
        else
        {
            DateTime velikonocniNedele = GetEasterSunday(datum.Year);
            if (datum == velikonocniNedele.AddDays(1))
            {
                return "Velikonoční pondělí";
            }
            if (datum == velikonocniNedele.AddDays(-2))
            {
                return "Velký pátek";
            }
        }

        return null;
    }

    /// <summary>
    /// Vrátí seznam všech státních svátků v daném roce s podrobnostmi.
    /// (Datum, Popis, Otevřené obchody)
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    public static IEnumerable<(DateTime Date, string Description, bool OpenStores)> GetStatniSvatkyDetailne(int year)
    {
        ValidateYear(year);
        List<(DateTime Date, string Description, bool OpenStores)> svatky = [];
        DateTime velikonocniNedele = GetEasterSunday(year);

        svatky.Add((velikonocniNedele.AddDays(-2), "Velký pátek", false));
        svatky.Add((velikonocniNedele.AddDays(1), "Velikonoční pondělí", true));
        svatky.AddRange(GetStatniSvatkyNaRokBezVelikonoc(year).Select(d => (d, GetPopisSvatku(d) ?? "Neznámý svátek", SvatkyBezVelikonoc.First(s => s.Month == d.Month && s.Day == d.Day).OpenStores))
        );

        return svatky.OrderBy(s => s.Date);
    }
}
