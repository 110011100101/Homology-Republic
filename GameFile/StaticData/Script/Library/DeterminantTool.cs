namespace RoseIsland.Library.CalculationTool.Determinant
{
    public static class Determinant
    {
        // 求取三阶行列式的正负
        /// <summary>
        /// 求取行列式的正负
        /// </summary>
        /// <param name="matrix">二维数组表示的矩阵</param>
        /// <returns>行列式的正负，1 表示正，-1 表示负，0 表示零</returns>
        public static int GetDeterminantSign(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double det = CalculateDeterminant(matrix, n);

            if (det > 0)
            {
                return 1;
            }
            else if (det < 0)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private static double CalculateDeterminant(double[,] matrix, int n)
        {
            if (n == 1)
            {
                return matrix[0, 0];
            }

            double det = 0;
            int sign = 1;

            for (int f = 0; f < n; f++)
            {
                double[,] temp = new double[n - 1, n - 1];

                for (int i = 1; i < n; i++)
                {
                    int colIndex = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if (j == f)
                        {
                            continue;
                        }
                        temp[i - 1, colIndex] = matrix[i, j];
                        colIndex++;
                    }
                }

                det += sign * matrix[0, f] * CalculateDeterminant(temp, n - 1);
                sign = -sign;
            }

            return det;
        }
    }
}
