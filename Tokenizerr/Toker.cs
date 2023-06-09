﻿/////////////////////////////////////////////////////////////////////////////////////
// Toker.cs:    This package provides the state based tokenizer using various       /
//             different methods and derived classes and which is then passed to    /
//             semiexpression package for the further analysis.                     /                                          
// version:    1.2                                                                  /              
// Languange:   C#, Visual Studio 2017                                              /
// Platform:    HP pavilion X360, Windows 10                                        /
// Application: Demonstatration of Project2 - Lexical Scanner Using State Based     /
//              Tokenizer.                                                          /
// Source:      Dr.JIm Fawcett                                                      /
// Author Name: Amruta Joshi                                                        /
// CSE681 :     Software Modeling and Analysis, Fall 2018                           /  
/////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Demonstrates how to build a tokenizer based on the State Pattern and 
 * classifies the different states of the tokens collected which can be 
 * sent to semiExpression for the further processing.
 * 
 * Public Interface:
 * -----------------
 * Toker toker = new Toker();
 * toker.getToken();
 * toker.open(fullpath);
 * toker.lineCount();
 * toker.isDone()
 * toker.getTok()
 * 
 * 
 * Required Files :  ITok.cs
 * 
 * 
 * 
 * Maintenance History
 * ------------------------------------------------------------------------------
 * ver 1.3:  07 October 2018
 * - added new states namely DoublePunctState, SingleCharacterState, CCommentState, CPPCommentState,
 *    DoubleQuoteState.
 * ver 1.2 : 03 Sep 2018
 * - added comments just above the definition of derived states, near line #209
 * ver 1.1 : 02 Sep 2018
 * - Changed Toker, TokenState, TokenFileSource, and TokenContext to fix a bug
 *   in setting the initial state.  These changes are cited, below.
 * - Removed TokenState state_ from toker so only TokenContext instance manages 
 *   the current state.
 * - Changed TokenFileSource() to TokenFileSource(TokenContext context) to allow the 
 *   TokenFileSource instance to set the initial state correctly.
 * - Changed TokenState.nextState() to static TokenState.nextState(TokenContext context).
 *   That allows TokenFileSource to use nextState to set the initial state correctly.
 * - Changed TokenState.nextState(context) to treat everything that is not whitespace
 *   and is not a letter or digit as punctuation.  Char.IsPunctuation was not inclusive
 *   enough for Toker.
 * - changed current_ to currentState_ for readability
 * ver 1.0 : 30 Aug 2018
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tokenizer;

namespace Tokenizer
{
    using Token = StringBuilder;

    //////////////////////////////////////////////////////////////////////
    // Toker class
    // - applications need to use only this class to collect tokens.

    public class Toker
    {
        private TokenContext context_;       // holds single instance of all states and token source

        //----< initialize state machine >-------------------------------

        public Toker()
        {
            context_ = new TokenContext();      // context is the glue that holds all of the state machine parts 
        }

        //----< attempt to open source of tokens >-----------------------
        /*
         * If src is successfully opened, it uses TokenState.nextState(context_)
         * to set the initial state, based on the source content.
         */

        public bool open(string path)
        {
            TokenSourceFile src = new TokenSourceFile(context_);
            context_.src = src;
            return src.open(path);
        }

        //----< close source of tokens >---------------------------------

        public void close()
        {
            context_.src.close();
        }
        //----< extract a token from source >----------------------------

        private bool isWhiteSpaceToken(Token tok)
        {
            return (tok.Length > 0 && Char.IsWhiteSpace(tok[0]));
        }
        // ------<get the next state of the tokens>-----------------------------

        public Token getTok()
        {
            Token tok = null;
            while (!isDone())
            {
                tok = context_.currentState_.getTok();
                context_.currentState_ = TokenContext.TokenState.nextState(context_);
                if (!isWhiteSpaceToken(tok))
                    break;
            }
            return tok;
        }

        //----< has Toker reached end of its source? >-------------------


        public bool isDone()
        {
            if (context_.currentState_ == null)
                return true;
            return context_.currentState_.isDone();
        }
        public int lineCount() { return context_.src.lineCount; }


        // ------< Get all the tokens to pass it to semiexpression.>-------------------- 

        public Token GetSemi(Toker toker)
        {
            Token tok = null;
            while (!toker.isDone())
            {
                tok = toker.getTok();
                Console.Write("\n -- line#{0, 4} : {1}", toker.lineCount(), tok);
                return tok;
            }
            toker.close();
            return tok;
        }

