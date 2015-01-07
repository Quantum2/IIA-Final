using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GAF;
using GAF.Operators;

namespace IIA
{
    static class Algoritmo
    {
        static List<int> lista;
        static Random randy;
        public const int PROBABILIDADE = 50;
        static int its;

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
            Console.Clear();
            Program.Main();
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

        internal static void evo(int[,] mat)
        {
            const double crossoverProbability = 0.50;
            const double mutationProbability = 0.08;

            Console.WriteLine("\nInsira o número de iterações desejadas :");
            its = Convert.ToInt32(Console.ReadLine());
            Console.SetWindowSize(115, Console.WindowHeight);

            var population = new Population(0, mat.GetLength(1), false, false);

            for (var p = 0; p < mat.GetLength(0); p++)
            {
                var chromosome = new Chromosome();
                for (var g = 0; g < mat.GetLength(1); g++)
                {
                    chromosome.Genes.Add(new Gene(mat[p,g]));
                }
                population.Solutions.Add(chromosome);
            }

            //create the genetic operators 
            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            var ga = new GeneticAlgorithm(population, CalculateFitness);

            //subscribe to the GAs Generation Complete event 
            ga.OnGenerationComplete += ga_OnGenerationComplete;

            //add the operators to the ga process pipeline 
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            //run the GA 
            ga.Run(TerminateFunction);

            Console.ReadLine();
            Console.Clear();
            Program.Main();
        }

        private static double CalculateFitness(Chromosome chromosome)
        {
            double fitnessValue = -1;
            if (chromosome != null)
            {
                double x = 0;
                List<Gene> genes = new List<Gene>(chromosome.Genes);
                for (int i = 0; i < genes.Count; i++)
                {
                    if(genes[i].BinaryValue == 1){
                        x++;
                    }
                }

                double result = x / chromosome.Count;

                fitnessValue = 1 - result;
            }
            else
            {
                //chromosome is null
                throw new ArgumentNullException("chromosome", "O cromossoma especificado é nulo");
            }

            return fitnessValue;
        }

        internal static bool TerminateFunction(Population population, int currentGeneration, long currentEvaluation) 
        {
            return currentGeneration > its - 2;
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            //get the best solution 
            var chromosome = e.Population.GetTop(1)[0];

            //decode chromosome
            List<Gene> genes = new List<Gene>(chromosome.Genes);

            for (int i = 0; i < genes.Count;  i++) {
                Console.Write("{0}", genes[i].BinaryValue);
            }

            Console.WriteLine(" ");

            //display the X, Y and fitness of the best chromosome in this generation 
            Console.WriteLine("  - Fitness : {0}", e.Population.MaximumFitness);
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
