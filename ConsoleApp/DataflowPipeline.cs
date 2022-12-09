using System.Threading.Tasks.Dataflow;
using Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class DataflowPipeline: IDataflowPipeline
{
    private int _maxCuncurrentInput;
    private int _maxCuncurrentOutput;
    private int _maxConcurrentProcessing;
    private string _input;
    private string _output;

    public DataflowPipeline(string input, string output, int maxCuncurrentInput, int maxCuncurrentOutput, int maxConcurrentProcessing)
    {
        _input = input;
        _output = output;
        _maxCuncurrentInput = maxCuncurrentInput;
        _maxCuncurrentOutput = maxCuncurrentOutput;
        _maxConcurrentProcessing = maxConcurrentProcessing;
    }

    public async Task ExecuteAsync()
    {
        var bufferBlock = new BufferBlock<string>();

        var readingBlock = new TransformBlock<string, string>(
            async path => await GetSourceCode(path),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxCuncurrentInput });

        var processingBlock = new TransformManyBlock<string, string>(
            content => ProcessSourceCode(content),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxConcurrentProcessing });

        var writingBlock = new ActionBlock<string>(async tests => await WriteTests(tests),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxCuncurrentOutput });

        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

        bufferBlock.LinkTo(readingBlock, linkOptions);
        readingBlock.LinkTo(processingBlock, linkOptions);
        processingBlock.LinkTo(writingBlock, linkOptions);

        foreach (var file in Directory.EnumerateFiles(_input))
        {
            bufferBlock.Post(file);
        }

        bufferBlock.Complete();

        await writingBlock.Completion;
    }

    private async Task<string> GetSourceCode(string filePath)
    {
        string sourceCode;
        using (var streamReader = new StreamReader(filePath))
        {
            sourceCode = await streamReader.ReadToEndAsync();
        }
        return sourceCode;
    }

    private List<string> ProcessSourceCode(string sourceCode)
    {
        var tests = XUnitTestGenerator.GenerateTests(sourceCode);
        return tests;
    }

    private async Task WriteTests(string tests)
    {
        var fileName = CSharpSyntaxTree.ParseText(tests).GetRoot()
            .DescendantNodes().OfType<ClassDeclarationSyntax>().First().Identifier.Text;

        using (var streamWriter = new StreamWriter($"{_output}\\{fileName}.cs"))
        {
            await streamWriter.WriteAsync(tests);
        }

    }
}