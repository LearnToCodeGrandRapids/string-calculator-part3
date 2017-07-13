using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace StringCalculator
{
    public class Test
    {
        [Fact]
        public void Add_ShouldReturn0_ForEmptyString()
        {
            var sc = new StringCalculator();

            const int expected = 0;

            var actual = sc.Add(string.Empty);
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_ShouldReturn7_ForString7()
        {
            var sc = new StringCalculator();

            const int expected = 7;

            var actual = sc.Add("7");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_ShouldReturn3_ForStrings1And2()
        {
            var sc = new StringCalculator();

            const int expected = 3;

            var actual = sc.Add("1,2");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_ShouldReturn100_ForABunchOfNumbers()
        {
            var sc = new StringCalculator();

            const int expected = 100;

            var actual = sc.Add("1,9,10,30,50");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_ShouldReturn6_For1NewLine2Comma3()
        {
            var sc = new StringCalculator();

            const int expected = 6;

            var actual = sc.Add("1\n2,3");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_ShouldReturn3_For1SemiColon2WithSemiColonDelimiter()
        {
            var sc = new StringCalculator();

            const int expected = 3;

            var actual = sc.Add("//;\n1;2");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_ShouldThrow_ForNegativeNumbers()
        {
            var sc = new StringCalculator();

            Assert.Throws<ArgumentException>(() => sc.Add("//;\n1;2;-7"));
        }

        [Theory]
        [InlineData("//;\n1001;2", 2)]
        [InlineData("//;\n1001;7", 7)]
        [InlineData("//;\n1;2", 3)]
        [InlineData("//;\n1000;2000", 1000)]
        public void Add_ShouldIgnore_NumbersLargerThan1000(string value, int expected)
        {
            var sc = new StringCalculator();
            
            var actual = sc.Add(value);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("//[\n1[2[3", 6)]
        [InlineData("//]\n1]2]3", 6)]
        [InlineData("//[***]\n1***2***3", 6)]
        [InlineData("//[################]\n1################2################3", 6)]
        [InlineData("//[*][%]\n1*2%3", 6)]
        [InlineData("//[***][################]\n1################2***3", 6)]
        public void Add_ShouldAllow_MutliCharacterDelim(string value, int expected)
        {
            var sc = new StringCalculator();

            var actual = sc.Add(value);

            Assert.Equal(expected, actual);
        }
    }

    public class StringCalculator
    {
        public string ExtractDelimiters(string numbers, List<string> delimiters)
        {
            var openingPos = numbers.IndexOf("[", StringComparison.Ordinal);
            var closingPos = numbers.IndexOf("]", StringComparison.Ordinal);

            if (openingPos == 0 && closingPos > 0)
            {
                var multiCharDelim = numbers.Substring(openingPos + 1, closingPos - openingPos - 1);

                delimiters.Add(multiCharDelim);

                numbers = ExtractDelimiters(numbers.Substring(closingPos + 1), delimiters);
            }
            else if (numbers[0] != '\n')
            {
                delimiters.Add(numbers[0].ToString());
                numbers = numbers.Substring(2);
            }

            return numbers;
        }

        public string ExtractDelimitersLoop(string numbers, List<string> delimiters)
        {
            while (numbers[0] != '\n')
            {
                var openingPos = numbers.IndexOf("[", StringComparison.Ordinal);
                var closingPos = numbers.IndexOf("]", StringComparison.Ordinal);

                if (openingPos == 0 && closingPos > 0)
                {
                    var multiCharDelim = numbers.Substring(openingPos + 1, closingPos - openingPos - 1);

                    delimiters.Add(multiCharDelim);

                    numbers = numbers.Substring(closingPos + 1);
                }
                else
                {
                    delimiters.Add(numbers[0].ToString());
                    numbers = numbers.Substring(1);
                }
            }

            return numbers;
        }

        public int Add(string numbers)
        {
            if (string.IsNullOrWhiteSpace(numbers))
            {
                return 0;
            }

            var delimiters = new List<string>(new[] { ",", "\n" });

            /* //[*][%]\n1*2%3 */
            
            // a custom delimiter will follow two forward slashes at the
            // beginning of the string
            if (numbers.StartsWith("//"))
            {
                //numbers = ExtractDelimiters(numbers.Substring(2), delimiters);
                numbers = ExtractDelimitersLoop(numbers.Substring(2), delimiters);
            }

            var numberTokens = numbers.Split(delimiters.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            var runningTotal = 0;

            foreach (var token in numberTokens)
            {
                var number = int.Parse(token);

                if (number < 0)
                {
                    throw new ArgumentException($"Invalid number: {number}. Negative numbers are not allowed.");
                }
                
                if (number < 1001)
                {
                    runningTotal += number;
                }
            }

            return runningTotal;
        }
    }
}
