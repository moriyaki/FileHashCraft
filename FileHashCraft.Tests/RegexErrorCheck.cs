using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit.Sdk;

/*
AlternationHasComment	            17	正規表現の交替構文にはコメントが含まれます。
AlternationHasMalformedCondition	2	正規表現内の交互配列に不正な条件があります。
AlternationHasMalformedReference	18	正規表現内の交替構文に不正な参照が含まれています。
AlternationHasNamedCapture	        16	正規表現における交替構文は、名前付きキャプチャを使用します。
AlternationHasTooManyConditions	    1	正規表現の交替は条件が多すぎます。
AlternationHasUndefinedReference	19	正規表現の交替構文に未定義の参照が含まれています。
CaptureGroupNameInvalid	            20	正規表現でキャプチャしたグループ名が無効です。
CaptureGroupOfZero	                21	正規表現は、ゼロという番号の付いた部分式を定義します。
ExclusionGroupNotLast	            23	除外グループを持つ正規表現の文字クラスは、文字クラスの最後の部分ではありません。
InsufficientClosingParentheses	    26	正規表現の左括弧がエスケープされていないか、右括弧が閉じられていません。
InsufficientOpeningParentheses	    30	正規表現にエスケープされていない右括弧があるか、左括弧がありません。
InsufficientOrInvalidHexDigits	    8	正規表現内の16進数エスケープシーケンスの桁数が足りないか、無効な桁が含まれています。
InvalidGroupingConstruct	        15	正規表現内のグループ化構文が無効または不正です。
InvalidUnicodePropertyEscape	    3	正規表現内の Unicode プロパティ・エスケープが無効または不明です。
MalformedNamedReference	            12	正規表現内の名前付き参照が不正です。
MalformedUnicodePropertyEscape	    4	Unicode プロパティのエスケープが不正です。
MissingControlCharacter	            7	正規表現の制御文字がありません。
NestedQuantifiersNotParenthesized	28	正規表現内で別の量化子の上に繰り返される量化子は、括弧でグループ化されていません。
QuantifierAfterNothing	            29	正規表現中の量化子は、正規表現の先頭やグループの中など、何かを量化できない位置にあります。
QuantifierOrCaptureGroupOutOfRange	9	正規表現内の捕捉グループまたは量化子が範囲内にない、つまりInt32.MaxValueより大きいです。
ReversedCharacterRange	            24	正規表現の文字クラスは、a-zの代わりにz-aのように、逆の文字範囲を含む。
ReversedQuantifierRange	            27	正規表現における量化子の範囲が、{1,10}の代わりに{10,1}のように逆数である。
ShorthandClassInCharacterRange	    25	正規表現の文字クラスは、文字クラスの内部では許可されないショートハンド・クラスを含んでいます。
UndefinedNamedReference	            10	正規表現で使用される名前付き参照が定義されていません。
UndefinedNumberedReference	        11	正規表現で使用される番号参照が定義されていません。
UnescapedEndingBackslash	        13	正規表現の最後は、エスケープされていないバックスラッシュで終わっています。
Unknown	                            0	不明な正規表現の解析エラーです。
UnrecognizedControlCharacter	    6	正規表現内の制御文字が認識されません。
UnrecognizedEscape	                5	正規表現内のエスケープ文字またはシーケンスが無効です。
UnrecognizedUnicodeProperty	        31	正規表現の unicode プロパティが認識されないか、無効です。
UnterminatedBracket	                22	正規表現の左角括弧がエスケープされていないか、右角括弧が閉じられていません。
UnterminatedComment	                14	正規表現中のコメントは終了しません。

+public enum RegexParseError
{
    発生条件がわからない3つ
+    UnknownParseError = 0,             // 他の条件が出現した場合に備えて、このキャッチオール機能を追加したいのか？
+    AlternationHasComment,
+    AlternationHasMalformedCondition,  // *もしかして？テストなし、コードはヒットしない
}
 */

