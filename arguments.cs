/*
* Arguments class: application arguments interpreter
*
* Authors:		R. LOPES
* Contributors:	R. LOPES
* Created:		25 October 2002
* Modified:		28 October 2002
*
* Version:		1.0
2015 - Modified by Ignacio J. Perez J.
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace WMIParserStr
{
    public class Arguments
    {
        // Variables
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        // Constructor
        public Arguments(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            Regex splitter = new Regex(@"^-{1,2}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex remover = new Regex(@"^[""]?(.*?)[""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;
            string[] parts;

            foreach (string arg in args)
            {
                string newArg = arg == "\\" ? arg + "\\" : arg;
                parts = splitter.Split(newArg, 3);

                switch (parts.Length)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(parameter))
                        {
                            if (!Parameters.ContainsKey(parameter))
                            {
                                string value = remover.Replace(parts[0], "$1");
                                Parameters.Add(parameter, value);
                            }
                            parameter = null;
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(parameter) && !Parameters.ContainsKey(parameter))
                        {
                            Parameters.Add(parameter, string.Empty);
                        }
                        parameter = parts[1];
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(parameter) && !Parameters.ContainsKey(parameter))
                        {
                            Parameters.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        if (!Parameters.ContainsKey(parameter))
                        {
                            string value = remover.Replace(parts[2], "$1");
                            Parameters.Add(parameter, value);
                        }
                        parameter = null;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(parameter) && !Parameters.ContainsKey(parameter))
            {
                Parameters.Add(parameter, string.Empty);
            }
        }

        public string this[string param]
        {
            get
            {
                return Parameters.ContainsKey(param) ? Parameters[param] : null;
            }
        }
    }
}