        //---------<List of all the Tokens>---------------------------------

        public List<Token> getToken()
        {
            List<Token> list = new List<Token>();
            while (!isDone())
            {
                Token tok = getTok();
                Console.Write("\n -- line#{0, 4} : {1}", lineCount(), tok);
                list.Add(tok);
            }
            //close();
            return list;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // TokenContext class
    // - holds all the tokenizer states
    // - holds source of tokens
    // - internal qualification limits access to this assembly

    public class TokenContext
    {
        internal TokenContext()
        {
            ws_ = new WhiteSpaceState(this);
            ps_ = new PunctState(this);
            as_ = new AlphaState(this);
            dp_ = new DoublePunctState(this);
            ssc_ = new SingleSpecialChar(this);
            ccom_ = new CCommentsState(this);
            cpp_ = new CppCommentsState(this);
            dq_ = new DoubleQuoteState(this);
            po_ = new PoundState(this);
            currentState_ = ws_;
        }

        internal WhiteSpaceState ws_ { get; set; }
        internal PunctState ps_ { get; set; }
        internal AlphaState as_ { get; set; }
        internal DoublePunctState dp_ { get; set; }
        internal SingleSpecialChar ssc_ { get; set; }
        internal CCommentsState ccom_ { get; set; }
        internal CppCommentsState cpp_ { get; set; }
        internal DoubleQuoteState dq_ { get; set; }
        internal PoundState po_ { get; set; }
        internal TokenState currentState_ { get; set; }
        internal TokenInterface.ITokenSource src { get; set; }  // can hold any derived class

        //-------------<Get and set property for tokens.>------------------------

        String temptokens = "";
        public string gettemptokens()
        {
            return temptokens;
        }
        public void settemptokens(string temptoks)
        {
            this.temptokens = temptoks;
        }


        ///////////////////////////////////////////////////////////////////
        // TokenState class
        // - base for all the tokenizer states

        public abstract class TokenState : TokenInterface.ITokenState
        {

            internal TokenContext context_ { get; set; }  // derived classes store context ref here

            //----< delegate source opening to context's src >---------------

            public bool open(string path)
            {
                return context_.src.open(path);
            }
            //----< pass interface's requirement onto derived states >-------

            public abstract Token getTok();

            //----< derived states don't have to know about other states >---

            static public TokenState nextState(TokenContext context)
            {
                int nextItem = context.src.peek();
                if (nextItem < 0)
                    return null;
                char ch = (char)nextItem;

                if (Char.IsWhiteSpace(ch))
                    return context.ws_;
                if (Char.IsLetterOrDigit(ch))
                    return context.as_;
                if (context.dp_.isdoubleChar())
                    return context.dp_;
                if (context.ssc_.issingleChar(nextItem))
                    return context.ssc_;
                if (context.ccom_.isCComment())
                    return context.ccom_;
                if (context.cpp_.isCppComment())
                    return context.cpp_;
                if (context.dq_.isDoubleQuote())
                    return context.dq_;
                if (context.po_.isPound())
                    return context.po_;

                return context.ps_;
            }

            //----< has tokenizer reached the end of its source? >-----------

            public bool isDone()
            {
                if (context_.src == null)
                    return true;
                return context_.src.end();
            }
        }
        ///////////////////////////////////////////////////////////////////
        // Derived State Classes
        // * - WhiteSpaceState          Token with space, tab, and newline chars
        // * - AlphaNumstate            Token with letters and digits
        // * -DoubleCharacterState      Token with the double characters like (==, !=,.....)
        //  * -SingleCHaracterState      Token with the single characters like(<, >, =,.....)
        //  * -SingleCHaracterState      Token with the single characters like(<, >, =,.....)
        //  * -SingleCHaracterState      Token with the single characters like(<, >, =,.....)
        //  * -CComment State            Token with the C Comments (//)
        //  * -CppCommentState           Token with the CPP Comments(/*.....*/)
        //  * -DoubleQuoteState          Token with double Quotes("......")
        //  * - PunctuationState         Token holding anything not included above
        //  * ----------------------------------------------------------------

        //  * - Each state class accepts a reference to the context in its
        //  * constructor and saves in its inherited context_ property.
        //  * - It is only required to provide a getTok() method which
        //  * returns a token conforming to its state, e.g., whitespace, ...
        //  * - getTok() assumes that the TokenSource's first character 
        //  * -  matches its type e.g., whitespace char, ...
        //  * - The nextState() method ensures that the condition, above, is
        //  *   satisfied.

        //  * - The getTok() method promises not to extract characters from
        //  * - the TokenSource that belong to another state.

        //  * - These requirements lead us to depend heavily on peeking into
        //  *   the TokenSource's content.

        ///////////////////////////////////////////////////////////////////
        // WhiteSpaceState class
        // - extracts contiguous whitespace chars as a token
        // - will be thrown away by tokenizer

        public class WhiteSpaceState : TokenState
        {
            public WhiteSpaceState(TokenContext context)
            {
                context_ = context;
            }
            //----< manage converting extracted ints to chars >--------------

            bool isWhiteSpace(int i)
            {
                int nextItem = context_.src.peek();
                if (nextItem < 0)
                    return false;
                char ch = (char)nextItem;
                return Char.IsWhiteSpace(ch);
            }
            //----< keep extracting until get none-whitespace >--------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());     // first is WhiteSpace

                while (isWhiteSpace(context_.src.peek()))  // stop when non-WhiteSpace
                {
                    tok.Append((char)context_.src.next());
                }
                return tok;
            }
        }
        ///////////////////////////////////////////////////////////////////
        // PunctState class
        // - extracts contiguous punctuation chars as a token

        public class PunctState : TokenState
        {
            public PunctState(TokenContext context)
            {
                context_ = context;
            }
            //----< manage converting extracted ints to chars >--------------

            bool isPunctuation(int i)
            {
                int nextItem = context_.src.peek();
                if (nextItem < 0)
                    return false;
                char ch = (char)nextItem;
                return (!Char.IsWhiteSpace(ch) && !Char.IsLetterOrDigit(ch));
            }
            //----< keep extracting until get none-punctuator >--------------

            override public Token getTok()
            {
                Token tok = new Token();
                //  tok.Append((char)context_.src.next());       // first is punctuator

                while (isPunctuation(context_.src.peek()))   // stop when non-punctuator
                {
                    tok.Append((char)context_.src.next());
                }
                return tok;
            }
        }
        ///////////////////////////////////////////////////////////////////
        //   AlphaState class
        // - extracts contiguous letter and digit chars as a token

        public class AlphaState : TokenState
        {
            public AlphaState(TokenContext context)
            {
                context_ = context;
            }
            //----< manage converting extracted ints to chars >--------------

            bool isLetterOrDigit(int i)
            {
                int nextItem = context_.src.peek();
                if (nextItem < 0)
                    return false;
                char ch = (char)nextItem;
                return Char.IsLetterOrDigit(ch);
            }
            //----< keep extracting until get none-alpha >-------------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());          // first is alpha

                while (isLetterOrDigit(context_.src.peek()))    // stop when non-alpha
                {
                    tok.Append((char)context_.src.next());
                }
                return tok;
            }
        }

