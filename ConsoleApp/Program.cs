using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

// Demonstrates how to create a basic dataflow pipeline.
// This program downloads the book "The Iliad of Homer" by Homer from the Web
// and finds all reversed words that appear in that book.
static class Program
{
    static async void Main()
    {
        string inputDir;
        inputDir = Console.ReadLine();
        while (!Directory.Exists(inputDir)){
            Console.WriteLine($"Directory doesn't exist: \"{inputDir}\".");
            inputDir = Console.ReadLine();
        }        
        Console.WriteLine($"Input directory: \"{inputDir}\".");

        string outputDir;
        outputDir = Console.ReadLine();
        while (!Directory.Exists(outputDir)){
            Console.WriteLine($"Directory doesn't exist: \"{outputDir}\".");
            outputDir = Console.ReadLine();
        }    
        Console.WriteLine($"Output directory: \"{outputDir}\".");  
        
        var dataFlowPipeline = new DataflowPipeline(inputDir, outputDir, 3, 3, 3);
        await dataFlowPipeline.GenerateTestsAsync();        
    }
}