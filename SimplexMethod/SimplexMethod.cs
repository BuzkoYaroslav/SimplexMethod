using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using library;
using System.IO;

namespace SimplexMethod
{
    class SimplexTable
    {
        List<int> basisIndexes;
        double[] coefs;
        double[] aZero;

        Vector[] allVectors;
        double[,] decomposition;

        public int BasisLength
        {
            get { return basisIndexes.Count; }
        }
        public int VarsCount { get { return coefs.Length; } }

        public double Function { get { return GetFunction(); } }
        public Tuple<int, double> this[int i] { get { return new Tuple<int, double>(basisIndexes[i], aZero[i]); } }

        public SimplexTable(Vector[] vectors, int[] basisIndexes, double[] coefs, Vector conditions) 
        {
            allVectors = new Vector[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
                allVectors[i] = new Vector((double[])vectors[i]);

            this.basisIndexes = new List<int>();
            for (int i = 0; i < basisIndexes.Length; i++)
                this.basisIndexes.Add(basisIndexes[i]);
            this.basisIndexes.Sort();

            this.coefs = new double[coefs.Length];
            for (int i = 0; i < coefs.Length; i++)
                this.coefs[i] = coefs[i];

            CalculateDecomposition();
            aZero = (double[])new GaussMethod().Solve(new SLAE(BasisMatrix(), conditions));
        }

        private Matrix BasisMatrix()
        {
            double[,] B = new double[BasisLength, BasisLength];

            for (int i = 0; i < BasisLength; i++)
            {
                Vector basisVect = allVectors[basisIndexes[i]];

                for (int j = 0; j < BasisLength; j++)
                    B[j, i] = basisVect[j];
            }

            return B;
        }
        private double GetDelta(int index)
        {
            double result = 0;

            for (int i = 0; i < BasisLength; i++)
                result += coefs[basisIndexes[i]] * decomposition[i, index];

            result -= coefs[index];

            return result;
        }
        private double GetFunction()
        {
            double result = 0;

            for (int i = 0; i < BasisLength; i++)
                result += coefs[basisIndexes[i]] * aZero[i];

            return result;
        }
        private void CalculateDecomposition()
        {
            Matrix BInversed = BasisMatrix().Inverse();

            decomposition = new double[BasisLength, VarsCount];

            for (int i = 0; i < VarsCount; i++)
            {
                int index = basisIndexes.IndexOf(i);

                if (index != -1)
                {
                    decomposition[index, i] = 1;
                    continue;
                }

                Vector vect = BInversed * allVectors[i];
                for (int j = 0; j < BasisLength; j++)
                    decomposition[j, i] = vect[j];
            }
        }

        public override string ToString()
        {
            string result = "";

            int length = 6 * VarsCount + 19;
            string gap = "Simplex Table";

            result += string.Format("{0}\n*{2}{1}*\n{0}\n", "".PadRight(length, '*'), gap, "".PadRight(length - 2 - gap.Length, ' '));
            result += string.Format("*{0, 2}*{1, 2}*{2, 5}*{3, 5}*", "#", "B", "CB", "A0");

            for (int i = 0; i < VarsCount - 1; i++)
                result += string.Format("{0, 5}*", i);
            result += string.Format("{0, 5}*\n", VarsCount - 1);
            result += string.Format("{0}\n", "".PadRight(length, '*'));

            for (int i = 0; i < BasisLength; i++)
            {
                result += string.Format("|{0, 2}|{1, 2}|{2, 5}|{3, 5}|",
                    i, basisIndexes[i], coefs[basisIndexes[i]], Math.Round(aZero[i], 2));

                for (int j = 0; j < VarsCount - 1; j++)
                    result += string.Format("{0, 5}|", Math.Round(decomposition[i, j], 2));
                result += string.Format("{0, 5}|\n", Math.Round(decomposition[i, VarsCount - 1], 2));
                result += string.Format("{0}\n", "".PadRight(length, '-'));
            }

            result += string.Format("|{0, 2}|{0, 2}|{0, 5}|{1, 5}|",
                    "", Math.Round(GetFunction(), 2));

            for (int j = 0; j < VarsCount - 1; j++)
                result += string.Format("{0, 5}|", Math.Round(GetDelta(j)));
            result += string.Format("{0, 5}|\n",Math.Round(GetDelta(VarsCount - 1)));
            result += string.Format("{0}\n", "".PadRight(length, '-'));

            return result;
        }

        public Tuple<int, int> ExchangeVectors(bool maximum)
        {
            int i = 0;
            int maxI = -1;
            double maxDelta = double.NegativeInfinity;

            while (i < VarsCount)
            {
                double delta = GetDelta(i);
                if ((delta < 0 && maximum ||
                    delta > 0 && !maximum) &&
                    Math.Abs(delta) > maxDelta)
               {
                    maxDelta = Math.Abs(delta);
                    maxI = i;
               }

                i++;
            }

            if (maxI == -1)
                return null;

            double minValue = double.PositiveInfinity;
            int minIndex = -1;

            for (int j = 0; j < BasisLength; j++)
            {
                double val = aZero[j] / decomposition[j, maxI];
                if (val < minValue && val > 0)
                {
                    minValue = val;
                    minIndex = j;
                }
            }

            return new Tuple<int, int>(maxI, basisIndexes[minIndex]);
        }
    }

