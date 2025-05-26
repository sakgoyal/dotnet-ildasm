using System;
using System.Text.RegularExpressions;

namespace DotNet.Ildasm
{
    public partial class AutoIndentOutputWriter(IOutputWriter writer) : IOutputWriter
    {
        private readonly IOutputWriter _writer = writer;
        private readonly int _numSpaces = 2;
        private int _currentLevel = 0;

        public void Dispose()
        {
            _writer?.Dispose();
        }

        public void Write(string value)
        {
            Apply(value);
        }

        public void WriteLine(string value)
        {
            Apply(value);
            _writer.Write(Environment.NewLine);
        }

        public void Apply(string code)
        {
            var alreadyUpdatedIndentation = false;

            if (IsSingleLineBreakRequired(code))
                _writer.WriteLine(string.Empty);

            if (IsDoubleLineBreakRequired(code))
            {
                _writer.WriteLine(string.Empty);
                _writer.Write(Environment.NewLine);
            }

            if (code.StartsWith('}'))
            {
                alreadyUpdatedIndentation = true;
                UpdateIndentationLevel(code);
            }

            var totalIndentation = _currentLevel * _numSpaces;
            if (IsIndentationRequired(code))
                _writer.Write(code.PadLeft(code.Length + totalIndentation));
            else
                _writer.Write(code);

            if (!alreadyUpdatedIndentation)
                UpdateIndentationLevel(code);
        }

        private static bool IsSingleLineBreakRequired(string code)
        {
            return RegexSingle().IsMatch(code);
        }

        private static bool IsDoubleLineBreakRequired(string code)
        {
            return RegexLineBr().IsMatch(code);
        }

        private static bool IsIndentationRequired(string code)
        {
            return RegexIndent().IsMatch(code);
        }

        private void UpdateIndentationLevel(string code)
        {
            var openBracketMatches = Regex.Matches(code, "{");
            var closeBracketMatches = Regex.Matches(code, "}");

            var delta = openBracketMatches.Count - closeBracketMatches.Count;
            _currentLevel += delta;

            if (_currentLevel < 0)
                _currentLevel = 0;
        }

        [GeneratedRegex("^(catch|IL|\\.|//|{|}){1}", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
        private static partial Regex RegexSingle();

        [GeneratedRegex("^(.field|.method|.property|.event){1}", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
        private static partial Regex RegexIndent();
		[GeneratedRegex("^(.class|.assembly|.module){1}", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
		private static partial Regex RegexLineBr();
	}
}
