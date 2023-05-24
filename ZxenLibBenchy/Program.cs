// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using ZxenLibBenchy.Extensions;

BenchmarkRunner.Run<FloatExtensionsBenchmarks>();

Console.ReadLine();