        ///////////////////////////////////////////////////////////////////
        //   DoublePunctState class
        // - extracts all the double characters as a token


        public class DoublePunctState : TokenState

        {
            public DoublePunctState(TokenContext context)
            {

                context_ = context;
            }
            List<string> str2 = new List<string>() { "<<", ">>", "::", "++", "--", "==", "+=", "-=", "*=", "/=", "&&", "||" };
            public void setdoubleSpecialChar(string doublechar)
            {
                str2.Add(doublechar);
            }

            public bool isdoubleChar()
            {
                int nextItem = context_.src.peek();
                int nextItem1 = context_.src.peek(1);
                char a = (char)nextItem;
                StringBuilder b = new StringBuilder();
                b.Append(a);
                char c = (char)nextItem1;
                b.Append(c);
                if (str2.Contains(b.ToString()))
                {
                    return true;
                }
                return false;
            }

            //----------<Keep Extracting until the none - DoublePunct>----------------------------

            override public Token getTok()
            {
                Token tok = new Token();
                if (isdoubleChar())
                {

                    tok.Append((char)context_.src.next());
                    tok.Append((char)context_.src.next());

                }
                return tok;
            }
        }


        ///////////////////////////////////////////////////////////////////
        //   SingleSpecialChar class
        // - extracts all teh single special characters as a token

