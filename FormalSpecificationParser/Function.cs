using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace FormalSpecificationParser
{
    class Argument {
        private string sName, sDatatype;
        private bool CanBeNegative = true, IsArray = false;

        public Argument(string name, string rawDatatype, bool array = false)
        {
            sName = name;
            IsArray = array;
            switch (rawDatatype)
            {
                case "N":
                    sDatatype = "int";
                    CanBeNegative = false;
                    break;
                case "Z":
                    sDatatype = "int";
                    break;
                case "R":
                    sDatatype = "float";
                    break;
                case "B":
                    sDatatype = "bool";
                    break;
                case "char*":
                    sDatatype = "string";
                    break;
                default:
                    break;
            }
        }

        public string getDatatype() { return sDatatype; }
        public string getName() { return sName; }
        public bool getNegative() { return CanBeNegative; }

        public override string ToString()
        {
            return sDatatype + " " + sName;
        }
    }

    class Case
    {
        private string sTodo;
        private List<string> conditions;

        public Case() { sTodo = null; conditions = new List<string>(); }
        public Case(string todo, List<string> cnd) { sTodo = todo;conditions = cnd; }

        public string getTodo() { return sTodo; }
        public void setTodo(string sTodo) { this.sTodo = sTodo; }
        public List<string> getConditions() { return conditions; }
        public void addCondition(string cond) { conditions.Add(cond); }
    }

    class Function
    {
        private string sFunctionName, sPre, sPost;
        private List<Argument> args;

        public Function() { }

        public Function(String input)
        {
            String[] fragments = ParseInput(input);
            sFunctionName = fragments[0];
            sPre = fragments[1];
            sPost = fragments[2];
            args = ParseArguments(input.Substring(Int32.Parse(fragments[3]), Int32.Parse(fragments[4]) - Int32.Parse(fragments[3])));
        }

        public List<Argument> ParseArguments(string sArgs)
        {
            const int PARSE_MODE_NAME = 0, PARSE_MODE_DATATYPE = 1;
            List<Argument> output = new List<Argument>();

            string argName = null, argDatatype = null;
            int ParseMode = PARSE_MODE_NAME;
            bool IsArray = false;
            foreach (char c in sArgs)
            {
                 switch (c)
                {
                    case '(':
                    case ' ':
                        break;
                    case ':':
                        ParseMode = PARSE_MODE_DATATYPE;
                        break;
                    case ',':
                    case ')':
                        output.Add(new Argument(argName, argDatatype, IsArray));
                        IsArray = false;
                        argName = argDatatype = null;
                        ParseMode = PARSE_MODE_NAME;
                        break;
                    case '*':
                        if (argDatatype.Equals("char"))
                            argDatatype = "char*";
                        else
                            IsArray = true;
                        break;
                    default:
                    {
                        if (ParseMode == PARSE_MODE_NAME)
                            argName += c;
                        else
                            argDatatype += c;
                        break;
                    }
                }
            }
  
            output.Add(new Argument(argName, argDatatype, IsArray));
            return output;
        }

        public String[] ParseInput(string s)
        {
            const int PARSE_MODE_FUNCTION = 0, PARSE_MODE_PRE = 1, PARSE_MODE_POST = 2;
            string name = null, pre = "p", post = "p";
            int parseMode = PARSE_MODE_FUNCTION, argStart = 0, argEnd = 0;
            for (int i = 0; i < s.Length; i++)
            {
                switch (s[i])
                {
                    case '(':
                        if (parseMode == PARSE_MODE_FUNCTION)
                            argStart = i;
                        else
                        {
                            if (parseMode == PARSE_MODE_PRE)
                                pre += s[i];
                            else
                                post += s[i];
                        }
                        break;
                    case 'p':
                        {
                            if (parseMode == PARSE_MODE_FUNCTION)
                            {
                                if ((s[i + 1] == 'r') && (s[i + 2] == 'e'))
                                {
                                    parseMode = PARSE_MODE_PRE;
                                    argEnd = i - 1;
                                }
                                else
                                    name += s[i];
                            }
                            else
                            {
                                if (parseMode == PARSE_MODE_PRE)
                                {
                                    if ((s[i + 1] == 'o') && (s[i + 2] == 's') && (s[i + 3] == 't'))
                                        parseMode = PARSE_MODE_POST;
                                    else
                                        pre += s[i];
                                }
                                else
                                    post += s[i];
                            }
                            break;
                        }
                    default:
                        if (parseMode == PARSE_MODE_FUNCTION)
                        {
                            if (argStart == 0)
                                name += s[i];
                        }
                        else
                        {
                            if (parseMode == PARSE_MODE_PRE)
                                pre += s[i];
                            else
                                post += s[i];
                        }
                        break;
                }
            }

            string[] result = { name, pre, post, argStart.ToString(), argEnd.ToString() };
            return result;
        }

        public String getName() { return sFunctionName; }
        public String getPre() { return sPre; }
        public String getPost() { return sPost; }
        public List<Argument> getArgs() { return args; }
    }
}
