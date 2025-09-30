using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinecraftVesnice
{
    enum Role
    {
        Delnik,
        Stavitel,
        Bojovnik,
        Kouzelnik
    }

    abstract class Obyvatel
    {
        public string Jmeno { get; set; }
        public int Level { get; set; }
        public int Zivoty { get; set; }
        public Role Role { get; set; }

        public Obyvatel() { } // Potřebné pro JSON

        public Obyvatel(string jmeno, int level, int zivoty, Role role)
        {
            Jmeno = jmeno;
            Level = level;
            Zivoty = zivoty;
            Role = role;
        }

        public abstract void Pracuj();
        public virtual int VypocitejOdmenu() => 5 + (Level * 2);
        public virtual int VypocitejOdmenu(int minimum) => VypocitejOdmenu() < minimum ? minimum : VypocitejOdmenu();
        public abstract int Bonus { get; }
        public static int operator +(Obyvatel a, Obyvatel b) => a.Bonus + b.Bonus;
        public override string ToString() => $"{Jmeno}, Role: {Role}, Level: {Level}, Životy: {Zivoty}";
    }

    class Delnik : Obyvatel
    {
        public Delnik() { }
        public Delnik(string jmeno, int level, int zivoty) : base(jmeno, level, zivoty, Role.Delnik) { }
        public override void Pracuj() => Console.WriteLine($"{Jmeno} staví bloky...");
        public override int VypocitejOdmenu() => base.VypocitejOdmenu() + (int)(base.VypocitejOdmenu() * 0.05);
        public override int Bonus => (int)(base.VypocitejOdmenu() * 0.05);
    }

    class Stavitel : Obyvatel
    {
        public int Domy { get; set; }
        public Stavitel() { }
        public Stavitel(string jmeno, int level, int zivoty, int domy) : base(jmeno, level, zivoty, Role.Stavitel)
        {
            Domy = domy;
        }
        public override void Pracuj() => Console.WriteLine($"{Jmeno} řídí stavby...");
        public override int VypocitejOdmenu()
        {
            int odmena = base.VypocitejOdmenu() + (Domy * 3);
            if (Domy > 10) odmena = (int)(odmena * 1.2);
            return odmena;
        }
        public override int Bonus => Domy > 10 ? (int)(Domy * 3 * 1.2) : Domy * 3;
    }

    class Bojovnik : Obyvatel
    {
        public int ZabitiMobove { get; set; }
        public int ZabitiBossove { get; set; }

        public Bojovnik() { }
        public Bojovnik(string jmeno, int level, int zivoty, int zabitiMobove, int zabitiBossove = 0)
            : base(jmeno, level, zivoty, Role.Bojovnik)
        {
            ZabitiMobove = zabitiMobove;
            ZabitiBossove = zabitiBossove;
        }
        public override void Pracuj() => Console.WriteLine($"{Jmeno} bojuje s nepřáteli...");
        public override int VypocitejOdmenu() => base.VypocitejOdmenu() + (int)(base.VypocitejOdmenu() * 0.10) + (2 * ZabitiMobove) + (50 * ZabitiBossove);
        public override int Bonus => (int)(base.VypocitejOdmenu() * 0.10) + (2 * ZabitiMobove) + (50 * ZabitiBossove);
    }

    class Kouzelnik : Obyvatel
    {
        public Kouzelnik() { }
        public Kouzelnik(string jmeno, int level, int zivoty) : base(jmeno, level, zivoty, Role.Kouzelnik) { }
        public override void Pracuj() => Console.WriteLine($"{Jmeno} vaří lektvary...");
        public override int VypocitejOdmenu() => base.VypocitejOdmenu() + (int)(base.VypocitejOdmenu() * 0.12);
        public override int Bonus => (int)(base.VypocitejOdmenu() * 0.12);
    }

    class DuplicateNameException : Exception
    {
        public DuplicateNameException(string message) : base(message) { }
    }

    class SpravceVesnice
    {
        public List<Obyvatel> Obyvatele { get; set; } = new List<Obyvatel>();

        public void Pridej(Obyvatel o)
        {
            if (Obyvatele.Exists(x => x.Jmeno == o.Jmeno))
                throw new DuplicateNameException($"Obyvatel s jménem {o.Jmeno} již existuje!");
            Obyvatele.Add(o);
        }

        public Obyvatel NajdiPodleJmena(string jmeno)
        {
            var o = Obyvatele.Find(x => x.Jmeno == jmeno);
            if (o == null) Console.WriteLine($"Obyvatel {jmeno} neexistuje.");
            return o;
        }

        public void SeradPodleLevelu(bool vzestupne = true)
        {
            Obyvatele.Sort((a, b) => vzestupne ? a.Level.CompareTo(b.Level) : b.Level.CompareTo(a.Level));
        }

        public int CelkoveSmaragdy() => Obyvatele.Sum(o => o.VypocitejOdmenu());

        public void OdeberPodleJmena(string jmeno)
        {
            var o = Obyvatele.Find(x => x.Jmeno == jmeno);
            if (o != null) Obyvatele.Remove(o);
            else Console.WriteLine($"Obyvatel {jmeno} neexistuje.");
        }

        public void Top3Odmeny()
        {
            var top3 = Obyvatele.OrderByDescending(o => o.VypocitejOdmenu()).Take(3);
            Console.WriteLine("\n=== Top 3 obyvatelé podle odměn ===");
            foreach (var o in top3) Console.WriteLine($"{o.Jmeno} - {o.VypocitejOdmenu()} smaragdů");
        }

        public void VypisBarevne()
        {
            foreach (var o in Obyvatele)
            {
                switch (o.Role)
                {
                    case Role.Bojovnik: Console.ForegroundColor = ConsoleColor.Red; break;
                    case Role.Kouzelnik: Console.ForegroundColor = ConsoleColor.Blue; break;
                    case Role.Stavitel: Console.ForegroundColor = ConsoleColor.Green; break;
                    case Role.Delnik: Console.ForegroundColor = ConsoleColor.Yellow; break;
                }
                Console.WriteLine(o);
            }
            Console.ResetColor();
        }

        public void UlozJSON(string cesta)
        {
            try
            {
                var json = JsonSerializer.Serialize(Obyvatele, new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } });
                File.WriteAllText(cesta, json);
                Console.WriteLine("Data byla uložena.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při ukládání: {ex.Message}");
            }
        }

        public void NactiJSON(string cesta)
        {
            try
            {
                if (!File.Exists(cesta)) { Console.WriteLine("Soubor neexistuje."); return; }
                var json = File.ReadAllText(cesta);
                var data = JsonSerializer.Deserialize<List<JsonElement>>(json);

                Obyvatele.Clear();
                foreach (var item in data)
                {
                    Role role = Enum.Parse<Role>(item.GetProperty("Role").GetString());
                    string jmeno = item.GetProperty("Jmeno").GetString();
                    int level = item.GetProperty("Level").GetInt32();
                    int zivoty = item.GetProperty("Zivoty").GetInt32();

                    switch (role)
                    {
                        case Role.Delnik:
                            Obyvatele.Add(new Delnik(jmeno, level, zivoty));
                            break;
                        case Role.Stavitel:
                            int domy = item.TryGetProperty("Domy", out JsonElement d) ? d.GetInt32() : 0;
                            Obyvatele.Add(new Stavitel(jmeno, level, zivoty, domy));
                            break;
                        case Role.Bojovnik:
                            int zabitiMobove = item.TryGetProperty("ZabitiMobove", out JsonElement zm) ? zm.GetInt32() : 0;
                            int zabitiBossove = item.TryGetProperty("ZabitiBossove", out JsonElement zb) ? zb.GetInt32() : 0;
                            Obyvatele.Add(new Bojovnik(jmeno, level, zivoty, zabitiMobove, zabitiBossove));
                            break;
                        case Role.Kouzelnik:
                            Obyvatele.Add(new Kouzelnik(jmeno, level, zivoty));
                            break;
                    }
                }

                Console.WriteLine("Data byla načtena.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při načítání: {ex.Message}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Vesnice Minecraft ===");
            SpravceVesnice vesnice = new SpravceVesnice();

            try
            {
                vesnice.Pridej(new Bojovnik("Martin", 12, 20, 5, 1));
                vesnice.Pridej(new Bojovnik("Lukas", 20, 30, 10, 2));
                vesnice.Pridej(new Kouzelnik("Pedro", 25, 20));
                vesnice.Pridej(new Stavitel("Johny", 14, 20, 3));
                vesnice.Pridej(new Stavitel("Sonya", 18, 25, 12));
                vesnice.Pridej(new Delnik("Vojgrc", 10, 15));
            }
            catch (DuplicateNameException ex)
            {
                Console.WriteLine(ex.Message);
            }

            vesnice.VypisBarevne();

            Console.WriteLine("\n=== Pracují ===");
            foreach (var o in vesnice.Obyvatele) o.Pracuj();

            Console.WriteLine("\n=== Odměny ===");
            foreach (var o in vesnice.Obyvatele) Console.WriteLine($"{o.Jmeno}: {o.VypocitejOdmenu()} smaragdů");

            Console.WriteLine("\n=== Součet bonusů Johny + Pedro ===");
            var johny = vesnice.NajdiPodleJmena("Johny");
            var pedro = vesnice.NajdiPodleJmena("Pedro");
            if (johny != null && pedro != null) Console.WriteLine($"{johny.Jmeno} + {pedro.Jmeno} = {johny + pedro} smaragdů");

            Console.WriteLine($"\nCelkem ve vesnici: {vesnice.CelkoveSmaragdy()} smaragdů");

            vesnice.Top3Odmeny();

            string cesta = "vesnice.json";
            vesnice.UlozJSON(cesta);
            vesnice.NactiJSON(cesta);
        }
    }
}
