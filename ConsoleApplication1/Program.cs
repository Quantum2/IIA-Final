using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IIA
{
    class Program
    {
        static void Main(string[] args)
        {
            string tipo;
            int[,] matriz;

            matriz = gerarMatriz();

            Console.Clear();
            Console.WriteLine("Escolha o tipo de algoritmo");
            Console.WriteLine("1 - Trepa Colinas");
            Console.WriteLine("2 - Algoritmo evolutivo");
            Console.WriteLine("3 - Método hibrido");
            tipo = Console.ReadLine();

            switch(Convert.ToInt32(tipo))
            {
                case 1:
                    Console.Clear();
                    Console.WriteLine("Escolhido Trepa Colinas...");
                    Algoritmo.TrepaColinas(matriz);
                    break;
                case 2:
                    Algoritmo.evo();
                    break;
                case 3:
                    Algoritmo.hib();
                    break;
                default:
                    Console.Clear();
                    Console.WriteLine("Input errado");
                    Console.ReadLine();
                    break;
            }
        }

        static int[,] gerarMatriz()
        {
            int dim_x, dim_y, contador = 4, contador2 = 0;
            string filename;

            Console.WriteLine("Escreva o nome do ficheiro de texto : ");
            filename = Console.ReadLine();
            Console.Clear();

            string[] lines = System.IO.File.ReadAllLines(filename);
            string[] temp = lines[0].Split(' ');
            dim_x = Convert.ToInt32(temp[0]);
            dim_y = Convert.ToInt32(temp[2]);

            Console.WriteLine("A dimensão X {0} e a Y {1}", dim_y, dim_x);

            int[,] matriz = new int[dim_x,dim_y];

            for (int s = 0; s < matriz.GetLength(0); s++)
            {
                string[] numeros_temp = lines[contador + 2].Split('\t');

                for (int i = 0; i < numeros_temp.Length - 1; i++)
                {
                    matriz[contador2, Convert.ToInt32(numeros_temp[i]) - 1] = 1;
                }

                contador2++;
                contador = contador + 4;
            }

            Console.WriteLine("A matriz é a seguinte : \n");

            for (int i = 0; i < matriz.GetLength(0); i++)
            {
                for (int j = 0; j < matriz.GetLength(1); j++)
                    Console.Write(" {0}", matriz[i, j].ToString());
                Console.WriteLine(" ");
            }

            Console.ReadLine();

            return matriz;
        }
    }
}
