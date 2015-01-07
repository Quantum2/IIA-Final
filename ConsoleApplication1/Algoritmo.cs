﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IIA
{
    static class Algoritmo
    {
        static List<int> lista;
        static Random randy;

        public static void TrepaColinas(int[,] mat)
        {
            int tempo;
            fazerFicheiroLog(mat);
            lista = new List<int>();
            randy = new Random();
            Console.Write("Insira o tempo de execução do algoritmo em segundos : ");
            tempo = Convert.ToInt32(Console.ReadLine());
            Console.Clear();
            Console.WriteLine("Solução obtida: ");
            BitArray resultado = new BitArray(TCRunner(mat, tempo).Result);
            Console.WriteLine("Número de iterações: {0} p/s", lista.Count/tempo);
            Console.ReadLine();
        }

        private static void fazerFicheiroLog(int[,] mat)
        {
            string path = "log.txt";

            File.Delete(path);

            System.IO.StreamWriter file = new System.IO.StreamWriter(path);

            for (int i = 0; i < mat.GetLength(0); i++)
            {
                for (int j = 0; j < mat.GetLength(1); j++)
                {
                    file.Write(mat[i, j]);
                    file.Write(' ');
                }
                file.Write('\n');
            }

            file.Close();
        }

        public static void PrintValues(IEnumerable myList, int myWidth)
        {
            int i = myWidth;
            foreach (Object obj in myList)
            {
                if (i <= 0)
                {
                    i = myWidth;
                    Console.WriteLine();
                }
                i--;
                Console.Write("{0,8}", obj);
            }
            Console.WriteLine();
        }

        static async Task<BitArray> TCRunner(int[,] mat, int tempo)
        {
            var search = new HillClimber(() => RandomBitArray(mat), Neighbours, Fitness);
            var optimized = await search.Optimize(TimeSpan.FromSeconds(tempo));

            return optimized;
        }

        public static BitArray RandomBitArray(int[,] mat)
        {
            BitArray b = new BitArray(mat.GetLength(0), false);
            int rand = randy.Next(0, mat.GetLength(1));
            lista.Add(rand);

            for (int i = 0; i < mat.GetLength(0); i++)
                b[i] = Convert.ToBoolean(mat[i, rand]);

            return b;
        }

        public static IEnumerable<BitArray> Neighbours(BitArray barray)
        {
            string[] lines = System.IO.File.ReadAllLines("log.txt");
            int indice = randy.Next(0, lines.Length);
            string[] temp = lines[indice].Split(' ');
            lines = lines.Where((source, index) => index != indice).ToArray();
            lista.Add(indice);

            for (int w = 0; w < barray.Length; w++)
            {
                var n = new BitArray(barray);
                n[w] = Convert.ToBoolean(Convert.ToInt32(temp[w]));

                yield return n;
            }
        }

        public static IEnumerable<bool> ToEnumerable(this BitArray barray)
        {
            for (int i = 0; i < barray.Length; i++)
            {
                yield return barray[i];
            }
        }

        public static int Fitness(BitArray barray)
        {
            return barray.ToEnumerable().Count(b => b);
        }

        internal static void evo()
        {
            throw new NotImplementedException();
        }

        internal static void hib()
        {
            throw new NotImplementedException();
        }
    }

    public class HillClimber
    {
        public Func<BitArray> RandomSolution { get; set; }
        public Func<BitArray, IEnumerable<BitArray>> Neighbours { get; set; }
        public Func<BitArray, int> Fitness { get; set; }

        public HillClimber(Func<BitArray> randomSolution, Func<BitArray, IEnumerable<BitArray>> neighbours, Func<BitArray, int> fitness)
        {
            this.RandomSolution = randomSolution;
            this.Neighbours = neighbours;
            this.Fitness = fitness;
        }

        public async Task<BitArray> Optimize(TimeSpan timeout)
        {
            DateTimeOffset start = DateTimeOffset.Now;
            BitArray current = this.RandomSolution();
            int currentFitness = this.Fitness(current);

            return await Task.Factory.StartNew(() =>
            {
                do
                {
                    BitArray steepestAscentNeighbour = default(BitArray);
                    int steepestAscentFitness = default(int);

                    foreach (var n in this.Neighbours(current))
                    {
                        steepestAscentFitness = this.Fitness(n);
                        if (Comparer.Default.Compare(currentFitness, steepestAscentFitness) > 0)
                        {
                            current = n;
                            currentFitness = steepestAscentFitness;
                            foreach (bool bit in current)
                            {
                                if(bit == true)
                                    Console.Write("1");
                                if (bit == false)
                                    Console.Write("0");
                            }
                            Console.WriteLine(' ');
                        }
                    }

                    if (steepestAscentNeighbour != null && Comparer.Default.Compare(steepestAscentFitness, currentFitness) >= 0)
                        current = steepestAscentNeighbour;

                } while (DateTimeOffset.Now - start < timeout);

                return current;
            });
        }
    }
}
