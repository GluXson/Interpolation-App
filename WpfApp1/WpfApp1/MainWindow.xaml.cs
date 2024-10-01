using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace InterpolationApp
{
    public partial class MainWindow : Window
    {
        private List<Tuple<double, double>> points;
        private double[] coefficients;

        public MainWindow()
        {
            InitializeComponent();
            points = new List<Tuple<double, double>>();
        }

        private void AddPoint_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(XInput.Text, out double x) && double.TryParse(YInput.Text, out double y))
            {
                var point = new Tuple<double, double>(x, y);
                points.Add(point);
                PointsList.Items.Add(point);
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter valid numbers.");
            }
        }

        private void RemovePoint_Click(object sender, RoutedEventArgs e)
        {
            if (PointsList.SelectedItem != null)
            {
                var selectedPoint = (Tuple<double, double>)PointsList.SelectedItem;
                points.Remove(selectedPoint);
                PointsList.Items.Remove(selectedPoint);
            }
            else
            {
                MessageBox.Show("No point selected. Please select a point to remove.");
            }
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            if (points.Count < 2)
            {
                MessageBox.Show("At least two points are required for interpolation.");
                return;
            }

            coefficients = CalculatePolynomialCoefficients(points);
            SaveToLatex(points, coefficients);
            MessageBox.Show("Calculation complete. LaTeX file saved on the desktop.");
        }

        private double[] CalculatePolynomialCoefficients(List<Tuple<double, double>> points)
        {
            int n = points.Count;
            double[,] matrix = new double[n, n];
            double[] rhs = new double[n];

            for (int i = 0; i < n; i++)
            {
                double xi = points[i].Item1;
                double yi = points[i].Item2;
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = Math.Pow(xi, j);
                }
                rhs[i] = yi;
            }

            return GaussianElimination(matrix, rhs);
        }

        private double[] GaussianElimination(double[,] matrix, double[] rhs)
        {
            int n = rhs.Length;
            for (int i = 0; i < n; i++)
            {
                int max = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(matrix[j, i]) > Math.Abs(matrix[max, i]))
                    {
                        max = j;
                    }
                }

                for (int k = i; k < n; k++)
                {
                    double temp = matrix[max, k];
                    matrix[max, k] = matrix[i, k];
                    matrix[i, k] = temp;
                }

                double temp2 = rhs[max];
                rhs[max] = rhs[i];
                rhs[i] = temp2;

                for (int j = i + 1; j < n; j++)
                {
                    double factor = matrix[j, i] / matrix[i, i];
                    rhs[j] -= factor * rhs[i];
                    for (int k = i; k < n; k++)
                    {
                        matrix[j, k] -= factor * matrix[i, k];
                    }
                }
            }

            double[] solution = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                {
                    sum += matrix[i, j] * solution[j];
                }
                solution[i] = (rhs[i] - sum) / matrix[i, i];
            }

            return solution;
        }

        private void SaveToLatex(List<Tuple<double, double>> points, double[] coefficients)
        {
            StringBuilder latex = new StringBuilder();
            latex.AppendLine("\\documentclass[11pt]{article}");
            latex.AppendLine("\\usepackage{tikz}");
            latex.AppendLine("\\usepackage{pgfplots}");
            latex.AppendLine("\\pgfplotsset{compat=1.12}");
            latex.AppendLine("\\usepgfplotslibrary{fillbetween}");
            latex.AppendLine("\\begin{document}");
            latex.AppendLine("\t\\begin{tikzpicture}");
            latex.AppendLine("\t\t\\pgfplotsset{scale only axis,}");
            latex.AppendLine("\t\t\\begin{axis}[xlabel=$x$, ylabel=$y$, samples=100]");
            latex.Append("\\addplot [only marks] table {");
            foreach (var point in points)
            {
                latex.AppendLine($"{point.Item1} {point.Item2}");
            }
            latex.AppendLine("};");

            latex.Append("\\addplot[][domain=");
            latex.Append($"{points.Min(p => p.Item1).ToString().Replace(',', '.')}:{points.Max(p => p.Item1).ToString().Replace(',', '.')}]{{");

            for (int i = coefficients.Length - 1; i >= 0; i--)
            {
                if (i < coefficients.Length - 1)
                {
                    latex.Append(coefficients[i] >= 0 ? "+" : "");
                }
                string coefficient = coefficients[i].ToString(CultureInfo.InvariantCulture);
                latex.Append($"{coefficient}*x^{i}");
            }


            latex.AppendLine("};");
            latex.AppendLine("\t\t\\end{axis}");
            latex.AppendLine("\t\\end{tikzpicture}");
            latex.AppendLine("\\end{document}");

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "interpolation.tex");
            File.WriteAllText(filePath, latex.ToString());
        }

        private void Evaluate_Click(object sender, RoutedEventArgs e)
        {
            if (coefficients == null)
            {
                MessageBox.Show("Please calculate the interpolation polynomial first.");
                return;
            }

            if (double.TryParse(EvaluateXInput.Text, out double x))
            {
                double y = EvaluatePolynomial(coefficients, x);
                ResultTextBlock.Text = $"P({x}) = {y}";
            }
            else
            {
                MessageBox.Show("Invalid input. Please enter a valid number for x.");
            }
        }

        private double EvaluatePolynomial(double[] coefficients, double x)
        {
            double result = 0;
            for (int i = 0; i < coefficients.Length; i++)
            {
                result += coefficients[i] * Math.Pow(x, i);
            }
            return result;
        }

        private void FirstDerivative_Click(object sender, RoutedEventArgs e)
        {
            if (coefficients == null)
            {
                MessageBox.Show("Please calculate the interpolation polynomial first.");
                return;
            }

            double[] firstDerivative = Differentiate(coefficients);
            ResultTextBlock.Text = "First Derivative: " + PolynomialToString(firstDerivative);
        }

        private void SecondDerivative_Click(object sender, RoutedEventArgs e)
        {
            if (coefficients == null)
            {
                MessageBox.Show("Please calculate the interpolation polynomial first.");
                return;
            }

            double[] firstDerivative = Differentiate(coefficients);
            double[] secondDerivative = Differentiate(firstDerivative);
            ResultTextBlock.Text = "Second Derivative: " + PolynomialToString(secondDerivative);
        }

        private double[] Differentiate(double[] coefficients)
        {
            double[] derivative = new double[coefficients.Length - 1];
            for (int i = 1; i < coefficients.Length; i++)
            {
                derivative[i - 1] = coefficients[i] * i;
            }
            return derivative;
        }

        private string PolynomialToString(double[] coefficients)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = coefficients.Length - 1; i >= 0; i--)
            {
                if (i < coefficients.Length - 1 && coefficients[i] >= 0)
                {
                    sb.Append("+");
                }
                sb.Append($"{coefficients[i]}*x^{i} ");
            }
            return sb.ToString();
        }
    }
}