namespace FileHashCraft.Tests
{
#pragma warning disable RCS1118 // const しろと五月蠅い
    public class RegexErrorCheck
    {
     
        /*
        /// <summary>
        /// AlternationHasComment,
        /// </summary>
        [Fact]
        public void AlternationHasCommentError()
        {
            var regexString = "(abc|d(?#comment)ef)xyz";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.AlternationHasComment, regException.Error);
                return;
            }
            Assert.Fail("Failed AlternationHasComment");
        }
        /// <summary>
        /// AlternationHasMalformedCondition,  // like "(?(99)def|ghi)"
        /// </summary>
        [Fact]
        public void AlternationHasMalformedConditionError()
        {
            var regexString = "(?(99)def|ghi)";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.AlternationHasMalformedCondition, regException.Error);
                return;
            }
            Assert.Fail("Failed AlternationHasMalformedCondition");
        }
        */
        /// <summary>
        /// AlternationHasMalformedReference,  // like @"(x)(?(3x|y)" (note that @"(x)(?(3)x|y)" gives next error)
        /// </summary>
        [Fact]
        public void AlternationHasMalformedReferenceEror()
        {
            var regexString = "(x)(?(3x|y)";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.AlternationHasMalformedReference, regException.Error);
                return;
            }
            Assert.Fail("Failed AlternationHasMalformedReference");
        }
        /// <summary>
        /// AlternationHasNamedCapture,        // like @"(?(?<x>)true|false)"
        /// </summary>
        [Fact]
        public void AlternationHasNamedCaptureError()
        {
            var regexString = "(?(?<x>)true|false)";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.AlternationHasNamedCapture, regException.Error);
                return;
            }
            Assert.Fail("Failed AlternationHasNamedCapture");
        }
        /// <summary>
        /// AlternationHasTooManyConditions,   // like @"(?(foo)a|b|c)"
        /// </summary>
        [Fact]
        public void AlternationHasTooManyConditionsError()
        {
            var regexString = "(?(foo)a|b|c)";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.AlternationHasTooManyConditions, regException.Error);
                return;
            }
            Assert.Fail("Failed AlternationHasTooManyConditions");
        }
        /// <summary>
        /// AlternationHasUndefinedReference,  // like @"(x)(?(3)x|y)" or @"(?(1))"
        /// </summary>
        [Fact]
        public void AlternationHasUndefinedReferenceError()
        {
            var regexString = "(?(1))";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.AlternationHasUndefinedReference, regException.Error);
                return;
            }
            Assert.Fail("Failed AlternationHasUndefinedReference");
        }
        /// <summary>
        /// CaptureGroupNameInvalid,           // like @"(?< >)" or @"(?'x)"
        /// </summary>
        [Fact]
        public void CaptureGroupNameInvalidError()
        {
            var regexString = "(?'x)";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.CaptureGroupNameInvalid, regException.Error);
                return;
            }
            Assert.Fail("Failed CaptureGroupNameInvalid");
        }
        /// <summary>
        /// CaptureGroupOfZero,                // like @"(?'0'foo)" or @("(?<0>x)"
        /// </summary>
        [Fact]
        public void CaptureGroupOfZeroError()
        {
            var regexString = "(?'0'foo)";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.CaptureGroupOfZero, regException.Error);
                return;
            }
            Assert.Fail("Failed CaptureGroupOfZero");
        }
        /// <summary>
        /// InsufficientOrInvalidHexDigits,    // like @"\uabc" or @"\xr"
        /// </summary>
        [Fact]
        public void InsufficientOrInvalidHexDigitsError()
        {
            var regexString = @"\xr";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.InsufficientOrInvalidHexDigits, regException.Error);
                return;
            }
            Assert.Fail("Failed InsufficientOrInvalidHexDigits");
        }
        /// <summary>
        /// InsufficientClosingParentheses,    // like @"(((foo))"
        /// </summary>
        [Fact]
        public void InsufficientClosingParenthesesError()
        {
            var regexString = "(((foo))";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.InsufficientClosingParentheses, regException.Error);
                return;
            }
            Assert.Fail("Failed InsufficientClosingParentheses");
        }
        /// <summary>
        /// InsufficientOpeningParentheses,    // like @"((foo)))"
        /// </summary>
        [Fact]
        public void InsufficientOpeningParenthesesError()
        {
            var regexString = "((foo)))";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.InsufficientOpeningParentheses, regException.Error);
                return;
            }
            Assert.Fail("Failed InsufficientOpeningParentheses");
        }
        /// <summary>
        /// InvalidGroupingConstruct,          // like @"(?" or @"(?<foo"
        /// </summary>
        [Fact]
        public void InvalidGroupingConstructError()
        {
            var regexString = "(?";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.InvalidGroupingConstruct, regException.Error);
                return;
            }
            Assert.Fail("Failed InvalidGroupingConstruct");
        }
        /// <summary>
        /// InvalidUnicodePropertyEscape,      // like @"\p{Ll" or @"\p{ L}"
        /// </summary>
        [Fact]
        public void InvalidUnicodePropertyEscapeError()
        {
            var regexString = @"\p{ L}";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.InvalidUnicodePropertyEscape, regException.Error);
                return;
            }
            Assert.Fail("Failed InvalidUnicodePropertyEscape");
        }
        /// <summary>
        /// MalformedNamedReference,           // like @"\k<"
        /// </summary>
        [Fact]
        public void MalformedNamedReferenceError()
        {
            var regexString = @"\k<";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.MalformedNamedReference, regException.Error);
                return;
            }
            Assert.Fail("Failed MalformedNamedReference");
        }
        /// <summary>
        /// MalformedUnicodePropertyEscape,    // like @"\p{}" or @"\p {L}"
        /// </summary>
        [Fact]
        public void MalformedUnicodePropertyEscapeError()
        {
            var regexString = @"\p {L}";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.MalformedUnicodePropertyEscape, regException.Error);
                return;
            }
            Assert.Fail("Failed MalformedUnicodePropertyEscape");
        }
        /// <summary>
        /// MissingControlCharacter,           // like @"\c"
        /// </summary>
        [Fact]
        public void MissingControlCharacterError()
        {
            var regexString = @"\c";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.MissingControlCharacter, regException.Error);
                return;
            }
            Assert.Fail("Failed MissingControlCharacter");
        }
        /// <summary>
        /// NestedQuantifiersNotParenthesized  // @"abc**"
        /// </summary>
        [Fact]
        public void NestedQuantifiersNotParenthesizedError()
        {
            var regexString = "abc**";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.NestedQuantifiersNotParenthesized, regException.Error);
                return;
            }
            Assert.Fail("Failed NestedQuantifiersNotParenthesized");
        }
        /// <summary>
        /// QuantifierAfterNothing,            // like @"((*foo)bar)"
        /// </summary>
        [Fact]
        public void QuantifierAfterNothingError()
        {
            var regexString = "((*foo)bar)";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.QuantifierAfterNothing, regException.Error);
                return;
            }
            Assert.Fail("Failed QuantifierAfterNothing");
        }
        /// <summary>
        /// QuantifierOrCaptureGroupOutOfRange,// like @"x{234567899988}" or @"x(?<234567899988>)" (must be < Int32.MaxValue)
        /// </summary>
        [Fact]
        public void QuantifierOrCaptureGroupOutOfRangeError()
        {
            var regexString = "x{234567899988}";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.QuantifierOrCaptureGroupOutOfRange, regException.Error);
                return;
            }
            Assert.Fail("Failed QuantifierOrCaptureGroupOutOfRange");
        }
        /// <summary>
        /// ReversedCharacterRange,            // like @"[z-a]"   (only in char classes, see also ReversedQuantifierRange)
        /// </summary>
        [Fact]
        public void ReversedCharacterRangeError()
        {
            var regexString = "[z-a]";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.ReversedCharacterRange, regException.Error);
                return;
            }
            Assert.Fail("Failed ReversedCharacterRange");
        }
        /// <summary>
        /// ReversedQuantifierRange,           // like @"abc{3,0}"  (only in quantifiers, see also ReversedCharacterRange)
        /// </summary>
        [Fact]
        public void ReversedQuantifierRangeError()
        {
            var regexString = "abc{3,0}";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.ReversedQuantifierRange, regException.Error);
                return;
            }
            Assert.Fail("Failed ReversedQuantifierRange");
        }
        /// <summary>
        /// ShorthandClassInCharacterRange,    // like @"[a-\w]" or @"[a-\p{L}]"
        /// </summary>
        [Fact]
        public void ShorthandClassInCharacterRangeError()
        {
            var regexString = @"[a-\w]";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.ShorthandClassInCharacterRange, regException.Error);
                return;
            }
            Assert.Fail("Failed ShorthandClassInCharacterRange");
        }
        /// <summary>
        /// UndefinedNamedReference,           // like @"\k<x>"
        /// </summary>
        [Fact]
        public void UndefinedNamedReferenceError()
        {
            var regexString = @"\k<x>";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UndefinedNamedReference, regException.Error);
                return;
            }
            Assert.Fail("Failed UndefinedNamedReference");
        }
        /// <summary>
        /// UndefinedNumberedReference,        // like @"(x)\2"
        /// </summary>
        [Fact]
        public void UndefinedNumberedReferenceError()
        {
            var regexString = @"(x)\2";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UndefinedNumberedReference, regException.Error);
                return;
            }
            Assert.Fail("Failed UndefinedNumberedReference");
        }
        /// <summary>
        /// UnescapedEndingBackslash,          // like @"foo\" or @"bar\\\\\"
        /// </summary>
        [Fact]
        public void UnescapedEndingBackslashError()
        {
            var regexString = @"foo\";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UnescapedEndingBackslash, regException.Error);
                return;
            }
            Assert.Fail("Failed UnescapedEndingBackslash");
        }
        /// <summary>
        /// UnrecognizedControlCharacter,      // like @"\c!"
        /// </summary>
        [Fact]
        public void UnrecognizedControlCharacterError()
        {
            var regexString = @"\c!";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UnrecognizedControlCharacter, regException.Error);
                return;
            }
            Assert.Fail("Failed UnrecognizedControlCharacter");
        }
        /// <summary>
        /// UnrecognizedEscape,                // like @"\C" or @"\k<" or @"[\B]"
        /// </summary>
        [Fact]
        public void UnrecognizedEscapeError()
        {
            var regexString = @"\C";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UnrecognizedEscape, regException.Error);
                return;
            }
            Assert.Fail("Failed UnrecognizedEscape");
        }
        /// <summary>
        /// UnrecognizedUnicodeProperty,       // like @"\p{Lll}"
        /// </summary>
        [Fact]
        public void UnrecognizedUnicodePropertyError()
        {
            var regexString = @"\p{Lll}";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UnrecognizedUnicodeProperty, regException.Error);
                return;
            }
            Assert.Fail("Failed UnrecognizedUnicodeProperty");
        }
        /// <summary>
        /// UnterminatedBracket,               // like @"[a-b"
        /// </summary>
        [Fact]
        public void UnterminatedBracketError()
        {
            var regexString = "[a-b";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UnterminatedBracket, regException.Error);
                return;
            }
            Assert.Fail("Failed UnterminatedBracket");
        }
        /// <summary>
        /// UnterminatedComment,               // like @"(?#comment .*"
        /// </summary>
        [Fact]
        public void UnterminatedCommentTest()
        {
            var regexString = "(?#comment .*";
            try
            {
                var regex = new Regex(regexString);
            }
            catch (RegexParseException regException)
            {
                Assert.Equal(RegexParseError.UnterminatedComment, regException.Error);
                return;
            }
            Assert.Fail("Failed UnterminatedComment");
        }
    }
#pragma warning restore RCS1118
}