    class SimplexMethod
    {
        private const int operationThreshold = 10;

        private bool maximumOptimum;
        private double[] coefs;
        private int[] startBasis;
        Vector[] allVectors;
        Vector condition;

        List<SimplexTable> tables;

        public SimplexMethod(double[,] conditionsMatrix, double[] coefs, int[] basisIndexes, bool maximum = true)
        {
            allVectors = new Vector[coefs.Length];
            maximumOptimum = maximum;

            for (int i = 0; i < allVectors.Length; i++)
            {
                allVectors[i] = new double[conditionsMatrix.GetLength(0)];
                for (int j = 0; j < allVectors[i].Count; j++)
                    allVectors[i][j] = conditionsMatrix[j, i];
            }

            condition = new double[conditionsMatrix.GetLength(0)];
            for (int j = 0; j < condition.Count; j++)
                condition[j] = conditionsMatrix[j, conditionsMatrix.GetLength(1) - 1];

            this.coefs = new double[coefs.Length];
            for (int i = 0; i < coefs.Length; i++)
                this.coefs[i] = coefs[i];

            startBasis = new int[basisIndexes.Length];
            for (int i = 0; i < basisIndexes.Length; i++)
                startBasis[i] = basisIndexes[i];
        }
        

        public void Solve(string fileName)
        {
            tables = new List<SimplexTable>();

            List<int> basis = new List<int>();
            foreach (int vect in startBasis)
                basis.Add(vect);

            int index = 0;
            Tuple<int, int> exchange;

            SimplexTable table = new SimplexTable(allVectors, basis.ToArray(), coefs, condition);
            tables.Add(table);

            while ((exchange = table.ExchangeVectors(maximumOptimum)) != null
                && index < operationThreshold)
            {
                int i = basis.IndexOf(exchange.Item2);
                basis[i] = exchange.Item1;

                table = new SimplexTable(allVectors, basis.ToArray(), coefs, condition);
                tables.Add(table);

                index++;
            }

            WriteResults(fileName);
        }
        private void WriteResults(string fileName)
        {
            StreamWriter wrt = new StreamWriter(fileName);

            wrt.Write(OptimumTaskDescription());

            foreach (SimplexTable table in tables)
            {
                wrt.Write(table.ToString());

                Tuple<int, int> exchange = table.ExchangeVectors(maximumOptimum);
                if (exchange != null)
                {
                    wrt.WriteLine("Вводим вектор {0}. Выводим вектор {1}.", exchange.Item1, exchange.Item2);
                } else
                {
                    wrt.WriteLine("Найден оптимум!\nF(X) = {0}", Math.Round(table.Function, 2));
                    for (int i = 0; i < table.BasisLength; i++)
                    {
                        Tuple<int, double> val = table[i];
                        wrt.WriteLine("x{0} = {1}", val.Item1, Math.Round(val.Item2, 2));
                    }
                }
            }

            wrt.Close();
        }
        private string OptimumTaskDescription()
        {
            string result = "";

            result += "F(X) = " + (maximumOptimum ? "max" : "min");

            result += "(";
            for (int i = 0; i < coefs.Length; i++)
            {
                if (coefs[i] != 0)
                {
                    result += (i == 0) ? "" : " + ";
                    result += string.Format("{0} * x{1}", coefs[i], i);
                }
            }
            result += ")\n";

            result += "Conditions:\n";

            for (int i = 0; i < condition.Count; i++)
            {
                for (int j = 0; j < allVectors.Length; j++)
                {
                    if (allVectors[j][i] != 0)
                    {
                        result += (j == 0) ? "" : " + ";
                        result += string.Format("{0} * x{1}", allVectors[j][i], j);
                    }
                }

                result += " = " +  condition[i] + "\n";
            }

            return result;
        }
    }
}