        public class SingleSpecialChar : TokenState
        {
            public SingleSpecialChar(TokenContext context)
            {
                context_ = context;
            }
            List<string> str = new List<string>() { "<", ">", "[", "]", "(", ")", "{", "}", ":", "=", "+", "-", "*" };
            public void setsingleSpecialChar(string singlechar)
            {
                str.Add(singlechar);
            }
            public bool issingleChar(int nextItem)
            {
                nextItem = context_.src.peek();
                char a = (char)nextItem;
                if (str.Contains(a.ToString()))
                {
                    return true;
                }
                return false;
            }
            //<-------------------Keep extracting until the none single character >-----------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());

                while (issingleChar(context_.src.peek()))
                {
                    tok.Append((char)context_.src.next());
                }
                return tok;
            }
        }

        ///////////////////////////////////////////////////////////////////
        //   CCommentsState class
        // - extracts C Comments as a token


        public class CCommentsState : TokenState
        {

            public CCommentsState(TokenContext context)
            {
                context_ = context;
            }
            public bool isCComment()
            {
                string temp = "";
                char tok;
                int nextItem = context_.src.peek();
                char next = (char)nextItem;
                if (next == '/')
                {
                    temp += next;

                    int nextItem1 = context_.src.peek(1);
                    if ((char)nextItem1 == '/')
                    {
                        temp += (char)nextItem1;
                        context_.src.next();
                        context_.src.next();

                        do
                        {
                            tok = (char)context_.src.peek();
                            temp += (char)context_.src.next();

                        } while (tok.ToString() != "\n");
                        context_.settemptokens(temp.ToString());
                        return true;
                    }

                }
                return false;
            }
            //----------------<keep extracting until none CComment>-------------------

            override public Token getTok()
            {
                StringBuilder gt = new StringBuilder();
                gt.Append(context_.gettemptokens());
                return gt;
            }
        }

        ///////////////////////////////////////////////////////////////////
        //   CppCommentsState class
        // - extracts c++ comments as a token

        public class CppCommentsState : TokenState
        {
            string temp;
            public CppCommentsState(TokenContext context)
            {
                context_ = context;
            }
            public bool isCppComment()
            {
                char tok;
                int nextItem = context_.src.peek();
                char next = (char)nextItem;

                if (next == '/')
                {
                    temp += next;

                    int nextItem1 = context_.src.peek(1);
                    char next1 = (char)nextItem1;
                    if (next1 == '*')
                    {
                        context_.src.next();
                        temp += (char)context_.src.next();

                        do
                        {
                            tok = (char)context_.src.peek();
                            temp += (char)context_.src.next();

                        } while (!EndOfComments(tok));

                        context_.settemptokens(temp.ToString());
                        return true;
                    }
                }
                return false;
            }

            //---------<Checking for the end of Comments in CPP>--------------------

            public bool EndOfComments(char tok)
            {
                if (tok == '*')
                {

                    int nextItem = context_.src.peek();
                    char nexttok = (char)nextItem;
                    if (nexttok == '/')
                    {
                        temp += (char)context_.src.next();
                        return true;
                    }
                }
                return false;
            }
            //----<Keep extracting until none cpp>------------------

            override public Token getTok()
            {
                StringBuilder gt = new StringBuilder();
                gt.Append(context_.gettemptokens());
                return gt;
            }
        }

        ///////////////////////////////////////////////////////////////////
        //   DoubleQuoteState class
        // - extracts DoubleQuotes as a token

        public class DoubleQuoteState : TokenState
        {

            public DoubleQuoteState(TokenContext context)
            {
                context_ = context;
            }
            public bool isDoubleQuote()
            {
                char tok;
                string temp = "";
                int nextItem = context_.src.peek();
                char nexttok = (char)nextItem;
                if (nexttok.Equals('\"'))
                {
                    temp += nexttok;
                    context_.src.next();

                    do
                    {
                        tok = (char)context_.src.peek();
                        temp += (char)context_.src.next();


                    } while (!tok.Equals('\"'));
                    context_.settemptokens(temp.ToString());
                    return true;
                }
                return false;
            }
            //-----<Keep extracting until none DoubleQuote>-------------

            public override Token getTok()
            {
                StringBuilder gt = new StringBuilder();
                gt.Append(context_.gettemptokens());
                return gt;
            }
        }


        //////////////////////////////////////////////////////////////
        /// PoundState Class
        /// - extracts Pound as a token

        public class PoundState : TokenState
        {

