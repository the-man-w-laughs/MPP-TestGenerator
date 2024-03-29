﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks.Dataflow;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Demonstrates how to create a basic dataflow pipeline.
// This program downloads the book "The Iliad of Homer" by Homer from the Web
// and finds all reversed words that appear in that book.

static class Program
{
    static async Task Main()
    {
        string inputDir;
        //Console.Write($"Input dir: ");
        //inputDir = Console.ReadLine();
        inputDir = "D:\\Проекты\\5йСеместр\\c#\\TestGenerator\\Tests\\InputTestsFilesDir";
        while (!Directory.Exists(inputDir))
        {
            Console.WriteLine($"Directory doesn't exist: \"{inputDir}\".");
            inputDir = Console.ReadLine();
        }

        string outputDir;
        //Console.Write($"Output dir: ");        
        //outputDir = Console.ReadLine();
        outputDir = "D:\\Проекты\\5йСеместр\\c#\\TestGenerator\\Tests\\OutputTestsFilesDir";
        while (!Directory.Exists(outputDir))
        {
            Console.WriteLine($"Directory doesn't exist: \"{outputDir}\".");
            outputDir = Console.ReadLine();
        }

        var dataFlowPipeline = new DataflowPipeline(inputDir, outputDir, 3, 3, 3);
        await dataFlowPipeline.ExecuteAsync();
    }
}