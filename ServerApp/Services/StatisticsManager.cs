using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerApp.Services
{
    public static class StatisticsManager
    {
        private static readonly Dictionary<string, List<(int duzina, double vremeDekripcije)>> statistika =
            new Dictionary<string, List<(int, double)>>()
            {
                { "DES", new List<(int, double)>() },
                { "RSA", new List<(int, double)>() }
            };

        private static readonly object lockObj = new object();

        public static void DodajStatistiku(string algoritam, int duzinaPoruke, double vremeMs)
        {
            lock (lockObj)
            {
                if (!statistika.ContainsKey(algoritam))
                {
                    statistika[algoritam] = new List<(int, double)>();
                }

                statistika[algoritam].Add((duzinaPoruke, vremeMs));
            }
        }
        public static void PrikaziStatistiku()
        {
            Console.WriteLine("\n==================== STATISTIKA DEKRIPCIJE ====================");

            foreach (var algoritam in statistika.Keys)
            {
                Console.WriteLine($"\n>> Algoritam: {algoritam}");

                var grupisano = statistika[algoritam]
                    .GroupBy(x => x.duzina)
                    .OrderBy(g => g.Key);

                foreach (var grupa in grupisano)
                {
                    double prosek = grupa.Average(x => x.vremeDekripcije);
                    Console.WriteLine($"   Poruke dužine {grupa.Key} karaktera: prosečno {prosek:F2} ms");
                }
            }

            Console.WriteLine("\n===============================================================\n");
        }
    }
}
