# Lexical-Scanner-using-State-Based-Tokenizer

This project is built using C# on visual studio 2015 platform.
The project mainly focuses on building the software tool for code analysis.
Code analysis consists of extracting lexical content from source code files, analyzing the code's syntax from its lexical content, 
and building an Abstract Syntax Tree (AST) that holds the results of our analysis.
I have built following packages :
1) Tokenizer : Extracts words, called tokens, from a stream of characters. Token boundaries are white-space characters, transitions between alphanumeric and punctuator characters, and comment and string boundaries. Certain classes of punctuator characters belong to single character or two character tokens so they require special rules for extraction.
2) SemiExpression : Groups tokens into sets, each of which contain all the information needed to analyze some grammatical construct without containing extra tokens that have to be saved for subsequent analyses. SemiExpressions are determined by special terminating characters: semicolon, open brace, closed brace, and newline when preceeded on the same line with 'using'.



# Install this project on your system

    
You will need Visual Studio 2015.
The project consists of run.bat and compile.bat files.
Run.bat - Runs the project. It is required for C# or VB.NET applications.
Compile.bat - Compiles the project automatically. 
Execute run.bat and compile.bat on your command propmpt to run this project.
