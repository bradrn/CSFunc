using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgebraicTypes;
using CodeContracts;

namespace Algebraic_Type_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write(">");
                string type = Console.ReadLine();
                Maybe<Tuple<string, string[], Tuple<string, string[]>[]>> parsedType = TryParse(type);
                Console.WriteLine(parsedType.Match
                    (
                        Nothing: () => "",
                        Just: t => MakeType(t, type)
                    ));
            }
        }

        static Maybe<Tuple<string, string[], Tuple<string, string[]>[]>> TryParse(string s)
        {
            if (!s.Contains("=")) return Maybe<Tuple<string, string[], Tuple<string, string[]>[]>>.Nothing();

            string name = "";
            List<string> vars = new List<string>();
            List<Tuple<string, string[]>> vals = new List<Tuple<string, string[]>>();
            Tuple<string, List<string>> curVal = Tuple.Create("", new List<string>());
            int state = 0;
            bool newVal = true;
            foreach (string word in s.Split(' '))
            {
                switch (state)
                {
                    case 0:
                        name = word;
                        state++;
                        break;
                    case 1:
                        if (word == "=")
                        {
                            state++;
                        }
                        else
                        {
                            vars.Add(word);
                        }
                        break;
                    case 2:
                        if (newVal)
                        {
                            curVal = Tuple.Create(word, new List<string>());
                            newVal = false;
                        }
                        else
                        {
                            if (word == "|")
                            {
                                vals.Add(Tuple.Create(curVal.Item1, curVal.Item2.ToArray()));
                                newVal = true;
                            }
                            else
                            {
                                curVal.Item2.Add(word);
                            }
                        }
                        break;
                }
            }
            vals.Add(Tuple.Create(curVal.Item1, curVal.Item2.ToArray()));
            return Maybe<Tuple<string, string[], Tuple<string, string[]>[]>>.Just(Tuple.Create(name, vars.ToArray(), vals.ToArray()));
        }

        private static string MakeType(Tuple<string, string[], Tuple<string, string[]>[]> parsedType, string original)
        {
            string name = parsedType.Item1;
            string[] vars = parsedType.Item2;
            Tuple<string, string[]>[] vals = parsedType.Item3;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"// {original}");
            sb.Append($"public class {name}");
            if (vars.Count() > 0)
            {
                sb.Append("<");
                sb.Append(string.Join(",", vars));
                sb.Append(">");
            }
            sb.AppendLine();
            sb.AppendLine("{");
            foreach (Tuple<string, string[]> val in vals)
            {
                string valname = val.Item1;
                string[] valvals = val.Item2;

                sb.Append($"private class {valname}Impl");
                AppendIndefVals(ref sb, valvals.Select(v => v + "1").ToArray(), vars.Select(v => v + "1").ToArray());
                sb.AppendLine();

                sb.AppendLine("{");
                int i = 1;
                foreach (string valval in valvals)
                {
                    if (vars.Contains(valval))
                    {
                        sb.AppendLine($"public {valval + "1"} Value{i}{{get;set;}}=default({valval + "1"});");
                    }
                    else
                    {
                        sb.AppendLine($"public {valval} Value{i}{{get;set;}}=default({valval});");
                    }
                    i++;
                }
                sb.Append($"public {valname}Impl(");
                sb.Append(string.Join(",", valvals.Select((v, j) => $"{(vars.Contains(v) ? v+"1" : v)} value{j+1}")));
                sb.AppendLine(")");
                sb.AppendLine("{");
                i = 1;
                foreach (string valval in valvals)
                {
                    sb.AppendLine($"Value{i} = value{i};");
                    i++;
                }
                sb.AppendLine("}");
                sb.AppendLine("}");
            }
            sb.AppendLine($"public {name}State State {{get; set;}}");
            foreach (Tuple<string, string[]> val in vals)
            {
                sb.Append($"private {val.Item1}Impl");
                AppendIndefVals(ref sb, val.Item2, vars);
                sb.AppendLine($" {val.Item1}Field;");
                sb.Append($"private {val.Item1}Impl");
                AppendIndefVals(ref sb, val.Item2, vars);
                sb.Append($" {val.Item1}Value {{ get {{ return {val.Item1}Field; }} set {{ {val.Item1}Field = value;");
                foreach (Tuple<string, string[]> val2 in vals)
                {
                    if (val2.Item1 != val.Item1)
                    {
                        sb.Append($"{val2.Item1}Field = null;");
                    }
                }
                sb.AppendLine($"State = {name}State.{val.Item1}; }} }}");
            }

            sb.AppendLine($"private {name}() {{ }}");

            foreach (Tuple<string, string[]> val in vals)
            {
                sb.Append($"public static {name}");
                AppendIndefVals(ref sb, vars, vars);
                sb.Append($" {val.Item1}(");
                sb.Append(string.Join(",", val.Item2.Select((v, j) => $"{v} value{j + 1}")));
                sb.AppendLine($")");
                sb.AppendLine("{");
                sb.Append(name);
                AppendIndefVals(ref sb, vars, vars);
                sb.Append($" result = new {name}");
                AppendIndefVals(ref sb, vars, vars);
                sb.AppendLine("();");
                sb.Append($"result.{val.Item1}Value = new {val.Item1}Impl");
                AppendIndefVals(ref sb, val.Item2, vars);
                sb.Append("(");
                sb.Append(string.Join(",", val.Item2.Select((v, j) => $"value{j + 1}")));
                sb.AppendLine(");");
                sb.AppendLine("return result;");
                sb.AppendLine("}");
            }

            string otherType = (vars.Count() > 0 ? vars[0] : "T") + "1";
            sb.Append($"public {otherType} Match<{otherType}>");
            sb.Append("(");
            foreach (Tuple<string, string[]> val in vals)
            {
                sb.Append("Func");
                List<string> newvars2 = new List<string>(val.Item2);
                newvars2.Add(otherType);
                AppendIndefVals(ref sb, newvars2.ToArray(), newvars2.ToArray());
                sb.Append(" " + val.Item1 + ",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.AppendLine("switch (State)");
            sb.AppendLine("{");
            foreach (Tuple<string, string[]> val in vals)
            {
                sb.Append($"case {name}State.{val.Item1}: return {val.Item1}(");
                sb.Append(string.Join(",", val.Item2.Select((v, j) => $"{val.Item1}Value.Value{j + 1}")));
                sb.AppendLine(");");
            }
            sb.AppendLine("}");
            sb.AppendLine($"return default({otherType});");
            sb.AppendLine("}");

            sb.AppendLine("}");

            sb.AppendLine($"public enum {name}State");
            sb.AppendLine("{");
            sb.AppendLine(string.Join(",", vals.Select(v => v.Item1)));
            sb.Append("}");

            return sb.ToString();
        }

        private static void AppendIndefVals(ref StringBuilder sb, string[] valvals, string[] vars)
        {
            IEnumerable<string> indefvalvals = valvals.Where(v => vars.Contains(v));
            if (indefvalvals.Count() > 0)
            {
                sb.Append("<");
                sb.Append(string.Join(",", indefvalvals));
                sb.Append(">");
            }
        }
    }
}
