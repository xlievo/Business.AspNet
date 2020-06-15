/*==================================
             ########
            ##########

             ########
            ##########
          ##############
         #######  #######
        ######      ######
        #####        #####
        ####          ####
        ####   ####   ####
        #####  ####  #####
         ################
          ##############
==================================*/

namespace Business.Client.Document
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading.Tasks;
    using TypeNameFormatter;

    public class FirstCharToLowerNamingPolicy : System.Text.Json.JsonNamingPolicy
    {
        public override string ConvertName(string name) => string.IsNullOrEmpty(name) ? name : name[0].ToString().ToLower() + name.Substring(1);
    }

    public class Doc
    {
        #region GetDocSource

        /// <summary>
        /// FirstCharToLowerNamingPolicy
        /// </summary>
        static readonly System.Text.Json.JsonSerializerOptions docJsonSettings = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            IgnoreNullValues = true,
            PropertyNamingPolicy = new FirstCharToLowerNamingPolicy(),
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        const string INDENT = "    ";
        static string GetINDENT(int indent)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < indent; i++)
            {
                sb.Append(INDENT);
            }

            return sb.ToString();
        }

        static string GetTypeName(string type) => Type.GetType($"System.{type}")?.GetFormattedName() ?? type;
        static string GetBooleanKey(bool type) => type ? "true" : "false";

        //static void AddGetValues(StringBuilder sb, IEnumerable<string> propertys, int indent)
        //{
        //    if (null == propertys || !propertys.Any()) { return; }

        //    var indent2 = GetINDENT(indent);
        //    var propertys2 = string.Join(", ", propertys.Select(c => $"this.{c}"));

        //    sb.AppendLine();
        //    sb.AppendLine($"{indent2}public object[] GetValues() => new object[] {{ {propertys2} }};");
        //}

        static void AddSummary(StringBuilder sb, string summary, string indent, Dictionary<string, string> param = null, string result = null, IList<string> descriptionArgs = null)
        {
            if (!string.IsNullOrWhiteSpace(summary))
            {
                sb.AppendLine($"{indent}/// <summary>");
                var lines = System.Text.RegularExpressions.Regex.Split(summary.Trim(), Environment.NewLine);

                for (int i = 0; i < lines.Length; i++)
                {
                    var item = lines[i];

                    if (0 == i)
                    {
                        sb.AppendLine($"{indent}/// {item.Trim()}");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}/// <br/>{item.Trim()}");
                    }
                }

                sb.AppendLine($"{indent}/// </summary>");

                //================== param ===================//

                if (0 < param?.Count)
                {
                    foreach (var item in param)
                    {
                        if (null != descriptionArgs && !descriptionArgs.Contains(item.Key))
                        {
                            continue;
                        }

                        var lines2 = System.Text.RegularExpressions.Regex.Split(item.Value?.Trim() ?? string.Empty, Environment.NewLine);

                        for (int i = 0; i < lines2.Length; i++)
                        {
                            var item2 = lines2[i];

                            if (0 == i)
                            {
                                sb.AppendLine($"{indent}/// <param name=\"{item.Key}\">{item2.Trim()}</param>");
                            }
                            else
                            {
                                sb.AppendLine($"{indent}/// <param name=\"{item.Key}\"><br/>{item2.Trim()}</param>");
                            }
                        }
                    }
                }

                //================== returns ===================//

                if (!string.IsNullOrEmpty(result))
                {
                    sb.AppendLine($"{indent}/// <returns>");

                    var lines3 = System.Text.RegularExpressions.Regex.Split(result.Trim(), Environment.NewLine);

                    for (int i = 0; i < lines3.Length; i++)
                    {
                        var item = lines3[i];

                        if (0 == i)
                        {
                            sb.AppendLine($"{indent}/// {item.Trim()}");
                        }
                        else
                        {
                            sb.AppendLine($"{indent}/// <br/>{item.Trim()}");
                        }
                    }

                    sb.AppendLine($"{indent}/// </returns>");
                }
            }
        }

        static bool Struct(StringBuilder sb, string name, Action content = null, bool newLine = false, int indent = 0, string summary = null, string key = "class")
        {
            var indent2 = GetINDENT(indent);
            var name2 = System.Text.RegularExpressions.Regex.Split(name, "`")[0].Replace('/', '_');

            if (newLine)
            {
                sb.AppendLine();
            }

            AddSummary(sb, summary, indent2);

            sb.AppendLine($"{indent2}public {key} {name2}");
            sb.AppendLine($"{indent2}{{");
            content?.Invoke();
            sb.AppendLine($"{indent2}}}");
            return true;
        }

        static bool AddProperty(StringBuilder sb, string type, string name, bool newLine = false, int indent = 0, string summary = null)
        {
            var indent2 = GetINDENT(indent);
            var type2 = System.Text.RegularExpressions.Regex.Split(type, "`")[0].Replace('/', '_');
            var name2 = System.Text.RegularExpressions.Regex.Split(name, "`")[0].Replace('/', '_');

            if (newLine)
            {
                sb.AppendLine();
            }

            AddSummary(sb, summary, indent2);

            sb.AppendLine($"{indent2}public {GetTypeName(type2)} {name2} {{ get; set; }}");
            return true;
        }

        static bool AddEnums(StringBuilder sb, string name, IEnumerable<EnumItems> value, bool newLine = false, int indent = 0, string summary = null)
        {
            var indent2 = GetINDENT(indent);
            var name2 = System.Text.RegularExpressions.Regex.Split(name, "`")[0].Replace('/', '_');

            if (newLine)
            {
                sb.AppendLine();
            }

            AddSummary(sb, summary, indent2);
            sb.AppendLine($"{indent2}public enum {name2}");
            sb.AppendLine($"{indent2}{{");

            var indent3 = GetINDENT(indent + 1);

            if (value.Any())
            {
                var enums = value.ToList();

                for (int i = 0; i < enums.Count; i++)
                {
                    if (!string.IsNullOrEmpty(enums[i].Description))
                    {
                        AddSummary(sb, enums[i].Description.Trim(), indent3);
                    }
                    sb.AppendLine($"{indent3}{enums[i].Name.Trim()} = {enums[i].Value},");

                    if (i < enums.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine($"{indent2}}}");
            return true;
        }

        static bool GetDefined(StringBuilder sb, string name, IList<DocArg> args, bool newLine = false, int indent = 0, string summary = null, bool nonProperty = false)
        {
            if (null == args) { return false; }

            Struct(sb, name, () =>
            {
                var objects2 = new Dictionary<string, DocArg>();
                var newLine2 = false;

                for (int i = 0; i < args.Count; i++)
                {
                    var arg = args[i];
                    if (arg.Token)
                    {
                        continue;
                    }
                    var type = arg.LastType;

                    if (null != arg.Children)
                    {
                        objects2.Add("object", arg);
                    }
                    else if (null != arg.Enums)
                    {
                        objects2.Add("enum", arg);
                    }
                    else if (arg.Array && "object" == arg.Items?.Type)
                    {
                        objects2.Add("array", arg);
                    }

                    if (arg.Array)
                    {
                        type = $"List<{GetTypeName(type)}>";
                    }

                    var indent2 = indent + 1;
                    var notLast = i < args.Count - 1;

                    if (!nonProperty)
                    {
                        newLine2 = AddProperty(sb, type, arg.Name ?? name, newLine2, indent2, arg.Description);
                    }

                    if (!notLast)
                    {
                        if (0 < objects2.Count && !nonProperty)
                        {
                            sb.AppendLine();
                        }

                        var newLine3 = false;//i2 < objects2.Count - 1;
                        foreach (var item in objects2)
                        {
                            var obj = item.Value;

                            switch (item.Key.Length)
                            {
                                case 6://object
                                    newLine3 = GetDefined(sb, obj.LastType, obj.Children.Values.ToList(), newLine3, indent2);
                                    break;
                                case 5://array
                                    newLine3 = GetDefined(sb, obj.LastType, obj.Items.Children.Values.ToList(), newLine3, indent2);
                                    break;
                                case 4://enum
                                    newLine3 = AddEnums(sb, obj.LastType, obj.Enums, newLine3, indent2, obj.Description);
                                    break;
                            }
                        }
                    }
                }
            }, newLine, indent, summary);

            return true;
        }

        static bool GetMethod(StringBuilder sb, Member member, bool newLine, int indent, string description, Dictionary<string, string> descriptionParam, string descriptionResult, IList<string> descriptionArgs)
        {
            var indent2 = GetINDENT(indent);

            if (newLine)
            {
                sb.AppendLine();
            }

            //=================Arg=================//

            var args = member.Args.Values.Where(c => !c.Token);

            var argsType = new List<string>();
            var argsName = new List<string>();

            var args2 = args.Select(c =>
            {
                argsName.Add(c.Name);

                var arg = $"{member.Name}_Arg {c.Name}";

                if ("object" == c.Items?.Type || null != c.Enums || null != c.Children)
                {
                    if (member.ArgSingle)
                    {
                        arg = $"{member.Name}_Arg";
                    }
                    else if (0 < member.Args?.Count) //array
                    {
                        arg = $"{member.Name}_Arg.{c.LastType}";
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    arg = $"{GetTypeName(c.LastType)}";
                }

                if (c.Array)
                {
                    arg = $"List<{arg}>";
                }

                argsType.Add(arg);

                return $"{arg} {c.Name}";
            }).ToList();

            //=================Returns=================//

            string result = null;
            string result2 = null;

            if (null != member.Returns)
            {
                if (null != member.Returns.Children || null != member.Returns.Enums)
                {
                    result = $"{member.Name}_Result";
                }
                else
                {
                    result = $"{GetTypeName(member.Returns.LastType)}";
                }

                if (member.Returns.Array)
                {
                    result = $"List<{result}>";
                }

                result2 = member.HasIResult ? $"IResult<{result}>" : result;

                result2 = $"<{result2}>";
            }

            AddSummary(sb, description, indent2, descriptionParam, descriptionResult, descriptionArgs);
            sb.AppendLine($"{indent2}public async ValueTask{result2} {member.Name}({string.Join(", ", args2)})");
            sb.AppendLine($"{indent2}{{");

            var argsType2 = 1 < argsType.Count ? "object[]" : 1 == argsType.Count ? argsType[0] : "object";
            var argsValue = 1 < argsName.Count ? $"new object[] {{ {string.Join(", ", argsName)} }}" : 1 == argsName.Count ? argsName[0] : "null";

            var result3 = member.HasIResult ? $"ResultObject<{result}>" : result;

            if (null != result)
            {
                sb.AppendLine($"{GetINDENT(indent + 1)}return await Config.GetResult<{result3}, {argsType2}>(this, {argsValue}, null, { GetBooleanKey(member.HasReturn)}, {GetBooleanKey(member.HasIResult)});");
            }
            else
            {
                sb.AppendLine($"{GetINDENT(indent + 1)}await Config.GetResult<object, {argsType2}>(this, {argsValue}, null, {GetBooleanKey(member.HasReturn)}, {GetBooleanKey(member.HasIResult)});");
                sb.AppendLine($"{GetINDENT(indent + 1)}return;");
            }

            sb.AppendLine($"{indent2}}}");

            return true;
        }

        static System.Net.Http.HttpClient httpClient = AspNet.Utils.Hosting.HttpClientFactory.CreateClient("");

        /// <summary>
        /// Build client source code
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static async ValueTask<string> Build(string host)
        {
            if (!Uri.TryCreate(host, UriKind.Absolute, out Uri uri))
            {
                throw new UriFormatException(host);
            }

            var data = await httpClient.GetStringAsync(uri.AbsoluteUri);

            var docs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Doc>>(data, docJsonSettings).Values.ToList();

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Business.Client;");
            sb.AppendLine();

            Struct(sb, uri.Authority.Replace(":", "_").Replace(".", "_"), () =>
            {
                sb.AppendLine($"{INDENT}public static Config Config {{ get; }} = new Config(\"{uri.Scheme}://{uri.Authority}\");");
                sb.AppendLine();

                foreach (var doc in docs)
                {
                    Struct(sb, $"{doc.Name} : API<{doc.Name}>", () =>
                    {
                        if (!doc.Group.TryGetValue(doc.Config?.Group ?? string.Empty, out Dictionary<string, Member> members))
                        {
                            members = doc.Group.FirstOrDefault().Value;
                        }

                        var newLine = false;
                        var indent = 2;

                        sb.AppendLine($"{INDENT}{INDENT}public {doc.Name.Replace('/', '_')}(Protocol protocol) : base(protocol, \"{doc.Name}\") {{ }}");
                        sb.AppendLine();

                        foreach (var member in members.Values)
                        {
                            newLine = GetMethod(sb, member, newLine, indent, member.Description, member.DescriptionParam, member.DescriptionResult, member.Args.Select(c => c.Key).ToList());
                        }

                        foreach (var member in members.Values)
                        {
                            //=================Arg=================//

                            if (member.ArgSingle)
                            {
                                var arg = member.Args?.FirstOrDefault().Value;

                                if (null != arg)
                                {
                                    if (null != arg.Children)
                                    {
                                        newLine = GetDefined(sb, $"{member.Name}_Arg", arg.Children.Values.ToList(), newLine, indent, arg.Description);
                                    }
                                    else if (null != arg.Enums)
                                    {
                                        newLine = AddEnums(sb, $"{member.Name}_Arg", arg.Enums, newLine, indent, arg.Description);
                                    }
                                    else if (arg.Array && "object" == arg.Items?.Type)
                                    {
                                        newLine = GetDefined(sb, $"{member.Name}_Arg", arg.Items.Children?.Values.ToList(), newLine, indent, arg.Description);
                                    }
                                }
                            }
                            else if (0 < member.Args?.Count) //array
                            {
                                //exclude non definition
                                var args = member.Args.Values.Where(c => null != c.Children || null != c.Enums || (c.Array && "object" == c.Items?.Type)).ToList();

                                if (0 < args.Count)
                                {
                                    newLine = GetDefined(sb, $"{member.Name}_Arg", args, newLine, indent, nonProperty: true);
                                }
                            }

                            //=================Returns=================//

                            if (null != member.Returns)
                            {
                                if (null != member.Returns.Children)
                                {
                                    newLine = GetDefined(sb, $"{member.Name}_Result", member.Returns?.Children?.Values.ToList(), newLine, indent, member.Returns.Description);
                                }
                                else if (null != member.Returns.Enums)
                                {
                                    newLine = AddEnums(sb, $"{member.Name}_Result", member.Returns.Enums, newLine, indent, member.Returns.Description);
                                }
                                //else if (member.Returns.Array)
                                //{
                                //    GetDefined(sb, $"{member.Name}_Result", member.Returns.Items.Children.Values.ToList(), notLast, 1);
                                //}
                            }
                        }
                    }, false, 1);
                }

            });

            return sb.ToString();
        }

        #endregion

        public string Name { get; set; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string Alias { get; set; }

        public Dictionary<string, Dictionary<string, Member>> Group { get; set; }

        public string Description { get; set; }

        public string GroupDefault { get; set; }

        public Config Config { get; set; }

        public IEnumerable<KeyValuePair<DocGroup, IEnumerable<DocInfo>>> DocGroup { get; set; }
    }

    public class DocArg
    {
        [System.Text.Json.Serialization.JsonPropertyName("default")]
        public object DefaultValue { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        public Dictionary<string, DocArg> Children { get; set; }

        public string Id { get; set; }

        public string Type { get; set; }

        public string LastType { get; set; }

        public bool Token { get; set; }

        public string Format { get; set; }

        public string Title { get; set; }

        public bool Array { get; set; }

        public IEnumerable<EnumItems> Enums { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> DescriptionTip { get; set; }

        public bool UniqueItems { get; set; }

        public Dictionary<string, object> Options { get; set; }

        public Items Items { get; set; }

        public string Name { get; set; }

        public bool ValueType { get; set; }
    }

    public struct EnumItems
    {
        public string Name { get; set; }

        public int Value { get; set; }

        public string Description { get; set; }
    }

    public class Items
    {
        public string Type { get; set; }

        public string Format { get; set; }

        public string Title { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        public Dictionary<string, DocArg> Children { get; set; }
    }

    public class Member
    {
        public string Key { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Friendly name group
        /// </summary>
        public string AliasGroup { get; set; }

        public bool HasReturn { get; set; }

        public bool HasIResult { get; set; }

        public DocArg Returns { get; set; }

        public string Description { get; set; }

        public Dictionary<string, string> DescriptionParam { get; set; }

        public string DescriptionResult { get; set; }

        public bool HasToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("properties")]
        public Dictionary<string, DocArg> Args { get; set; }

        public bool ArgSingle { get; set; }

        public bool HttpFile { get; set; }

        public Dictionary<string, Testing> Testing { get; set; }
    }

    public struct Testing
    {
        /// <summary>
        /// test key
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// test args
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// test result check
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// test fixed roken
        /// </summary>
        public string Token { get; set; }
    }

    public struct DocGroup
    {
        /// <summary>
        /// Group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Badge
        /// </summary>
        public string Badge { get; set; }

        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; }
    }

    public struct DocInfo
    {
        public string Key { get; set; }

        public string Group { get; set; }

        public string Name { get; set; }

        public string Badge { get; set; }
    }

    public class Config
    {
        public string Host { get; set; }

        /// <summary>
        /// Whether to render the Debug element in the UI
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Whether to render the Benchmark element in the UI
        /// </summary>
        public bool Benchmark { get; set; }

        /// <summary>
        /// Whether to render the Testing element in the UI
        /// </summary>
        public bool Testing { get; set; }

        /// <summary>
        /// Generate only documents for the specified group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Currently selected group
        /// </summary>
        public string GroupSelect { get; set; }

        /// <summary>
        /// Whether to render the Group element in the UI
        /// </summary>
        public bool GroupEnable { get; set; }

        /// <summary>
        /// Whether to render the SetToken element in the UI
        /// </summary>
        public bool SetToken { get; set; }

        /// <summary>
        /// Whether to open the side navigation bar
        /// </summary>
        public bool Navigtion { get; set; }

        /// <summary>
        /// Benchmark tests whether the passed parameters are JSON serialized. By default false, does not need to be serialized
        /// </summary>
        public bool BenchmarkJSON { get; set; }
    }
}
