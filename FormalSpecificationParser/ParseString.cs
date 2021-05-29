using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormalSpecificationParser
{
    class ParseString
    {
        public static string ArgListWithoutResult(List<Argument> args)
        {
            string result = String.Empty;
            for (int i = 0; i < args.Count() - 1; i++)
            {
                result += args[i].ToString();
                if (i < args.Count() - 2)
                    result += ", ";
                else
                    result += ")";
            }
            return result;
        }

        public static string ArgListWithoutDatatype(List<Argument> args)
        {
            string result = String.Empty;
            for (int i = 0; i < args.Count() - 1; i++)
            {
                result += args[i].getName();
                if (i < args.Count() - 2)
                    result += ", ";
                else
                    result += ")";
            }
            return result;
        }

        public static string ParsePre(string s, string func_name, List<Argument> args)
        {
            string result = "bool Check_" + func_name + "(";

            result += ArgListWithoutResult(args) + " {\n";

            s = s.Substring(3);

            if (String.IsNullOrWhiteSpace(s))
                result += "\treturn true;\n}\n\n";
            else
                result += "\tif (" + s + ")\n\t\treturn true;\n\telse\n\t\treturn false;\n}\n\n";

            return result;
        }

        public static List<Case> createCaseList(string post)
        {
            const int PARSE_MODE_CND = 0;
            const int PARSE_MODE_TODO = 1;

            List<Case> output = new List<Case>();
            string str = null;
            List<string> cnd = new List<string>();
            int parseMode = PARSE_MODE_TODO;
            bool HasAddedEmptyCase = false;

            for (int i = 0; i < post.Length; i++)
            {
                switch (post[i])
                {
                    case '(':
                        if (HasAddedEmptyCase)
                        {
                            if (String.IsNullOrWhiteSpace(str))
                                str += post[i];
                        }
                        else
                        {
                            output.Add(new Case());
                            str += post[i];
                            HasAddedEmptyCase = true;
                        }
                        break;
                    case '<':
                    case '>':
                    case '!':
                        parseMode = PARSE_MODE_CND;
                        str += post[i];
                        break;
                    case '=':
                        {
                            if ((post[i - 1] != '=') && (post[i + 1] != '='))
                            {
                                if ((post[i - 1] == '<') || (post[i - 1] == '>') || (post[i - 1] == '!'))
                                    parseMode = PARSE_MODE_CND;
                                else
                                    parseMode = PARSE_MODE_TODO;
                            }
                            else
                                parseMode = PARSE_MODE_CND;
                            str += post[i];
                            break;
                        }
                    case ')':
                        {
                            if (!String.IsNullOrWhiteSpace(str))
                            {
                                if (str.Last() != ')')
                                    str += post[i];

                                if (parseMode == PARSE_MODE_CND)
                                    output[output.Count() - 1].addCondition(str);
                                else
                                {
                                    str = str.Replace("(", "");
                                    str = str.Replace(")", "");
                                    output[output.Count() - 1].setTodo(str);
                                }
                                str = String.Empty;
                            }
                            break;
                        }
                    case '|':
                        HasAddedEmptyCase = false;
                        parseMode = PARSE_MODE_TODO;
                        break;
                    case '&':
                        break;
                    default:
                        str += post[i];
                        break;
                }
            }
            return output;
        }

        public static string ParsePost(string s, string func_name, List<Argument> args)
        {
            string result = args[args.Count() - 1].getDatatype() + " " + func_name + "(";

            result += ArgListWithoutResult(args) + " {\n";

            string output = args[args.Count() - 1].ToString();

            switch (args[args.Count() - 1].getDatatype()) {
                case "bool":
                    result += "\t" + output + " = true;\n";
                    break;
                case "int":
                case "float":
                    result += "\t" + output + " = 0;\n";
                    break;
                case "string":
                    result += "\t" + output + ";\n";
                    break;
            }

            s = s.Substring(4);

            if (!s.Contains("|"))
            {
                s = s.Replace("(", "");
                s = s.Replace(")", "");
                result += "\t" + s + ";\n";
                result += "\treturn " + args[args.Count() - 1].getName() + ";\n";
                result += "}\n\n";
            }
            else
            {
                List<Case> cases = createCaseList(s);
                foreach (Case c in cases)
                {
                    if (c.getConditions().Count() >= 2)
                        result += "\tif (";
                    else
                        result += "\tif ";

                    for (int i = 0; i < c.getConditions().Count(); i++)
                    {
                        result += c.getConditions()[i];
                        if (i < c.getConditions().Count() - 1)
                            result += " && ";
                    }

                    if (c.getConditions().Count() >= 2)
                        result += ")";
                    
                    result += "\n\t\t" + c.getTodo() + ";\n";
                }
                result += "\treturn " + args[args.Count() - 1].getName() + ";\n";
                result += "}\n\n";
            }
            return result;
        }

        public static string createInputFunction(List<Argument> args)
        {
            string output = String.Empty;
            output += "void Input(";
            for (int i = 0; i < args.Count() - 1; i++)
            {
                output += args[i].getDatatype() + " &" + args[i].getName();
                if (i == args.Count() - 2)
                    output += ") {\n";
                else
                    output += ",";
            }
            for (int i = 0; i < args.Count() - 1; i++)
            {
                switch (args[i].getDatatype())
                {
                    case "int":
                        {
                            if (args[i].getNegative())
                            {
                                output += "\tcout << \"Input integer variable " + args[i].getName() + ": \";\n";
                                output += "\tcin >> " + args[i].getName() + ";\n";
                            }
                            else
                            {
                                output += "\tdo{\n\t\tcout << \"Input natural variable " + args[i].getName() + ": \";\n";
                                output += "\t\tcin >> " + args[i].getName() + ";\n";
                                output += "\t} while (" + args[i].getName() + " < 0);\n";
                            }
                            break;
                        }
                    case "bool":
                        string temp = args[i].getName() + "_temp";
                        output += "\tint " + temp + ";\n";
                        output += "\tdo{\n\t\tcout << \"Input bool variable " + args[i].getName() + ": \";\n";
                        output += "\t\tcin >> " + temp + ";\n";
                        output += "\t} while ((" + temp + " != 0) && (" + temp + " != 1));\n";
                        output += "\tif (" + temp + " == 0)\n";
                        output += "\t\t" + args[i].getName() + " = false;\n";
                        output += "\telse\n";
                        output += "\t\t" + args[i].getName() + " = true;\n";
                        break;
                    case "float":
                        output += "\tcout << \"Input float variable " + args[i].getName() + ": \";\n";
                        output += "\tcin >> " + args[i].getName() + ";\n";
                        break;
                    case "string":
                        output += "\tcout << \"Input string " + args[i].getName() + ": \";\n";
                        output += "\tcin >> " + args[i].getName() + ";\n";
                        break;
                }
            }
            output += "}\n\n";
            return output;
        }
    }
}
