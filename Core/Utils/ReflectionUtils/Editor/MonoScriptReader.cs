using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RivenFramework.Utils.Reflection
{
    [Todo("Need to clean up, very sloppy")]
    public class MonoScriptReader
    {
        public List<TokenInfo> previousTokens { get; private set; }
        private TokenInfo info;
        private string fileChars;
        private char thisChar;
        private char lastChar;
        private bool doClearToken;
        public Stack<Context> contextStack;
        bool isAlphaToken = false;
        public struct TokenInfo
        {
            public string token;
            public int charPosition;
            public int linePosition;
            public Context context => contextStack[0];
            public Context[] contextStack;

            public override string ToString() =>
                $"[char:{charPosition},line:{linePosition}, context: {context}] {token}";
        }

        private Queue<TokenSearch> s_searchQueue = new Queue<TokenSearch>();
        private Queue<TokenFilter> s_permanentFilters = new Queue<TokenFilter>();
        public void IgnoreComments()
        {
            s_permanentFilters.Enqueue(new IgnoreContextFilter(Context.LineComment));
            s_permanentFilters.Enqueue(new IgnoreContextFilter(Context.BlockComment));
        }
        public void FindType(Type typeToFind)
        {
            if (typeToFind == null) throw new ArgumentNullException();
            if (typeToFind.IsPrimitive) throw new ArgumentException();

            Type findType = typeToFind;
            string findTypeKeyword = "struct";
            SearchForBackwardParseMatch filterSet = new SearchForBackwardParseMatch(() => previousTokens);

            //Get type identifier token and add it to chain to find
            if (findType.IsClass) findTypeKeyword = "class";
            else if (findType.IsInterface) findTypeKeyword = "interface";
            else if (findType.IsEnum) findTypeKeyword = "enum";
            else if (typeof(Delegate).IsAssignableFrom(findType))
                throw new NotImplementedException("Havent implemented searching for delegates");
            filterSet.AddFilter(new MatchesTokenFilter(findTypeKeyword));

            //Get nongeneric name of given type and add it to chain to find
            string typeName = findType.NameWithoutGenericOrArray();

            filterSet.AddFilter(new MatchesTokenFilter(typeName));

            if (findType.IsGenericType)
            {
                filterSet.AddFilter(new MatchesTokenFilter("<"));
                int count = findType.GetGenericArguments().Length;
                for (int i = 1; i < count; i++)
                {
                    filterSet.AddSearch(new SearchForMatchingToken(","));
                }

                filterSet.AddFilter(new MatchesTokenFilter(">"));
            }
            s_searchQueue.Enqueue(filterSet);
            // */
        }
        public void FindMemberInfo(MemberInfo memberToFind)
        {
            //todo: REMOVE THIS SHIETTT
            Type type = null;
            
            //Type type = typeof(Dictionary<List<Actor.someInterface>, Dictionary<bool, Action<string, string>>>);

            string[] tokens = ConvertTypeToTokens(type).ToArray();
            Debug.Log("--------Tokens-------");
            StringBuilder sb = new StringBuilder();
            foreach (string s in tokens)
                sb.Append(s);
            Debug.Log(sb.ToString());
            Debug.Log("---------------------");

            if (memberToFind is TypeInfo) FindType(memberToFind.ReflectedType);
            else if (memberToFind is MethodInfo method) FindMethod(method);
            else if (memberToFind is FieldInfo field) FindField(field);
            else if (memberToFind is PropertyInfo property) FindProperty(property);
            else if (memberToFind is ConstructorInfo constructor) FindConstructor(constructor);
            else if (memberToFind is EventInfo eventInfo) FindEvent(eventInfo);
        }
        public void FindMethod(MethodInfo method)
        {
            FindType(method.ReflectedType);

            SearchForBackwardParseMatch filterSet = new SearchForBackwardParseMatch(() => previousTokens);
            string returnType = method.ReturnType.Name;
            if (returnType == "Void") returnType = "void";
            filterSet.AddFilter(new MatchesTokenFilter(returnType));
            Type[] genericTypes = method.ReturnType.GenericTypeArguments;
            for (int i = 0; i < genericTypes.Length; i++)
            {
                Type currentType = genericTypes[i];
                if (i == 0)
                    filterSet.AddFilter(new MatchesTokenFilter("<"));

                if (i + 1 >= genericTypes.Length)
                {
                    filterSet.AddFilter(new MatchesTokenFilter(">"));
                }
            }
            filterSet.AddFilter(new MatchesTokenFilter(method.Name));
            //filterSet.AddSearch(new SearchForMatchingToken("("));
            //filterSet.AddSearch(new SkipTokens(1));
            //filterSet.AddFilter(new MatchesTokenFilter(")"));

            s_searchQueue.Enqueue(filterSet);
        }
        public void FindField(FieldInfo field)
        {
            throw new NotImplementedException();
        }
        public void FindProperty(PropertyInfo property)
        {
            throw new NotImplementedException();
        }
        public void FindConstructor(ConstructorInfo constructor)
        {
            throw new NotImplementedException();
        }
        public void FindEvent(EventInfo eventInfo)
        {
            throw new NotImplementedException();
        }

        public static List<string> ConvertTypeToTokens(Type type)
        {
            List<string> toReturn = new List<string>();
            Type[] types = type.GenericTypeArguments;
            if (types.Length > 0)
            {
                toReturn.Add(type.Name.Substring(0, type.Name.IndexOf('`')));
                toReturn.Add("<");
                for (int i = 0; i < types.Length; i++)
                {
                    toReturn.AddRange(ConvertTypeToTokens(types[i]));
                    if (i + 1 < types.Length)
                        toReturn.Add(",");
                }
                toReturn.Add(">");
            }
            else
                toReturn.Add(type.Name);
            
            return toReturn;
        }
        public static List<string> ConvertNamespaceToTokens(string toTokenize)
        {
            List<string> tokens = new List<string>();
            string currentToken = "";
            foreach (char c in toTokenize)
            {
                if (c == '.')
                {
                    tokens.Add(currentToken);
                    tokens.Add(".");
                    currentToken = "";
                }
                else
                    currentToken += c;
            }
            if (currentToken.Length > 0)
                tokens.Add(currentToken);

            return tokens;
        }
        
        //Working on this currently
        public TokenInfo GetMemberInfoTokenImmediate(MemberInfo member)
        {
            if (member == null) return new TokenInfo();

            Type typeContext = member is TypeInfo typeInfo ? typeInfo : member.ReflectedType;
            if (!fileChars.Contains(typeContext.NameWithoutGenericOrArray())) return new TokenInfo();

            string basicName = member.Name;
            int genericSymbolIndex = basicName.IndexOf('`');
            if (genericSymbolIndex != -1) basicName = basicName.Substring(0, genericSymbolIndex);
            if (!fileChars.Contains(basicName)) return new TokenInfo();

            IEnumerable<TokenInfo> tokens = GetTokensInternal();

            tokens = RemoveComments(tokens);
            tokens = SelectNamespace(tokens, typeContext.Namespace);
            tokens = GetTypeContext(tokens, typeContext, removeComments: false);

            if (!tokens.Any()) return new TokenInfo();
            int inTypeContextDepth = tokens.First().contextStack.Length;
            bool inBrackets = false;
            bool isLambda = false;
            tokens.Where((token) =>
            {
                return true;
                int tokenDepth = token.contextStack.Length;
                if (isLambda)
                {
                    if (token.token == ";")
                    {
                        isLambda = false;
                        return true;
                    }
                    return false;
                }

                if (tokenDepth == inTypeContextDepth)
                    return true;
                if (inBrackets)
                {
                    if (inBrackets && token.context == Context.CurlyBrackets && tokenDepth == inTypeContextDepth + 1)
                        inBrackets = true;
                    return false;
                }
                else
                {
                    if (tokenDepth < inTypeContextDepth + 1)
                        inBrackets = false;
                    if (token.token == "=>")
                    {
                        isLambda = true;
                        return false;
                    }
                }
                return true;
            }
            );

            DebugPrintTokens(tokens, "After Hiding Brackets");

            if (member is MethodInfo method)
                tokens = AfterTheseTokens(tokens, method.Name);
            else if (member is FieldInfo field)
                tokens = AfterTheseTokens(tokens, member.Name);
            else if (member is PropertyInfo property)
                tokens = AfterTheseTokens(tokens, member.Name);
            else if (member is ConstructorInfo constructor)
                tokens = AfterTheseTokens(tokens, member.ReflectedType.Name);

            if (tokens.Any())
                return tokens.First();
            else
                return new TokenInfo();
        }
        public static string GetTypeKeyword(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            if (type.IsPrimitive) throw new ArgumentException();

            //Get type identifier token and add it to chain to find
            if (type.IsClass) return "class";
            if (type.IsInterface) return "interface";
            if (type.IsEnum) return "enum";
            if (typeof(Delegate).IsAssignableFrom(type))
                throw new NotImplementedException("Havent implemented searching for delegates");

            return "struct";
        }
        public static IEnumerable<TokenInfo> RemoveComments(IEnumerable<TokenInfo> tokens) =>
            tokens.Where((t) => t.context != Context.LineComment && t.context != Context.BlockComment).ToArray();
        public static IEnumerable<TokenInfo> AfterTheseTokens(IEnumerable<TokenInfo> tokens, params string[] toSearch)
        {
            int toSearchIndex = 0;
            foreach(TokenInfo token in tokens)
            {
                if (toSearchIndex == toSearch.Length)
                    yield return token;
                else
                {
                    if (token.token == toSearch[toSearchIndex])
                        toSearchIndex++;
                    else
                        toSearchIndex = 0;
                }
            }
        }
        public static IEnumerable<TokenInfo> BeforeTheseTokens(IEnumerable<TokenInfo> tokens, params string[] toSearch)
        {
            TokenInfo[] gatheredTokens = tokens.ToArray();
            for (int i = 0; i < gatheredTokens.Length - toSearch.Length + 1; i++)
            {
                yield return gatheredTokens[i];

                bool success = true;
                for (int e = 0; e < toSearch.Length; e++)
                {
                    if (gatheredTokens[i].token != toSearch[e])
                    {
                        success = false;
                        break;
                    }
                }
                if (success)
                    break;
            }
        }
        public static void DebugPrintTokens(IEnumerable<TokenInfo> tokens, string prefix)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("| ");
            sb.AppendJoin(" | ", tokens.Select((t) => t.token));
            sb.Replace(";", "; |\n");
            sb.Append(" |");
            Debug.Log(prefix + "\n" + sb.ToString());
        }
        public static int CountTokens(IEnumerable<TokenInfo> tokens, string toCount) =>
            tokens.Where((t) => t.token == toCount).Count();
        public static IEnumerable<TokenInfo> GetNextContextOf(IEnumerable<TokenInfo> tokens, Context contextToGet)
        {
            int contextDepth = 0;
            IEnumerator<TokenInfo> tokenEnumerator = tokens.GetEnumerator();
            bool enumeratorEnd = tokenEnumerator.MoveNext();

            while (!enumeratorEnd && tokenEnumerator.Current.context != contextToGet)
            {
                contextDepth = tokenEnumerator.Current.contextStack.Length;
                enumeratorEnd = !tokenEnumerator.MoveNext();
            }
            if (!enumeratorEnd)
            {
                yield return tokenEnumerator.Current;

                while (tokenEnumerator.MoveNext() && tokenEnumerator.Current.contextStack.Length >= contextDepth)
                    yield return tokenEnumerator.Current;
            }
            
            //foreach(TokenInfo token in tokens)
            //{
            //    if (!foundContext)
            //    {
            //        if (token.context == contextToGet)
            //        {
            //            startContext = token;
            //            yield return token;
            //            foundContext = true;
            //        }
            //    }
            //    else
            //    {
            //        if (token.contextStack.Length < startContext.contextStack.Length)
            //            break;
            //
            //        yield return token;
            //    }
            //}
        }
        public static IEnumerable<TokenInfo> GetTypeContext(IEnumerable<TokenInfo> tokensIn, Type typeToFind, bool removeComments = true)
        {
            IEnumerable<TokenInfo> tokens = tokensIn;
            //This requires no comments to work
            if (removeComments)
                tokens = RemoveComments(tokens);

            //Get the type keyword and name of type as strings to search for these as tokens later on. This is how we identify the class
            string typeKeyword = GetTypeKeyword(typeToFind);
            string typeName = typeToFind.NameWithoutGenericOrArray();
            bool isGeneric = typeToFind.IsGenericType;
            int commasToExpect = isGeneric ? typeToFind.GetGenericArguments().Length - 1 : 0;

            while (tokens.Any())
            {
                tokens = AfterTheseTokens(tokens, typeKeyword, typeName);
                if (!tokens.Any())
                    break;

                if (isGeneric ^ (tokens.First().token == "<"))
                    continue;

                if (isGeneric && CountTokens(GetNextContextOf(tokens, Context.GenericBracket), ",") != commasToExpect)
                    continue;

                return GetNextContextOf(tokens, Context.CurlyBrackets);
            }
            return tokens;
        }
        public static IEnumerable<TokenInfo> SelectNamespace(IEnumerable<TokenInfo> tokens, string namespaceToSelect)
        {
            //if namespace is null, remove all namespaces
            if(string.IsNullOrEmpty(namespaceToSelect))
            {
                int currentContextDepth = -1;
                bool lookingForBracket = false;
                foreach (TokenInfo token in tokens)
                {
                    if (currentContextDepth == -1)
                    {
                        if (lookingForBracket)
                        {
                            if (token.token == "{")
                            {
                                currentContextDepth = token.contextStack.Length;
                                lookingForBracket = false;
                            }
                            continue;
                        }
                        if (token.token == "namespace")
                        {
                            lookingForBracket = true;
                            continue;
                        }
                    }
                    else
                    {
                        if (token.token == "}" && currentContextDepth == token.contextStack.Length)
                            currentContextDepth = -1;
                        continue;
                    }
                    yield return token;
                }
            }
            else
            {
                string[] namespaceTokens = ConvertNamespaceToTokens(namespaceToSelect).ToArray();
                while (true)
                {
                    tokens = AfterTheseTokens(tokens, "namespace");
                    if (!tokens.Any()) break;

                    bool hasNamespaceTokens = true;
                    IEnumerator<TokenInfo> tokenEnumerator = tokens.GetEnumerator();
                    for (int i = 0; i < namespaceTokens.Length; i++)
                    {
                        if ((!tokenEnumerator.MoveNext()) || namespaceTokens[i] != tokenEnumerator.Current.token)
                        {
                            hasNamespaceTokens = false;
                            break;
                        }
                    }
                    if ((!hasNamespaceTokens) || (!tokenEnumerator.MoveNext()) || tokenEnumerator.Current.token == ".")
                        continue;

                    break;
                }
                if (tokens.Any())
                {
                    tokens = GetNextContextOf(tokens, Context.CurlyBrackets);
                    foreach (TokenInfo token in tokens)
                        yield return token;
                }
            }
        }


        public interface TokenParser { }
        public interface TokenFilter : TokenParser { public bool PassesFilter(TokenInfo token); }
        public interface TokenSearch : TokenParser { public bool PassesSearch(TokenInfo token); }
        
        public class IgnoreContextFilter : TokenFilter
        {
            private Context contextToFilter;
            public IgnoreContextFilter(Context contextToFilter) =>
                this.contextToFilter = contextToFilter;
            public bool PassesFilter(TokenInfo token) => token.context != contextToFilter;
        }
        public class FuncTokenFilter : TokenFilter
        {
            private Func<TokenInfo, bool> passesFilter;
            public FuncTokenFilter(Func<TokenInfo, bool> passesFilter) =>
                this.passesFilter = passesFilter;
            public bool PassesFilter(TokenInfo token) => passesFilter.Invoke(token);
        }
        public class FuncTokenSearch : TokenSearch
        {
            private Func<TokenInfo, bool> passesSearch;
            public FuncTokenSearch(Func<TokenInfo, bool> passesSearch) =>
                this.passesSearch = passesSearch;
            public bool PassesSearch(TokenInfo token) => passesSearch.Invoke(token);
        }

        public class MatchesTokenFilter : TokenFilter
        {
            private string passingToken;
            public MatchesTokenFilter(string passingToken) =>
                this.passingToken = passingToken;
            public bool PassesFilter(TokenInfo token) => token.token == passingToken;
        }
        public class SearchForMatchingToken : TokenSearch
        {
            private string passingToken;
            public SearchForMatchingToken(string passingToken) =>
                this.passingToken = passingToken;

            public bool PassesSearch(TokenInfo token) => token.token == passingToken;
        }
        public class SkipTokens : TokenSearch
        {
            private int toSkip;
            private int haveSkipped = 0;
            public SkipTokens(int toSkip) => this.toSkip = toSkip;

            public bool PassesSearch(TokenInfo token)
            {
                if (haveSkipped >= toSkip)
                    return true;
                haveSkipped++;
                return false;
            }
        }
        public class SkipContext : TokenSearch
        {
            private Context toSkip;
            private bool hasReachedContext;
            public SkipContext(Context toSkip, bool findContextFirst)
            {
                this.toSkip = toSkip;
                hasReachedContext = !findContextFirst;
            }

            public bool PassesSearch(TokenInfo token)
            {
                bool isInContext = token.context == toSkip;
                hasReachedContext |= isInContext;
                return hasReachedContext && !isInContext;
            }
        }
        public class SearchForBackwardParseMatch : TokenSearch
        {
            TokenInfo searchResult = new();

            private List<TokenParser> parsers = new List<TokenParser>();
            private Func<List<TokenInfo>> getPreviousTokens;

            public SearchForBackwardParseMatch(Func<List<TokenInfo>> getPreviousTokens) =>
                this.getPreviousTokens = getPreviousTokens;

            public void AddSearch(TokenSearch search) => parsers.Add(search);
            public void AddFilter(TokenFilter search) => parsers.Add(search);

            public bool PassesSearch(TokenInfo token)
            {
                searchResult = token;

                Stack<TokenParser> parseStack = new Stack<TokenParser>();
                foreach(TokenParser parser in parsers)
                    parseStack.Push(parser);

                List<TokenInfo> tokens = getPreviousTokens.Invoke();
                for (int i = tokens.Count - 1; i >= 0; i--)
                {
                    TokenInfo previousToken = tokens[i];

                    TokenParser currentParse = parseStack.Peek();
                    if (currentParse is TokenFilter filter)
                    {
                        if (!filter.PassesFilter(previousToken))
                            return false;
                    }
                    else if (currentParse is TokenSearch search)
                    {
                        if (search.PassesSearch(previousToken))
                            i = Mathf.Min(i + 1, tokens.Count);
                        else
                            continue;
                    }
                    parseStack.Pop();
                    if (parseStack.Count == 0) 
                        return true;
                }
                return true;
            }

            public TokenInfo GetResult() => searchResult;
        }

        public class IsEndOfTypeTokens : TokenFilter
        {
            TokenInfo searchResult = new();
            private Type type;
            private Func<List<TokenInfo>> getPreviousTokens;
            private List<TokenInfo> tokens;

            

            public IsEndOfTypeTokens(Type type, Func<List<TokenInfo>> getPreviousTokens)
            {
                this.type = type;
                this.getPreviousTokens = getPreviousTokens;
            }

            public bool PassesFilter(TokenInfo token)
            {
                tokens = getPreviousTokens.Invoke();
                return RecursiveTypeSearch(type, tokens.Count - 1);
            }

            private bool RecursiveTypeSearch(Type type, int index)
            {
                TokenInfo tokenInfo = tokens[index];

                List<string> toReturn = new List<string>();
                Type[] types = type.GenericTypeArguments;
                if (types.Length > 0)
                {
                    toReturn.Add(type.Name.Substring(0, type.Name.IndexOf('`')));
                    toReturn.Add("<");
                    for (int i = 0; i < types.Length; i++)
                    {
                        toReturn.AddRange(ConvertTypeToTokens(types[i]));
                        if (i + 1 < types.Length)
                            toReturn.Add(",");
                    }
                    toReturn.Add(">");
                }
                else
                {

                }

                return true;
            }

            private bool IsNonGenericMatch(string combinedTokens, Type type)
            {
                throw new NotImplementedException();
            }

            public TokenInfo GetResult() => searchResult;
        }


        public enum Context
        {
            Parenthesis,
            CurlyBrackets,
            SquareBrackets,
            GenericBracket,
            String,
            Char,
            LineComment,
            BlockComment,
            WholeFile,
            None
        }

        public MonoScriptReader(MonoScript script) => fileChars = script.text;
        public MonoScriptReader(string assetPath) => fileChars = System.IO.File.ReadAllText(assetPath);

        public IEnumerable<TokenInfo> GetTokens()
        {
            foreach (TokenInfo tokenInfo in GetTokensInternal())
            {
                Debug.Log(tokenInfo);
                while(s_searchQueue.TryPeek(out TokenSearch search))
                {
                    if (search.PassesSearch(tokenInfo))
                        s_searchQueue.Dequeue();
                    else
                        break;
                }
                if (s_searchQueue.Count > 0)
                    continue;

                bool skipToken = false;
                foreach(TokenFilter filter in s_permanentFilters)
                {
                    if (!filter.PassesFilter(tokenInfo))
                    {
                        skipToken = true;
                        break;
                    }
                }
                if (!skipToken)
                    yield return tokenInfo;
            }
        }
        
        public TokenInfo[] GetFilteredTokens()
        {
            List<TokenInfo> tokens = new List<TokenInfo>();
            foreach(TokenInfo token in GetTokens())
            {
                tokens.Add(token);
            }
            return tokens.ToArray();
        }

        private IEnumerable<TokenInfo> GetTokensInternal()
        {
            InitializeTokenScan();
            for (info.charPosition = 0; info.charPosition < fileChars.Length; info.charPosition++)
            {
                thisChar = fileChars[info.charPosition];
                lastChar = info.charPosition == 0 ? ' ' : fileChars[info.charPosition - 1];
                info.contextStack = contextStack.ToArray();

                if (doClearToken)
                {
                    info.token = string.Empty;
                    doClearToken = false;
                }
                if (lastChar == '\n') info.linePosition++;

                if (ShouldLeaveContext())
                {
                    if (GrabsAllCharacters())
                    {
                        if (thisChar != '\n')
                            info.token += thisChar.ToString();
                        yield return PassToken();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(info.token))
                            yield return PassToken();

                        info.token = thisChar.ToString();
                        yield return PassToken();
                    }
                    contextStack.Pop();
                    continue;
                }

                //If in BlockComment context : Halt to add chars to token until "*/" is found
                if (info.context == Context.BlockComment)
                {
                    if (lastChar == '*' && thisChar == '/')
                        yield return PassToken();
                    else
                        info.token += thisChar;

                    continue;
                }
                //If "/*" is found, switch to BlockComment context
                if (!GrabsAllCharacters() && lastChar == '/' && thisChar == '*')
                {
                    info.token += thisChar;
                    contextStack.Push(Context.BlockComment);
                    continue;
                }

                bool isCurrentlyAlpha = IsAlphaChar(thisChar);
                bool isFirstCharInToken = string.IsNullOrEmpty(info.token);
                bool isWhitespace = char.IsWhiteSpace(thisChar);

                if (isFirstCharInToken)
                    isAlphaToken = isCurrentlyAlpha;
                if (GrabsAllCharacters())
                {
                    info.token += thisChar;
                    continue;
                }
                if (!isFirstCharInToken && isWhitespace)
                {
                    yield return PassToken();
                    continue;
                }
                if (!isFirstCharInToken && (isAlphaToken ^ isCurrentlyAlpha))
                {
                    yield return PassToken();
                    isAlphaToken = isCurrentlyAlpha;
                }
                if (!isWhitespace)
                    info.token += thisChar;

                if (CheckForContextChange())
                {
                    if (!GrabsAllCharacters())
                        yield return PassToken();

                    continue;
                }
            }
        }
        private void InitializeTokenScan()
        {
            previousTokens = new List<TokenInfo>();
            contextStack = new Stack<Context>();
            contextStack.Push(Context.WholeFile);
            info = new();
            info.token = string.Empty;
            info.charPosition = 0;
            info.linePosition = 1;
            isAlphaToken = false;
        }
        private TokenInfo PassToken()
        {
            TokenInfo copiedToken = info;
            copiedToken.contextStack = contextStack.ToArray();
            previousTokens.Add(copiedToken);
            info.token = string.Empty;
            isAlphaToken = false;
            return copiedToken;
        }
        
        private bool IsAlphaChar(char someChar) =>
            char.IsLetterOrDigit(someChar) || someChar == '_';
        private bool ShouldLeaveContext()
        {
            switch (contextStack.Peek())
            {
                case Context.None: return false;
                case Context.WholeFile: return false;
                case Context.Parenthesis: return thisChar == ')';
                case Context.CurlyBrackets: return thisChar == '}';
                case Context.SquareBrackets: return thisChar == ']';
                case Context.GenericBracket: return thisChar == '>';
                case Context.String: return thisChar == '\"' && lastChar != '\\';
                case Context.Char: return thisChar == '\'' && lastChar != '\\';
                case Context.LineComment: return thisChar == '\n';
                case Context.BlockComment: return lastChar == '*' && thisChar == '/';
                default: throw new NotImplementedException();
            }
        }
        private bool GrabsAllCharacters()
        {
            switch (contextStack.Peek())
            {
                case Context.None: return false;
                case Context.WholeFile: return false;
                case Context.Parenthesis: return false;
                case Context.CurlyBrackets: return false;
                case Context.SquareBrackets: return false;
                case Context.GenericBracket: return false;
                case Context.String: return true;
                case Context.Char: return true;
                case Context.LineComment: return true;
                case Context.BlockComment: return true;
                default: throw new NotImplementedException();
            }
        }
        private bool CheckForContextChange()
        {
            if (lastChar == '\\') return false;

            if (thisChar == '(') { contextStack.Push(Context.Parenthesis); return true; }
            if (thisChar == '{') { contextStack.Push(Context.CurlyBrackets); return true; }
            if (thisChar == '[') { contextStack.Push(Context.SquareBrackets); return true; }
            if (thisChar == '<') { contextStack.Push(Context.GenericBracket); return true; }
            if (thisChar == '\"') { contextStack.Push(Context.String); return true; }
            if (thisChar == '\'') { contextStack.Push(Context.Char); return true; }

            if (lastChar == '/')
            {
                if (thisChar == '/') { contextStack.Push(Context.LineComment); return true; }
            }

            return false;
        }
    }

    public static class MonoScriptReaderExtensions
    {

    }
}