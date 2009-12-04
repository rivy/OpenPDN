using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

[assembly: AssemblyTitle("ResxCheck")]
[assembly: AssemblyProduct("ResxCheck")]
[assembly: AssemblyCopyright("Copyright © 2008 dotPDN LLC")]
[assembly: AssemblyVersion("3.30.*")]
[assembly: AssemblyFileVersion("3.30.0.0")]

namespace ResxCheck
{
    public enum Warning
    {
        Duplicate,
        Missing,
        ExtraFormatTags,
        MalformedFormat,
        Extra,
    }

    public static class Extensions
    {
        public static IEnumerable<T> DoForEach<T>(this IEnumerable<T> source, Action<T> f)
        {
            foreach (T item in source)
            {
                f(item);
                yield return item;
            }
        }

        public static void Execute<T>(this IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                // Nothing. Hopefully you have a Do() clause in there.
            }
        }
    }

    class Program
    {
        static void PrintHeader(TextWriter output)
        {
            output.WriteLine("ResxCheck v{0}", Assembly.GetExecutingAssembly().GetName().Version);
            output.WriteLine("Copyright (C) 2008 dotPDN LLC, http://www.dotpdn.com/");
            output.WriteLine();
        }

        static void PrintUsage(TextWriter output)
        {
            string ourName = Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase);

            output.WriteLine("Usage:");
            output.WriteLine("  {0} <base.resx> [[mui1.resx] [mui2.resx] [mui3.resx] ... [muiN.resx]]", ourName);
            output.WriteLine();
            output.WriteLine("base.resx should be the original resx that is supplied by the developer or design team.");
            output.WriteLine();
            output.WriteLine("mui1.resx through muiN.resx should be the translated resx files based off of base.resx.");
            output.WriteLine("You may specify as many mui resx files as you would like to check.");
            output.WriteLine("(You can also not specify any, and then only base.resx will be checked)");
            output.WriteLine("TIP: You can specify a wildcard, such as *.resx");
            output.WriteLine();
            output.WriteLine("This program will check for:");
            output.WriteLine("  * base.resx must not have any string defined more than once");
            output.WriteLine("  * base.resx must not have any strings with incorrect formatting tags, e.g. having a { but no closing }, or vice versa");
            output.WriteLine();
            output.WriteLine("If any mui.resx files are specified, then these rules will also be checked:");
            output.WriteLine("  * mui.resx must not have any string defined more than once");
            output.WriteLine("  * mui.resx must have all the strings that base.resx defines");
            output.WriteLine("  * mui.resx must not have any strings defined that are not defined in base.resx");
            output.WriteLine("  * mui.resx must not have any strings with incorrect formatting tags, e.g. having a { but no closing }, or vice versa");
            output.WriteLine("  * mui.resx must not have any additional formatting tags, e.g. {2}");
            output.WriteLine();
            output.WriteLine("Examples:");
            output.WriteLine();
            output.WriteLine("  {0} strings.resx Strings.DE.resx String.IT.resx String.JP.resx", ourName);
            output.WriteLine("  This will use strings.resx as the 'base', and then check the DE, IT, and JP translations to ensure they pass the constraints and rules described above.");
            output.WriteLine();
            output.WriteLine("  {0} strings.resx translations\\*.resx", ourName);
            output.WriteLine("  This will use strings.resx as the 'base', and then all of the RESX files found in the translations directory will be validated against it.");
        }

        delegate void WarnFn(Warning reason, string extraFormat, params object[] extraArgs);

        static void Warn(TextWriter output, string name, Warning reason, string extraFormat, params object[] extraArgs)
        {
            string reasonText;

            switch (reason)
            {
                case Warning.Duplicate:
                    reasonText = "duplicate string name";
                    break;

                case Warning.Extra:
                    reasonText = "extra string";
                    break;

                case Warning.ExtraFormatTags:
                    reasonText = "extra format tags";
                    break;

                case Warning.MalformedFormat:
                    reasonText = "invalid format";
                    break;

                case Warning.Missing:
                    reasonText = "missing string";
                    break;

                default:
                    throw new InvalidEnumArgumentException();
            }

            output.WriteLine(
                "{0}: {1}{2}",
                name,
                reasonText,
                string.IsNullOrEmpty(extraFormat) ? 
                    "" : 
                    string.Format(": {0}", string.Format(extraFormat, extraArgs)));
        }

        static void AnalyzeSingle(WarnFn warnFn, IEnumerable<IGrouping<string, string>> grouping)
        {
            // Verify that all strings can have their formatting requirements satisified
            grouping.SelectMany(item => item.Select(val => new KeyValuePair<string, string>(item.Key, val)))
                    .Where(kvPair => CountFormatArgs(kvPair.Value, 100) == -1)
                    .DoForEach(kvPair => warnFn(Warning.MalformedFormat, "'{0}' = '{1}'", kvPair.Key, kvPair.Value))
                    .Execute();                                            

            // Verify there are no strings defined more than once.
            grouping.Where(item => item.Take(2).Count() > 1)
                    .SelectMany(item => item.Select(val => new KeyValuePair<string, string>(item.Key, val)))
                    .DoForEach(val => warnFn(Warning.Duplicate, "'{0}' = '{1}'", val.Key, val.Value))
                    .Execute();
        }

        static int CountFormatArgs(string formatString)
        {
            return CountFormatArgs(formatString, 1000);
        }

        static int CountFormatArgs(string formatString, int max)
        {
            bool isError;
            List<string> argsList = new List<string>();

            do
            {
                string[] args = argsList.ToArray();

                try
                {
                    string.Format(formatString, args);
                    isError = false;
                }

                catch (Exception)
                {
                    isError = true;
                }

                argsList.Add("x");
            } while (isError && (max == -1 || argsList.Count <= max));

            int count = argsList.Count - 1;

            if (count == max)
            {
                return -1;
            }
            else
            {
                return argsList.Count - 1;
            }
        }

        static void AnalyzeMuiWithBase(
            WarnFn baseWarnFn,
            IEnumerable<IGrouping<string, string>> baseList, 
            WarnFn muiWarnFn,
            IEnumerable<IGrouping<string, string>> muiList)
        {
            AnalyzeSingle(muiWarnFn, muiList);

            var baseDict = baseList.ToDictionary(v => v.Key, v => v.First());

            var muiDict = muiList.ToDictionary(v => v.Key, v => v.First());

            // Verify that mui has everything that base defines
            baseDict.Keys.Except(muiDict.Keys)
                         .DoForEach(key => muiWarnFn(Warning.Missing, key))
                         .Execute();

            // Verify that mui doesn't have any extra entries
            muiDict.Keys.Except(baseDict.Keys)
                        .DoForEach(key => muiWarnFn(Warning.Extra, "'{0}' = '{1}'", key, muiDict[key]))
                        .Execute();

            // Verify that the formatting of the strings works. So if base defines a string 
            // with {0}, {1} then that must be in the mui string as well.
            // To do this, we convert baseList and muiList to a lookup from key -> val1,val2
            // Then, we filter to only the items where calling string.Format(val1) raises an exception
            // Then, for each of these we determine how many formatting parameters val1 requires, 
            // and then make sure that val2 does not throw an exception when formatted with that many.
            baseDict.Keys
                    .Where(key => muiDict.ContainsKey(key))
                    .ToDictionary(key => key, key => new { Base = baseDict[key], Mui = muiDict[key] })
                    .Where(item => CountFormatArgs(item.Value.Base) != -1)
                    .Where(item => CountFormatArgs(item.Value.Mui) > CountFormatArgs(item.Value.Base))
                    .Select(item => item.Key)
                    .DoForEach(key => muiWarnFn(Warning.ExtraFormatTags, "'{0}' = '{1}'", key, muiDict[key]))
                    .Execute();
        }

        static IEnumerable<KeyValuePair<string, string>> FromResX(string resxFileName)
        {
            XDocument xDoc = XDocument.Load(resxFileName);

            var query = from xe in xDoc.XPathSelectElements("/root/data")
                        let attributes = xe.Attributes()
                        let name = (from attribute in attributes
                                    where attribute.Name.LocalName == "name"
                                    select attribute.Value)
                        let elements = xe.Elements()
                        let value = (from element in elements
                                     where element.Name.LocalName == "value"
                                     select element.Value)
                        select new KeyValuePair<string, string>(name.First(), value.First());

            return query;
        }

        static T Eval<T>(Func<T> f, T valueIfError)
        {
            T value;

            try
            {
                value = f();
                return value;
            }

            catch (Exception)
            {
                return valueIfError;
            }
        }

        static int Main(string[] args)
        {
            PrintHeader(Console.Out);

            if (args.Length < 1)
            {
                PrintUsage(Console.Out);
                return 1;
            }

            DateTime startTime = DateTime.Now;
            Console.WriteLine("--- Start @ {0}", startTime.ToLongTimeString());

            string dir = Environment.CurrentDirectory;

            string baseName = args[0];
            string basePathName = Path.GetFullPath(baseName);
            string baseDir = Path.GetDirectoryName(basePathName);
            string baseFileName = Path.GetFileName(basePathName);
            string baseFileNameNoExt = Path.GetFileNameWithoutExtension(baseFileName);
            var baseEnum = FromResX(basePathName);
            var baseGrouping = baseEnum.GroupBy(item => item.Key, item => item.Value);

            bool anyErrors = false;

            WarnFn baseWarnFn = new WarnFn(
                (reason, format, formatArgs) =>
                    {
                        anyErrors = true;
                        Warn(Console.Out, baseFileName, reason, format, formatArgs);
                    });

            List<Action> waitActions = new List<Action>();
            Action<Action> addWaitAction = a => { waitActions.Add(a); };

            ManualResetEvent e0 = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(ignored =>
                {
                    Console.WriteLine("Analyzing base {0} ...", baseFileName);

                    try
                    {
                        AnalyzeSingle(baseWarnFn, baseGrouping);
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine("{0} : {1}", baseFileName, ex);
                    }

                    finally
                    {
                        e0.Set();
                    }
                });

            addWaitAction(() => e0.WaitOne());

            var muiNames = args.Skip(1)
                               .SelectMany(spec => Eval(() => Directory.GetFiles(dir, spec), new string[0]));

            foreach (string muiName in muiNames)
            {
                string muiPathName = Path.GetFullPath(muiName);
                string muiDir = Path.GetDirectoryName(muiPathName);
                string muiFileName = Path.GetFileName(muiPathName);
                string muiFileNameNoExt = Path.GetFileNameWithoutExtension(muiFileName);

                ManualResetEvent eN = new ManualResetEvent(false);

                ThreadPool.QueueUserWorkItem(ignored =>
                    {
                        try
                        {
                            WarnFn muiNWarnFn = new WarnFn(
                                (reason, format, formatArgs) =>
                                    {
                                        anyErrors = true;
                                        Warn(Console.Out, muiFileName, reason, format, formatArgs);
                                    });

                            Console.WriteLine("Analyzing mui {0} ...", muiFileName);

                            var muiEnum = FromResX(muiPathName);
                            var muiGrouping = muiEnum.GroupBy(item => item.Key, item => item.Value);

                            AnalyzeMuiWithBase(baseWarnFn, baseGrouping, muiNWarnFn, muiGrouping);
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine("{0} : {1}", muiFileName, ex);
                        }

                        finally
                        {
                            eN.Set();
                        }
                    });

                addWaitAction(() => eN.WaitOne());
            }

            foreach (Action waitAction in waitActions)
            {
                waitAction();
            }

            DateTime endTime = DateTime.Now;

            Console.WriteLine(
                "--- End @ {0} ({1} ms), processed {2} resx files",
                endTime.ToLongTimeString(),
                (endTime - startTime).TotalMilliseconds,
                1 + muiNames.Count());

            Console.WriteLine("There were{0} errors", anyErrors ? "" : " no");

            return anyErrors ? 1 : 0;
            // pause
            //Console.Read();
        }
    }
}
