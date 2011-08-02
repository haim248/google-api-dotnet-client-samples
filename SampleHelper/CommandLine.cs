﻿/*
Copyright 2011 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Google.Apis.Util;

namespace Google.Apis.Samples.Helper
{
    /// <summary>
    /// Contains helper methods for command line operation
    /// </summary>
    public class CommandLine
    {
        private static readonly Regex ArgumentRegex = new Regex(
            "^-[-]?([^-][^=]*)(=(.*))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Creates a new instance of T and fills all public fields by requesting input from the user
        /// </summary>
        /// <typeparam name="T">Class with a default constructor</typeparam>
        /// <returns>Instance of T with filled in public fields</returns>
        public static T CreateClassFromUserinput<T>()
        {
            var type = typeof (T);

            // Create an instance of T
            T settings = Activator.CreateInstance<T>();

            WriteLine("^1 Please enter values for the {0}:", Reflection.GetDescriptiveName(type));

            // Fill in parameters
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(settings);

                // Let the user input a value
                RequestUserInput(Reflection.GetDescriptiveName(field), ref value, field.FieldType);

                field.SetValue(settings, value);
            }

            WriteLine();
            return settings;
        }
    
        /// <summary>
        /// Requests an user input for the specified value
        /// </summary>
        /// <param name="name">Name to display</param>
        /// <param name="value">Default value, and target value</param>
        public static void RequestUserInput<T>(string name, ref T value)
        {
            object val = value;
            RequestUserInput(name, ref val, typeof(T));
            value = (T) val;
        }

        /// <summary>
        /// Requests an user input for the specified value, and returns the entered value.
        /// </summary>
        /// <param name="name">Name to display</param>
        public static T RequestUserInput<T>(string name)
        {
            object val = default(T);
            RequestUserInput(name, ref val, typeof(T));
            return (T) val;
        }

        /// <summary>
        /// Requests an user input for the specified value
        /// </summary>
        /// <param name="name">Name to display</param>
        /// <param name="value">Default value, and target value</param>
        /// <param name="valueType">Type of the target value</param>
        private static void RequestUserInput(string name, ref object value, Type valueType)
        {
            do
            {
                if (value != null)
                {
                    Write("   ^1{0} [^8{1}^1]: ^9", name, value);
                }
                else
                {
                    Write("   ^1{0}: ^9", name);
                }

                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    return; // No change required
                }

                try
                {
                    value = Convert.ChangeType(input, valueType);
                    return;
                }
                catch (InvalidCastException)
                {
                    WriteLine(" ^6Please enter a valid value!");
                }
            } while (true); // Run this loop until the user gives a valid input
        }

        /// <summary>
        /// Displays the Google Sample Header
        /// </summary>
        public static void DisplayGoogleSampleHeader(string applicationName)
        {
            applicationName.ThrowIfNull("applicationName");

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();

            WriteLine(@"^3   ___  ^6      ^8      ^3       ^4 _  ^6    ");
            WriteLine(@"^3  / __| ^6 ___  ^8 ___  ^3 __ _  ^4| | ^6 __  ");
            WriteLine(@"^3 | (_ \ ^6/ _ \ ^8/ _ \ ^3/ _` | ^4| | ^6/-_) ");
            WriteLine(@"^3  \___| ^6\___/ ^8\___/ ^3\__, | ^4|_| ^6\___| ");
            WriteLine(@"^3        ^6      ^8      ^3|___/  ^4    ^6    ");
            WriteLine();
            WriteLine("^4 API Samples -- {0}", applicationName);
            WriteLine("^4 Copyright 2011 Google Inc");
            WriteLine();
        }

        /// <summary>
        /// Displays the default "Press any key to exit" message, and waits for an user key input
        /// </summary>
        public static void PressAnyKeyToExit()
        {
            WriteLine();
            WriteLine("^8 Press any key to exit^1");
            Console.ReadKey();
        }

        /// <summary>
        /// Terminates the application.
        /// </summary>
        public static void Exit()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }

        /// <summary>
        /// Displays the default "Press ENTER to continue" message, and waits for an user key input
        /// </summary>
        public static void PressEnterToContinue()
        {
            WriteLine();
            WriteLine("^8 Press ENTER to continue^1");
            while (Console.ReadKey().Key != ConsoleKey.Enter) {}
        }

        /// <summary>
        /// Gives the user a choice of options to choose from
        /// </summary>
        /// <param name="question">The question which should be asked</param>
        /// <param name="choices">All possible choices</param>
        public static void RequestUserChoice(string question, params UserOption[] choices)
        {
            // Validate parameters
            question.ThrowIfNullOrEmpty("question");
            choices.ThrowIfNullOrEmpty("choices");

            // Show the question
            WriteLine(" ^9{0}", question);

            // Display all choices
            int i = 1;

            foreach (UserOption option in choices)
            {
                WriteLine("   ^8{0}.)^9 {1}", i++, option.Name);
            }

            WriteLine();

            // Request user input
            UserOption choice = null;

            do
            {
                Write(" ^1Please pick an option: ^9");
                string input = Console.ReadLine();

                // Check if this is a valid choice
                uint num;

                if (uint.TryParse(input, out num) && num > 0 && choices.Length >= num)
                {
                    // It is a number
                    choice = choices[num - 1];
                }
                else
                {
                    // Check if the user typed in the keyword
                    foreach (UserOption option in choices)
                    {
                        if (String.Equals(option.Name, input, StringComparison.InvariantCultureIgnoreCase))
                        {
                            choice = option;
                            break; // Valid choice
                        }
                    }
                }

                if (choice == null)
                {
                    WriteLine(" ^6Please pick one of the options displayed above!");
                }    
             
            } while (choice == null);

            // Execute the option the user picked
            choice.Target();
        }

        /// <summary>
        /// Gives the user a Yes/No choice and waits for his answer.
        /// </summary>
        public static bool RequestUserChoice(string question)
        {
            question.ThrowIfNull("question");

            // Show the question.
            Write("   ^1{0} [^8{1}^1]: ^9", question, "y/n");

            // Wait for the user input.
            char c;
            do
            {
                c = Console.ReadKey(true).KeyChar;
            } while (c != 'y' && c != 'n');
            WriteLine(c.ToString());

            return c == 'y';
        }

        /// <summary>
        /// Enables the command line exception handling
        /// Prevents the application from just exiting, but tries to display helpful error message instead
        /// </summary>
        public static void EnableExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

        }

        private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exception = args.ExceptionObject as Exception;

            // Display the exception
            WriteLine();
            WriteLine(" ^6An error has occured:");

            WriteLine("    ^6{0}", exception == null ? "<unknown error>" : exception.Message);

            // Display stacktrace
            WriteLine();
            WriteLine("^8 Press any key to display the stacktrace");
            Console.ReadKey();
            WriteLine();
            WriteLine(" ^1{0}", exception);

            // Close the application
            PressAnyKeyToExit();
            Environment.Exit(-1);
        }

        /// <summary>
        /// Writes the specified text to the console
        /// Applies special color filters (^0, ^1, ...)
        /// </summary>
        public static void Write(string format, params object[] values)
        {
            string text = String.Format(format, values);
            Console.ForegroundColor = ConsoleColor.Gray;

            while (text.Contains("^"))
            {
                int index = text.IndexOf("^");

                // Check if a number follows the index
                if (index+1 < text.Length && Char.IsDigit(text[index+1]))
                {
                    // Yes - it is a color notation
                    Console.Write(text.Substring(0, index)); // Pre-Colornotation text
                    Console.ForegroundColor = (ConsoleColor) (text[index + 1] - '0' + 6);
                    text = text.Substring(index + 2); // Skip the two-char notation
                }
                else
                {
                    // Skip ahead
                    Console.Write(text.Substring(0, index));
                    text = text.Substring(index + 1);
                }
            }

            // Write the remaining text
            Console.Write(text);
        }

        /// <summary>
        /// Writes the specified text to the console
        /// Applies special color filters (^0, ^1, ...)
        /// </summary>
        public static void WriteLine(string format, params object[] values)
        {
            Write(format+Environment.NewLine, values);
        }

        /// <summary>
        /// Writes an empty line into the console stream
        /// </summary>
        public static void WriteLine()
        {
            WriteLine("");
        }

        /// <summary>
        /// Writes a result into the console stream.
        /// </summary>
        public static void WriteResult(string name, object value)
        {
            if (value == null)
            {
                value = "<null>";
            }
            string strValue = value.ToString();
            if (strValue.Length == 0)
            {
                strValue = "<empty>";
            }

            WriteLine("   ^4{0}: ^9{1}", name, strValue);
        }

        /// <summary>
        /// Writes an action statement into the console stream.
        /// </summary>
        public static void WriteAction(string action)
        {
            WriteLine(" ^8{0}", action);
        }

        /// <summary>
        /// Writes an error into the console stream.
        /// </summary>
        public static void WriteError(string error, params object[] values)
        {
            WriteLine(" ^6"+error, values);
        }

        #region Command-Line Argument handling

        /// <summary>
        /// Defines the command line argument structure of a property.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        public class ArgumentAttribute : Attribute
        {
            private readonly string name;

            /// <summary>
            /// The full name of this command line argument, e.g. "source-directory".
            /// </summary>
            public string Name { get { return name; } }

            /// <summary>
            /// The short name of this command line argument, e.g. "src". Optional.
            /// </summary>
            public string ShortName { get; set; }

            /// <summary>
            /// The description of this command line argument, e.g. "The directory to fetch the data from". Optional.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The category to which this argument belongs, e.g. "I/O flags". Optional.
            /// </summary>
            public string Category { get; set; }

            /// <summary>
            /// Defines the command line argument structure of a property.
            /// </summary>
            public ArgumentAttribute(string name)
            {
                this.name = name;
            }
        }

        /// <summary>
        /// Parses the specified command line arguments into the specified class.
        /// </summary>
        /// <typeparam name="T">Class where the command line arguments are stored.</typeparam>
        /// <param name="configuration">Class which stores the command line arguments.</param>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Array of unresolved arguments.</returns>
        public static string[] ParseArguments<T>(T configuration, params string[] args)
        {
            var unresolvedArguments = new List<string>();
            foreach (string arg in args)
            {
                // Parse the argument.
                Match match = ArgumentRegex.Match(arg);
                if (!match.Success) // This is not a typed argument.
                {
                    unresolvedArguments.Add(arg);
                    continue;
                }

                // Extract the argument details.
                bool isShortname = !arg.StartsWith("--");
                string name = match.Groups[1].ToString();
                string value = match.Groups[2].Length > 0 ? match.Groups[2].ToString().Substring(1) : null;
                
                // Find the argument.
                const StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                PropertyInfo property =
                    (from kv in typeof(T).GetProperties(flags).WithAttribute<PropertyInfo, ArgumentAttribute>()
                     where name.Equals(isShortname ? kv.Value.ShortName : kv.Value.Name, ignoreCase)
                     select kv.Key).SingleOrDefault();

                // Check if this is a special argument we should handle.
                if (name == "help")
                {
                    foreach (string line in GenerateCommandLineHelp(configuration))
                    {
                        WriteAction(line);
                    }

                    if (property == null)
                    {
                         // If this isn't handled seperately, close this application.
                        Exit();
                        return null;
                    }
                }
                else if (property == null)
                {
                    WriteError("Unknown argument: " + (isShortname ? "-" : "--") + name);
                    continue;
                }

                // Change the property.
                object convertedValue = null;
                if (value == null)
                {
                    if (property.PropertyType == typeof(bool))
                    {
                        convertedValue = true;
                    }
                }
                else
                {
                    convertedValue = Convert.ChangeType(value, property.PropertyType);
                }

                if (convertedValue == null)
                {
                    WriteError(
                        string.Format(
                            "Argument '{0}' requires a value of the type '{1}'.", name, property.PropertyType.Name));
                    continue;
                }
                property.SetValue(configuration, convertedValue, null);
            }
            return unresolvedArguments.ToArray();
        }

        /// <summary>
        /// Generates the commandline argument help for a specified type.
        /// </summary>
        /// <typeparam name="T">Configuration.</typeparam>
        public static IEnumerable<string> GenerateCommandLineHelp<T>(T configuration)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var query = from kv in typeof(T).GetProperties(flags).WithAttribute<PropertyInfo, ArgumentAttribute>()
                        orderby kv.Value.Name
                        // Group the sorted arguments by their category.
                        group kv by kv.Value.Category into g
                        orderby g.Key
                        select g;

            // Go through each category and list all the arguments.
            yield return "Arguments:";
            foreach (var category in query)
            {
                if (!string.IsNullOrEmpty(category.Key))
                {
                    yield return " " + category.Key;
                }

                foreach (KeyValuePair<PropertyInfo, ArgumentAttribute> pair in category)
                {
                    PropertyInfo info = pair.Key;
                    object value = info.GetValue(configuration, null);
                    yield return "   " + FormatCommandLineHelp(pair.Value, info.PropertyType, value);
                }

                yield return "";
            }
        }

        /// <summary>
        /// Generates a single command line help for the specified argument
        /// Example:
        ///     -s, --source=[Something]      Sets the source of ...
        /// </summary>
        private static string FormatCommandLineHelp(ArgumentAttribute attribute, Type propertyType, object value)
        {
            // Generate the list of keywords ("-s, --source").
            var keywords = new List<string>(2);
            if (!string.IsNullOrEmpty(attribute.ShortName))
            {
                keywords.Add("-"+attribute.ShortName);
            }
            keywords.Add("--"+attribute.Name);
            string joinedKeywords = keywords.Aggregate((a, b) => a + ", " + b);

            // Add the assignment-tag, if applicable.
            string assignment = "";
            if (propertyType != typeof(bool))
            {
                assignment = string.Format("=[^1{0}^9]", (value == null) ? ".." : value.ToString());
            }

            // Create the joined left half, and return the full string.
            string left = (joinedKeywords + assignment).PadRight(20);
            return string.Format("^9{0}  ^1{1}", left, attribute.Description);
        }

        #endregion
    }
}
