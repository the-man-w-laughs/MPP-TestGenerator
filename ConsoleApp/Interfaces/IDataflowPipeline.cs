using System.Threading.Tasks.Dataflow;
using Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public interface IDataflowPipeline
{
    public Task ExecuteAsync();
}