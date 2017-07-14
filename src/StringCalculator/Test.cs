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

        [Fact]
        public void Add_ShouldLog3_WhenCalledWith1And2()
        {
            var logger = new LoggerMock();

            var sc = new StringCalculator(logger);

            const string expected = "3";

            sc.Add("1,2");

            var actual = logger.LastEntry;

            Assert.Equal(expected, actual);
        }

        // When calling Add() and the logger throws an exception, the string calculator 
        // should notify an IWebservice of some kind that logging has failed with the 
        // message from the logger’s exception (you will need a mock and a stub).
        [Fact]
        public void Add_NotifiesWebServerOfLoggingExceptions_WhenLoggerThrows()
        {
            var logger = new LoggerMock();
            var webService = new WebServiceMock();

            var sc = new StringCalculator(logger, webService);

            const string expectedExceptionMessage = "LoggerThrew";

            sc.Add("616");

            var actualExceptionMessage = webService.LastException.Message;

            Assert.Equal(expectedExceptionMessage, actualExceptionMessage);
        }

        [Fact]
        public void Add_Sends3ToConsole_WhenPassed1And2()
        {
            var logger = new LoggerMock();
            var webService = new WebServiceMock();
            var output = new OutputMock();
            
            var sc = new StringCalculator(logger, webService, output);

            const string expected = "3";

            sc.Add("1,2");

            var actual = output.LastLine;

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Create a program (test first)that uses string calculator, 
        /// which the user can invoke through the terminal/console by 
        /// calling “scalc ‘1,2,3’” and will output the following 
        /// line before exiting: “The result is 6”
        /// </summary>
        [Fact]
        public void Main_Writes6ToConsule_When123ProvidedByUser()
        {
            var output = new OutputMock();
            
            var sc = new StringCalculator(output);

            const string expected = "The result is 6";

            sc.Main(new [] { "scalc", "'1,2,3'" });

            var actual = output.History[output.History.Count - 2];

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void Main_RequestsInput_UntilNoneIsProvided()
        {
            var output = new OutputMock();
            var input = new InputMock();

            var sc = new StringCalculator(output, input);

            var expected = new[]
            {
                "6",
                "The result is 6",
                "another input please",
                "3",
                "The result is 3",
                "another input please",
                "5",
                "The result is 5",
                "another input please",
            };

            sc.Main(new[] { "scalc", "'1,2,3'" });

            var actual = output.History;

            Assert.Equal(expected[0], actual[0]);
            Assert.Equal(expected[1], actual[1]);
            Assert.Equal(expected[2], actual[2]);
            Assert.Equal(expected[3], actual[3]);
            Assert.Equal(expected[4], actual[4]);
            Assert.Equal(expected[5], actual[5]);
            Assert.Equal(expected[6], actual[6]);
            Assert.Equal(expected[7], actual[7]);
            Assert.Equal(expected[8], actual[8]);
        }
    }

    public interface IInput
    {
        string ReadLine();
    }

    public class InputMock : IInput
    {
        private int Count { get; set; }

        public InputMock()
        {
            Count = 0;
        }

        public string ReadLine()
        {
            Count++;

            switch (Count)
            {
                case 1:
                    return "'1,2'";
                case 2:
                    return "'1,2,2'";
                default:
                    return string.Empty;
            }
        }
    }

    public interface IOutput
    {
        void WriteLine(string value);
    }

    public class OutputMock : IOutput
    {
        public string LastLine { get; set; }
        public List<string> History { get; set; }

        public OutputMock()
        {
            History = new List<string>();
        }

        public void WriteLine(string value)
        {
            LastLine = value;
            History.Add(value);
        }
    }

    public interface IWebService
    {
        void TrackException(Exception ex);
    }

    public class WebServiceMock : IWebService
    {
        public Exception LastException { get; set; }

        public void TrackException(Exception ex)
        {
            LastException = ex;
        }
    }

    //  ILogger.Write()) 
    public interface ILogger
    {
        void Write(string value);
    }

    public class LoggerMock : ILogger
    {
        public string LastEntry { get; set; } 

        public void Write(string value)
        {
            if (value == "616")
            {
                throw new Exception("LoggerThrew");
            }

            LastEntry = value;
        }
    }

    public class StringCalculator
    {
        private ILogger Logger { get; }
        private IWebService WebService { get; }
        private IOutput Output { get; }
        private IInput Input { get; }

        public StringCalculator()
        {
        }

        public StringCalculator(ILogger logger)
        {
            Logger = logger;
        }

        public StringCalculator(ILogger logger, IWebService webService)
        {
            Logger = logger;
            WebService = webService;
        }

        public StringCalculator(ILogger logger, IWebService webService, IOutput output)
        {
            Logger = logger;
            WebService = webService;
            Output = output;
        }

        public StringCalculator(IOutput output)
        {
            Output = output;
        }

        public StringCalculator(IOutput output, IInput input)
        {
            Output = output;
            Input = input;
        }

        public void Main(string[] args)
        {
            var verb = args[0];

            if (verb == "scalc")
            {
                var input = args[1];

                while (!string.IsNullOrWhiteSpace(input))
                {
                    input = input.Substring(1);
                    input = input.Substring(0, input.Length - 1);

                    var result = Add(input);

                    Output.WriteLine($"The result is {result}");

                    Output.WriteLine("another input please");

                    input = (Input != null) ? Input.ReadLine() : string.Empty;
                }
            }
        }

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

            try
            {
                Logger?.Write(runningTotal.ToString());
            }
            catch (Exception ex)
            {
                WebService?.TrackException(ex);
            }

            Output?.WriteLine(runningTotal.ToString());

            return runningTotal;
        }
    }
}
