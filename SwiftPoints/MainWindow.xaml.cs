using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace SwiftPoints
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _sourceText;
        private readonly Regex _contReg = new Regex(@"(\bcontinue\b)");
        private readonly Regex _breakReg = new Regex(@"(\bbreak\b)");
        private readonly Regex _regFor =
            new Regex(@"(for\s+\w*\s+\w+\s*[=]\s*\w+\s*[;]\s*\w+\s*\W+\s*\w+\s*[;]\s*\w+([+]|[-])([+]|[-])\s*\W)");
        private readonly Regex _ifReg = new Regex(@"(if\s+\w+\s*\W+\s*\w+\s*\W)");
        private readonly Regex _elseReg = new Regex(@"(\belse\b\s*\W)");
        private readonly Regex _closeBr = new Regex(@"(})");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = @"W:\VisualSt\MSiSInf\Laba2\",
                Title = "Open File"
            };
            if (ofd.ShowDialog(Swift) != true) return;
            var source = new StreamReader(ofd.OpenFile());
            SourceBox.Text = source.ReadToEnd();
            source.Close();
        }

        private int[,] CreateMassive(string text, int strcount)
        {
            int[,] mas = new int[strcount, 7];
            var position = 0;
            var j = 0;
            for (var i = 0; i < text.Length - 1; i++)
            {
                if (text.Substring(i, 2) == "\r\n")
                {
                    var tempstr = text.Substring(position, i-position);
                    var forMatch = _regFor.Match(tempstr);
                    var ifMatch = _ifReg.Match(tempstr);
                    var elseMatch = _elseReg.Match(tempstr);
                    var contMatch = _contReg.Match(tempstr);
                    var breakMatch = _breakReg.Match(tempstr);
                    var closebrMatch = _closeBr.Match(tempstr);
                    for (var k = 0; k < 7; k++)
                    {
                        if (forMatch.Success)
                        {
                            if (k == 0 || k == 4)
                                mas[j, k] = 1;
                            else
                                mas[j, k] = 0;

                        }
                        if (ifMatch.Success)
                        {
                            if (k == 1 || k == 4)
                                mas[j, k] = 1;
                            else
                                mas[j, k] = 0;
                        }
                        if (elseMatch.Success)
                        {
                            if (k == 4 || k == 6)
                                mas[j, k] = 1;
                            else
                                mas[j, k] = 0;
                        }
                        if (contMatch.Success)
                        {
                            if (k == 2)
                                mas[j, k] = 1;
                            else
                                mas[j, k] = 0;
                        }
                        if (breakMatch.Success)
                        {
                            if (k == 3)
                                mas[j, k] = 1;
                            else
                                mas[j, k] = 0;
                        }
                        if (closebrMatch.Success)
                        {
                            if (k == 5)
                                mas[j, k] = 1;
                            else
                                mas[j, k] = 0;
                        }
                       
                    } 
                    j++;
                    position = i;
                }
            }
            return mas;
        }

        private void MasAnalizator(string text, int strcount)
        {
            int[,] mas = CreateMassive(text, strcount);
            int forCount = 0,
                ifCount = 0,
                openCount = 0,
                closeCount = 0,
                crossPoints = 0,
                elseCount = 0;
            for (var i = 0; i < strcount; i++)
            {
                if (mas[i, 0] == 1 && mas[i, 4] == 1)
                {
                    forCount++;
                    openCount++;
                    if (forCount > 1)
                    {
                        ifCount = 0;
                        elseCount = 0;
                    }
                }
                if (mas[i, 1] == 1 && mas[i, 4] == 1)
                {
                    ifCount++;
                    openCount++;
                }
                if (mas[i, 4] == 1 && mas[i, 6] == 1)
                {
                    elseCount++;
                    openCount++;
                }
                if (mas[i, 2] == 1)//cont
                {
                    if (forCount == 0)
                        crossPoints += 0;
                    if (forCount == 1)
                        crossPoints += ifCount - elseCount;
                    else
                    {
                        crossPoints += ifCount;
                    }
                }
                if (mas[i, 3] == 1)//break
                {
                    if (forCount == 0)
                        crossPoints += 0;
                    if (forCount == 1)
                        crossPoints += ifCount +elseCount+1;
                    else
                    {
                        crossPoints += ifCount+1;
                    }
                    
                }
                if (mas[i, 5] == 1)
                {
                    closeCount++;
                    if (openCount - ifCount == closeCount)
                        ifCount = 0;
                }
                if (closeCount == openCount && closeCount !=0 && openCount!=0)
                {
                    forCount = 0;
                    ifCount = 0;
                    elseCount = 0;
                    closeCount = 0;
                    openCount = 0;
                }
            }
            FinishBox.FontSize = 150;
            FinishBox.Text = crossPoints.ToString();
        }



        private void DoButton_Click(object sender, RoutedEventArgs e)
        {
            FinishBox.Text = "";
            _sourceText = SourceBox.Text.Trim()+"\r\n";
            var strCount = 0;

            for (var i = 0; i < _sourceText.Length-1; i++)
            {
                if (_sourceText.Substring(i, 2) == "\r\n")
                    strCount++;
            }
            MasAnalizator(_sourceText, strCount);
        }
    }
}

