using hcl_net.Parse.HCL;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var input = File.ReadAllText("Input/main.tf");
var sut = new Parser(input);
string parserError;
var file = sut.Parse(out parserError);
Console.WriteLine($"{parserError}");