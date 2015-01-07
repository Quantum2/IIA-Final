using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using btl.generic;
using GAF;
using GAF.Operators;

namespace IIA
{
    static class Algoritmo
    {
        static List<int> lista;
        static Random randy;
        public const int PROBABILIDADE = 50;

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

        internal static void evo(int[,] mat)
        {
            const double crossoverProbability = 0.65;
            const double mutationProbability = 0.08;
            const int elitismPercentage = 5;

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
            var elite = new Elite(elitismPercentage);

            var crossover = new Crossover(crossoverProbability, true)
            {
                CrossoverType = CrossoverType.SinglePoint
            };

            var mutation = new BinaryMutate(mutationProbability, true);

            var ga = new GeneticAlgorithm(population, CalculateFitness);

            //subscribe to the GAs Generation Complete event 
            ga.OnGenerationComplete += ga_OnGenerationComplete;

            //add the operators to the ga process pipeline 
            ga.Operators.Add(elite);
            ga.Operators.Add(crossover);
            ga.Operators.Add(mutation);

            //run the GA 
            ga.Run(TerminateFunction);

            Console.ReadLine();
        }

        private static double CalculateFitness(Chromosome chromosome)
        {
            double fitnessValue = -1;
            if (chromosome != null)
            {
                //this is a range constant that is used to keep the x/y range between -100 and +100
                var rangeConst = 200 / (System.Math.Pow(2, chromosome.Count / 2) - 1);

                //get x and y from the solution
                var x1 = Convert.ToInt32(chromosome.ToBinaryString(0, chromosome.Count / 2), 2);
                var y1 = Convert.ToInt32(chromosome.ToBinaryString(chromosome.Count / 2, chromosome.Count / 2), 2);

                //Adjust range to -100 to +100
                var x = (x1 * rangeConst) - 100;
                var y = (y1 * rangeConst) - 100;

                //using binary F6 for fitness.
                var temp1 = System.Math.Sin(System.Math.Sqrt(x * x + y * y));
                var temp2 = 1 + 0.001 * (x * x + y * y);
                var result = 0.5 + (temp1 * temp1 - 0.5) / (temp2 * temp2);

                fitnessValue = 1 - result;
            }
            else
            {
                //chromosome is null
                throw new ArgumentNullException("chromosome", "The specified Chromosome is null.");
            }

            return fitnessValue;
        }

        internal static bool TerminateFunction(Population population, int currentGeneration, long currentEvaluation) 
        {
            return currentGeneration > 1000;
        }

        private static void ga_OnGenerationComplete(object sender, GaEventArgs e)
        {
            //get the best solution 
            var chromosome = e.Population.GetTop(1)[0];

            //decode chromosome

            //get x and y from the solution 
            var x1 = Convert.ToInt32(chromosome.ToBinaryString(0, chromosome.Count / 2), 2);
            var y1 = Convert.ToInt32(chromosome.ToBinaryString(chromosome.Count / 2, chromosome.Count / 2), 2);

            //Adjust range to -100 to +100 
            var rangeConst = 200 / (System.Math.Pow(2, chromosome.Count / 2) - 1);
            var x = (x1 * rangeConst) - 100;
            var y = (y1 * rangeConst) - 100;

            //display the X, Y and fitness of the best chromosome in this generation 
            Console.WriteLine("x:{0} y:{1} Fitness{2}", x, y, e.Population.MaximumFitness);
        }

        /*internal static void evo(int[,] mat)
        {
            int k, contador = 0, contador2, escolhido = 0, fit1, fit2, its = 0;
            int[] melhor_actual;
            lista = new List<int>();
            melhor_actual = new int [3];
            randy = new Random();
            Console.Clear();
            Console.WriteLine("Insira o tempo de execução do algoritmo em segundos : ");
            k = Convert.ToInt32(Console.ReadLine());
            Console.Clear();
            DateTimeOffset start = DateTimeOffset.Now;
            do{
                int crossover_point = randy.Next(1, mat.GetLength(0));

                for (int s = 0; s < mat.GetLength(0); s++)              //Selecionador por torneio 
                {
                    for (int w = 0; w < mat.GetLength(1); w++) {
                        if(mat[s,w] == 0){
                            contador++;
                        }
                    }

                    if(contador > melhor_actual[1]){
                        melhor_actual[0] = s;
                        melhor_actual[1] = contador;
                    }
                    contador = 0;
                }

                do{
                    if(contador >= mat.GetLength(0)){
                        contador = 0;
                    }
                    int r = randy.Next(0,100);
                    if(r <= PROBABILIDADE){
                        melhor_actual[2] = contador;
                        escolhido = 1;
                    }
                    contador++;
                }while(escolhido == 0);

                for (int a = 0; a <= crossover_point; a++)
                {
                    int temp;
                    temp = mat[melhor_actual[0], a];
                    mat[melhor_actual[0],a] = mat[melhor_actual[2],a];
                    mat[melhor_actual[2],a] = temp;
                }

                mat = inverterBit(mat, melhor_actual);
                contador = 0;
                contador2 = 0;

                for(int q=0;q < mat.GetLength(1);q++){
                    if(mat[melhor_actual[0], q] == 0){
                        contador = contador + 1;
                    }
                    if(mat[melhor_actual[2], q] == 0){
                        contador2 = contador2 + 1;
                    }
                }

                fit1 = contador;
                fit2 = contador2;

                if(fit1 > fit2){
                    for (int q = 0; q < mat.GetLength(0); q++)
                    {
                        mat[melhor_actual[2], q] = mat[melhor_actual[0], q];
                        Console.Write("{0}", mat[melhor_actual[2], q]);
                    }
                    Console.WriteLine(" ");
                }
                if (fit2 > fit1)
                {
                    for (int q = 0; q < mat.GetLength(0); q++)
                    {
                        mat[melhor_actual[0], q] = mat[melhor_actual[2], q];
                        Console.Write("{0}", mat[melhor_actual[0], q]);
                    }
                    Console.WriteLine(" ");
                }
                its++;
            } while (DateTimeOffset.Now - start < TimeSpan.FromSeconds(k));

            Console.WriteLine("Número de iterações : {0} p/s", its);
            Console.ReadLine();
        }*/

        private static int[,] inverterBit(int[,] mat, int[] melhor)
        {
            randy = new Random();
            
            if(mat[melhor[0],randy.Next(0,mat.GetLength(1))] == 1){
                mat[melhor[0], randy.Next(0, mat.GetLength(1))] = 0;
            }
            else
            {
                mat[melhor[0], randy.Next(0, mat.GetLength(1))] = 1;
            }

            if (mat[melhor[2], randy.Next(0, mat.GetLength(1))] == 1)
            {
                mat[melhor[2], randy.Next(0, mat.GetLength(1))] = 0;
            }
            else
            {
                mat[melhor[2], randy.Next(0, mat.GetLength(1))] = 1;
            }

            return mat;
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