            public PoundState(TokenContext context)
            {
                context_ = context;
            }
            public bool isPound()
            {
                string temp = "";
                char tok;
                int nextItem = context_.src.peek();
                char next = (char)nextItem;
                if (next == '#')
                {
                    temp += next;
                    context_.src.next();

                    do
                    {
                        tok = (char)context_.src.peek();
                        temp += (char)context_.src.next();

                    } while (tok.ToString() != "\n");
                    context_.settemptokens(temp.ToString());
                    return true;
                }
                return false;
            }
            //------<Keep extracting until none Pound >------------

            override public Token getTok()
            {
                StringBuilder gt = new StringBuilder();
                gt.Append(context_.gettemptokens());
                return gt;
            }
        }

    }


    ///////////////////////////////////////////////////////////////////
    // TokenSourceFile class
    // - extracts integers from token source
    // - Streams often use terminators that can't be represented by
    //   a character, so we collect all elements as ints
    // - keeps track of the line number where a token is found
    // - uses StreamReader which correctly handles byte order mark
    // - characters and alternate text encodings.

    public class TokenSourceFile : TokenInterface.ITokenSource
    {
        public int lineCount { get; set; } = 1;
        private System.IO.StreamReader fs_;           // physical source of text
        private List<int> charQ_ = new List<int>();   // enqueing ints but using as chars
        private TokenContext context_;

        public TokenSourceFile(TokenContext context)
        {
            context_ = context;
        }
        //----< attempt to open file with a System.IO.StreamReader >-----

        public bool open(string path)
        {
            try
            {
                fs_ = new System.IO.StreamReader(path, true);
                context_.currentState_ = TokenContext.TokenState.nextState(context_);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}\n", ex.Message);
                return false;
            }
            return true;
        }
        //----< close file >---------------------------------------------

        public void close()
        {
            fs_.Close();
        }
        //----< extract the next available integer >---------------------
        /*
         *  - checks to see if previously enqueued peeked ints are available
         *  - if not, reads from stream
         */
        public int next()
        {
            int ch;
            if (charQ_.Count == 0)  // no saved peeked ints
            {
                if (end())
                    return -1;
                ch = fs_.Read();
            }
            else                    // has saved peeked ints, so use the first
            {
                ch = charQ_[0];
                charQ_.Remove(ch);
            }
            if ((char)ch == '\n')   // track the number of newlines seen so far
                ++lineCount;
            return ch;
        }
        //----< peek n ints into source without extracting them >--------
        /*
         *  - This is an organizing prinicple that makes tokenizing easier
         *  - We enqueue because file streams only allow peeking at the first int
         *    and even that isn't always reliable if an error occurred.
         
        */
        public int peek(int n = 0)
        {
            if (n < charQ_.Count)  // already peeked, so return
            {
                return charQ_[n];
            }
            else                  // nth int not yet peeked
            {
                for (int i = charQ_.Count; i <= n; ++i)
                {
                    if (end())
                        return -1;
                    charQ_.Add(fs_.Read());  // read and enqueue
                }
                return charQ_[n];   // now return the last peeked
            }
        }

        //------------ Checks for the end of the stream>-----------------------------

        public bool end()
        {
            return fs_.EndOfStream;
        }
    }
    //---------------------TEST STUB fOR TEST STUB>------------------------
#if (TEST_TOKER)

    class DemoToker
    {
        static bool testToker(string path)
        {
            Toker toker = new Toker();

            string fqf = System.IO.Path.GetFullPath(path);
            if (!toker.open(fqf))
            {
                Console.Write("\n can't open {0}\n", fqf);
                return false;
            }
            else
            {
                Console.Write("\n  processing file: {0}", fqf);
            }
            while (!toker.isDone())
            {

                Token tok = toker.getTok();
                Console.Write("\n -- line#{0, 4} : {1}", toker.lineCount(), tok);
            }
            toker.close();
            return true;
        }
        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrate Toker class");
            Console.Write("\n =========================");

            StringBuilder msg = new StringBuilder();
            msg.Append("\n  Some things this demo does not do for CSE681 Project #2:");
            msg.Append("\n  - collect comments as tokens");
            msg.Append("\n  - collect double quoted strings as tokens");
            msg.Append("\n  - collect single quoted strings as tokens");
            msg.Append("\n  - collect specified single characters as tokens");
            msg.Append("\n  - collect specified character pairs as tokens");
            msg.Append("\n  - integrate with a SemiExpression collector");
            msg.Append("\n  - provide the required package structure");
            msg.Append("\n");

            Console.Write(msg);

            testToker("../../../Test.txt");

            Console.Write("\n\n");
        }
    }


#endif
}





