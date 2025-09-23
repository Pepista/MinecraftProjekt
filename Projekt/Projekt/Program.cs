using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MinecraftVesnice
{
    
    abstract class Obyvatel
    {
        public string Jmeno { get; set; }
        public int Level { get; set; }
        public int Zivoty { get; set; }

        public Obyvatel(string jmeno, int level, int zivoty)
        {
            Jmeno = jmeno;
            Level = level;
            Zivoty = zivoty;
        }

        public abstract void Pracuj();

        
        public virtual int VypocitejOdmenu()
        {
            return 5 + (Level * 2);
        }

        public virtual int VypocitejOdmenu(int minimum)
        {
            int odmena = VypocitejOdmenu();
            return odmena < minimum ? minimum : odmena;
        }

        
        public abstract int Bonus { get; }

        public static int operator +(Obyvatel a, Obyvatel b)
        {
            return a.Bonus + b.Bonus;
        }

        public override string ToString()
        {
            return $"{Jmeno}, Level: {Level}, Životy: {Zivoty}";
        }
    }

    
    class Delnik : Obyvatel
    {
        public Delnik(string jmeno, int level, int zivoty) : base(jmeno, level, zivoty) { }

        public override void Pracuj() => Console.WriteLine($"{Jmeno} staví bloky...");
        public override int VypocitejOdmenu() => base.VypocitejOdmenu() + (int)(base.VypocitejOdmenu() * 0.05);
        public override int Bonus => (int)(base.VypocitejOdmenu() * 0.05);
    }

    
    class Bojovnik : Obyvatel
    {
        public int ZabitiMobove { get; set; }

        public Bojovnik(string jmeno, int level, int zivoty, int zabitiMobove) : base(jmeno, level, zivoty)
        {
            ZabitiMobove = zabitiMobove;
        }

        public override void Pracuj() => Console.WriteLine($"{Jmeno} bojuje s kostlivci...");
        public override int VypocitejOdmenu() => base.VypocitejOdmenu() + (int)(base.VypocitejOdmenu() * 0.10) + (2 * ZabitiMobove);
        public override int Bonus => (int)(base.VypocitejOdmenu() * 0.10) + (2 * ZabitiMobove);
    }

    
    class Kouzelnik : Obyvatel
    {
        public Kouzelnik(string jmeno, int level, int zivoty) : base(jmeno, level, zivoty) { }

        public override void Pracuj() => Console.WriteLine($"{Jmeno} vaří lektvary...");
        public override int VypocitejOdmenu() => base.VypocitejOdmenu() + (int)(base.VypocitejOdmenu() * 0.12);
        public override int Bonus => (int)(base.VypocitejOdmenu() * 0.12);
    }

    
    class Stavitel : Obyvatel
    {
        public int Domy { get; set; }

        public Stavitel(string jmeno, int level, int zivoty, int domy) : base(jmeno, level, zivoty)
        {
            Domy = domy;
        }

        public override void Pracuj() => Console.WriteLine($"{Jmeno} řídí stavby...");
        public override int VypocitejOdmenu() => base.VypocitejOdmenu() + (Domy * 3);
        public override int Bonus => Domy * 3;
    }

    
    class SpravceVesnice
    {
        public List<Obyvatel> Obyvatele { get; set; } = new List<Obyvatel>();

        public void Pridej(Obyvatel o) => Obyvatele.Add(o);

        public void Vypis()
        {
            foreach (var o in Obyvatele)
                Console.WriteLine(o);
        }

        public void UlozJSON(string cesta)
        {
            var json = JsonSerializer.Serialize(Obyvatele, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(cesta, json);
        }

        public void NactiJSON(string cesta)
        {
            if (File.Exists(cesta))
            {
                var json = File.ReadAllText(cesta);
                Obyvatele = JsonSerializer.Deserialize<List<Obyvatel>>(json);
            }
        }

        public Obyvatel NajdiPodleJmena(string jmeno)
        {
            return Obyvatele.Find(o => o.Jmeno == jmeno);
        }

        public int CelkoveSmaragdy()
        {
            int suma = 0;
            foreach (var o in Obyvatele)
                suma += o.VypocitejOdmenu();
            return suma;
        }
    }

    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Vesnice Minecraft ===");

            SpravceVesnice vesnice = new SpravceVesnice();

            var martin = new Bojovnik("Martin", 12, 20, 5);
            var pedro = new Kouzelnik("Pedro", 25, 20);
            var johny = new Stavitel("Johny", 14, 20, 3);

            vesnice.Pridej(martin);
            vesnice.Pridej(pedro);
            vesnice.Pridej(johny);

            vesnice.Vypis();

            Console.WriteLine("\n=== Pracují ===");
            foreach (var o in vesnice.Obyvatele)
                o.Pracuj();

            Console.WriteLine("\n=== Odměny ===");
            foreach (var o in vesnice.Obyvatele)
                Console.WriteLine($"{o.Jmeno}: {o.VypocitejOdmenu()} smaragdů");

            Console.WriteLine("\n=== Součet bonusů ===");
            Console.WriteLine($"Johny + Pedro = {johny + pedro} smaragdů");

            Console.WriteLine($"\nCelkem ve vesnici: {vesnice.CelkoveSmaragdy()} smaragdů");
        }
    }
}
